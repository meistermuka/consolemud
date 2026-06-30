using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class HelpCommand : ICommand
{
    private readonly Dictionary<string, ICommand> _commands;
    public HelpCommand(Dictionary<string, ICommand> commands) => _commands = commands;

    public string Description => "Show how to use a command, with an example.";
    public string Usage => "help <command>";
    public string Example => "help wield";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0)
        {
            ColorConsole.WriteLine("Usage: help <command>   (try 'commands' to see them all)");
            return;
        }

        string verb = args[0];
        if (!_commands.TryGetValue(verb, out var command))
        {
            ColorConsole.WriteLine($"There is no help for '{verb}'. Type 'commands' to see what's available.");
            return;
        }

        ColorConsole.WriteLine($"\n=== {verb} ===", ConsoleColor.Cyan);
        ColorConsole.WriteLine(command.Description);
        if (!string.IsNullOrEmpty(command.Usage))   ColorConsole.WriteLine($"Usage:   {command.Usage}");
        if (!string.IsNullOrEmpty(command.Example))  ColorConsole.WriteLine($"Example: {command.Example}");
        ColorConsole.WriteLine();
    }
}
