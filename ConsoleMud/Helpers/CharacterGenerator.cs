using ConsoleMud.Core.Services;
using ConsoleMud.Entities;
using ConsoleMud.Entities.Definitions;
using ConsoleMud.Enums;

namespace ConsoleMud.Helpers;

public static class CharacterGenerator
{
    public static Player CreateNewPlayer(Guid startingRoomId, DefinitionRegistry definitions)
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== CHARACTER CREATION ===");
            Console.ResetColor();

            string name = CreationSteps.PromptName();
            var species = CreationSteps.PromptSpecies(definitions);
            var cls = CreationSteps.PromptClass(definitions);

            int[] rolled = CreationSteps.RollWithRerolls();
            var baseStats = CreationSteps.AssignValues(rolled);
            var finalStats = CreationSteps.ApplyModifiers(baseStats, species.Modifiers);

            var player = BuildPlayer(startingRoomId, definitions, name, species, cls, finalStats);

            ShowSheet(player, species, cls, baseStats, finalStats);

            if (CreationSteps.Confirm())
                return player;

            Console.WriteLine("\nStarting over...");
        }
    }

    private static Player BuildPlayer(
        Guid startingRoomId,
        DefinitionRegistry definitions,
        string name,
        SpeciesDefinition species,
        ClassDefinition cls,
        CreationSteps.RolledStats stats)
    {
        Enum.TryParse<Species>(species.Id, true, out var speciesEnum);
        Enum.TryParse<CharacterClass>(cls.Id, true, out var classEnum);

        int maxHp = 50 + stats.Con * 3 + cls.HpBonus;
        int maxMana = 10 + stats.Int * 2 + cls.ManaBonus;
        if (maxHp < 1) maxHp = 1;
        if (maxMana < 0) maxMana = 0;

        var player = new Player
        {
            Name = name,
            Species = speciesEnum,
            Class = classEnum,
            Level = 1,
            Experience = 0,
            Strength = stats.Str,
            Dexterity = stats.Dex,
            Constitution = stats.Con,
            Intelligence = stats.Int,
            Wisdom = stats.Wis,
            Charisma = stats.Cha,
            MaxHealth = maxHp,
            Health = maxHp,
            MaxMana = maxMana,
            Mana = maxMana,
            CurrentRoomId = startingRoomId
        };

        // Species damage matrix -> the resolver's per-character multipliers.
        foreach (var kv in species.DamageMultipliers)
            if (Enum.TryParse<DamageType>(kv.Key, true, out var dt))
                player.DamageMultipliers[dt] = kv.Value;

        // Learn every class skill unlocked at or below the starting level.
        foreach (var entry in cls.Skills.Where(e => e.Level <= player.Level))
        {
            double proficiency = definitions.Skills.TryGetValue(entry.SkillId, out var def)
                ? def.StartingProficiency
                : 1.0;
            player.KnownSkills[entry.SkillId] = proficiency;
        }

        Core.Skills.PassiveService.Refresh(player);
        return player;
    }

    private static void ShowSheet(
        Player player,
        SpeciesDefinition species,
        ClassDefinition cls,
        CreationSteps.RolledStats baseStats,
        CreationSteps.RolledStats finalStats)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n=== {player.Name}, {species.Name} {cls.Name} (Level {player.Level}) ===");
        Console.ResetColor();
        Console.WriteLine("  Attribute   base -> final");
        PrintStat("STR", baseStats.Str, finalStats.Str);
        PrintStat("DEX", baseStats.Dex, finalStats.Dex);
        PrintStat("CON", baseStats.Con, finalStats.Con);
        PrintStat("INT", baseStats.Int, finalStats.Int);
        PrintStat("WIS", baseStats.Wis, finalStats.Wis);
        PrintStat("CHA", baseStats.Cha, finalStats.Cha);
        Console.WriteLine($"  Health: {player.MaxHealth}   Mana: {player.MaxMana}");

        string skills = player.KnownSkills.Count == 0 ? "none yet" : string.Join(", ", player.KnownSkills.Keys);
        Console.WriteLine($"  Starting skills: {skills}");
    }

    private static void PrintStat(string label, int baseValue, int finalValue)
    {
        string delta = finalValue == baseValue ? "" : $"  ({finalValue - baseValue:+0;-0})";
        Console.WriteLine($"  {label}:  {baseValue,2} -> {finalValue,2}{delta}");
    }
}
