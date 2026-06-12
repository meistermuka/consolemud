using System.Text.Json;
using ConsoleMud.Entities.Definitions;

namespace ConsoleMud.Core.Services;

/// <summary>
/// Loads species, skill, and class definitions from JSON once at startup and
/// exposes them by id. Mirrors the AreaLoaderService blueprint pattern.
/// </summary>
public class DefinitionRegistry
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public Dictionary<string, SpeciesDefinition> Species { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, SkillDefinition> Skills { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, ClassDefinition> Classes { get; } = new(StringComparer.OrdinalIgnoreCase);

    public void LoadAll(string definitionsFolder)
    {
        LoadInto(Path.Combine(definitionsFolder, "species.json"), Species, s => s.Id, "species");
        LoadInto(Path.Combine(definitionsFolder, "skills.json"), Skills, s => s.Id, "skills");
        LoadInto(Path.Combine(definitionsFolder, "classes.json"), Classes, c => c.Id, "classes");

        ValidateClassSkillReferences();
    }

    private void LoadInto<T>(string filePath, Dictionary<string, T> target, Func<T, string> keySelector, string label)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[DefinitionRegistry] {label} file not found: {filePath}");
            return;
        }

        var json = File.ReadAllText(filePath);
        var entries = JsonSerializer.Deserialize<List<T>>(json, Options);
        if (entries == null)
        {
            Console.WriteLine($"[DefinitionRegistry] Failed to parse {label}: {filePath}");
            return;
        }

        foreach (var entry in entries)
        {
            var key = keySelector(entry);
            if (string.IsNullOrWhiteSpace(key))
            {
                Console.WriteLine($"[DefinitionRegistry] Skipped a {label} entry with no id.");
                continue;
            }
            target[key] = entry;
        }

        Console.WriteLine($"[DefinitionRegistry] Loaded {target.Count} {label}.");
    }

    private void ValidateClassSkillReferences()
    {
        foreach (var cls in Classes.Values)
        {
            foreach (var entry in cls.Skills)
            {
                if (!Skills.ContainsKey(entry.SkillId))
                    Console.WriteLine($"[Warning] Class '{cls.Id}' references unknown skill '{entry.SkillId}'.");
            }
        }
    }
}
