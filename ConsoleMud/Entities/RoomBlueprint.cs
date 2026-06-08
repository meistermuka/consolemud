namespace ConsoleMud.Entities;

public class RoomBlueprint
{
    public string VirtualId { get; set; } // ex: forest_entrance
    public string Name { get; set; }
    public string Description { get; set; }
    // Maps a direction to the virtual ID "deep_woods" of target room
    public Dictionary<string, string> Exits { get; set; } = new();
    public SpawnRulesBlueprint Spawns { get; set; } = new();
}