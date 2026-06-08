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
        npc.CombatTarget = player;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n⚔️ COMBAT ENGAGED: {player.Name} vs {npc.Name} ⚔️");
        Console.ResetColor();
    }
    
    private void ExecuteAttack(Character attacker, Character defender)
    {
        string verb;
        string dice;

        // Dynamic scaling based on equipped items
        if (attacker.EquippedWeapon == null)
        {
            verb = "punch";
            dice = "1d3"; // Base unarmed damage
        }
        else
        {
            var weapon = attacker.EquippedWeapon;
            dice = weapon.DiceNotation;
            // Pick a dynamic verb assigned to the item structure
            verb = weapon.AttackVerbs[_random.Next(weapon.AttackVerbs.Length)];
        }

        int damage = DiceRoller.Roll(dice);
        defender.Health -= damage;

        // Formatting text output dynamically
        Console.WriteLine($"{attacker.Name} {verb}s {defender.Name} for {damage} damage! ({dice}) -> [{defender.Name} HP: {Math.Max(0, defender.Health)}/{defender.MaxHealth}]");
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
            corpse.Contents.Add(lootItem);
        
        // For testing, let's inject a piece of gold into every corpse
        corpse.Contents.Add(new Item { Name = "gold coin", Description = "A shiny gold coin.", IsGetable = true });

        room.Items.Add(corpse);
        Console.WriteLine($"A {corpse.Name} is lying on the ground.");
    }
}