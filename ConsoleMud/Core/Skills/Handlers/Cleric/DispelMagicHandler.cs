using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Cleric;

public class DispelMagicHandler : ISkillHandler
{
    public string SkillId => "dispel_magic";

    public void Execute(SkillContext ctx)
    {
        // Strips magical/negative effects from a target (enemy buffs or an ally's curses).
        var target = ctx.ResolveNpcTarget() ?? ctx.Caster;

        int removed = target.StatusEffects.RemoveAll(e =>
            e.SourceSkillId == null || !e.SourceSkillId.StartsWith("passive:")  // never strip innate passives
                ? (e.Type == EffectType.Magic || e.Type == EffectType.Curse || e.Polarity == EffectPolarity.Negative
                   || (ReferenceEquals(target, ctx.Caster) == false && e.Polarity == EffectPolarity.Positive))
                : false);

        Helpers.ColorConsole.WriteLine(
            removed > 0 ? $"Arcane energies unravel around {target.Name} ({removed} effect(s) dispelled)."
                        : $"There is nothing to dispel on {target.Name}.",
            ConsoleColor.Gray);
    }
}
