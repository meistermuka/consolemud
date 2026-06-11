using ConsoleMud.Enums;

namespace ConsoleMud.Entities;

public abstract class Character
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Stats
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    
    public int Mana { get; set; }
    public int MaxMana { get; set; }
    
    // Location tracking
    public Guid CurrentRoomId { get; set; }
    
    // Inventory
    public List<Item> Inventory { get; set; } = new();
    
    // New eqipment slot
    public Item EquippedWeapon { get; set; }
    public Item EquippedArmour { get; set; }
    public Character CombatTarget { get; set; }
    
    public Dictionary<string, DateTime> Cooldowns { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<ActiveEffect> StatusEffects { get; set; } = new();
    
    public Dictionary<EquipmentSlot, Item> Equipment { get; set; } = new();
    // Dynamically calculate total mitigation for all equipped armour items
    public int TotalArmourRating => Equipment.Values.Sum(i => i.ArmourRating);
    
    public Item MainHandWeapon => Equipment.TryGetValue(EquipmentSlot.MainHand, out var item) && item.IsWeapon ? item : null;
    public Item OffHandWeapon => Equipment.TryGetValue(EquipmentSlot.OffHand, out var item) && item.IsWeapon ? item : null;
    
    public CharacterClass Class { get; set; }
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Intelligence { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }
}