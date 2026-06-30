using ConsoleMud.Core.Services;
using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class SaveCommand : ICommand
{
    public string Description => "Save your character to disk.";
    public string Usage => "save";
    public string Example => "save";

    public void Execute(Player player, string[] args, WorldState world)
    {
        SaveService.Save(player, world);
        ColorConsole.WriteLine("Your progress has been saved.");
    }
}
