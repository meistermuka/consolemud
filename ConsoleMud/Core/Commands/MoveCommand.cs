using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class MoveCommand : ICommand
{
    private readonly Direction _direction;
    public MoveCommand(Direction direction) => _direction = direction;

    public string Description => $"Travel {_direction.ToString().ToLower()} through that exit if one is open.";
    public string Usage => _direction.ToString().ToLower();
    public string Example => _direction.ToString().ToLower();

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (player.IsRooted)
        {
            ColorConsole.WriteLine("You are rooted in place and cannot move!");
            return;
        }

        var currentRoom = world.Rooms[player.CurrentRoomId];
        if (currentRoom.Exits.TryGetValue(_direction, out var targetRoomId))
        {
            player.Position = Position.Standing;
            player.BreakHidden();
            player.LastExit = _direction;

            world.MoveCharacter(player, targetRoomId);
            PetSystem.FollowOwner(player, world);
            ColorConsole.WriteLine($"You move {_direction.ToString().ToLower()}.");
            new LookCommand().Execute(player, args, world);
        }
        else
            ColorConsole.WriteLine($"You can't go {_direction.ToString().ToLower()}.");
    }
}
