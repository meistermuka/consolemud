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
            TryUntouchable(attacker);
        }
    }

    // Fighter "second wind" reads its own skills.json parameters.
    private static void TrySecondWind(Character c)
    {
        if (!c.KnownSkills.ContainsKey("second_wind"))
            return;
        double threshold = Skills.PassiveService.SkillParam("second_wind", "thresholdPct", 25) / 100.0;
        if (c.Health <= 0 || c.Health > c.MaxHealth * threshold)
            return;
        if (!c.EncounterFlags.Add("second_wind")) // already triggered this fight
            return;

        int heal = (int)Skills.PassiveService.SkillParam("second_wind", "healAmount", 30);
        c.Health = Math.Min(c.MaxHealth, c.Health + heal);
        Helpers.ColorConsole.WriteLine(
            $"\n{c.Name} catches a second wind and recovers {heal} health!", ConsoleColor.Green);
    }

    // Thief "untouchable" reads its own skills.json parameters.
    private static void TryUntouchable(Character c)
    {
        if (!c.KnownSkills.ContainsKey("untouchable") || c.Health <= 0)
            return;
        double threshold = Skills.PassiveService.SkillParam("untouchable", "thresholdPct", 20) / 100.0;
        if (c.Health > c.MaxHealth * threshold)
            return;
        if (c.Cooldowns.TryGetValue("untouchable", out var ready) && DateTime.UtcNow < ready)
            return;

        int cooldown = (int)Skills.PassiveService.SkillParam("untouchable", "cooldownSeconds", 600);
        c.Cooldowns["untouchable"] = DateTime.UtcNow.AddSeconds(cooldown);
        c.StatusEffects.Add(new StatusEffect
        {
            Name = "untouchable", Modifier = EffectModifier.ImmunityOverride, DamageType = DamageType.Physical,
            Polarity = EffectPolarity.Positive,
            TicksRemaining = (int)Skills.PassiveService.SkillParam("untouchable", "durationTicks", 3)
        });
        Helpers.ColorConsole.WriteLine($"\n{c.Name} phases out of harm's way, briefly untouchable!", ConsoleColor.Cyan);
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

        // Thorns: the defender's briars wound the attacker (works for any warded target).
        var thorns = defender.StatusEffects.FirstOrDefault(e => e.Modifier == EffectModifier.Thorns && !e.IsExpired);
        if (thorns != null && attacker.Health > 0)
        {
            int reflected = DamageResolver.Apply(attacker, DamageType.Physical, (int)thorns.Magnitude);
            attacker.Health -= reflected;
            Helpers.ColorConsole.WriteLine($"{attacker.Name} is pricked by thorns for {reflected}!", ConsoleColor.Gray);
            if (attacker.Health <= 0)
            {
                DeathService.HandleDeath(attacker, _world, defender);
                return;
            }
        }

        // Natural attunement: outdoors, the ranger's strikes carry bonus magic damage (scales with Wisdom).
        if (attacker.KnownSkills.ContainsKey("natural_attunement") && defender.Health > 0
            && _world.Rooms.TryGetValue(attacker.CurrentRoomId, out var atrm) && atrm.IsOutside)
        {
            int bonus = Math.Max(1, (attacker.Wisdom - 10) / 2);
            int dealt = DamageResolver.Apply(defender, DamageType.Magic, bonus);
            defender.Health -= dealt;
            Helpers.ColorConsole.WriteLine($"Nature's power adds {dealt} magic damage!", ConsoleColor.Gray);
            if (defender.Health <= 0) { DeathService.HandleDeath(defender, _world, attacker); return; }
        }

        // Poison coat: the attacker's coated weapon may poison the defender (charges deplete).
        var coat = attacker.StatusEffects.FirstOrDefault(e => e.Modifier == EffectModifier.WeaponCoat && e.Charges > 0);
        if (coat != null && defender.Health > 0)
        {
            coat.Charges--;
            if (coat.Charges <= 0)
                attacker.StatusEffects.Remove(coat);
            if (Random.Shared.NextDouble() < Skills.PassiveService.SkillParam("poison", "procChance", 0.25))
            {
                defender.StatusEffects.Add(new StatusEffect
                {
                    Name = "poison", Modifier = EffectModifier.DamageOverTime, Magnitude = coat.Magnitude,
                    DamageType = DamageType.Poison, Type = EffectType.Poison,
                    Polarity = EffectPolarity.Negative,
                    TicksRemaining = (int)Skills.PassiveService.SkillParam("poison", "dotTicks", 3)
                });
                Helpers.ColorConsole.WriteLine($"Your venom courses through {defender.Name}!", ConsoleColor.Gray);
            }
        }
    }
}