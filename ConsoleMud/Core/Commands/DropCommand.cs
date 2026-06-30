using ConsoleMud.Entities;
using ConsoleMud.Helpers;

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
            ColorConsole.WriteLine("Drop what?");
            return;
        }

        string targetItemName = string.Join(" ", args).ToLower();
        var room = world.Rooms[player.CurrentRoomId];

        var item = player.Inventory.FirstOrDefault(i => i.MatchesKeyword(targetItemName));

        if (item == null)
        {
            ColorConsole.WriteLine($"You aren't carrying a '{targetItemName}'.");
            return;
        }

        player.Inventory.Remove(item);
        room.Items.Add(item);

        ColorConsole.WriteLine($"You drop the {item.Name} onto the ground.", ConsoleColor.Gray);
    }
}
