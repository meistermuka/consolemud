using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class LookCommand : ICommand
{
    public void Execute(Player player, string[] args, WorldState world)
    {
        var room = world.Rooms[player.CurrentRoomId];
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n[{room.Name}]");
        Console.ResetColor();
        Console.WriteLine(room.Description);
        
        // Show exits
        var exits = string.Join(", ", room.Exits.Select(e => e.Key.ToString().ToLower()));
        Console.WriteLine($"Exits: {(string.IsNullOrEmpty(exits) ? "none" : exits)}");
        
        // Show items
        foreach (var item in room.Items)
            Console.WriteLine($"You see a {item.Name} here.");
        
        // Show NPCs
        foreach (var character in room.Characters.Where(c => c != player))
            Console.WriteLine($"{character.Name} here.");
        
        Console.WriteLine();
    }
}