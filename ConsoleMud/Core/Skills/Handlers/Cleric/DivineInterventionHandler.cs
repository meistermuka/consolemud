using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Cleric;

public class DivineInterventionHandler : ISkillHandler
{
    public string SkillId => "divine_intervention";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveFriendlyTarget();

        target.Health = target.MaxHealth;
        int cleansed = target.StatusEffects.RemoveAll(e => e.Polarity == EffectPolarity.Negative);

        string who = ReferenceEquals(target, ctx.Caster) ? "you" : target.Name;
        Helpers.ColorConsole.WriteLine(
            $"Your deity answers: {who} is restored to full health and cleansed of {cleansed} ailment(s)!",
            ConsoleColor.Yellow);
    }
}
