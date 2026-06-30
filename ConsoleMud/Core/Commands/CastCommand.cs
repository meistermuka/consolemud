using ConsoleMud.Core.Skills;
using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class CastCommand : ICommand
{
    private readonly SkillExecutor _executor;

    public CastCommand(SkillExecutor executor) => _executor = executor;

    public string Description => "Cast a spell you have learned, optionally at a target.";
    public string Usage => "cast <spell> [target]";
    public string Example => "cast magic missile";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0)
        {
            ColorConsole.WriteLine("Cast what?");
            return;
        }

        string skillId = args[0];
        var targetArgs = args.Skip(1).ToArray();

        if (!_executor.TryUse(player, skillId, targetArgs, world))
            ColorConsole.WriteLine($"You don't know a spell called '{skillId}'.");
    }
}
