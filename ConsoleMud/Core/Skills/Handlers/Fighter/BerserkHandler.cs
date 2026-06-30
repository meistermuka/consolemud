using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Fighter;

public class BerserkHandler : ISkillHandler
{
    public string SkillId => "berserk";

    public void Execute(SkillContext ctx)
    {
        int duration = ctx.Definition.DurationTicks > 0 ? ctx.Definition.DurationTicks : 3;
        double bonus = ctx.Param("damageBonusPct", 50);
        int currentArmor = ctx.Caster.TotalArmourRating;

        void Buff(EffectModifier mod, double mag, DamageType type = DamageType.Physical) =>
            ctx.Caster.StatusEffects.Add(new StatusEffect
            {
                Name = "berserk",
                SourceSkillId = "berserk",
                Modifier = mod,
                Magnitude = mag,
                DamageType = type,
                Polarity = EffectPolarity.Positive,
                TicksRemaining = duration
            });

        Buff(EffectModifier.DamageDealtMod, bonus);
        Buff(EffectModifier.ImmunityOverride, 1, DamageType.Charm);
        Buff(EffectModifier.ImmunityOverride, 1, DamageType.Fear);
        Buff(EffectModifier.ArmorMod, -currentArmor); // drops armor to zero for the duration

        Helpers.ColorConsole.WriteLine(
            "You fly into a berserk rage! Your blows hit harder, but you abandon all defense.", ConsoleColor.Red);
    }
}
