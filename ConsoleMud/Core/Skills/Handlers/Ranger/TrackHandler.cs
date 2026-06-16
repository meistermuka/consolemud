using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Ranger;

public class TrackHandler : ISkillHandler
{
    public string SkillId => "track";

    public void Execute(SkillContext ctx)
    {
        // Find the quarry in this room, or one exit away if it has moved on.
        var target = ctx.ResolveNpcTarget() ?? FindAdjacent(ctx);
        if (target == null)
        {
            Console.WriteLine($"You find no trail for '{ctx.TargetName}'.");
            return;
        }

        // Mark it (mark_of_the_hunter applies the damage bonus in combat).
        ctx.Caster.MarkedTarget = target;

        string trail = target.LastExit.HasValue
            ? $"Its trail leads {target.LastExit.Value.ToString().ToLower()}."
            : "It hasn't moved from where you see it.";
        ColorConsole.WriteLine($"You study {target.Name}'s tracks and mark it as your quarry. {trail}", ConsoleColor.Gray);
    }

    private static NonPlayerCharacter FindAdjacent(SkillContext ctx)
    {
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room))
            return null;
        var (_, keyword) = KeywordParser.ExtractIndex(string.Join(" ", ctx.Args));
        foreach (var exitId in room.Exits.Values)
            if (ctx.World.Rooms.TryGetValue(exitId, out var adj))
            {
                var npc = adj.Characters.OfType<NonPlayerCharacter>().FirstOrDefault(n => n.MatchesKeyword(keyword));
                if (npc != null) return npc;
            }
        return null;
    }
}
