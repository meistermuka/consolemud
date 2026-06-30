using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class InventoryCommand : ICommand
{
    public string Description => "List what you are carrying.";
    public string Usage => "inventory";
    public string Example => "inv";

    public void Execute(Player player, string[] args, WorldState world)
    {
        ColorConsole.WriteLine("\nYou are carrying:");

        if (!player.Inventory.Any())
        {
            ColorConsole.WriteLine("  Nothing. Your hands are empty.");
            return;
        }

        foreach (var item in player.Inventory)
        {
            ColorConsole.WriteLine($"  - {item.Name}: {item.Description}", ConsoleColor.Gray);
        }
        ColorConsole.WriteLine();
    }
}
