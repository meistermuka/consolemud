using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class KillCommand : ICommand
{
    private static readonly Random _random = new();

    public string Description => "Attack a target and engage it in combat.";
    public string Usage => "kill <target>";
    public string Example => "kill wolf";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Kill what?");
            return;
        }
        
        string rawInput = string.Join(" ", args);
        var (targetIndex, cleanKeyword) = KeywordParser.ExtractIndex(rawInput);
        var room = world.Rooms[player.CurrentRoomId];

        if (!player.CanSee(room))
        {
            Console.WriteLine("It's too dark to make out anything to attack.");
            return;
        }

        // Locate the target NPC
        NonPlayerCharacter targetNpc = null;
        int matchCount = 0;
        foreach (var npc in room.Characters.OfType<NonPlayerCharacter>())
        {
            if (npc.MatchesKeyword(cleanKeyword))
            {
                matchCount++;
                if (matchCount == targetIndex)
                {
                    targetNpc = npc;
                    break;
                }
            }
        }

        if (targetNpc == null)
        {
            Console.WriteLine($"There is no '{rawInput}' here to attack.");
            return;
        }

        if (player.CombatTarget == targetNpc)
        {
            Console.WriteLine($"You are already engaged in combat with {targetNpc.Name}.");
            return;
        }
         
        // Attacking breaks stealth.
        player.BreakHidden();

        // Establish mutual engagement
        player.CombatTarget = targetNpc;
        // If the NPC isn't fighting anyone yet, have them retaliate against the player
        if (targetNpc.CombatTarget == null)
        {
            targetNpc.CombatTarget = player;
        }

        Helpers.ColorConsole.WriteLine($"\nCOMBAT ENGAGED: {player.Name} vs {targetNpc.Name}", ConsoleColor.Red);
    }
}