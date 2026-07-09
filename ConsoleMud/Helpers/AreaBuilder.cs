using System.Text.Json;
using System.Text.RegularExpressions;
using ConsoleMud.Core;
using ConsoleMud.Core.Services;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Helpers;

/// <summary>
/// Interactive command-line wizard that builds an area JSON file. It assembles
/// the real AreaBlueprint/*Blueprint objects and serializes them with the same
/// System.Text.Json the loader uses, so the output cannot drift from the schema.
/// Run with: dotnet run -- build-area
/// </summary>
public static class AreaBuilder
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    private static readonly string[] Directions = { "North", "South", "East", "West", "Up", "Down" };

    // Declared (generic) slots authors choose from; physical pair slots are internal.
    private static readonly string[] DeclaredSlots =
    {
        "Head", "Mask", "Necklace", "Torso", "Gloves", "Belt", "Pants", "Shins", "Boots",
        "Ring", "Earring", "Arm", "Forearm", "OffHand"
    };

    public static void Run()
    {
        Console.WriteLine("=== Area Builder ===");
        Console.WriteLine("Build templates first, then rooms, then link exits.\n");

        var area = new AreaBlueprint
        {
            Name = Ask("Area name", "New Area"),
            Description = Ask("Area description", "")
        };
        string file = Sanitize(Ask("Output file name (no .json)", area.Name));

        BuildItems(area);
        BuildNpcs(area);
        BuildRooms(area);
        LinkExits(area);

        Review(area);

        var errors = Validate(area);
        if (errors.Count > 0)
        {
            Console.WriteLine("\nProblems found:");
            foreach (var e in errors) Console.WriteLine("  - " + e);
            if (!AskBool("Write the file anyway?", false))
            {
                Console.WriteLine("Aborted; nothing written.");
                return;
            }
        }

        Directory.CreateDirectory("Areas");
        string path = Path.Combine("Areas", file + ".json");
        File.WriteAllText(path, JsonSerializer.Serialize(area, Json));
        Console.WriteLine($"\nWrote {path}.");

        // Final safety net: load it back through the real loader.
        Console.WriteLine("Verifying by loading it back...");
        AreaLoaderService.LoadAreaFile(path, new WorldState(), new DefinitionRegistry());
    }

    // ---- sections ----

    private static void BuildItems(AreaBlueprint area)
    {
        Console.WriteLine("\n-- Item templates --");
        while (AskBool(area.ItemTemplates.Count == 0 ? "Add an item template?" : "Add another item?", area.ItemTemplates.Count == 0))
        {
            var item = new ItemBlueprint
            {
                VirtualId = Ask("  VirtualId (e.g. steel_sword)"),
                Name = Ask("  Name"),
                Description = Ask("  Description", ""),
                IsGetable = AskBool("  Can it be picked up?", true)
            };

            if (item.IsWeapon = AskBool("  Is it a weapon?", false))
            {
                item.IsEquippable = true;
                item.WeaponType = AskEnum<WeaponType>("  Weapon type");
                item.DiceNotation = AskDice("  Damage dice", "1d6");
                item.AttackVerbs = Ask("  Attack verbs (comma-separated)", "hit")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                item.TargetSlot = AskEnumDefault("  Wield slot", EquipmentSlot.MainHand);
            }

            if (item.IsArmor = AskBool("  Is it armor?", false))
            {
                item.IsEquippable = true;
                item.ArmorRating = AskInt("  Armor rating", 1);
                item.TargetSlot = AskFromArray("  Armor slot", DeclaredSlots);
                item.IsShield = AskBool("  Is it a shield (off-hand)?", false);
            }

            if (item.IsContainer = AskBool("  Is it a container?", false))
            {
                item.IsCloseable = AskBool("    Can it be closed?", false);
                if (item.IsCloseable && AskBool("    Does it start locked?", false))
                {
                    item.StartsLocked = true;
                    item.LockKeyId = Ask("    Lock key id (a key with this id opens it)");
                }
            }

            if (AskBool("  Is it a key?", false))
                item.KeyId = Ask("    Key id (matches a lock's key id)");

            item.IsLightSource = AskBool("  Is it a light source?", false);
            item.GrantsDarkvision = AskBool("  Does it grant darkvision when carried?", false);

            area.ItemTemplates.Add(item);
            Console.WriteLine($"  Added item '{item.VirtualId}'.");
        }
    }

    private static void BuildNpcs(AreaBlueprint area)
    {
        Console.WriteLine("\n-- NPC templates --");
        while (AskBool(area.NpcTemplates.Count == 0 ? "Add an NPC template?" : "Add another NPC?", area.NpcTemplates.Count == 0))
        {
            int hp = AskInt("  Max health", 20);
            var npc = new NpcBlueprint
            {
                VirtualId = Ask("  VirtualId (e.g. gray_wolf)"),
                Name = Ask("  Name"),
                Description = Ask("  Description", ""),
                MaxHealth = hp,
                Health = hp,
                Level = AskInt("  Level", 1),
                XpReward = AskInt("  XP reward (0 = auto)", 0),
                IsAggressive = AskBool("  Aggressive (attacks on sight)?", false),
                HasDarkvision = AskBool("  Has darkvision?", false),
                Archetypes = AskArchetypes()
            };

            if (area.ItemTemplates.Count > 0 && AskBool("  Give it an equipped weapon?", false))
                npc.EquippedWeaponTemplateId = PickFrom("  Weapon template", area.ItemTemplates.Select(i => i.VirtualId).ToList());

            area.NpcTemplates.Add(npc);
            Console.WriteLine($"  Added NPC '{npc.VirtualId}'.");
        }
    }

    private static void BuildRooms(AreaBlueprint area)
    {
        Console.WriteLine("\n-- Rooms --");
        int count = AskInt("How many rooms?", 1);
        for (int i = 0; i < count; i++)
        {
            Console.WriteLine($"\nRoom {i + 1} of {count}:");
            var room = new RoomBlueprint
            {
                VirtualId = Ask("  VirtualId (e.g. forest_entrance)"),
                Name = Ask("  Name"),
                Description = Ask("  Description", ""),
                IsOutside = AskBool("  Is it outdoors?", false),
                IsDark = AskBool("  Is it dark (needs light/darkvision)?", false)
            };

            // Item spawns
            while (area.ItemTemplates.Count > 0 && AskBool("  Spawn an item here?", false))
            {
                var id = PickFrom("    Item template", area.ItemTemplates.Select(t => t.VirtualId).ToList());
                if (id != null)
                    room.Spawns.Items.Add(new SpawnReference { TemplateId = id, Count = AskInt("    Count", 1) });
            }

            // NPC spawns
            while (area.NpcTemplates.Count > 0 && AskBool("  Spawn an NPC here?", false))
            {
                var id = PickFrom("    NPC template", area.NpcTemplates.Select(t => t.VirtualId).ToList());
                if (id != null)
                    room.Spawns.Npcs.Add(new SpawnReference { TemplateId = id, Count = AskInt("    Count", 1) });
            }

            area.Rooms.Add(room);
        }
    }

    private static void LinkExits(AreaBlueprint area)
    {
        if (area.Rooms.Count < 2)
            return;

        Console.WriteLine("\n-- Exits --");
        var roomIds = area.Rooms.Select(r => r.VirtualId).ToList();
        foreach (var room in area.Rooms)
        {
            while (AskBool($"Add an exit from '{room.VirtualId}'?", false))
            {
                string dir = AskFromArray("  Direction", Directions);
                var target = PickFrom("  Leads to room", roomIds.Where(id => id != room.VirtualId).ToList());
                if (target == null) continue;

                room.Exits[dir] = target;
                if (AskBool($"  Add the reciprocal {Opposite(dir).ToLower()} exit back?", true))
                {
                    var back = area.Rooms.First(r => r.VirtualId == target);
                    back.Exits[Opposite(dir)] = room.VirtualId;
                }
            }
        }
    }

    private static void Review(AreaBlueprint area)
    {
        Console.WriteLine($"\n=== {area.Name} ===");
        Console.WriteLine($"Items: {area.ItemTemplates.Count}, NPCs: {area.NpcTemplates.Count}, Rooms: {area.Rooms.Count}");
        foreach (var r in area.Rooms)
        {
            string exits = r.Exits.Count == 0 ? "none" : string.Join(", ", r.Exits.Select(e => $"{e.Key.ToLower()}->{e.Value}"));
            Console.WriteLine($"  [{r.VirtualId}] {r.Name}{(r.IsOutside ? " (outside)" : "")} | exits: {exits} " +
                              $"| items: {r.Spawns.Items.Count} | npcs: {r.Spawns.Npcs.Count}");
        }
    }

    private static List<string> Validate(AreaBlueprint area)
    {
        var errors = new List<string>();
        var roomIds = area.Rooms.Select(r => r.VirtualId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var itemIds = area.ItemTemplates.Select(i => i.VirtualId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var npcIds = area.NpcTemplates.Select(n => n.VirtualId).ToHashSet(StringComparer.OrdinalIgnoreCase);

        void Dupes(IEnumerable<string> ids, string what)
        {
            foreach (var g in ids.GroupBy(x => x, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
                errors.Add($"Duplicate {what} VirtualId: '{g.Key}'");
        }
        Dupes(area.Rooms.Select(r => r.VirtualId), "room");
        Dupes(area.ItemTemplates.Select(i => i.VirtualId), "item");
        Dupes(area.NpcTemplates.Select(n => n.VirtualId), "NPC");

        foreach (var r in area.Rooms)
        {
            foreach (var e in r.Exits.Where(e => !roomIds.Contains(e.Value)))
                errors.Add($"Room '{r.VirtualId}' exit {e.Key} points to unknown room '{e.Value}'");
            foreach (var s in r.Spawns.Items.Where(s => !itemIds.Contains(s.TemplateId)))
                errors.Add($"Room '{r.VirtualId}' spawns unknown item '{s.TemplateId}'");
            foreach (var s in r.Spawns.Npcs.Where(s => !npcIds.Contains(s.TemplateId)))
                errors.Add($"Room '{r.VirtualId}' spawns unknown NPC '{s.TemplateId}'");
        }
        foreach (var n in area.NpcTemplates.Where(n => !string.IsNullOrEmpty(n.EquippedWeaponTemplateId) && !itemIds.Contains(n.EquippedWeaponTemplateId)))
            errors.Add($"NPC '{n.VirtualId}' equips unknown item '{n.EquippedWeaponTemplateId}'");

        return errors;
    }

    // ---- prompt helpers ----

    private static string Ask(string prompt, string fallback = null)
    {
        Console.Write(fallback != null ? $"{prompt} [{fallback}]: " : $"{prompt}: ");
        string input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? (fallback ?? "") : input.Trim();
    }

    private static int AskInt(string prompt, int fallback)
    {
        while (true)
        {
            string s = Ask(prompt, fallback.ToString());
            if (int.TryParse(s, out int n)) return n;
            Console.WriteLine("  Enter a whole number.");
        }
    }

    private static bool AskBool(string prompt, bool fallback)
    {
        string s = Ask(prompt + " (y/n)", fallback ? "y" : "n").ToLower();
        return s.StartsWith("y");
    }

    private static string AskDice(string prompt, string fallback)
    {
        while (true)
        {
            string s = Ask(prompt + " (NdM)", fallback);
            if (Regex.IsMatch(s, @"^\d+d\d+$")) return s;
            Console.WriteLine("  Use dice notation like 2d6.");
        }
    }

    private static string AskFromArray(string prompt, string[] options)
    {
        for (int i = 0; i < options.Length; i++) Console.WriteLine($"    {i + 1}) {options[i]}");
        return options[AskInt(prompt + " (number)", 1) is var n && n >= 1 && n <= options.Length ? n - 1 : 0];
    }

    private static string AskEnum<TEnum>(string prompt) where TEnum : struct, Enum =>
        AskFromArray(prompt, Enum.GetNames(typeof(TEnum)));

    private static string AskEnumDefault<TEnum>(string prompt, TEnum fallback) where TEnum : struct, Enum
    {
        Console.WriteLine($"  {prompt} default: {fallback}");
        return AskBool($"  Use a different slot than {fallback}?", false) ? AskEnum<TEnum>(prompt) : fallback.ToString();
    }

    private static string[] AskArchetypes()
    {
        if (!AskBool("  Set archetypes (Animal/Undead/Fiend/...)?", false))
            return Array.Empty<string>();
        var chosen = new List<string>();
        var names = Enum.GetNames(typeof(Archetype));
        for (int i = 0; i < names.Length; i++) Console.WriteLine($"    {i + 1}) {names[i]}");
        Console.WriteLine("  Enter numbers separated by commas (blank for none).");
        string line = Console.ReadLine() ?? "";
        foreach (var part in line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            if (int.TryParse(part, out int n) && n >= 1 && n <= names.Length) chosen.Add(names[n - 1]);
        return chosen.Distinct().ToArray();
    }

    private static string PickFrom(string prompt, List<string> options)
    {
        if (options.Count == 0) return null;
        for (int i = 0; i < options.Count; i++) Console.WriteLine($"    {i + 1}) {options[i]}");
        Console.WriteLine("    0) (cancel)");
        int n = AskInt(prompt + " (number)", 1);
        return n >= 1 && n <= options.Count ? options[n - 1] : null;
    }

    private static string Opposite(string dir) => dir switch
    {
        "North" => "South", "South" => "North",
        "East" => "West", "West" => "East",
        "Up" => "Down", "Down" => "Up",
        _ => "South"
    };

    private static string Sanitize(string name)
    {
        var clean = new string((name ?? "").Select(c => char.IsLetterOrDigit(c) ? char.ToLower(c) : '_').ToArray());
        return string.IsNullOrWhiteSpace(clean) ? "new_area" : clean;
    }
}
