using ConsoleMud.Entities;
using ConsoleMud.Enums;

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
            Console.WriteLine("You can't sit down while fighting!");
            return;
        }

        if (player.Position == Position.Sitting)
        {
            Console.WriteLine("You are already sitting.");
            return;
        }

        player.Position = Position.Sitting;
        Console.WriteLine("You sit down and catch your breath.");
    }
}
