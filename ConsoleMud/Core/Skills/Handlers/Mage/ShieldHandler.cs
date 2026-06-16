using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Mage;

public class ShieldHandler : ISkillHandler
{
    public string SkillId => "shield";

    public void Execute(SkillContext ctx)
    {
        int duration = ctx.Definition.DurationTicks > 0 ? ctx.Definition.DurationTicks : 10;
        ctx.Caster.StatusEffects.Add(new StatusEffect
        {
            Name = "shield",
            SourceSkillId = "shield",
            Modifier = EffectModifier.ArmorMod,
            Magnitude = ctx.Param("armorBonus", 5),
            Polarity = EffectPolarity.Positive,
            TicksRemaining = duration
        });
        Helpers.ColorConsole.WriteLine("A barrier of force shimmers into place around you.", ConsoleColor.Gray);
    }
}
