using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public interface ICommand
{
    void Execute(Player player, string[] args, WorldState world);

    // Help metadata. Defaults let commands opt in without breaking anything.
    string Description => "No description available.";
    string Usage => "";
    string Example => "";
}
