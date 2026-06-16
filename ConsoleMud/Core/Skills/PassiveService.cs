using ConsoleMud.Core.Services;
using ConsoleMud.Core.Skills.Handlers.Cleric;
using ConsoleMud.Core.Skills.Handlers.Mage;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills;

/// <summary>
/// Applies "static" passive skills (permanent bonuses) as long-lived StatusEffects
/// tagged by the skill id. Recomputed whenever skills or attributes change
/// (creation, level-up, load) so attribute-scaled passives stay current.
///
/// Trigger-based passives (parry-as-avoidance is handled here; on-event passives
/// like counterstrike are wired into combat separately).
/// </summary>
public static class PassiveService
{
    private const string Tag = "passive:";
    private static DefinitionRegistry _definitions;
    private static readonly TriggerBus _bus = new();

    public static void Initialize(DefinitionRegistry definitions)
    {
        _definitions = definitions;

        // Event-triggered passives subscribe to the bus once.
        _bus.Register(new RetributionAuraPassive());
        _bus.Register(new HolyFervorPassive());
        _bus.Register(new ChannelingFlowPassive());
    }

    /// <summary>Fires a passive trigger for an owner (only runs if they know the skill).</summary>
    public static void Fire(SkillTrigger trigger, Character owner, Character other, WorldState world, object payload = null)
    {
        if (owner == null)
            return;
        _bus.Fire(trigger, new TriggerContext { Owner = owner, Other = other, World = world, Payload = payload });
    }

    public static void Refresh(Character c)
    {
        // Clear previously-applied passive effects, then reapply from current skills.
        c.StatusEffects.RemoveAll(e => e.SourceSkillId != null && e.SourceSkillId.StartsWith(Tag));

        foreach (var (skillId, proficiency) in c.KnownSkills)
            ApplyStaticPassive(c, skillId, proficiency);
    }

    private static double Param(string skillId, string key, double fallback)
    {
        if (_definitions != null && _definitions.Skills.TryGetValue(skillId, out var def)
            && def.Parameters.TryGetValue(key, out var v))
            return v;
        return fallback;
    }

    private static void Add(Character c, string skillId, EffectModifier mod, double magnitude, DamageType type = DamageType.Physical)
    {
        c.StatusEffects.Add(new StatusEffect
        {
            Name = skillId,
            SourceSkillId = Tag + skillId,
            Modifier = mod,
            Magnitude = magnitude,
            DamageType = type,
            Polarity = EffectPolarity.Positive,
            TicksRemaining = -1 // permanent until refreshed
        });
    }

    private static void ApplyStaticPassive(Character c, string skillId, double proficiency)
    {
        switch (skillId.ToLower())
        {
            // --- Fighter ---
            case "armor_optimization":
                Add(c, skillId, EffectModifier.ArmorMod, Param(skillId, "armorBonus", 2));
                break;
            case "parry":
                // Avoidance chance scales with proficiency (capped to a sane band).
                Add(c, skillId, EffectModifier.AvoidanceMod, Math.Min(40.0, proficiency * 0.4));
                break;
            case "indomitable_will":
                Add(c, skillId, EffectModifier.ImmunityOverride, 1, DamageType.Fear);
                double red = Param(skillId, "magicPsychicReductionPct", 25);
                Add(c, skillId, EffectModifier.FlatDamageReduction, red, DamageType.Magic);
                Add(c, skillId, EffectModifier.FlatDamageReduction, red, DamageType.Psychic);
                break;

            // --- Cleric ---
            case "divine_armor":
                Add(c, skillId, EffectModifier.ArmorMod, Math.Floor(c.Wisdom * Param(skillId, "wisFraction", 0.15)));
                break;
            case "soul_ward":
                Add(c, skillId, EffectModifier.ImmunityOverride, 1, DamageType.Psychic);
                break;

            // --- Mage ---
            case "elemental_mastery":
                if (c is Player p && p.Specialization is { } spec)
                    Add(c, skillId, EffectModifier.ImmunityOverride, 1, spec);
                break;
        }
    }
}
