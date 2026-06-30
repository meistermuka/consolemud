using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Cleric;

public class CurePoisonHandler : ISkillHandler
{
    public string SkillId => "cure_poison";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveFriendlyTarget();

        int removed = target.StatusEffects.RemoveAll(e =>
            e.Type == EffectType.Poison ||
            (e.Modifier == EffectModifier.DamageOverTime && e.Polarity == EffectPolarity.Negative));

        string who = ReferenceEquals(target, ctx.Caster) ? "you" : target.Name;
        Helpers.ColorConsole.WriteLine(
            removed > 0 ? $"You purge {removed} affliction(s) from {who}." : $"{who} carries no poison to cure.",
            ConsoleColor.Gray);
    }
}
