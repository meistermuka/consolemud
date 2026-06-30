using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class SitCommand : ICommand
{
    public string Description => "Sit down to recover health and mana a little faster.";
    public string Usage => "sit";
    public string Example => "sit";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (player.CombatTarget != null)
        {
            ColorConsole.WriteLine("You can't sit down while fighting!");
            return;
        }

        if (player.Position == Position.Sitting)
        {
            ColorConsole.WriteLine("You are already sitting.");
            return;
        }

        player.Position = Position.Sitting;
        ColorConsole.WriteLine("You sit down and catch your breath.");
    }
}
