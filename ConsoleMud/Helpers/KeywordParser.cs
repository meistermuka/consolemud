namespace ConsoleMud.Helpers;

public static class KeywordParser
{
    /// <summary>
    /// Parses commands like "2.dagger" into an item index (2) and a raw keyword ("dagger")
    /// If no dot prefix is found, the item index is 1
    /// </summary>
    public static (int TargetIndex, string CleanKeyword) ExtractIndex(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
            return (1, "");
        
        int dotIndex = rawInput.IndexOf('.');
        if (dotIndex > 0)
        {
            string numberPart = rawInput.Substring(0, dotIndex);
            string keywordPart = rawInput.Substring(dotIndex + 1);
            if (int.TryParse(numberPart, out int parsedIndex) && parsedIndex > 0)
                return (parsedIndex, keywordPart);
        }
        
        return (1, rawInput);
    }
}