using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Ranger;

public class SpiritOfThePackHandler : ISkillHandler
{
    public string SkillId => "spirit_of_the_pack";

    public void Execute(SkillContext ctx)
    {
        int duration = ctx.Definition.DurationTicks > 0 ? ctx.Definition.DurationTicks : 4;
        double rate = ctx.Param("attackRate", 1);

        void Haste(Character c) => c.StatusEffects.Add(new StatusEffect
        {
            Name = "pack speed", SourceSkillId = "spirit_of_the_pack", Modifier = EffectModifier.AttackRateMod,
            Magnitude = rate, Polarity = EffectPolarity.Positive, TicksRemaining = duration
        });

        Haste(ctx.Caster);
        if (ctx.Caster is Player { Pet: { } pet } && pet.Health > 0)
            Haste(pet);

        Helpers.ColorConsole.WriteLine("You loose a primal war cry; you and your companion move like lightning!", ConsoleColor.Gray);
    }
}
