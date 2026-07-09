using System.Text.Json;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Services;

public static class AreaLoaderService
{
    public static void LoadAreaFile(string filePath, WorldState world, DefinitionRegistry definitions)
    {
        if (!File.Exists(filePath))
            Console.WriteLine($"Area file not found: {filePath}");

        string jsonText = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var areaBlueprint = JsonSerializer.Deserialize<AreaBlueprint>(jsonText, options);

        if (areaBlueprint == null)
            Console.WriteLine($"Failed to parse area file: {filePath}");

        Console.WriteLine($"Loaded area: {areaBlueprint.Name}");
        
        // Merge this area's item templates into the global registry so scripts can
        // mint them by VirtualId at runtime. Last-wins on collisions, with a warning.
        foreach (var itemBp in areaBlueprint.ItemTemplates)
        {
            if (world.ItemTemplates.ContainsKey(itemBp.VirtualId))
                Console.WriteLine($"[Warning] Duplicate item template VirtualId '{itemBp.VirtualId}' — overwriting the previous definition.");
            world.ItemTemplates[itemBp.VirtualId] = itemBp;
        }

        var itemTemplates = world.ItemTemplates;
        var npcTemplates = areaBlueprint.NpcTemplates.ToDictionary(t => t.VirtualId, StringComparer.OrdinalIgnoreCase);

        foreach (var npcBp in areaBlueprint.NpcTemplates)
        {
            if(world.NpcTemplates.ContainsKey(npcBp.VirtualId))
                Console.WriteLine($"[Warning] Duplicate npc template VirtualId '{npcBp.VirtualId}' - overwriting the previous definition.");
            world.NpcTemplates[npcBp.VirtualId] = npcBp;
        }


        // A temporary map linking the file's text ID to our live runtime Guid
        var idTranslationTable = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var createdRooms = new List<(Room RoomEntity, RoomBlueprint Blueprint)>();

        // Pass 1: Create the Room entities and assign fresh Guids
        foreach (var bp in areaBlueprint.Rooms)
        {
            var liveRoom = new Room
            {
                VirtualId = bp.VirtualId,
                Name = bp.Name,
                Description = bp.Description,
                IsOutside = bp.IsOutside,
                IsDark = bp.IsDark,
                ScriptId = bp.ScriptId
            };
            // Pair the textual VirtualId to this room's permanent memory Guid
            idTranslationTable[bp.VirtualId] = liveRoom.Id;
            world.RoomsByVirtualId[bp.VirtualId] = liveRoom.Id;

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
                        liveRoom.Exits[direction] = targetGuid;
                    else
                        Console.WriteLine($"[Warning] Room '{blueprint.VirtualId}' points to an invalid exit target: '{targetVirtualId}'");
                }
            }
            
            // Execute Item Spawning Configuration
            foreach (var spawnRef in blueprint.Spawns.Items)
            {
                if (itemTemplates.TryGetValue(spawnRef.TemplateId, out var itemBp))
                {
                    for (int i = 0; i < spawnRef.Count; i++)
                    {
                        liveRoom.Items.Add(ItemFactory.CreateLiveItem(itemBp));
                    }
                }
            }
            
            // Execute NPC Spawning Configuration
            foreach (var spawnRef in blueprint.Spawns.Npcs)
            {
                if (npcTemplates.TryGetValue(spawnRef.TemplateId, out var npcBp))
                {
                    for (int i = 0; i < spawnRef.Count; i++)
                    {
                        var liveNpc = NpcFactory.CreateLiveNpc(npcBp, liveRoom.Id, itemTemplates, definitions);
                        liveRoom.Characters.Add(liveNpc);
                        world.Characters[liveNpc.Id] = liveNpc; // Track globally in main thread state
                    }
                }
            }
            // 4. inject completed room into active live worldstate memory loop
            world.Rooms[liveRoom.Id] = liveRoom;
        }
        
        Console.WriteLine($"Successfully loaded {createdRooms.Count} rooms from '{areaBlueprint.Name}'.\n");
    }
}