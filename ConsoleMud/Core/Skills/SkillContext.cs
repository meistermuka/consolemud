using ConsoleMud.Core.Services;
using ConsoleMud.Entities;
using ConsoleMud.Entities.Definitions;

namespace ConsoleMud.Core.Skills;

/// <summary>
/// Everything a skill handler needs to run: who cast it, the world, the skill's
/// definition (dice, parameters), and the raw target arguments.
/// </summary>
public class SkillContext
{
    public Character Caster { get; }
    public WorldState World { get; }
    public SkillDefinition Definition { get; }
    public string[] Args { get; }
    public DefinitionRegistry Definitions { get; }

    public SkillContext(Character caster, WorldState world, SkillDefinition definition, string[] args, DefinitionRegistry definitions)
    {
        Caster = caster;
        World = world;
        Definition = definition;
        Args = args ?? Array.Empty<string>();
        Definitions = definitions;
    }

    public string TargetName => string.Join(" ", Args);

    /// <summary>Reads a numeric tunable from the skill's Parameters bag.</summary>
    public double Param(string key, double fallback = 0.0) =>
        Definition.Parameters.TryGetValue(key, out var v) ? v : fallback;
}
