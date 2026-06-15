using ConsoleMud.Core.Combat;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Mage;

public class MagicMissileHandler : ISkillHandler
{
    public string SkillId => "magic_missile";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveNpcTarget();
        if (target == null)
        {
            Console.WriteLine("Fire your missile at what?");
            return;
        }

        ctx.Engage(target);

        // Pure force; ignores armor and shields.
        var outcome = AttackResolver.Resolve(
            ctx.Caster, target,
            ctx.Definition.DiceNotation ?? "1d4",
            DamageType.Force,
            ignoresArmor: true);

        if (!outcome.Hit)
        {
            Helpers.ColorConsole.WriteLine($"Your magic missile streaks past {target.Name}!", ConsoleColor.Gray);
            return;
        }

        target.Health -= outcome.Damage;
        Helpers.ColorConsole.WriteLine(
            $"Your magic missile strikes {target.Name} for {outcome.Damage} force damage! -> [{target.Name} HP: {Math.Max(0, target.Health)}]",
            ConsoleColor.Gray);

        if (target.Health <= 0)
            DeathService.HandleDeath(target, ctx.World, ctx.Caster);
    }
}
