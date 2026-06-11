namespace ConsoleMud.Entities;

public class NpcBlueprint
{
    public string VirtualId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public string EquippedWeaponTemplateId { get; set; }
    public bool IsAggressive { get; set; }
}