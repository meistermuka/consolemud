using ConsoleMud.Enums;

namespace ConsoleMud.Entities;

/// <summary>
/// A timed modifier on a character: buffs, debuffs, damage/heal over time,
/// immunities, and flat reductions. Replaces the old damage-only ActiveEffect.
/// </summary>
public class StatusEffect
{
    public string Name { get; set; } = string.Empty;
    public EffectType Type { get; set; } = EffectType.Generic;
    public EffectPolarity Polarity { get; set; } = EffectPolarity.Negative;
    public EffectModifier Modifier { get; set; }

    // Generic value the modifier interprets: HP per tick, armor points, % amounts, etc.
    public double Magnitude { get; set; }

    // The damage type this effect deals (DoT) or affects (ImmunityOverride, FlatDamageReduction).
    public DamageType DamageType { get; set; } = DamageType.Physical;

    // Lifespan in status pulses. -1 means permanent until removed.
    public int TicksRemaining { get; set; }

    // Optional use-charges (e.g. poison's 5 strikes). -1 means not charge-based.
    public int Charges { get; set; } = -1;

    // Provenance for display and stacking rules.
    public string SourceSkillId { get; set; } = string.Empty;

    public bool IsPermanent => TicksRemaining < 0;
    public bool IsExpired => !IsPermanent && TicksRemaining <= 0;
}
