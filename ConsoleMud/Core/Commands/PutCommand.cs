using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class PutCommand : ICommand
{
    public string Description => "Place an item from your inventory into a container.";
    public string Usage => "put <item> in <container>";
    public string Example => "put ring in sack";

    public void Execute(Player player, string[] args, WorldState world)
    {
        int inIndex = Array.IndexOf(args, "in");
        if (inIndex <= 0 || inIndex == args.Length - 1)
        {
            Console.WriteLine("Syntax: put <item> in <container>");
            return;
        }

        string itemName = string.Join(" ", args.Take(inIndex)).ToLower();
        string containerName = string.Join(" ", args.Skip(inIndex + 1)).ToLower();

        var room = world.Rooms[player.CurrentRoomId];

        // 1. Find item in player's inventory
        var item = player.Inventory.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (item == null) { Console.WriteLine($"You aren't carrying a '{itemName}'."); return; }

        // 2. Find container nearby
        var container = room.Items.FirstOrDefault(i => i.Name.Equals(containerName, StringComparison.OrdinalIgnoreCase))
                        ?? player.Inventory.FirstOrDefault(i => i.Name.Equals(containerName, StringComparison.OrdinalIgnoreCase));

        if (container == null) { Console.WriteLine($"You don't see a '{containerName}' here."); return; }
        if (!container.IsContainer) { Console.WriteLine($"You can't put things inside the {container.Name}."); return; }

        // 3. Perform transfer
        player.Inventory.Remove(item);
        container.Contents.Add(item);
        Console.WriteLine($"You slide the {item.Name} safely into the {container.Name}.");
    }
}