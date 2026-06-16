using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Cleric;

public class BlessHandler : ISkillHandler
{
    public string SkillId => "bless";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveFriendlyTarget();
        int duration = ctx.Definition.DurationTicks > 0 ? ctx.Definition.DurationTicks : 15;

        target.StatusEffects.Add(new StatusEffect
        {
            Name = "bless",
            SourceSkillId = "bless",
            Modifier = EffectModifier.AccuracyMod,
            Magnitude = ctx.Param("accuracyBonus", 10),
            Polarity = EffectPolarity.Positive,
            TicksRemaining = duration
        });

        string who = ReferenceEquals(target, ctx.Caster) ? "your" : $"{target.Name}'s";
        Helpers.ColorConsole.WriteLine($"Divine favor steadies {who} aim.", ConsoleColor.Gray);
    }
}
