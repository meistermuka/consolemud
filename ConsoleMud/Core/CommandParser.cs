using ConsoleMud.Core.Commands;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core;

public class CommandParser
{
    private readonly Dictionary<string, ICommand> _commands = new();

    public CommandParser()
    {
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
        
        _commands["get"] = new GetCommand();
        _commands["take"] = new GetCommand();
        _commands["inventory"] = new InventoryCommand();
        _commands["inv"] = new InventoryCommand();
        _commands["i"] = new InventoryCommand();
        _commands["drop"] = new DropCommand();
        _commands["d"] = new DropCommand();
        _commands["put"] = new PutCommand();
        
        _commands["kill"] = new KillCommand();
        _commands["k"] = new KillCommand();
        _commands["attack"] = new KillCommand();
        
        _commands["wield"] = new WieldCommand();
        _commands["wear"] = new WearCommand();
        _commands["second"] = new SecondCommand();
        _commands["sec"] = new SecondCommand();
        _commands["bash"] = new BashCommand();
        _commands["status"] = new StatusCommand();
        _commands["equipment"] = new EquipmentCommand();
        _commands["equip"] = new EquipmentCommand();
        _commands["eq"] = new EquipmentCommand();

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
        
        if (_commands.TryGetValue(verb, out var command))
            command.Execute(player, args, world);
        else
            Console.WriteLine($"Unknown command: {verb}");
    }
}