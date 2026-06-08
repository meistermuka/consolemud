using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class InventoryCommand : ICommand
{
    public void Execute(Player player, string[] args, WorldState world)
    {
        Console.WriteLine("\nYou are carrying:");
        
        if (!player.Inventory.Any())
        {
            Console.WriteLine("  Nothing. Your hands are empty.");
            return;
        }

        foreach (var item in player.Inventory)
        {
            Console.WriteLine($"  - {item.Name}: {item.Description}");
        }
        Console.WriteLine();
    }
}