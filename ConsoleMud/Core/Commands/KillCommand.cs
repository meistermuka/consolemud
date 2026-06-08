using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class KillCommand : ICommand
{
    private static readonly Random _dice = new();

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

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n--- Combat Round vs {npc.Name.ToUpper()} ---");
        Console.ResetColor();

        // 1. Player attacks NPC
        int playerDamage = _dice.Next(3, 8); // Rolls 3 to 7 damage
        npc.Health -= playerDamage;
        Console.WriteLine($"You slash the {npc.Name} for {playerDamage} damage!");

        // Check if NPC died
        if (npc.Health <= 0)
        {
            HandleNpcDeath(npc, room, world);
            return;
        }

        // 2. NPC counter-attacks player
        int npcDamage = _dice.Next(1, 5); // Rolls 1 to 4 damage
        player.Health -= npcDamage;
        Console.WriteLine($"The {npc.Name} bites you back for {npcDamage} damage! (Your Health: {player.Health}/{player.MaxHealth})");

        // Check if Player died
        if (player.Health <= 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("\n*** YOU HAVE DIED ***\nGame Over.");
            Console.ResetColor();
            Environment.Exit(0);
        }
        
        Console.WriteLine("-----------------------------\n");
    }

    private void HandleNpcDeath(NonPlayerCharacter npc, Room room, WorldState world)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"The {npc.Name} lets out a final screech and collapses dead!");
        Console.ResetColor();

        // Evict NPC from active entity tracking arrays
        room.Characters.Remove(npc);
        world.Characters.Remove(npc.Id);

        // Turn the fallen NPC into a dynamic room container
        var corpse = new Item
        {
            Name = $"corpse of a {npc.Name}",
            Description = $"The cold, lifeless remains of a {npc.Name}.",
            IsGetable = false,
            IsContainer = true
        };

        // Transfer whatever the NPC was carrying directly into their corpse
        foreach (var lootItem in npc.Inventory)
        {
            corpse.Contents.Add(lootItem);
        }
        
        // For testing, let's inject a piece of gold into every corpse
        corpse.Contents.Add(new Item { Name = "gold coin", Description = "A shiny gold coin.", IsGetable = true });

        room.Items.Add(corpse);
        Console.WriteLine($"A {corpse.Name} is lying on the ground.");
    }
}