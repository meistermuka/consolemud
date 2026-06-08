using ConsoleMud.Enums;

namespace ConsoleMud.Entities;

public class Item
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Composition flags instead of deep inheritance
    public bool IsGetable { get; set; }
    public bool IsContainer { get; set; }
    public List<Item> Contents { get; set; } = new(); // Used if IsContainer is True
    
    // Weapon properties
    public bool IsWeapon { get; set; }
    public string DiceNotation { get; set; } // e.g. "1d6"
    public string[] AttackVerbs { get; set; } // ["slash", "stab", "slice"]
    
    // Armour properties
    public bool IsArmour { get; set; }
    public int ArmourRating { get; set; }
    
    public bool IsEquippable { get; set; }
    public EquipmentSlot TargetSlot { get; set; }
    public bool IsShield { get; set; }
}