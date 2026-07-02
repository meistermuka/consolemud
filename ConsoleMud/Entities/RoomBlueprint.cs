namespace ConsoleMud.Entities;

public class RoomBlueprint
{
    public string VirtualId { get; set; } // ex: forest_entrance
    public string Name { get; set; }
    public string Description { get; set; }
    // Maps a direction to the virtual ID "deep_woods" of target room
    public Dictionary<string, string> Exits { get; set; } = new();
    public bool IsOutside { get; set; }
    public bool IsDark { get; set; }
    public SpawnRulesBlueprint Spawns { get; set; } = new();

    // Optional: relative path to a Scripts/rooms/*.lua file (without extension).
    // e.g. "rooms/throne_room". Null means no entry event.
    public string? ScriptId { get; set; }
}