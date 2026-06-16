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
        // Age crowd-control effects on the combat pulse (1s), so durations are
        // measured in combat rounds rather than the slower status pulse.
        foreach (var character in _world.Characters.Values)
            AgeControlEffects(character);

        // Companions pick up their owner's target and refresh their link.
        PetSystem.UpdatePets(_world);

        // Loop through all characters globally who are currently targeting someone
        foreach (var attacker in _world.Characters.Values.ToList())
        {
            var defender = attacker.CombatTarget;

            // Validation: Ensure target exists, is alive, and is in the same room
            if (defender == null || defender.Health <= 0 || attacker.CurrentRoomId != defender.CurrentRoomId)
            {
                attacker.CombatTarget = null; // Clear broken combat links
                attacker.EncounterFlags.Clear(); // combat over: reset once-per-fight passives
                continue;
            }

            // A stunned combatant loses its round.
            if (attacker.IsStunned)
            {
                Helpers.ColorConsole.WriteLine($"\n{attacker.Name} is stunned and cannot act!", ConsoleColor.DarkGray);
                continue;
            }

            // Execute a singular attack round for this character
            ExecuteAttack(attacker, defender);

            // Fighter "second wind": one emergency heal per fight under 25% health.
            TrySecondWind(attacker);
        }
    }

    // Mirrors the second_wind skill definition (threshold 25%, heal 30).
    private const double SecondWindThreshold = 0.25;
    private const int SecondWindHeal = 30;

    private static void TrySecondWind(Character c)
    {
        if (!c.KnownSkills.ContainsKey("second_wind"))
            return;
        if (c.Health <= 0 || c.Health > c.MaxHealth * SecondWindThreshold)
            return;
        if (!c.EncounterFlags.Add("second_wind")) // already triggered this fight
            return;

        c.Health = Math.Min(c.MaxHealth, c.Health + SecondWindHeal);
        Helpers.ColorConsole.WriteLine(
            $"\n{c.Name} catches a second wind and recovers {SecondWindHeal} health!", ConsoleColor.Green);
    }

    private static void AgeControlEffects(Character character)
    {
        for (int i = character.StatusEffects.Count - 1; i >= 0; i--)
        {
            var effect = character.StatusEffects[i];
            if (effect.Modifier is not (EffectModifier.Stun or EffectModifier.Root or EffectModifier.Blind))
                continue;
            if (effect.IsPermanent)
                continue;

            effect.TicksRemaining--;
            if (effect.IsExpired)
                character.StatusEffects.RemoveAt(i);
        }
    }

    private void ExecuteAttack(Character attacker, Character defender)
    {
        // A shapeshifted attacker uses its natural attack profile instead of weapons.
        var form = Skills.ShapeshiftService.GetForm(attacker);
        if (form != null)
        {
            if (form.LocksPhysical || string.IsNullOrWhiteSpace(form.AttackDice))
                return; // e.g. owl cannot melee
            int beastSwings = attacker.AttackRate;
            for (int i = 0; i < beastSwings && defender.Health > 0; i++)
                ResolveSingleHit(attacker, defender, form.Name, form.AttackVerb ?? "strike", form.AttackDice, "Beast", form.AttackAttribute);
            return;
        }

        // --- MAIN HAND: one swing per attack-rate (haste/slow modify the count) ---
        var mainWeapon = attacker.MainHandWeapon;
        string mainName = mainWeapon?.Name ?? attacker.Name; // unarmed falls back to the attacker
        string mainDice = mainWeapon?.DiceNotation ?? DefaultDiceNotation;

        int swings = attacker.AttackRate;
        for (int i = 0; i < swings && defender.Health > 0; i++)
            ResolveSingleHit(attacker, defender, mainName, PickVerb(mainWeapon, DefaultAttackVerb), mainDice, "Main Hand");

        // --- OFF HAND: one follow-up swing when dual-wielding ---
        var offWeapon = attacker.OffHandWeapon;
        if (defender.Health > 0 && offWeapon != null)
            ResolveSingleHit(attacker, defender, offWeapon.Name, PickVerb(offWeapon, "strike"), offWeapon.DiceNotation, "Off Hand");
    }

    private static string PickVerb(Item weapon, string fallback)
    {
        if (weapon?.AttackVerbs == null || weapon.AttackVerbs.Length == 0)
            return fallback;
        return weapon.AttackVerbs[Random.Shared.Next(weapon.AttackVerbs.Length)];
    }

    private void ResolveSingleHit(Character attacker, Character defender, string weaponName, string verb, string dice, string handLabel, string attributeBonus = null)
    {
        bool critMastery = attacker.KnownSkills.ContainsKey("critical_mastery");
        var outcome = AttackResolver.Resolve(attacker, defender, dice, DamageType.Physical, attributeBonus: attributeBonus, critOnMaxRoll: critMastery);

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
        {
            DeathService.HandleDeath(defender, _world, attacker);
            return;
        }

        // Event passives fire only when the defender survives the hit.
        Skills.PassiveService.Fire(Skills.SkillTrigger.OnOutgoingHit, attacker, defender, _world);
        Skills.PassiveService.Fire(Skills.SkillTrigger.OnIncomingHit, defender, attacker, _world);
    }
}