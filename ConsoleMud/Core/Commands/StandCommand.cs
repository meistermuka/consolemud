using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class StandCommand : ICommand
{
    public string Description => "Get back on your feet.";
    public string Usage => "stand";
    public string Example => "stand";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (player.Position == Position.Standing)
        {
            ColorConsole.WriteLine("You are already standing.");
            return;
        }

        player.Position = Position.Standing;
        ColorConsole.WriteLine("You get to your feet.");
    }
}
