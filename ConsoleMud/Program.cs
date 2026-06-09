// See https://aka.ms/new-console-template for more information

using ConsoleMud.Core;
using ConsoleMud.Core.Commands;
using ConsoleMud.Core.Services;
using ConsoleMud.Helpers;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Booting local world...standby...");
        var world = new WorldState();
        var parser = new CommandParser();
        
        //var foyerId = world.Rooms.First(r => r.Value.Name.Contains("Foyer")).Key;
        AreaLoaderService.LoadAreaFile("Areas/emerald_forest.json", world);
        var startingRoom = world.Rooms.Values.First();

        var player = CharacterGenerator.CreateNewPlayer(startingRoom.Id);
        
        // Tracking player in the master state
        startingRoom.Characters.Add(player);
        world.Characters[player.Id] = player;
        
        var timeEngine = new  TimeEngine(world);
        var cts = new CancellationTokenSource();
        Task.Run(() => timeEngine.StartAsync(cts.Token));
        
        Console.WriteLine("=== Welcome to the Sandbox MUD ===");
        
        // Render initial starting room
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