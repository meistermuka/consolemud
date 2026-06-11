using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class CommandsCommand : ICommand
{
    private readonly Dictionary<string, ICommand> _commands;
    public CommandsCommand(Dictionary<string, ICommand> commands) => _commands = commands;

    public string Description => "List every command you can use.";
    public string Usage => "commands";
    public string Example => "commands";

    public void Execute(Player player, string[] args, WorldState world)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n=== Available Commands ===");
        Console.ResetColor();

        var verbs = _commands.Keys.OrderBy(k => k).ToList();

        // Print in tidy columns, 6 per row
        const int perRow = 6;
        for (int i = 0; i < verbs.Count; i += perRow)
        {
            var row = verbs.Skip(i).Take(perRow).Select(v => v.PadRight(12));
            Console.WriteLine("  " + string.Join("", row));
        }

        Console.WriteLine("\nType 'help <command>' for details on any one.\n");
    }
}
