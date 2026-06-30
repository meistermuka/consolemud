using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class RestCommand : ICommand
{
    public string Description => "Lie down to rest, recovering health and mana fastest.";
    public string Usage => "rest";
    public string Example => "rest";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (player.CombatTarget != null)
        {
            ColorConsole.WriteLine("You can't rest while fighting!");
            return;
        }

        if (player.Position == Position.Resting)
        {
            ColorConsole.WriteLine("You are already resting.");
            return;
        }

        player.Position = Position.Resting;
        ColorConsole.WriteLine("You lie back and rest.");
    }
}
