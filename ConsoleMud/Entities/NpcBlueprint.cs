namespace ConsoleMud.Entities;

public class NpcBlueprint
{
    public string VirtualId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Level { get; set; } = 1;
    public int XpReward { get; set; } // 0 = use the fallback formula
    public string EquippedWeaponTemplateId { get; set; }
    public bool IsAggressive { get; set; }
    public string[] Archetypes { get; set; } // e.g. ["Animal"], ["Undead"]
}