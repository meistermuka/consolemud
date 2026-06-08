namespace ConsoleMud.Entities;

public abstract class Character
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Stats
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    
    // Location tracking
    public Guid CurrentRoomId { get; set; }
    
    // Inventory
    public List<Item> Inventory { get; set; } = new();
    
    // New eqipment slot
    public Item EquippedWeapon { get; set; }
    public Item EquippedArmour { get; set; }
    public Character CombatTarget { get; set; }
}