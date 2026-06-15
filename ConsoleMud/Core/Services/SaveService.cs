using System.Text.Json;
using ConsoleMud.Entities;

namespace ConsoleMud.Core.Services;

/// <summary>
/// Reads and writes player saves as plain JSON under the Saves/ folder, one
/// file per character. Location is stored by room VirtualId; transient state
/// (combat target, cooldowns, status effects) is dropped.
/// </summary>
public static class SaveService
{
    private const string SaveFolder = "Saves";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static bool Exists(string name) => File.Exists(PathFor(name));

    public static void Save(Player player, WorldState world)
    {
        Directory.CreateDirectory(SaveFolder);

        string roomVirtualId = world.Rooms.TryGetValue(player.CurrentRoomId, out var room)
            ? room.VirtualId
            : null;

        var save = new PlayerSave
        {
            Name = player.Name,
            Species = player.Species,
            Class = player.Class,
            Level = player.Level,
            Experience = player.Experience,
            Strength = player.Strength,
            Dexterity = player.Dexterity,
            Constitution = player.Constitution,
            Intelligence = player.Intelligence,
            Wisdom = player.Wisdom,
            Charisma = player.Charisma,
            Health = player.Health,
            MaxHealth = player.MaxHealth,
            Mana = player.Mana,
            MaxMana = player.MaxMana,
            Specialization = player.Specialization,
            KnownSkills = new Dictionary<string, double>(player.KnownSkills),
            DamageMultipliers = new Dictionary<Enums.DamageType, double>(player.DamageMultipliers),
            Inventory = player.Inventory,
            Equipment = player.Equipment,
            CurrentRoomVirtualId = roomVirtualId
        };

        File.WriteAllText(PathFor(player.Name), JsonSerializer.Serialize(save, Options));
    }

    public static bool TryLoad(string name, WorldState world, out Player player)
    {
        player = null;
        if (!Exists(name))
            return false;

        var save = JsonSerializer.Deserialize<PlayerSave>(File.ReadAllText(PathFor(name)), Options);
        if (save == null)
            return false;

        // Resolve the saved room; fall back to the safe room, then any room.
        Guid roomId;
        if (world.TryGetRoomByVirtualId(save.CurrentRoomVirtualId, out var room))
            roomId = room.Id;
        else if (world.SafeRoomId is { } safe)
            roomId = safe;
        else
            roomId = world.Rooms.Keys.First();

        player = new Player
        {
            Name = save.Name,
            Species = save.Species,
            Class = save.Class,
            Level = save.Level,
            Experience = save.Experience,
            Strength = save.Strength,
            Dexterity = save.Dexterity,
            Constitution = save.Constitution,
            Intelligence = save.Intelligence,
            Wisdom = save.Wisdom,
            Charisma = save.Charisma,
            Health = save.Health,
            MaxHealth = save.MaxHealth,
            Mana = save.Mana,
            MaxMana = save.MaxMana,
            Specialization = save.Specialization,
            KnownSkills = save.KnownSkills ?? new Dictionary<string, double>(),
            DamageMultipliers = save.DamageMultipliers ?? new Dictionary<Enums.DamageType, double>(),
            Inventory = save.Inventory ?? new List<Item>(),
            Equipment = save.Equipment ?? new Dictionary<Enums.EquipmentSlot, Item>(),
            CurrentRoomId = roomId
        };

        return true;
    }

    private static string PathFor(string name) => Path.Combine(SaveFolder, $"{Sanitize(name)}.json");

    private static string Sanitize(string name)
    {
        var clean = new string((name ?? "").Where(char.IsLetterOrDigit).ToArray());
        return string.IsNullOrEmpty(clean) ? "hero" : clean.ToLower();
    }
}
