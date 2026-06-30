using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Mage;

public class HasteHandler : ISkillHandler
{
    public string SkillId => "haste";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveFriendlyTarget();
        int duration = ctx.Definition.DurationTicks > 0 ? ctx.Definition.DurationTicks : 10;

        target.StatusEffects.Add(new StatusEffect
        {
            Name = "haste",
            SourceSkillId = "haste",
            Modifier = EffectModifier.AttackRateMod,
            Magnitude = ctx.Param("attackRate", 1),
            Polarity = EffectPolarity.Positive,
            TicksRemaining = duration
        });

        string who = ReferenceEquals(target, ctx.Caster) ? "You blur" : $"{target.Name} blurs";
        Helpers.ColorConsole.WriteLine($"{who} with supernatural speed!", ConsoleColor.Gray);
    }
}
