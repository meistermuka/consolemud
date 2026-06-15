using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Services;

/// <summary>
/// Experience and level progression. Initialized once with the definition
/// registry so death/combat can award XP without threading it everywhere.
/// </summary>
public static class LevelingService
{
    public const int MaxLevel = 101;

    private static DefinitionRegistry _definitions;

    public static void Initialize(DefinitionRegistry definitions) => _definitions = definitions;

    /// <summary>XP required to advance from the given level to the next.</summary>
    public static long XpForNextLevel(int level) => 100L * level;

    public static void AwardXp(Player player, long amount)
    {
        if (amount <= 0 || player.Level >= MaxLevel)
            return;

        player.Experience += amount;
        ColorConsole.WriteLine($"You gain {amount} experience.", ConsoleColor.DarkYellow);

        while (player.Level < MaxLevel && player.Experience >= XpForNextLevel(player.Level))
        {
            player.Experience -= XpForNextLevel(player.Level);
            LevelUp(player);
        }
    }

    private static void LevelUp(Player player)
    {
        player.Level++;

        int hpGain = 8;
        int manaGain = 3;
        if (_definitions != null && _definitions.Classes.TryGetValue(player.Class.ToString(), out var cls))
        {
            hpGain = cls.HpPerLevel;
            manaGain = cls.ManaPerLevel;
        }

        hpGain = Math.Max(1, hpGain + Modifier(player.Constitution));
        manaGain = Math.Max(0, manaGain + Modifier(player.Intelligence));

        player.MaxHealth += hpGain;
        player.MaxMana += manaGain;

        // Level-up fully restores vitals.
        player.Health = player.MaxHealth;
        player.Mana = player.MaxMana;

        ColorConsole.WriteLine(
            $"\n*** You advance to level {player.Level}! (+{hpGain} HP, +{manaGain} mana) ***",
            ConsoleColor.Yellow);

        LearnNewSkills(player);
    }

    private static void LearnNewSkills(Player player)
    {
        if (_definitions == null || !_definitions.Classes.TryGetValue(player.Class.ToString(), out var cls))
            return;

        foreach (var entry in cls.Skills.Where(e => e.Level <= player.Level))
        {
            if (player.KnownSkills.ContainsKey(entry.SkillId))
                continue;

            double startProf = _definitions.Skills.TryGetValue(entry.SkillId, out var def)
                ? def.StartingProficiency
                : 1.0;
            player.KnownSkills[entry.SkillId] = startProf;

            string name = def?.Name ?? entry.SkillId;
            ColorConsole.WriteLine($"You have learned {name}!", ConsoleColor.Green);
        }
    }

    private static int Modifier(int attribute) => (attribute - 10) / 2;
}
