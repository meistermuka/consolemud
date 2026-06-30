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
        if (args.Length == 0)
        {
            LookAtRoom(player, world);
            return;
        }

        var targetArgs = args;
        if (args[0].Equals("in", StringComparison.OrdinalIgnoreCase))
            targetArgs = args.Skip(1).ToArray();

        if (targetArgs.Length == 0)
        {
            ColorConsole.WriteLine("Look in what?");
            return;
        }

        string rawInput = string.Join(" ", targetArgs);
        LookAtTarget(player, world, rawInput);
    }

    private void LookAtRoom(Player player, WorldState world)
    {
        var room = world.Rooms[player.CurrentRoomId];

        if (!player.CanSee(room))
        {
            ColorConsole.WriteLine("\nIt is pitch black. You can't see a thing.", ConsoleColor.DarkGray);
            return;
        }

        ColorConsole.WriteLine();
        ColorConsole.WriteLine($"[{room.Name}]", ConsoleColor.Cyan);
        ColorConsole.WriteLine(room.Description, ConsoleColor.Gray);

        var exits = string.Join(", ", room.Exits.Select(e => e.Key.ToString().ToLower()));
        ColorConsole.WriteLine($"Exits: {(string.IsNullOrEmpty(exits) ? "none" : exits)}");

        foreach (var item in room.Items)
            ColorConsole.WriteLine($"You see a {item.Name} here.", ConsoleColor.Gray);

        bool canPeek = player.KnownSkills.ContainsKey("peek");
        foreach (var character in room.Characters.Where(c => c != player && !c.IsHidden))
        {
            ColorConsole.WriteLine($"{character.Name} here.", ConsoleColor.Gray);

            if (canPeek && character is Entities.NonPlayerCharacter npc)
            {
                var carried = npc.Inventory.Concat(npc.Equipment.Values).Select(i => i.Name).ToList();
                if (carried.Count > 0)
                    ColorConsole.WriteLine($"    (peek) carrying: {string.Join(", ", carried)}", ConsoleColor.DarkGray);
            }
        }

        if (room.IsOutside && player.KnownSkills.ContainsKey("eco_location"))
        {
            var nearby = Services.PerceptionService.ScanNearby(world, room, 2);
            foreach (var line in nearby)
                ColorConsole.WriteLine($"  (sense) {line}", ConsoleColor.DarkGray);
        }

        ColorConsole.WriteLine();
    }

    private void LookAtTarget(Player player, WorldState world, string rawInput)
    {
        var (targetIndex, cleanKeyword) = KeywordParser.ExtractIndex(rawInput);
        var room = world.Rooms[player.CurrentRoomId];

        if (!player.CanSee(room))
        {
            ColorConsole.WriteLine("It is too dark to make anything out.", ConsoleColor.DarkGray);
            return;
        }

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
            ColorConsole.WriteLine($"You don't see a '{rawInput}' here.");
            return;
        }

        if (!target.IsContainer)
        {
            ColorConsole.WriteLine(target.Description, ConsoleColor.Gray);
            return;
        }

        if (!target.IsOpen)
        {
            ColorConsole.WriteLine($"The {target.Name} is closed.", ConsoleColor.Gray);
            return;
        }

        ColorConsole.WriteLine();
        ColorConsole.WriteLine($"[{target.Name}]", ConsoleColor.Cyan);

        if (target.Contents.Count == 0)
        {
            ColorConsole.WriteLine("It is empty.");
        }
        else
        {
            ColorConsole.WriteLine("It contains:");
            foreach (var item in target.Contents)
                ColorConsole.WriteLine($"  {item.Name}", ConsoleColor.Gray);
        }

        ColorConsole.WriteLine();
    }
}
