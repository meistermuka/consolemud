using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Cleric;

public class SanctuaryHandler : ISkillHandler
{
    public string SkillId => "sanctuary";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveFriendlyTarget();
        int duration = ctx.Definition.DurationTicks > 0 ? ctx.Definition.DurationTicks : 15;

        // Modeled as a strong avoidance ward: foes struggle to land blows.
        target.StatusEffects.Add(new StatusEffect
        {
            Name = "sanctuary",
            SourceSkillId = "sanctuary",
            Modifier = EffectModifier.AvoidanceMod,
            Magnitude = ctx.Param("avoidance", 50),
            Polarity = EffectPolarity.Positive,
            TicksRemaining = duration
        });

        string who = ReferenceEquals(target, ctx.Caster) ? "you" : target.Name;
        Helpers.ColorConsole.WriteLine($"A shimmering ward of sanctuary surrounds {who}.", ConsoleColor.Gray);
    }
}
