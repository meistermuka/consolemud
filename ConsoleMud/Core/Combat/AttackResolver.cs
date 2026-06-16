using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Combat;

/// <summary>
/// The single attack pipeline: to-hit, crit, attribute bonus, outgoing buffs,
/// armor, then the defender's damage-type multiplier. Auto-attacks and skill
/// hits both flow through here so the rules stay in one place.
/// </summary>
public static class AttackResolver
{
    private static int BaseHitChance => Services.TuningRegistry.GetInt("combat.baseHitChance", 85);
    private static double CritMultiplier => Services.TuningRegistry.Get("combat.critMultiplier", 2.0);
    private static double MarkBonusMultiplier => Services.TuningRegistry.Get("combat.markBonusMultiplier", 1.2);

    public readonly record struct AttackOutcome(bool Hit, bool Crit, int Damage, int Roll, int MaxRoll);

    public static AttackOutcome Resolve(
        Character attacker,
        Character defender,
        string diceNotation,
        DamageType type,
        string attributeBonus = null,
        bool ignoresArmor = false,
        bool critOnMaxRoll = false)
    {
        // 1. To-hit: base, plus attacker accuracy, minus defender avoidance.
        double hitChance = BaseHitChance + attacker.AccuracyBonus - defender.AvoidanceChance;
        if (Random.Shared.Next(1, 101) > hitChance)
            return new AttackOutcome(false, false, 0, 0, 0);

        // 2. Damage roll + attribute modifier.
        int roll = DiceRoller.Roll(diceNotation, out int maxRoll);
        int raw = Math.Max(1, roll + AttributeModifier(attacker, attributeBonus));

        // 3. Crit: a natural max roll (if the attacker qualifies) or a buff-driven chance.
        bool crit = (critOnMaxRoll && maxRoll > 0 && roll == maxRoll)
                    || (attacker.CritChanceBonus > 0 && Random.Shared.Next(1, 101) <= attacker.CritChanceBonus);
        if (crit)
            raw = (int)Math.Round(raw * CritMultiplier);

        // 4. Outgoing damage buffs (berserk, etc.).
        raw = (int)Math.Round(raw * attacker.DamageDealtMultiplier);

        // Hunter's mark: bonus damage against the marked target.
        if (attacker.MarkedTarget == defender && attacker.KnownSkills.ContainsKey("mark_of_the_hunter"))
            raw = (int)Math.Round(raw * MarkBonusMultiplier);

        // 5. Armor mitigates physical unless the attack ignores it.
        int afterArmor = (!ignoresArmor && type == DamageType.Physical)
            ? Math.Max(0, raw - defender.TotalArmourRating)
            : raw;

        // 6. Defender damage-type multiplier (species matrix + overrides + reductions).
        double multiplier = DamageResolver.GetDamageMultiplier(defender, type);
        int finalDamage = multiplier <= 0.0 ? 0 : Math.Max(1, (int)Math.Round(afterArmor * multiplier));

        return new AttackOutcome(true, crit, finalDamage, roll, maxRoll);
    }

    private static int AttributeModifier(Character c, string attribute)
    {
        if (string.IsNullOrWhiteSpace(attribute))
            return 0;

        int score = attribute.Trim().ToLower() switch
        {
            "strength" or "str" => c.Strength,
            "dexterity" or "dex" => c.Dexterity,
            "constitution" or "con" => c.Constitution,
            "intelligence" or "int" => c.Intelligence,
            "wisdom" or "wis" => c.Wisdom,
            "charisma" or "cha" => c.Charisma,
            _ => 10
        };

        return (score - 10) / 2; // standard tabletop modifier
    }
}
