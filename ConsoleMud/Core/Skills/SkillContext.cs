using ConsoleMud.Core.Services;
using ConsoleMud.Entities;
using ConsoleMud.Entities.Definitions;
using ConsoleMud.Helpers;

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

    /// <summary>
    /// The hostile target: a named NPC in the room, or the current combat target
    /// when no name is given.
    /// </summary>
    public NonPlayerCharacter ResolveNpcTarget()
    {
        if (Args.Length == 0)
            return Caster.CombatTarget as NonPlayerCharacter;

        if (!World.Rooms.TryGetValue(Caster.CurrentRoomId, out var room))
            return null;

        // Can't pick out a target you can't see.
        if (!Caster.CanSee(room))
            return null;

        var (index, keyword) = KeywordParser.ExtractIndex(string.Join(" ", Args));
        int matches = 0;
        foreach (var npc in room.Characters.OfType<NonPlayerCharacter>())
            if (npc.MatchesKeyword(keyword) && ++matches == index)
                return npc;
        return null;
    }

    /// <summary>The friendly target for buffs/heals: a named ally, else the caster.</summary>
    public Character ResolveFriendlyTarget()
    {
        if (Args.Length == 0)
            return Caster;

        if (World.Rooms.TryGetValue(Caster.CurrentRoomId, out var room))
        {
            var (index, keyword) = KeywordParser.ExtractIndex(string.Join(" ", Args));
            int matches = 0;
            foreach (var npc in room.Characters.OfType<NonPlayerCharacter>())
                if (npc.MatchesKeyword(keyword) && ++matches == index)
                    return npc;
        }
        return Caster;
    }

    /// <summary>Engages a target in combat (used by offensive spells/skills).</summary>
    public void Engage(Character target)
    {
        Caster.BreakHidden();
        Caster.CombatTarget = target;
        if (target.CombatTarget == null)
            target.CombatTarget = Caster;
    }

    /// <summary>Extra healing from the divine_grace passive (an extra Wisdom modifier).</summary>
    public int HealScaleBonus() =>
        Caster.KnownSkills.ContainsKey("divine_grace") ? Math.Max(0, AttributeModifier("Wisdom")) : 0;

    /// <summary>Extra spell damage from the sage_insight passive (an Intelligence modifier).</summary>
    public int SpellPowerBonus() =>
        Caster.KnownSkills.ContainsKey("sage_insight") ? Math.Max(0, AttributeModifier("Intelligence")) : 0;

    /// <summary>Standard tabletop attribute modifier for an attribute name.</summary>
    public int AttributeModifier(string attribute)
    {
        int score = (attribute ?? "").Trim().ToLower() switch
        {
            "strength" or "str" => Caster.Strength,
            "dexterity" or "dex" => Caster.Dexterity,
            "constitution" or "con" => Caster.Constitution,
            "intelligence" or "int" => Caster.Intelligence,
            "wisdom" or "wis" => Caster.Wisdom,
            "charisma" or "cha" => Caster.Charisma,
            _ => 10
        };
        return (score - 10) / 2;
    }
}
