using ConsoleMud.Helpers;

namespace ConsoleMud.Entities;

public class NonPlayerCharacter : Character
{
    public bool IsAggressive { get; set; }
    public int XpReward { get; set; }

    // Optional Lua script key for custom per-tick AI behaviour (Layer 3).
    // e.g. "npcs/goblin_shaman". Null means the default aggressive AI only.
    public string? ScriptId { get; set; }

    // Set when this creature is a tamed pet; identifies its owning player.
    public Guid? OwnerId { get; set; }
    public bool IsPet => OwnerId.HasValue;
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