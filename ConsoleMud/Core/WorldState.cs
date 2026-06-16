using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core;

public class WorldState
{
    public Dictionary<Guid, Room> Rooms { get; set; } = new();
    public Dictionary<Guid, Character> Characters { get; set; } = new();

    // Current outdoor weather; skills and the weather tick read this.
    public Weather CurrentWeather { get; set; } = Weather.Clear;
    public bool IsStormy => CurrentWeather is Weather.Raining or Weather.Storming;

    // Stable-id lookup so saves can store a room by VirtualId and resolve it back
    // after a restart, when the runtime Guids have all changed.
    public Dictionary<string, Guid> RoomsByVirtualId { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // Where the dead respawn. Defaults to the starting room until a safe room is flagged.
    public Guid? SafeRoomId { get; set; }

    public bool TryGetRoomByVirtualId(string virtualId, out Room room)
    {
        room = null;
        return virtualId != null
               && RoomsByVirtualId.TryGetValue(virtualId, out var id)
               && Rooms.TryGetValue(id, out room);
    }

    public void MoveCharacter(Character character, Guid targetRoomId)
    {
        if (Rooms.TryGetValue(character.CurrentRoomId, out var oldRoom))
        {
            oldRoom.Characters.Remove(character);
        }

        if (Rooms.TryGetValue(targetRoomId, out var newRoom))
        {
            newRoom.Characters.Add(character);
            character.CurrentRoomId = targetRoomId;
        }
    }
}
