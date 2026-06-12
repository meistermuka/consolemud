using ConsoleMud.Enums;

namespace ConsoleMud.Entities;

public class Item
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string[] Keywords => Name?.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
    public string Description { get; set; } = string.Empty;
    
    // Composition flags instead of deep inheritance
    public bool IsGetable { get; set; } = true;
    public bool IsContainer { get; set; }
    public List<Item> Contents { get; set; } = new(); // Used if IsContainer is True
    
    // Weapon properties
    public bool IsWeapon { get; set; }
    public WeaponType WeaponType { get; set; } = WeaponType.Unarmed;
    public string? DiceNotation { get; set; } // e.g. "1d6"
    public string[] AttackVerbs { get; set; } = { }; // ["slash", "stab", "slice"]
    
    // Armour properties
    public bool? IsArmour { get; set; }
    public int ArmourRating { get; set; }
    
    public bool IsEquippable { get; set; }
    public EquipmentSlot TargetSlot { get; set; }
    public bool IsShield { get; set; }

    /// <summary>
    /// Check if a player's query text matches any of the item's keywords.'
    /// </summary>
    public bool MatchesKeyword(string query)
    {
        if (string.IsNullOrEmpty(query))
            return false;

        string lowerQuery = query.ToLower().Trim();
        
        if (Name.Equals(lowerQuery, StringComparison.OrdinalIgnoreCase)) 
            return true;
        
        return Keywords.Contains(lowerQuery);
    }
}