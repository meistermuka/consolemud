using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class AffectCommand : ICommand
{
    public string Description => "List the active affects on you.";
    public string Usage => "affect";
    public string Example => "affect | aff";

    public void Execute(Player player, string[] args, WorldState world)
    {
        ColorConsole.WriteLine("\n========== You are affected by ==========", ConsoleColor.Cyan);
        if (!player.StatusEffects.Any())
        {
            ColorConsole.WriteLine("  Nothing.");
        }
        else
        {
            foreach (var effect in player.StatusEffects)
            {
                if (effect.IsPermanent)
                    ColorConsole.WriteLine($" {{R(Permanent){{x : {effect.Name}");
                else
                    ColorConsole.WriteLine($" {effect.TicksRemaining} ticks : {effect.Name}");
            }    
        }
        ColorConsole.WriteLine("\n=========================================", ConsoleColor.Cyan);
    }
}