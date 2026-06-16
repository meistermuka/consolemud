using ConsoleMud.Core.Combat;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Mage;

public class DisintegrateHandler : ISkillHandler
{
    public string SkillId => "disintegrate";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveNpcTarget();
        if (target == null) { Console.WriteLine("Disintegrate what?"); return; }

        ctx.Engage(target);
        var outcome = AttackResolver.Resolve(ctx.Caster, target, ctx.Definition.DiceNotation ?? "8d8",
                                             DamageType.Force, ignoresArmor: true);
        if (!outcome.Hit) { Helpers.ColorConsole.WriteLine($"The beam misses {target.Name}.", ConsoleColor.Gray); return; }

        int dmg = outcome.Damage + ctx.SpellPowerBonus();
        target.Health -= dmg;
        Helpers.ColorConsole.WriteLine(
            $"A beam of pure force tears into {target.Name} for {dmg}! -> [{target.Name} HP: {Math.Max(0, target.Health)}]",
            ConsoleColor.Gray);

        if (target.Health <= 0)
        {
            // Vaporized: no lootable corpse.
            target.Equipment.Clear();
            target.Inventory.Clear();
            Helpers.ColorConsole.WriteLine($"{target.Name} is vaporized, leaving nothing behind.", ConsoleColor.Gray);
            DeathService.HandleDeath(target, ctx.World, ctx.Caster);
        }
    }
}
