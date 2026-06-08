namespace ConsoleMud.Core;

public static class DiceRoller
{
    private static readonly Random _random = new();

    public static int Roll(string diceNotation)
    {
        if (string.IsNullOrWhiteSpace(diceNotation))
            return _random.Next(1, 3); // default unarmed attack

        var parts = diceNotation.ToLower().Split('d');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int count) || !int.TryParse(parts[1], out int sides))
            return 1; // fallback for invalid notations

        int total = 0;
        for (int i = 0; i < count; i++)
            total += _random.Next(1, sides + 1);
        
        return total;
    }
}