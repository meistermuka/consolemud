using ConsoleMud.Core;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Cleric;

public class MajorHealHandler : ISkillHandler
{
    public string SkillId => "major_heal";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveFriendlyTarget();

        int amount = DiceRoller.Roll(ctx.Definition.DiceNotation ?? "4d8")
                     + ctx.AttributeModifier(ctx.Definition.AttributeBonus)
                     + ctx.HealScaleBonus();
        if (amount < 1) amount = 1;

        int healed = Math.Min(amount, target.MaxHealth - target.Health);
        target.Health += healed;

        string who = ReferenceEquals(target, ctx.Caster) ? "yourself" : target.Name;
        Helpers.ColorConsole.WriteLine(
            healed > 0
                ? $"A surge of divine power closes {who}'s wounds for {healed}. -> [{target.Name} HP: {target.Health}/{target.MaxHealth}]"
                : (ReferenceEquals(target, ctx.Caster) ? "You are already at full health." : $"{target.Name} is already at full health."),
            ConsoleColor.Gray);
    }
}
