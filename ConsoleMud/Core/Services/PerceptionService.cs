using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Services;

/// <summary>
/// Cross-room perception used by the ranger 'scout' skill and the druid
/// 'eco_location' passive: peek into a neighbouring room, or scan outward.
/// </summary>
public static class PerceptionService
{
    /// <summary>One-line summary of the room through a given exit, without entering it.</summary>
    public static string DescribeAdjacent(WorldState world, Room from, Direction dir)
    {
        if (!from.Exits.TryGetValue(dir, out var id) || !world.Rooms.TryGetValue(id, out var room))
            return $"There is no exit {dir.ToString().ToLower()}.";

        int items = room.Items.Count;
        int mobs = room.Characters.OfType<NonPlayerCharacter>().Count(n => !n.IsHidden);
        return $"{dir} -> [{ColorMarkup.Strip(room.Name)}] : {mobs} creature(s), {items} item(s)";
    }

    /// <summary>Names of characters within <paramref name="radius"/> rooms (breadth-first).</summary>
    public static List<string> ScanNearby(WorldState world, Room origin, int radius)
    {
        var found = new List<string>();
        var seen = new HashSet<Guid> { origin.Id };
        var frontier = new List<(Room room, int dist)> { (origin, 0) };

        while (frontier.Count > 0)
        {
            var (room, dist) = frontier[0];
            frontier.RemoveAt(0);

            if (dist > 0)
                foreach (var c in room.Characters.Where(c => !c.IsHidden))
                    found.Add($"{ColorMarkup.Strip(c.Name)} ({dist} room(s) away in {ColorMarkup.Strip(room.Name)})");

            if (dist >= radius) continue;
            foreach (var exitId in room.Exits.Values)
                if (seen.Add(exitId) && world.Rooms.TryGetValue(exitId, out var next))
                    frontier.Add((next, dist + 1));
        }
        return found;
    }
}
