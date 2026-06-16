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
        Console.WriteLine();
        ColorConsole.WriteLine($"[{room.Name}]", ConsoleColor.Cyan);
        ColorConsole.WriteLine(room.Description, ConsoleColor.Gray);

        // Show exits
        var exits = string.Join(", ", room.Exits.Select(e => e.Key.ToString().ToLower()));
        Console.WriteLine($"Exits: {(string.IsNullOrEmpty(exits) ? "none" : exits)}");

        // Show items
        foreach (var item in room.Items)
            ColorConsole.WriteLine($"You see a {item.Name} here.", ConsoleColor.Gray);

        // Show NPCs and other characters, skipping anyone hidden.
        foreach (var character in room.Characters.Where(c => c != player && !c.IsHidden))
            ColorConsole.WriteLine($"{character.Name} here.", ConsoleColor.Gray);

        // Eco-location: a druid outdoors senses creatures up to two rooms away.
        if (room.IsOutside && player.KnownSkills.ContainsKey("eco_location"))
        {
            var nearby = Services.PerceptionService.ScanNearby(world, room, 2);
            foreach (var line in nearby)
                ColorConsole.WriteLine($"  (sense) {line}", ConsoleColor.DarkGray);
        }

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
            ColorConsole.WriteLine(target.Description, ConsoleColor.Gray);
            return;
        }

        // Describe the container's contents
        Console.WriteLine();
        ColorConsole.WriteLine($"[{target.Name}]", ConsoleColor.Cyan);

        if (target.Contents.Count == 0)
        {
            Console.WriteLine("It is empty.");
        }
        else
        {
            Console.WriteLine("It contains:");
            foreach (var item in target.Contents)
                ColorConsole.WriteLine($"  {item.Name}", ConsoleColor.Gray);
        }

        Console.WriteLine();
    }
}
