namespace ConsoleMud.Core.Skills;

/// <summary>
/// Skill proficiency rules: a tiny failure chance even at the cap, and gains
/// that come easily early and crawl near 100. Proficiency rises on every
/// attempt, success or failure.
/// </summary>
public static class ProficiencyMath
{
    private static double Ceiling => Services.TuningRegistry.Get("proficiency.ceiling", 99.999);
    private static double BaseGain => Services.TuningRegistry.Get("proficiency.baseGain", 2.0);
    private static double MinGain => Services.TuningRegistry.Get("proficiency.minGain", 0.01);

    public static bool RollSuccess(double proficiency)
    {
        double effective = Math.Min(proficiency, Ceiling);
        return Random.Shared.NextDouble() * 100.0 < effective;
    }

    public static double Gain(double proficiency)
    {
        double gain = Math.Max(MinGain, BaseGain * (1.0 - proficiency / 100.0));
        return Math.Min(100.0, proficiency + gain);
    }
}
