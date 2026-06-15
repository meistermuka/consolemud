using ConsoleMud.Core.Combat;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Fighter;

public class KickHandler : ISkillHandler
{
    public string SkillId => "kick";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.Caster.CombatTarget;
        if (target == null)
        {
            Console.WriteLine("You have nothing to kick. Engage a target first.");
            return;
        }

        var outcome = AttackResolver.Resolve(
            ctx.Caster, target,
            ctx.Definition.DiceNotation ?? "1d4",
            DamageType.Physical,
            attributeBonus: ctx.Definition.AttributeBonus);

        if (!outcome.Hit)
        {
            Console.WriteLine($"You swing a kick at {target.Name} but miss!");
            return;
        }

        target.Health -= outcome.Damage;
        Console.WriteLine($"You kick {target.Name} for {outcome.Damage} damage! -> [{target.Name} HP: {Math.Max(0, target.Health)}]");

        if (target.Health <= 0)
            DeathService.HandleDeath(target, ctx.World);
    }
}
