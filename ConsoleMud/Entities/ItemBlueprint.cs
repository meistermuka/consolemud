using ConsoleMud.Enums;

namespace ConsoleMud.Entities;

public class ItemBlueprint
{
    public string VirtualId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsGetable { get; set; }
    public bool IsContainer { get; set; }
    public bool IsWeapon { get; set; }
    public string WeaponType { get; set; } // e.g. "Sword", "Bow"; defaults to Unarmed
    public string DiceNotation { get; set; }
    public string[] AttackVerbs { get; set; }
    public bool IsArmor { get; set; }
    public bool IsEquippable { get; set; }
    public bool IsShield { get; set; }
    public int ArmorRating { get; set; }
    public string TargetSlot { get; set; }
}