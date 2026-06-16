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
        
        var itemTemplates = areaBlueprint.ItemTemplates.ToDictionary(t => t.VirtualId, StringComparer.OrdinalIgnoreCase);
        var npcTemplates = areaBlueprint.NpcTemplates.ToDictionary(t => t.VirtualId, StringComparer.OrdinalIgnoreCase);


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
                IsOutside = bp.IsOutside
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
                        liveRoom.Items.Add(CreateLiveItem(itemBp));
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
                        var liveNpc = CreateLiveNpc(npcBp, liveRoom.Id, itemTemplates);
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

    private static Item CreateLiveItem(ItemBlueprint bp)
    {
        Enum.TryParse<EquipmentSlot>(bp.TargetSlot, true, out var targetSlot);
        return new Item
        {
            Name = bp.Name,
            Description = bp.Description,
            IsGetable = bp.IsGetable,
            IsContainer = bp.IsContainer,
            IsWeapon = bp.IsWeapon,
            DiceNotation = bp.DiceNotation,
            AttackVerbs = bp.AttackVerbs,
            IsArmour = bp.IsArmor,
            IsEquippable = bp.IsEquippable,
            IsShield = bp.IsShield,
            TargetSlot = targetSlot,
            ArmourRating = bp.ArmorRating
        };
    }
    
    private static NonPlayerCharacter CreateLiveNpc(NpcBlueprint bp, Guid roomId, Dictionary<string, ItemBlueprint> itemTemplates)
    {
        var npc = new NonPlayerCharacter
        {
            Name = bp.Name,
            Description = bp.Description,
            Health = bp.Health,
            MaxHealth = bp.MaxHealth,
            Level = bp.Level < 1 ? 1 : bp.Level,
            CurrentRoomId = roomId,
            IsAggressive = bp.IsAggressive,
            // Fallback reward scales with the NPC's level and toughness.
            XpReward = bp.XpReward > 0 ? bp.XpReward : (bp.Level < 1 ? 1 : bp.Level) * 10 + bp.MaxHealth,
        };

        // If the NPC template requests an equipped starter item weapon, generate it automatically
        if (!string.IsNullOrEmpty(bp.EquippedWeaponTemplateId) && itemTemplates.TryGetValue(bp.EquippedWeaponTemplateId, out var weaponBp))
            npc.Equipment[EquipmentSlot.MainHand] = CreateLiveItem(weaponBp);

        return npc;
    }
}