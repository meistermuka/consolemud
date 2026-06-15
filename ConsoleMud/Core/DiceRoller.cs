namespace ConsoleMud.Core;

public static class DiceRoller
{
    private static readonly Random _random = new();

    public static int Roll(string diceNotation) => Roll(diceNotation, out _);

    /// <summary>The maximum possible total for a dice notation (count * sides).</summary>
    public static int Max(string diceNotation)
    {
        if (string.IsNullOrWhiteSpace(diceNotation)) return 2;
        var parts = diceNotation.ToLower().Split('d');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int count) || !int.TryParse(parts[1], out int sides))
            return 1;
        return count * sides;
    }

    /// <summary>
    /// Rolls dice notation like "3d4" and also reports the maximum possible total
    /// (count * sides), which combat uses to detect natural max-roll criticals.
    /// </summary>
    public static int Roll(string diceNotation, out int maxPossible)
    {
        if (string.IsNullOrWhiteSpace(diceNotation))
        {
            maxPossible = 2; // default unarmed attack rolls 1..2
            return _random.Next(1, 3);
        }

        var parts = diceNotation.ToLower().Split('d');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int count) || !int.TryParse(parts[1], out int sides))
        {
            maxPossible = 1; // fallback for invalid notations
            return 1;
        }

        maxPossible = count * sides;

        int total = 0;
        for (int i = 0; i < count; i++)
            total += _random.Next(1, sides + 1);

        return total;
    }
}
