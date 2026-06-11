using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class DropCommand : ICommand
{
    public string Description => "Drop an item from your inventory onto the ground.";
    public string Usage => "drop <item>";
    public string Example => "drop dagger";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Drop what?");
            return;
        }

        string targetItemName = string.Join(" ", args).ToLower();
        var room = world.Rooms[player.CurrentRoomId];

        // Find the item in the player's inventory
        var item = player.Inventory.FirstOrDefault(i => i.MatchesKeyword(targetItemName));

        if (item == null)
        {
            Console.WriteLine($"You aren't carrying a '{targetItemName}'.");
            return;
        }

        // Atomic transfer: Remove from player, add to room
        player.Inventory.Remove(item);
        room.Items.Add(item);

        Console.WriteLine($"You drop the {item.Name} onto the ground.");
    }
}