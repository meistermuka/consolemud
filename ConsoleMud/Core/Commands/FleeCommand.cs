using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class FleeCommand : ICommand
{
    public string Description => "Flee through a random exit, breaking off combat.";
    public string Usage => "flee";
    public string Example => "flee";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (player.IsRooted)
        {
            ColorConsole.WriteLine("You are rooted in place and cannot flee!");
            return;
        }

        var room = world.Rooms[player.CurrentRoomId];
        if (room.Exits.Count == 0)
        {
            ColorConsole.WriteLine("There is nowhere to flee!");
            return;
        }

        var exits = room.Exits.ToList();
        var chosen = exits[Random.Shared.Next(exits.Count)];

        player.CombatTarget = null;
        foreach (var c in world.Characters.Values.Where(c => c.CombatTarget == player))
            c.CombatTarget = null;

        player.Position = Position.Standing;
        player.BreakHidden();
        player.LastExit = chosen.Key;

        world.MoveCharacter(player, chosen.Value);
        PetSystem.FollowOwner(player, world);
        ColorConsole.WriteLine($"\nYou flee {chosen.Key.ToString().ToLower()}!");
        new LookCommand().Execute(player, Array.Empty<string>(), world);
    }
}
