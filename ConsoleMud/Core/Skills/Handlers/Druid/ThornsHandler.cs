using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Druid;

public class ThornsHandler : ISkillHandler
{
    public string SkillId => "thorns";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveFriendlyTarget();
        int duration = ctx.Definition.DurationTicks > 0 ? ctx.Definition.DurationTicks : 7;
        int magnitude = Math.Max(1, ctx.AttributeModifier("Wisdom"));

        target.StatusEffects.Add(new StatusEffect
        {
            Name = "thorns", SourceSkillId = "thorns", Modifier = EffectModifier.Thorns,
            Magnitude = magnitude, DamageType = DamageType.Physical,
            Polarity = EffectPolarity.Positive, TicksRemaining = duration
        });

        string who = ReferenceEquals(target, ctx.Caster) ? "you" : target.Name;
        Helpers.ColorConsole.WriteLine($"Sharp briars wreathe {who}; attackers will bleed for them.", ConsoleColor.Gray);
    }
}
