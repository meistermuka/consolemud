namespace ConsoleMud.Core.Skills;

/// <summary>
/// Skill proficiency rules: a tiny failure chance even at the cap, and gains
/// that come easily early and crawl near 100. Proficiency rises on every
/// attempt, success or failure.
/// </summary>
public static class ProficiencyMath
{
    private const double Ceiling = 99.999; // 100% still fails ~0.001%
    private const double BaseGain = 2.0;
    private const double MinGain = 0.01;    // keeps the climb to 100 alive

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
