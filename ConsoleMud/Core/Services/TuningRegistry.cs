using System.Text.Json;

namespace ConsoleMud.Core.Services;

/// <summary>
/// Central store of engine-wide balance constants, loaded from
/// Definitions/tuning.json. Each entry carries its value and a description, so
/// the single file documents itself. Timing constants stay in TimeEngine.
/// </summary>
public static class TuningRegistry
{
    public class Entry
    {
        public double Value { get; set; }
        public string Desc { get; set; }
    }

    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };
    private static Dictionary<string, Entry> _entries = new(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyDictionary<string, Entry> All => _entries;

    public static void Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[TuningRegistry] tuning file not found: {filePath} (using code fallbacks)");
            return;
        }

        var parsed = JsonSerializer.Deserialize<Dictionary<string, Entry>>(File.ReadAllText(filePath), Options);
        if (parsed != null)
            _entries = new Dictionary<string, Entry>(parsed, StringComparer.OrdinalIgnoreCase);

        Console.WriteLine($"[TuningRegistry] Loaded {_entries.Count} tuning values.");
    }

    /// <summary>Reads a value; the fallback is used (and is the documented default) if the key is missing.</summary>
    public static double Get(string key, double fallback) =>
        _entries.TryGetValue(key, out var e) ? e.Value : fallback;

    public static int GetInt(string key, int fallback) => (int)Math.Round(Get(key, fallback));
}
