using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core;

public class CombatSystem
{
    private readonly WorldState _world;
    private const string DefaultDiceNotation = "1d3";
    private const string DefaultAttackVerb = "punch";
    
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
        // --- MAIN HAND: one swing per attack-rate (haste/slow modify the count) ---
        var mainWeapon = attacker.MainHandWeapon;
        string mainDice = mainWeapon?.DiceNotation ?? DefaultDiceNotation;

        int swings = attacker.AttackRate;
        for (int i = 0; i < swings && defender.Health > 0; i++)
            ResolveSingleHit(attacker, defender, PickVerb(mainWeapon, DefaultAttackVerb), mainDice, "Main Hand");

        // --- OFF HAND: one follow-up swing when dual-wielding ---
        var offWeapon = attacker.OffHandWeapon;
        if (defender.Health > 0 && offWeapon != null)
            ResolveSingleHit(attacker, defender, PickVerb(offWeapon, "strike"), offWeapon.DiceNotation, "Off Hand");
    }

    private static string PickVerb(Item weapon, string fallback)
    {
        if (weapon?.AttackVerbs == null || weapon.AttackVerbs.Length == 0)
            return fallback;
        return weapon.AttackVerbs[Random.Shared.Next(weapon.AttackVerbs.Length)];
    }

    private void ResolveSingleHit(Character attacker, Character defender, string verb, string dice, string handLabel)
    {
        var outcome = AttackResolver.Resolve(attacker, defender, dice, DamageType.Physical);

        if (!outcome.Hit)
        {
            Console.WriteLine($"\n[{handLabel}] {attacker.Name} {verb}s at {defender.Name} but misses!");
            return;
        }

        defender.Health -= outcome.Damage;
        string critTag = outcome.Crit ? " CRITICAL!" : "";
        Console.WriteLine($"\n[{handLabel}] {attacker.Name} {verb}s {defender.Name} for {outcome.Damage} damage!{critTag} " +
                          $"-> [{defender.Name} HP: {Math.Max(0, defender.Health)}]");

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
            foreach (var gear in npc.Equipment.Values)
                corpse.Contents.Add(gear);
            corpse.Contents.AddRange(npc.Inventory);
            room.Items.Add(corpse);
        }
        
        // Reprint command line prompt gracefully
        Console.Write("\n> ");
    }
}