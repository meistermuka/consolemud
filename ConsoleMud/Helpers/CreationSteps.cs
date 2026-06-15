using ConsoleMud.Core;
using ConsoleMud.Core.Services;
using ConsoleMud.Entities.Definitions;

namespace ConsoleMud.Helpers;

/// <summary>
/// The individual prompts of character creation, kept separate so the order is
/// easy to reshuffle and each step re-prompts on bad input.
/// </summary>
public static class CreationSteps
{
    public record RolledStats(int Str, int Dex, int Con, int Int, int Wis, int Cha);

    private const int MaxRerolls = 3;
    private static readonly string[] AttributeOrder = { "STR", "DEX", "CON", "INT", "WIS", "CHA" };

    public static string PromptName()
    {
        while (true)
        {
            Console.Write("Enter your character's name: ");
            string name = Console.ReadLine();
            name = string.IsNullOrWhiteSpace(name) ? "Hero" : name.Trim();

            if (Core.Services.SaveService.Exists(name))
            {
                Console.WriteLine($"A character named '{name}' already exists. Choose a different name.");
                continue;
            }
            return name;
        }
    }

    public static SpeciesDefinition PromptSpecies(DefinitionRegistry registry)
    {
        var species = registry.Species.Values.ToList();

        Console.WriteLine("\nChoose your species:");
        for (int i = 0; i < species.Count; i++)
        {
            var s = species[i];
            Console.WriteLine($"  {i + 1}) {s.Name,-10} {ModifierLine(s.Modifiers)}{ResistLine(s)}");
        }

        return species[PromptIndex(species.Count)];
    }

    public static ClassDefinition PromptClass(DefinitionRegistry registry)
    {
        var classes = registry.Classes.Values.ToList();

        Console.WriteLine("\nChoose your class:");
        for (int i = 0; i < classes.Count; i++)
        {
            var c = classes[i];
            string firstSkill = c.Skills.OrderBy(s => s.Level).FirstOrDefault()?.SkillId ?? "none";
            Console.WriteLine($"  {i + 1}) {c.Name,-8} (HP {c.HpBonus:+0;-0;+0}, Mana {c.ManaBonus:+0;-0;+0}, first skill: {firstSkill})");
        }

        return classes[PromptIndex(classes.Count)];
    }

    /// <summary>Rolls six 3d6 values, offering up to 3 rerolls of the whole set.</summary>
    public static int[] RollWithRerolls()
    {
        int[] values = RollSix();
        int rerollsLeft = MaxRerolls;

        while (true)
        {
            Console.WriteLine($"\nRolled (3d6 x6): {string.Join(", ", values)}");
            if (rerollsLeft == 0)
            {
                Console.WriteLine("No rerolls left. Keeping this set.");
                return values;
            }

            Console.Write($"Keep this set, or reroll? ({rerollsLeft} reroll(s) left) [keep/reroll]: ");
            string choice = (Console.ReadLine() ?? "keep").Trim().ToLower();
            if (choice is "reroll" or "r")
            {
                values = RollSix();
                rerollsLeft--;
            }
            else
            {
                return values;
            }
        }
    }

    /// <summary>Lets the player place each rolled value into a chosen attribute.</summary>
    public static RolledStats AssignValues(int[] rolled)
    {
        var pool = rolled.ToList();
        var assigned = new int[AttributeOrder.Length];

        for (int a = 0; a < AttributeOrder.Length; a++)
        {
            Console.WriteLine($"\nRemaining values: {string.Join(", ", pool.Select((v, i) => $"{i + 1}){v}"))}");
            Console.Write($"Assign which value to {AttributeOrder[a]}? ");
            int pick = PromptIndex(pool.Count);
            assigned[a] = pool[pick];
            pool.RemoveAt(pick);
        }

        return new RolledStats(assigned[0], assigned[1], assigned[2], assigned[3], assigned[4], assigned[5]);
    }

    public static RolledStats ApplyModifiers(RolledStats baseStats, AttributeModifiers mod) =>
        new(
            baseStats.Str + mod.Str,
            baseStats.Dex + mod.Dex,
            baseStats.Con + mod.Con,
            baseStats.Int + mod.Int,
            baseStats.Wis + mod.Wis,
            baseStats.Cha + mod.Cha);

    public static bool Confirm()
    {
        Console.Write("\nKeep this character? [yes/no]: ");
        string choice = (Console.ReadLine() ?? "yes").Trim().ToLower();
        return choice is "yes" or "y" or "";
    }

    // ---- helpers ----

    private static int[] RollSix() =>
        Enumerable.Range(0, 6).Select(_ => DiceRoller.Roll("3d6")).ToArray();

    /// <summary>Prompts for a 1..count choice and returns a zero-based index.</summary>
    private static int PromptIndex(int count)
    {
        while (true)
        {
            Console.Write($"Choose (1-{count}): ");
            string input = Console.ReadLine();
            if (input == null) // end of input: default to the first option
                return 0;
            if (int.TryParse(input, out int n) && n >= 1 && n <= count)
                return n - 1;
            Console.WriteLine("Invalid choice.");
        }
    }

    private static string ModifierLine(AttributeModifiers m)
    {
        var parts = new List<string>();
        void Add(string label, int v) { if (v != 0) parts.Add($"{label} {v:+0;-0}"); }
        Add("STR", m.Str); Add("DEX", m.Dex); Add("CON", m.Con);
        Add("INT", m.Int); Add("WIS", m.Wis); Add("CHA", m.Cha);
        return parts.Count == 0 ? "(no modifiers)" : string.Join(" ", parts);
    }

    private static string ResistLine(SpeciesDefinition s)
    {
        if (s.DamageMultipliers.Count == 0)
            return "";

        var parts = s.DamageMultipliers.Select(kv =>
        {
            string word = kv.Value == 0 ? "immune" : kv.Value < 1 ? "resist" : "vulnerable";
            return $"{word} {kv.Key}";
        });
        return "  [" + string.Join(", ", parts) + "]";
    }
}
