using ConsoleMud.Core.Skills;
using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class ShapeshiftCommand : ICommand
{
    private readonly SkillExecutor _executor;

    public ShapeshiftCommand(SkillExecutor executor) => _executor = executor;

    public string Description => "Shapeshift into a beast form, or back to human.";
    public string Usage => "shapeshift <bear|wolf|owl|dragon|human>";
    public string Example => "shapeshift bear";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0)
        {
            ColorConsole.WriteLine($"You are currently in {player.Form} form. Usage: {Usage}");
            return;
        }

        string what = args[0].ToLower();
        if (what is "human" or "normal")
        {
            ShapeshiftService.Revert(player);
            return;
        }

        if (!Enum.TryParse<Form>(what, true, out var form) || form == Form.Human)
        {
            ColorConsole.WriteLine("Shapeshift into what? (bear, wolf, owl, dragon, human)");
            return;
        }

        if (!_executor.TryUse(player, "shapeshift_" + what, Array.Empty<string>(), world))
            ColorConsole.WriteLine($"You don't know how to take {what} form.");
    }
}
