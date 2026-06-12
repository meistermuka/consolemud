namespace ConsoleMud.Entities.Definitions;

public class SpeciesDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Flat attribute modifiers applied after the player assigns rolled values.
    public AttributeModifiers Modifiers { get; set; } = new();

    // DamageType name -> multiplier (0 immune, 0.5 resist, 2 vulnerable).
    // Anything unlisted defaults to 1.0.
    public Dictionary<string, double> DamageMultipliers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class AttributeModifiers
{
    public int Str { get; set; }
    public int Dex { get; set; }
    public int Con { get; set; }
    public int Int { get; set; }
    public int Wis { get; set; }
    public int Cha { get; set; }
}
