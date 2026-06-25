using ConsoleMud.Enums;

namespace ConsoleMud.Entities;

public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();
    // Stable id from the area file; survives restarts (the runtime Guid does not).
    public string VirtualId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    // Maps a direction to the GUID of target room
    public Dictionary<Direction, Guid> Exits { get; set; } = new();
    // Who and what is in the room
    public List<Character> Characters { get; set; } = new();
    public List<Item> Items { get; set; } = new();
    public List<Trap> Traps { get; set; } = new();

    public bool IsOutside { get; set; }
    public bool IsDark { get; set; } // a light source or darkvision is needed to see here
}