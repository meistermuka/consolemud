using ConsoleMud.Core.Commands;
using ConsoleMud.Core.Services;
using ConsoleMud.Core.Skills;
using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core;

public class CommandParser
{
    private readonly Dictionary<string, ICommand> _commands = new();
    private readonly SkillExecutor _skillExecutor;

    public CommandParser(SkillExecutor skillExecutor, DefinitionRegistry definitions)
    {
        _skillExecutor = skillExecutor;
        _commands["look"] = new LookCommand();
        _commands["l"] = new LookCommand();
        
        _commands["north"] = new MoveCommand(Direction.North);
        _commands["n"] = new MoveCommand(Direction.North);
        _commands["south"] = new MoveCommand(Direction.South);
        _commands["s"] = new MoveCommand(Direction.South);
        _commands["east"] = new MoveCommand(Direction.East);
        _commands["e"] = new MoveCommand(Direction.East);
        _commands["west"] = new MoveCommand(Direction.West);
        _commands["w"] = new MoveCommand(Direction.West);
        _commands["up"] = new MoveCommand(Direction.Up);
        _commands["u"] = new MoveCommand(Direction.Up);
        _commands["down"] = new MoveCommand(Direction.Down);
        _commands["d"] = new MoveCommand(Direction.Down);
        
        _commands["get"] = new GetCommand();
        _commands["take"] = new GetCommand();
        _commands["inventory"] = new InventoryCommand();
        _commands["inv"] = new InventoryCommand();
        _commands["i"] = new InventoryCommand();
        _commands["drop"] = new DropCommand();
        _commands["give"] = new GiveCommand();
        _commands["put"] = new PutCommand();
        _commands["open"] = new OpenCommand();
        _commands["close"] = new CloseCommand();
        _commands["lock"] = new LockCommand();
        _commands["unlock"] = new UnlockCommand();
        
        _commands["kill"] = new KillCommand();
        _commands["k"] = new KillCommand();
        _commands["attack"] = new KillCommand();
        
        _commands["wield"] = new WieldCommand();
        _commands["wear"] = new WearCommand();
        _commands["second"] = new SecondCommand();
        _commands["sec"] = new SecondCommand();
        _commands["status"] = new StatusCommand();
        _commands["st"] = new StatusCommand();
        _commands["score"] = new StatusCommand();
        _commands["sc"] = new StatusCommand();
        _commands["flee"] = new FleeCommand();
        _commands["sit"] = new SitCommand();
        _commands["rest"] = new RestCommand();
        _commands["stand"] = new StandCommand();
        _commands["save"] = new SaveCommand();
        _commands["skills"] = new SkillsCommand(definitions);
        _commands["skill"] = new SkillsCommand(definitions);
        _commands["specialize"] = new SpecializeCommand();
        _commands["weather"] = new WeatherCommand();
        _commands["shapeshift"] = new ShapeshiftCommand(_skillExecutor);
        _commands["shift"] = new ShapeshiftCommand(_skillExecutor);
        _commands["breath"] = new BreathCommand();
        _commands["equipment"] = new EquipmentCommand();
        _commands["equip"] = new EquipmentCommand();
        _commands["eq"] = new EquipmentCommand();
        _commands["remove"] = new RemoveCommand();
        _commands["rem"] = new RemoveCommand();

        _commands["cast"] = new CastCommand(_skillExecutor);

        _commands["help"] = new HelpCommand(_commands);
        _commands["commands"] = new CommandsCommand(_commands);
        _commands["cmds"] = new CommandsCommand(_commands);
    }

    public void ParseAndExecute(string input, Player player, WorldState world)
    {
        if (string.IsNullOrWhiteSpace(input)) return;
        // Tokenize text input (lowercase, split by space)
        var parts = input.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var verb = parts[0];
        var args = parts.Skip(1).ToArray();

        // Record activity for the idle-based stealth roll.
        player.LastActionUtc = DateTime.UtcNow;

        if (_commands.TryGetValue(verb, out var command))
        {
            command.Execute(player, args, world);
            return;
        }

        // Not a built-in verb: try it as a learned active skill (e.g. "kick rat").
        if (_skillExecutor.TryUse(player, verb, args, world))
            return;

        ColorConsole.WriteLine($"Unknown command: {verb}");
    }
}