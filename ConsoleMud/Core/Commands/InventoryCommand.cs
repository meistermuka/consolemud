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

        foreach (var (count, item) in player.Inventory.GroupedByName())
        {
            var prefix = count > 1 ? $"{count} x " : string.Empty;
            ColorConsole.WriteLine($"  - {prefix}{item.Name}", ConsoleColor.Gray);
        }
        ColorConsole.WriteLine();
    }
}
