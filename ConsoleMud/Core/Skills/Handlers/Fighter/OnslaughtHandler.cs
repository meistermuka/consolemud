using ConsoleMud.Core.Combat;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Fighter;

public class OnslaughtHandler : ISkillHandler
{
    public string SkillId => "onslaught";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveNpcTarget();
        if (target == null)
        {
            ColorConsole.WriteLine("Unleash your onslaught on what?");
            return;
        }

        ctx.Engage(target);

        int hits = Math.Max(1, (int)ctx.Param("hits", 5));
        double ramp = ctx.Param("rampPct", 10) / 100.0;
        string dice = ctx.Caster.MainHandWeapon?.DiceNotation ?? "1d4";

        Helpers.ColorConsole.WriteLine($"You explode into a flurry of blows against {target.Name}!", ConsoleColor.Red);

        for (int i = 0; i < hits && target.Health > 0; i++)
        {
            var outcome = AttackResolver.Resolve(ctx.Caster, target, dice, DamageType.Physical);
            if (!outcome.Hit)
            {
                Helpers.ColorConsole.WriteLine($"  hit {i + 1}: miss!", ConsoleColor.Gray);
                continue;
            }

            int dmg = Math.Max(1, (int)Math.Round(outcome.Damage * (1.0 + ramp * i)));
            target.Health -= dmg;
            Helpers.ColorConsole.WriteLine(
                $"  hit {i + 1}: {dmg} damage -> [{target.Name} HP: {Math.Max(0, target.Health)}]", ConsoleColor.Gray);

            if (target.Health <= 0)
            {
                DeathService.HandleDeath(target, ctx.World, ctx.Caster);
                break;
            }
        }
    }
}
