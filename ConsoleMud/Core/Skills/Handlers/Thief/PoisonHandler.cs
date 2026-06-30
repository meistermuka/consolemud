using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Thief;

public class PoisonHandler : ISkillHandler
{
    public string SkillId => "poison";

    public void Execute(SkillContext ctx)
    {
        if (ctx.Caster.CombatTarget != null)
        {
            ColorConsole.WriteLine("You can't carefully coat your blade mid-fight.");
            return;
        }

        // A weapon coat with charges; combat procs it on hits (see CombatSystem).
        ctx.Caster.StatusEffects.RemoveAll(e => e.Modifier == EffectModifier.WeaponCoat);
        ctx.Caster.StatusEffects.Add(new StatusEffect
        {
            Name = "poison coat", SourceSkillId = "poison", Modifier = EffectModifier.WeaponCoat,
            Magnitude = ctx.Param("tickDamage", 2), Polarity = EffectPolarity.Positive,
            Charges = (int)ctx.Param("charges", 5), TicksRemaining = -1
        });

        Helpers.ColorConsole.WriteLine("You carefully coat your blade in venom.", ConsoleColor.Gray);
    }
}
