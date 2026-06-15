using ConsoleMud.Entities;
using ConsoleMud.Enums;

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
            Console.WriteLine("You are rooted in place and cannot flee!");
            return;
        }

        var room = world.Rooms[player.CurrentRoomId];
        if (room.Exits.Count == 0)
        {
            Console.WriteLine("There is nowhere to flee!");
            return;
        }

        // Pick a random available exit.
        var exits = room.Exits.ToList();
        var chosen = exits[Random.Shared.Next(exits.Count)];

        // Break combat both ways before bolting.
        player.CombatTarget = null;
        foreach (var c in world.Characters.Values.Where(c => c.CombatTarget == player))
            c.CombatTarget = null;

        player.Position = Position.Standing;
        player.BreakHidden();

        world.MoveCharacter(player, chosen.Value);
        Console.WriteLine($"\nYou flee {chosen.Key.ToString().ToLower()}!");
        new LookCommand().Execute(player, Array.Empty<string>(), world);
    }
}
