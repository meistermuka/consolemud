using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Common;

/// <summary>Grants a target darkvision for a long duration (Mage/Cleric spell).</summary>
public class DarkvisionHandler : ISkillHandler
{
    public string SkillId => "darkvision";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveFriendlyTarget();
        int duration = ctx.Definition.DurationTicks > 0 ? ctx.Definition.DurationTicks : 40;

        // Refresh: drop any existing darkvision effect first.
        target.StatusEffects.RemoveAll(e => e.Modifier == EffectModifier.Darkvision);
        target.StatusEffects.Add(new StatusEffect
        {
            Name = "darkvision", SourceSkillId = "darkvision", Modifier = EffectModifier.Darkvision,
            Polarity = EffectPolarity.Positive, TicksRemaining = duration
        });

        string who = ReferenceEquals(target, ctx.Caster) ? "Your eyes" : $"{target.Name}'s eyes";
        Helpers.ColorConsole.WriteLine($"{who} adjust to pierce the dark.", ConsoleColor.Gray);
    }
}
