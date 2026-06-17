// See https://aka.ms/new-console-template for more information

using ConsoleMud.Core;
using ConsoleMud.Core.Commands;
using ConsoleMud.Core.Services;
using ConsoleMud.Core.Skills;
using ConsoleMud.Entities;
using ConsoleMud.Helpers;

class Program
{
    static void Main(string[] args)
    {
        // Offline tool: build an area file interactively, then exit.
        if (args.Length > 0 && args[0].Equals("build-area", StringComparison.OrdinalIgnoreCase))
        {
            AreaBuilder.Run();
            return;
        }

        Console.WriteLine("Booting local world...standby...");
        var world = new WorldState();

        TuningRegistry.Load("Definitions/tuning.json");

        var definitions = new DefinitionRegistry();
        definitions.LoadAll("Definitions");
        LevelingService.Initialize(definitions);
        PassiveService.Initialize(definitions);
        ShapeshiftService.Initialize(definitions);

        var skillHandlers = new SkillHandlerRegistry();
        var skillExecutor = new SkillExecutor(definitions, skillHandlers);
        var parser = new CommandParser(skillExecutor, definitions);

        AreaLoaderService.LoadAreaFile("Areas/emerald_forest.json", world);
        var startingRoom = world.Rooms.Values.First();
        world.SafeRoomId = startingRoom.Id;

        var player = SelectCharacter(world, definitions, startingRoom.Id);

        // Track the player in the master state and drop them into their room.
        world.Characters[player.Id] = player;
        if (world.Rooms.TryGetValue(player.CurrentRoomId, out var room))
            room.Characters.Add(player);

        var timeEngine = new TimeEngine(world);
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
                SaveService.Save(player, world);
                Console.WriteLine("Your progress has been saved. Goodbye!");
                break;
            }

            parser.ParseAndExecute(input, player, world);
        }
    }

    // Startup menu: load an existing character or create a new one.
    private static Player SelectCharacter(WorldState world, DefinitionRegistry definitions, Guid startingRoomId)
    {
        Console.Write("Do you want to (l)oad an existing character or create a (n)ew one? [l/n]: ");
        string choice = (Console.ReadLine() ?? "n").Trim().ToLower();

        if (choice is "l" or "load")
        {
            Console.Write("Character name: ");
            string name = (Console.ReadLine() ?? "").Trim();
            if (SaveService.TryLoad(name, world, out var loaded))
            {
                Console.WriteLine($"Welcome back, {loaded.Name}.");
                return loaded;
            }
            Console.WriteLine($"No saved character named '{name}'. Let's make a new one.");
        }

        return CharacterGenerator.CreateNewPlayer(startingRoomId, definitions);
    }
}