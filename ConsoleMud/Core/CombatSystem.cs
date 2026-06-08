using ConsoleMud.Entities;

namespace ConsoleMud.Core;

public class CombatSystem
{
    private readonly WorldState _world;
    
    public CombatSystem(WorldState world) => _world = world;

    public void Tick()
    {
        // Loop through all characters globally who are currently targeting someone
        foreach (var attacker in _world.Characters.Values.ToList())
        {
            var defender = attacker.CombatTarget;

            // Validation: Ensure target exists, is alive, and is in the same room
            if (defender == null || defender.Health <= 0 || attacker.CurrentRoomId != defender.CurrentRoomId)
            {
                attacker.CombatTarget = null; // Clear broken combat links
                continue;
            }

            // Execute a singular attack round for this character
            ExecuteAttack(attacker, defender);
        }
    }
    
    private void ExecuteAttack(Character attacker, Character defender)
    {
        string verb = attacker.EquippedWeapon?.AttackVerbs[Random.Shared.Next(attacker.EquippedWeapon.AttackVerbs.Length)] ?? "punch";
        string dice = attacker.EquippedWeapon?.DiceNotation ?? "1d3";

        int rawDamage = DiceRoller.Roll(dice);
        int armorMitigation = defender.EquippedArmour?.ArmourRating ?? 0;
        
        // Damage Reduction Math (Minimum of 1 damage ensures fights resolve)
        int finalDamage = Math.Max(1, rawDamage - armorMitigation);
        defender.Health -= finalDamage;

        // Print the output to the console
        Console.WriteLine($"\n⚔️ {attacker.Name} {verb}s {defender.Name} for {finalDamage} damage! " +
                          $"({dice} rolled {rawDamage}, armor reduced -{armorMitigation}) -> [{defender.Name} HP: {Math.Max(0, defender.Health)}]");

        if (defender.Health <= 0)
            HandleDeath(defender);
    }
    
    private void HandleDeath(Character deadCharacter)
    {
        // Break combat engagements immediately
        deadCharacter.CombatTarget = null;

        if (deadCharacter is Player)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("\n*** YOU HAVE BEEN SLAIN! ***\nGame Over.");
            Console.ResetColor();
            Environment.Exit(0);
        }
        else if (deadCharacter is NonPlayerCharacter npc)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n🎉 The {npc.Name} collapses to the ground, dead!");
            Console.ResetColor();

            var room = _world.Rooms[npc.CurrentRoomId];
            room.Characters.Remove(npc);
            _world.Characters.Remove(npc.Id);

            // Cleanly break targeting loop for anyone else targeting this dead NPC
            foreach (var ch in _world.Characters.Values.Where(c => c.CombatTarget == npc))
                ch.CombatTarget = null;

            // Spawn dynamic container corpse populated with their gear
            var corpse = new Item { Name = $"corpse of a {npc.Name}", IsContainer = true, Description = $"The cold remains of a {npc.Name}." };
            if (npc.EquippedWeapon != null) corpse.Contents.Add(npc.EquippedWeapon);
            room.Items.Add(corpse);
        }
        
        // Reprint command line prompt gracefully
        Console.Write("\n> ");
    }
}