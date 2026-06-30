using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using ConsoleMud.Core;
using ConsoleMud.Core.Commands;
using ConsoleMud.Core.Services;
using ConsoleMud.Core.Skills;
using ConsoleMud.Entities;
using ConsoleMud.Helpers;
using ConsoleMud.Views;
using TGuiAttr = Terminal.Gui.Drawing.Attribute;

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

        // --- PRE-TUI BOOT (plain console) ---
        // Character creation uses raw Console I/O before Terminal.Gui takes over.
        Console.WriteLine("Booting local world... standby...");
        var world = new WorldState();

        TuningRegistry.Load("Definitions/tuning.json");

        var definitions = new DefinitionRegistry();
        definitions.LoadAll("Definitions");
        LevelingService.Initialize(definitions);
        PassiveService.Initialize(definitions);
        ShapeshiftService.Initialize(definitions);

        var skillHandlers = new SkillHandlerRegistry();
        var skillExecutor  = new SkillExecutor(definitions, skillHandlers);
        var parser         = new CommandParser(skillExecutor, definitions);

        AreaLoaderService.LoadAreaFile("Areas/emerald_forest.json", world);
        var startingRoom = world.Rooms.Values.First();
        world.SafeRoomId = startingRoom.Id;

        var player = SelectCharacter(world, definitions, startingRoom.Id);

        world.Characters[player.Id] = player;
        if (world.Rooms.TryGetValue(player.CurrentRoomId, out var startRoom))
            startRoom.Characters.Add(player);

        // --- TUI SESSION ---
        // Use the modern instance-based Application API.
        var app = Application.Create();
        app.Init();

        // Force the 16-color palette. The default driver emits true-color (24-bit)
        // SGR sequences, which many terminals render as monochrome; our output
        // attributes target ColorName16, so the standard 16-color codes render
        // reliably everywhere.
        if (app.Driver != null)
            app.Driver.Force16Colors = true;

        // Output pane: fills everything except the bottom two rows.
        var outputView = new GameOutputView
        {
            X      = 0,
            Y      = 0,
            Width  = Dim.Fill(),
            Height = Dim.Fill(2)
        };

        // Status bar: one row sitting immediately above the input field.
        var statusBar = new Label
        {
            X      = 0,
            Y      = Pos.AnchorEnd(2),
            Width  = Dim.Fill(),
            Height = 1,
            Text   = " HP: -- / --   Mana: -- / --   [ loading... ]"
        };
        SchemeManager.AddScheme("StatusBar",
            new Scheme(new TGuiAttr(ColorName16.Black, ColorName16.Cyan)));
        statusBar.SchemeName = "StatusBar";

        // Input field: pinned to the very bottom, full width.
        var inputField = new TextField
        {
            X     = 0,
            Y     = Pos.AnchorEnd(1),
            Width = Dim.Fill()
        };

        // Window is the modern replacement for Toplevel and implements IRunnable.
        var window = new Window { Width = Dim.Fill(), Height = Dim.Fill() };
        window.Add(outputView, statusBar, inputField);

        // Wire the output bridge BEFORE any game output can fire.
        GameOutput.Setup(outputView, statusBar, app);

        // --- Command history (UP / DOWN arrow keys) ---
        var history = new List<string>();
        int histIdx = -1;

        inputField.KeyDown += (s, e) =>
        {
            if (e == Key.CursorUp && history.Count > 0)
            {
                histIdx = Math.Min(histIdx + 1, history.Count - 1);
                inputField.Text = history[histIdx];
                e.Handled = true;
            }
            else if (e == Key.CursorDown)
            {
                histIdx = Math.Max(histIdx - 1, -1);
                inputField.Text = histIdx < 0 ? "" : history[histIdx];
                e.Handled = true;
            }
        };

        // --- Enter key: execute command ---
        inputField.Accepting += (s, e) =>
        {
            var input = (inputField.Text?.ToString() ?? "").Trim();
            inputField.Text = "";
            histIdx = -1;

            if (!string.IsNullOrWhiteSpace(input))
                history.Insert(0, input);

            if (input.Equals("quit", StringComparison.OrdinalIgnoreCase)
             || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                SaveService.Save(player, world);
                GameOutput.Print("\nYour progress has been saved. Goodbye!\n", ConsoleColor.Yellow);
                app.RequestStop();
                e.Handled = true;
                return;
            }

            if (!string.IsNullOrWhiteSpace(input))
                parser.ParseAndExecute(input, player, world);

            GameOutput.UpdateStatus(player, world);
            e.Handled = true;
        };

        // Start the background tick engine.
        // All its output goes via GameOutput.Print → app.Invoke(), thread-safe.
        var cts        = new CancellationTokenSource();
        var timeEngine = new TimeEngine(world);
        Task.Run(() => timeEngine.StartAsync(cts.Token));

        // Render the initial room and set the status bar.
        new LookCommand().Execute(player, Array.Empty<string>(), world);
        GameOutput.UpdateStatus(player, world);

        inputField.SetFocus();

        app.Run(window);
        app.Dispose();
        cts.Cancel();
    }

    // Runs BEFORE app.Init() so plain Console.ReadLine() is safe here.
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
