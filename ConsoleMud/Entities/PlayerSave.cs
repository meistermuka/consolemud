using ConsoleMud.Enums;

namespace ConsoleMud.Entities;

/// <summary>
/// A serializable snapshot of a player. Deliberately a flat DTO rather than the
/// live Player, so we never serialize CombatTarget (a live reference) or other
/// transient state, and the location rides as a stable VirtualId.
/// </summary>
public class PlayerSave
{
    public string Name { get; set; }

    public Species Species { get; set; }
    public CharacterClass Class { get; set; }
    public int Level { get; set; }
    public long Experience { get; set; }

    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Intelligence { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }

    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Mana { get; set; }
    public int MaxMana { get; set; }

    public DamageType? Specialization { get; set; }
    public bool InnateDarkvision { get; set; }

    public Dictionary<string, double> KnownSkills { get; set; } = new();
    public Dictionary<DamageType, double> DamageMultipliers { get; set; } = new();

    public List<Item> Inventory { get; set; } = new();
    public Dictionary<EquipmentSlot, Item> Equipment { get; set; } = new();

    // Location by stable id; resolved to a live room on load.
    public string CurrentRoomVirtualId { get; set; }
}
