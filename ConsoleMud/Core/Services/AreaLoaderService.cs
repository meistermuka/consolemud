using System.Text.Json;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Services;

public static class AreaLoaderService
{
    public static void LoadAreaFile(string filePath, WorldState world)
    {
        if (!File.Exists(filePath))
            Console.WriteLine($"Area file not found: {filePath}");

        string jsonText = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var areaBlueprint = JsonSerializer.Deserialize<AreaBlueprint>(jsonText, options);

        if (areaBlueprint == null)
            Console.WriteLine($"Failed to parse area file: {filePath}");

        Console.WriteLine($"Loaded area: {areaBlueprint.Name}");

        // A temporary map linking the file's text ID to our live runtime Guid
        var idTranslationTable = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var createdRooms = new List<(Room RoomEntity, RoomBlueprint Blueprint)>();

        // 2. First Pass: Create the Room entities and assign fresh Guids
        foreach (var bp in areaBlueprint.Rooms)
        {
            var liveRoom = new Room
            {
                Name = bp.Name,
                Description = bp.Description
            };

            // Pair the textual VirtualId to this room's permanent memory Guid
            idTranslationTable[bp.VirtualId] = liveRoom.Id;

            // Hold on to these pairs temporarily so we can wire them up in pass two
            createdRooms.Add((liveRoom, bp));
        }

        // 3. Second Pass: Resolve text exits into absolute Guid links
        foreach (var pair in createdRooms)
        {
            var liveRoom = pair.RoomEntity;
            var blueprint = pair.Blueprint;

            foreach (var exitPair in blueprint.Exits)
            {
                // Convert text string ("North") to our Direction Enum safely
                if (Enum.TryParse<Direction>(exitPair.Key, true, out var direction))
                {
                    string targetVirtualId = exitPair.Value;

                    // Locate the destination room's Guid via our lookup map
                    if (idTranslationTable.TryGetValue(targetVirtualId, out var targetGuid))
                    {
                        liveRoom.Exits[direction] = targetGuid;
                    }
                    else
                    {
                        Console.WriteLine(
                            $"[Warning] Room '{blueprint.VirtualId}' points to an invalid exit target: '{targetVirtualId}'");
                    }
                }
            }
            // 4. inject completed room into active live worldstate memory loop
            world.Rooms[liveRoom.Id] = liveRoom;
        }
        
        Console.WriteLine($"Successfully loaded {createdRooms.Count} rooms from '{areaBlueprint.Name}'.\n");
    }
}