// See https://aka.ms/new-console-template for more information

using ConsoleMud.Core;
using ConsoleMud.Core.Commands;
using ConsoleMud.Entities;
using ConsoleMud.Helpers;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Booting local world...standby...");
        var world = WorldBuilder.CreateSampleWorld();
        var parser = new CommandParser();
        
        var foyerId = world.Rooms.First(r => r.Value.Name.Contains("Foyer")).Key;

        var player = new Player
        {
            Name = "Kronos",
            Description = "An elf",
            Health = 100,
            MaxHealth = 100,
            CurrentRoomId = foyerId
        };
        
        // Tracking player in the master state
        world.Rooms[foyerId].Characters.Add(player);
        world.Characters[player.Id] = player;
        
        Console.Clear();
        Console.WriteLine("=== Welcome to the Sandbox MUD ===");
        
        // Renter initial starting room
        new LookCommand().Execute(player, Array.Empty<string>(), world);
        
        // Main loop
        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine();

            if (input?.ToLower() == "exit" || input?.ToLower() == "quit")
            {
                Console.WriteLine("Goodbye!");
                break;
            }
            
            parser.ParseAndExecute(input, player, world);
        }
    }
}