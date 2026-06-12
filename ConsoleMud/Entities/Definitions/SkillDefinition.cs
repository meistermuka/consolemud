namespace ConsoleMud.Entities.Definitions;

public class SkillDefinition
{
    public string Id { get; set; }            // unique key; binds to the code effect handler
    public string Name { get; set; }
    public string Description { get; set; }

    public string Kind { get; set; }          // "Active" | "Passive"
    public bool IsSpell { get; set; }         // true only for magical skills; gates ManaCost
    public int ManaCost { get; set; }         // 0 unless IsSpell
    public int CooldownSeconds { get; set; }  // 0 for no cooldown
    public int DurationTicks { get; set; }    // 0 if instantaneous

    public string DamageType { get; set; }    // "Physical" default
    public string DiceNotation { get; set; }  // optional, e.g. "1d4", "10d10"
    public string AttributeBonus { get; set; }// optional, e.g. "Strength"

    public double StartingProficiency { get; set; } = 1.0;
    public string[] Tags { get; set; } = { };

    // Free-form numeric tunables the handler reads (multipliers, chances, thresholds, charges).
    public Dictionary<string, double> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
