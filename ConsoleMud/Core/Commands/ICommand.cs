using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public interface ICommand
{
    void Execute(Player player, string[] args, WorldState world);
}