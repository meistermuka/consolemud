using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class KillCommand : ICommand
{
    private static readonly Random _random = new();

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Kill what?");
            return;
        }

        string targetName = string.Join(" ", args).ToLower();
        var room = world.Rooms[player.CurrentRoomId];

        // Locate the target NPC
        var npc = room.Characters
            .OfType<NonPlayerCharacter>()
            .FirstOrDefault(c => c.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));

        if (npc == null)
        {
            Console.WriteLine($"There is no '{targetName}' here to attack.");
            return;
        }
        
        // Establish mutual engagement
        player.CombatTarget = npc;
        // If the NPC isn't fighting anyone yet, have them retaliate against the player
        if (npc.CombatTarget == null)
        {
            npc.CombatTarget = player;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n⚔️ COMBAT ENGAGED: {player.Name} vs {npc.Name} ⚔️");
        Console.ResetColor();
    }
}