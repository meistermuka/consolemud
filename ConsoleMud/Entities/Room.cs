using ConsoleMud.Enums;

namespace ConsoleMud.Entities;

public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Description { get; set; }
    // Maps a direction to the GUID of target room
    public Dictionary<Direction, Guid> Exits { get; set; } = new();
    // Who and what is in the room
    public List<Character> Characters { get; set; } = new();
    public List<Item> Items { get; set; } = new();
}