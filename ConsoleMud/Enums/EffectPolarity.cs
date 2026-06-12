namespace ConsoleMud.Enums;

public enum EffectPolarity
{
    Positive, // a buff; not stripped by hostile dispels, removed by nothing friendly
    Negative  // a debuff; cleanse/dispel target these
}
