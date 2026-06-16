using ConsoleMud.Core.Combat;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Cleric;

public class SmiteHandler : ISkillHandler
{
    public string SkillId => "smite";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveNpcTarget();
        if (target == null)
        {
            Console.WriteLine("Smite what?");
            return;
        }

        ctx.Engage(target);

        var outcome = AttackResolver.Resolve(
            ctx.Caster, target,
            ctx.Definition.DiceNotation ?? "2d6",
            DamageType.Holy,
            attributeBonus: ctx.Definition.AttributeBonus,
            ignoresArmor: true);

        if (!outcome.Hit)
        {
            Helpers.ColorConsole.WriteLine($"Your smite flares wide of {target.Name}.", ConsoleColor.Gray);
            return;
        }

        int dmg = outcome.Damage;
        bool blessedFoe = target.Archetypes.Contains(Archetype.Undead) || target.Archetypes.Contains(Archetype.Fiend);
        if (blessedFoe)
            dmg = (int)(dmg * ctx.Param("archetypeBonus", 2));

        target.Health -= dmg;
        Helpers.ColorConsole.WriteLine(
            $"Divine fire smites {target.Name} for {dmg} holy damage{(blessedFoe ? " (double vs unholy)" : "")}! " +
            $"-> [{target.Name} HP: {Math.Max(0, target.Health)}]", ConsoleColor.Gray);

        if (target.Health <= 0)
            DeathService.HandleDeath(target, ctx.World, ctx.Caster);
    }
}
