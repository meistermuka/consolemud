using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Combat;

/// <summary>
/// Resolves the final damage multiplier against a defender for a given type.
/// Layers, in order:
///   1. Species base matrix (0 immune, 0.5 resist, 2 vulnerable, 1 default).
///   2. Active immunity overrides (berserk, elemental_mastery) -> force 0.
///   3. Flat percentage reductions (indomitable_will) -> multiply by (1 - pct/100).
/// </summary>
public static class DamageResolver
{
    public static double GetDamageMultiplier(Character defender, DamageType type)
    {
        // 1. Species base.
        double multiplier = defender.DamageMultipliers.TryGetValue(type, out var baseMult)
            ? baseMult
            : 1.0;

        foreach (var effect in defender.StatusEffects)
        {
            if (effect.DamageType != type)
                continue;

            // 2. Immunity overrides win outright.
            if (effect.Modifier == EffectModifier.ImmunityOverride)
                return 0.0;

            // 3. Flat reductions stack multiplicatively after the species matrix.
            if (effect.Modifier == EffectModifier.FlatDamageReduction)
                multiplier *= Math.Max(0.0, 1.0 - effect.Magnitude / 100.0);
        }

        return multiplier;
    }

    /// <summary>
    /// Applies the multiplier to raw damage. Immunity (0) yields 0; otherwise a
    /// minimum of 1 so connected hits always register at least a scratch.
    /// </summary>
    public static int Apply(Character defender, DamageType type, int rawDamage)
    {
        double multiplier = GetDamageMultiplier(defender, type);
        if (multiplier <= 0.0)
            return 0;

        return Math.Max(1, (int)Math.Round(rawDamage * multiplier));
    }
}
