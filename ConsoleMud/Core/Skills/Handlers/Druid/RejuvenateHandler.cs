using ConsoleMud.Core;
using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Druid;

public class RejuvenateHandler : ISkillHandler
{
    public string SkillId => "rejuvenate";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveFriendlyTarget();
        int perTick = Math.Max(1, DiceRoller.Roll(ctx.Definition.DiceNotation ?? "1d4")
                                  + ctx.AttributeModifier(ctx.Definition.AttributeBonus));
        int duration = ctx.Definition.DurationTicks > 0 ? ctx.Definition.DurationTicks : 5;

        target.StatusEffects.Add(new StatusEffect
        {
            Name = "rejuvenation", SourceSkillId = "rejuvenate", Modifier = EffectModifier.HealOverTime,
            Magnitude = perTick, Polarity = EffectPolarity.Positive, Type = EffectType.Magic, TicksRemaining = duration
        });

        string who = ReferenceEquals(target, ctx.Caster) ? "you" : target.Name;
        Helpers.ColorConsole.WriteLine($"A healing bloom settles over {who}, knitting wounds over time.", ConsoleColor.Gray);
    }
}
