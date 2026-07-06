using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class RemoveCommand : ICommand
{
    public string Description => "Wear a piece of armor or equipment, or 'wear all'.";
    public string Usage => "wear <item|all>";
    public string Example => "wear all";

    public void Execute(Player player, string[] args, WorldState world)
    {}
}