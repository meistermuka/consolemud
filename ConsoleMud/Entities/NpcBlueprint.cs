namespace ConsoleMud.Entities;

public class NpcBlueprint
{
    public string VirtualId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Mana { get; set; }
    public int MaxMana { get; set; }
    public int Level { get; set; } = 1;
    public int XpReward { get; set; } // 0 = use the fallback formula
    public string EquippedWeaponTemplateId { get; set; }
    public bool IsAggressive { get; set; }
    public bool HasDarkvision { get; set; }
    public string[] Archetypes { get; set; } // e.g. ["Animal"], ["Undead"]

    // Skill ids this NPC knows (e.g. ["thunder_bolt"]). Granted at mastery
    // proficiency so scripted casts reliably succeed. Null/empty means none.
    public string[] Skills { get; set; }

    // Optional: relative path to a Scripts/npcs/*.lua file (without extension).
    // e.g. "npcs/goblin_shaman". Null means default AI only.
    public string? ScriptId { get; set; }
}