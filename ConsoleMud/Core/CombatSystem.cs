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
            ResolveSingleHit(attacker, defender, mainWeapon.Name, PickVerb(mainWeapon, DefaultAttackVerb), mainDice, "Main Hand");

        // --- OFF HAND: one follow-up swing when dual-wielding ---
        var offWeapon = attacker.OffHandWeapon;
        if (defender.Health > 0 && offWeapon != null)
            ResolveSingleHit(attacker, defender, mainWeapon.Name, PickVerb(offWeapon, "strike"), offWeapon.DiceNotation, "Off Hand");
    }

    private static string PickVerb(Item weapon, string fallback)
    {
        if (weapon?.AttackVerbs == null || weapon.AttackVerbs.Length == 0)
            return fallback;
        return weapon.AttackVerbs[Random.Shared.Next(weapon.AttackVerbs.Length)];
    }

    private void ResolveSingleHit(Character attacker, Character defender, string weaponName, string verb, string dice, string handLabel)
    {
        var outcome = AttackResolver.Resolve(attacker, defender, dice, DamageType.Physical);

        if (!outcome.Hit)
        {
            Helpers.ColorConsole.WriteLine($"\n{weaponName} {verb}s at {defender.Name} but misses!", ConsoleColor.Gray);
            return;
        }

        defender.Health -= outcome.Damage;
        string critTag = outcome.Crit ? " CRITICAL!" : "";
        Helpers.ColorConsole.WriteLine($"\n{weaponName} {verb}s {defender.Name} for {outcome.Damage} damage!{critTag} " +
                          $"-> [{defender.Name} HP: {Math.Max(0, defender.Health)}]", ConsoleColor.Gray);

        if (defender.Health <= 0)
            DeathService.HandleDeath(defender, _world);
    }
}