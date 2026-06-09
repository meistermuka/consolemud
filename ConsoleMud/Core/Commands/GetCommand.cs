using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class GetCommand : ICommand
{
    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Get what?");
            return;
        }

        int fromIndex = Array.IndexOf(args, "from");

        // Case A: Standard retrieval (get sword)
        if (fromIndex == -1)
        {
            HandleStandardGet(player, string.Join(" ", args), world);
        }
        // Case B: Container retrieval (get sword from chest)
        else
        {
            string itemName = string.Join(" ", args.Take(fromIndex)).ToLower();
            string containerName = string.Join(" ", args.Skip(fromIndex + 1)).ToLower();
            HandleContainerGet(player, itemName, containerName, world);
        }
    }

    private void HandleStandardGet(Player player, string itemName, WorldState world)
    {
        var room = world.Rooms[player.CurrentRoomId];
        var (targetIndex, cleanKeyword) = KeywordParser.ExtractIndex(itemName);
        Item foundItem = null;
        int currentMatchCount = 0;
        
        foreach (var item in room.Items)
        {
            if (item.MatchesKeyword(cleanKeyword))
            {
                currentMatchCount++;
                if (currentMatchCount == targetIndex)
                {
                    foundItem = item;
                    break;
                }
            }
        }

        if (foundItem == null) { Console.WriteLine($"You don't see a '{itemName}' here."); return; }
        if (!foundItem.IsGetable) { Console.WriteLine($"The {foundItem.Name} is too heavy."); return; }

        room.Items.Remove(foundItem);
        player.Inventory.Add(foundItem);
        Console.WriteLine($"You pick up the {foundItem.Name}.");
    }

    private void HandleContainerGet(Player player, string itemName, string containerName, WorldState world)
    {
        var room = world.Rooms[player.CurrentRoomId];
        
        // Find the container in the room OR in the player's personal inventory
        var container = room.Items.FirstOrDefault(i => i.Name.Equals(containerName, StringComparison.OrdinalIgnoreCase))
                     ?? player.Inventory.FirstOrDefault(i => i.Name.Equals(containerName, StringComparison.OrdinalIgnoreCase));

        if (container == null) { Console.WriteLine($"You don't see a '{containerName}' here."); return; }
        if (!container.IsContainer) { Console.WriteLine($"The {container.Name} cannot hold items."); return; }

        // Find item inside the container
        var item = container.Contents.FirstOrDefault(i => i.MatchesKeyword(itemName));
        if (item == null) { Console.WriteLine($"There is no '{itemName}' inside the {container.Name}."); return; }

        container.Contents.Remove(item);
        player.Inventory.Add(item);
        Console.WriteLine($"You pull the {item.Name} out of the {container.Name}.");
    }
}