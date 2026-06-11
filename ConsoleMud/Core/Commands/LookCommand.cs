using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class LookCommand : ICommand
{
    public string Description => "Look at your surroundings, an item, or inside a container.";
    public string Usage => "look [in] [target]";
    public string Example => "look in chest";

    public void Execute(Player player, string[] args, WorldState world)
    {
        // "look" with no target -> describe the current room
        if (args.Length == 0)
        {
            LookAtRoom(player, world);
            return;
        }

        // Allow both "look chest" and "look in chest"
        var targetArgs = args;
        if (args[0].Equals("in", StringComparison.OrdinalIgnoreCase))
            targetArgs = args.Skip(1).ToArray();

        if (targetArgs.Length == 0)
        {
            Console.WriteLine("Look in what?");
            return;
        }

        string rawInput = string.Join(" ", targetArgs);
        LookAtTarget(player, world, rawInput);
    }

    private void LookAtRoom(Player player, WorldState world)
    {
        var room = world.Rooms[player.CurrentRoomId];
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n[{room.Name}]");
        Console.ResetColor();
        Console.WriteLine(room.Description);

        // Show exits
        var exits = string.Join(", ", room.Exits.Select(e => e.Key.ToString().ToLower()));
        Console.WriteLine($"Exits: {(string.IsNullOrEmpty(exits) ? "none" : exits)}");

        // Show items
        foreach (var item in room.Items)
            Console.WriteLine($"You see a {item.Name} here.");

        // Show NPCs
        foreach (var character in room.Characters.Where(c => c != player))
            Console.WriteLine($"{character.Name} here.");

        Console.WriteLine();
    }

    private void LookAtTarget(Player player, WorldState world, string rawInput)
    {
        var (targetIndex, cleanKeyword) = KeywordParser.ExtractIndex(rawInput);
        var room = world.Rooms[player.CurrentRoomId];

        // Search the room floor first, then the player's own inventory
        var candidates = room.Items.Concat(player.Inventory);

        Item target = null;
        int matchCount = 0;
        foreach (var item in candidates)
        {
            if (item.MatchesKeyword(cleanKeyword))
            {
                matchCount++;
                if (matchCount == targetIndex)
                {
                    target = item;
                    break;
                }
            }
        }

        if (target == null)
        {
            Console.WriteLine($"You don't see a '{rawInput}' here.");
            return;
        }

        if (!target.IsContainer)
        {
            // Not a container: just describe it
            Console.WriteLine(target.Description);
            return;
        }

        // Describe the container's contents
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n[{target.Name}]");
        Console.ResetColor();

        if (target.Contents.Count == 0)
        {
            Console.WriteLine("It is empty.");
        }
        else
        {
            Console.WriteLine("It contains:");
            foreach (var item in target.Contents)
                Console.WriteLine($"  {item.Name}");
        }

        Console.WriteLine();
    }
}
