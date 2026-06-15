using ConsoleMud.Helpers;

namespace ConsoleMud.Entities;

public class NonPlayerCharacter : Character
{
    public bool IsAggressive { get; set; }
    public string[] Keywords => ColorMarkup.Strip(Name).ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

    public bool MatchesKeyword(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return false;

        string lowerQuery = query.ToLower().Trim();

        if (ColorMarkup.Strip(Name).Equals(lowerQuery, StringComparison.OrdinalIgnoreCase))
            return true;

        return Keywords.Contains(lowerQuery);
    }
}