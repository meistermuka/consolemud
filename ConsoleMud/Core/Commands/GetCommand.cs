using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class GetCommand : ICommand
{
    public string Description => "Pick up an item or everything; optionally from a container.";
    public string Usage => "get <item|all> [<keyword>] [from <container>]";
    public string Example => "get all from corpse";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Get what?");
            return;
        }

        int fromIndex = Array.IndexOf(args, "from");
        var whatTokens = fromIndex == -1 ? args : args.Take(fromIndex).ToArray();
        string containerName = fromIndex == -1 ? null : string.Join(" ", args.Skip(fromIndex + 1));

        var room = world.Rooms[player.CurrentRoomId];

        // Bulk: "get all", "get all <keyword>", "get all [<keyword>] from <container>".
        if (whatTokens.Length > 0 && whatTokens[0].Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            string keyword = string.Join(" ", whatTokens.Skip(1)).Trim();
            if (containerName == null)
                BulkGet(player, room.Items, keyword, "the ground", null);
            else
                BulkGetFromContainer(player, room, containerName, keyword);
            return;
        }

        string itemName = string.Join(" ", whatTokens);
        if (containerName == null)
            HandleStandardGet(player, room, itemName);
        else
            HandleContainerGet(player, room, itemName, containerName);
    }

    private void HandleStandardGet(Player player, Room room, string itemName)
    {
        var (targetIndex, cleanKeyword) = KeywordParser.ExtractIndex(itemName);
        Item foundItem = null;
        int matches = 0;
        foreach (var item in room.Items)
            if (item.MatchesKeyword(cleanKeyword) && ++matches == targetIndex)
            {
                foundItem = item;
                break;
            }

        if (foundItem == null) { Console.WriteLine($"You don't see a '{itemName}' here."); return; }
        if (!foundItem.IsGetable) { Console.WriteLine($"The {foundItem.Name} is too heavy."); return; }

        room.Items.Remove(foundItem);
        player.Inventory.Add(foundItem);
        ColorConsole.WriteLine($"You pick up the {foundItem.Name}.", ConsoleColor.Gray);
    }

    private void HandleContainerGet(Player player, Room room, string itemName, string containerName)
    {
        var container = FindContainer(player, room, containerName);
        if (container == null) { Console.WriteLine($"You don't see a '{containerName}' here."); return; }
        if (!container.IsContainer) { Console.WriteLine($"The {container.Name} cannot hold items."); return; }

        var item = container.Contents.FirstOrDefault(i => i.MatchesKeyword(itemName));
        if (item == null) { Console.WriteLine($"There is no '{itemName}' inside the {container.Name}."); return; }

        container.Contents.Remove(item);
        player.Inventory.Add(item);
        ColorConsole.WriteLine($"You pull the {item.Name} out of the {container.Name}.", ConsoleColor.Gray);
    }

    private void BulkGetFromContainer(Player player, Room room, string containerName, string keyword)
    {
        var container = FindContainer(player, room, containerName);
        if (container == null) { Console.WriteLine($"You don't see a '{containerName}' here."); return; }
        if (!container.IsContainer) { Console.WriteLine($"The {container.Name} cannot hold items."); return; }
        BulkGet(player, container.Contents, keyword, container.Name, container.Name);
    }

    /// <summary>Moves every getable, keyword-matching item from a source list into the player.</summary>
    private void BulkGet(Player player, List<Item> source, string keyword, string sourceLabel, string fromContainer)
    {
        var taken = source
            .Where(i => i.IsGetable && (keyword.Length == 0 || i.MatchesKeyword(keyword)))
            .ToList();

        if (taken.Count == 0)
        {
            string what = keyword.Length == 0 ? "nothing you can take" : $"no '{keyword}'";
            Console.WriteLine($"There is {what} {(fromContainer == null ? "here" : $"in the {sourceLabel}")}.");
            return;
        }

        foreach (var item in taken)
        {
            source.Remove(item);
            player.Inventory.Add(item);
            string verb = fromContainer == null ? "pick up" : "take";
            ColorConsole.WriteLine($"You {verb} the {item.Name}.", ConsoleColor.Gray);
        }

        Console.WriteLine($"({taken.Count} item{(taken.Count == 1 ? "" : "s")} taken.)");
    }

    private static Item FindContainer(Player player, Room room, string name) =>
        room.Items.FirstOrDefault(i => i.MatchesKeyword(name))
        ?? player.Inventory.FirstOrDefault(i => i.MatchesKeyword(name));
}
