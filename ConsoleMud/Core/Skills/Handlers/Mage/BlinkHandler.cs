using ConsoleMud.Core.Commands;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Mage;

public class BlinkHandler : ISkillHandler
{
    public string SkillId => "blink";

    public void Execute(SkillContext ctx)
    {
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room) || room.Exits.Count == 0)
        {
            Console.WriteLine("There is nowhere to blink to.");
            return;
        }

        // Sever combat both ways.
        ctx.Caster.CombatTarget = null;
        foreach (var c in ctx.World.Characters.Values.Where(c => c.CombatTarget == ctx.Caster))
            c.CombatTarget = null;
        ctx.Caster.Position = Position.Standing;

        var exits = room.Exits.ToList();
        var chosen = exits[Random.Shared.Next(exits.Count)];
        ctx.Caster.LastExit = chosen.Key;
        ctx.World.MoveCharacter(ctx.Caster, chosen.Value);
        Helpers.ColorConsole.WriteLine("\nReality folds and you blink away!", ConsoleColor.Gray);

        if (ctx.Caster is Player p)
            new LookCommand().Execute(p, Array.Empty<string>(), ctx.World);
    }
}
