namespace ConsoleMud.Enums;

/// <summary>
/// The removable category of an effect. Cleanse/dispel commands target by this:
/// cure_poison removes Poison, dispel_magic removes Magic/Curse, etc.
/// </summary>
public enum EffectType
{
    Generic,
    Poison,
    Disease,
    Bleed,
    Curse,
    Magic,
    Mental,   // charm, fear, sleep, psychic-sourced
    Physical
}
