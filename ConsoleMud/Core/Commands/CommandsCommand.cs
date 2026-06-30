using ConsoleMud.Entities;
using ConsoleMud.Helpers;

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
        ColorConsole.WriteLine("\n=== Available Commands ===", ConsoleColor.Cyan);

        var verbs = _commands
            .Where(kvp => kvp.Key.Length >= CanonicalName(kvp.Value).Length)
            .Select(kvp => kvp.Key)
            .OrderBy(k => k)
            .ToList();

        const int perRow = 6;
        for (int i = 0; i < verbs.Count; i += perRow)
        {
            var row = verbs.Skip(i).Take(perRow).Select(v => v.PadRight(12));
            ColorConsole.WriteLine("  " + string.Join("", row));
        }

        ColorConsole.WriteLine("\nType 'help <command>' for details on any one.\n");
    }

    private static string CanonicalName(ICommand command) =>
        string.IsNullOrWhiteSpace(command.Usage)
            ? ""
            : command.Usage.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
}
