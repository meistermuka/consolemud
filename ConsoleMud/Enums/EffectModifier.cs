namespace ConsoleMud.Enums;

/// <summary>
/// What an active effect actually does each tick or while it lasts.
/// One effect carries one modifier kind; stack multiple effects for combos.
/// </summary>
public enum EffectModifier
{
    DamageOverTime,      // Magnitude HP lost per status tick (poison, insect_swarm)
    HealOverTime,        // Magnitude HP restored per status tick (rejuvenate)
    ArmorMod,            // Magnitude added to armor rating (shield, skin_of_oak buff)
    AccuracyMod,         // Magnitude added to to-hit chance % (bless, insect_swarm debuff)
    AttackRateMod,       // Magnitude added to attacks-per-round (haste +, ice_storm -)
    DamageDealtMod,      // Magnitude % change to outgoing damage (berserk +50)
    ImmunityOverride,    // grants immunity to DamageType while active
    FlatDamageReduction, // Magnitude % reduction to incoming DamageType (indomitable_will)
    AvoidanceMod,        // Magnitude % chance to fully avoid an incoming attack (dodge, parry)
    CritChanceMod        // Magnitude % chance added to land a critical hit (opportunist)
}
