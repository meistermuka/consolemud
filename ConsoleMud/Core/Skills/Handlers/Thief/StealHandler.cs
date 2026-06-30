using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Thief;

public class StealHandler : ISkillHandler
{
    public string SkillId => "steal";

    public void Execute(SkillContext ctx)
    {
        if (ctx.Caster is not Player thief) return;

        var target = ctx.ResolveNpcTarget();
        if (target == null) { ColorConsole.WriteLine("Steal from whom?"); return; }

        var loot = target.Inventory.FirstOrDefault(i => i.IsGetable)
                   ?? target.Equipment.Values.FirstOrDefault(i => i.IsGetable);
        if (loot == null)
        {
            Helpers.ColorConsole.WriteLine($"{target.Name} has nothing worth stealing.", ConsoleColor.Gray);
            return;
        }

        target.Inventory.Remove(loot);
        foreach (var slot in target.Equipment.Where(kv => kv.Value == loot).Select(kv => kv.Key).ToList())
            target.Equipment.Remove(slot);
        thief.Inventory.Add(loot);
        Helpers.ColorConsole.WriteLine($"You deftly lift the {loot.Name} from {target.Name}.", ConsoleColor.Gray);

        // Risk: the mark may notice and turn hostile.
        if (Random.Shared.NextDouble() < ctx.Param("catchChance", 0.5))
        {
            ctx.Caster.BreakHidden();
            if (target.CombatTarget == null) target.CombatTarget = thief;
            if (thief.CombatTarget == null) thief.CombatTarget = target;
            Helpers.ColorConsole.WriteLine($"{target.Name} catches you in the act and attacks!", ConsoleColor.Red);
        }
    }
}
