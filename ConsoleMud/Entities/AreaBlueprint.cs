namespace ConsoleMud.Entities;

public class AreaBlueprint
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<RoomBlueprint> Rooms { get; set; } = new();
    public List<ItemBlueprint> ItemTemplates { get; set; } = new();
    public List<NpcBlueprint> NpcTemplates { get; set; } = new();
}