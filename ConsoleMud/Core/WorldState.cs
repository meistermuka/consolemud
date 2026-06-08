using ConsoleMud.Entities;

namespace ConsoleMud.Core;

public class WorldState
{
    public Dictionary<Guid, Room> Rooms { get; set; } = new();
    public Dictionary<Guid, Character> Characters { get; set; } = new();

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