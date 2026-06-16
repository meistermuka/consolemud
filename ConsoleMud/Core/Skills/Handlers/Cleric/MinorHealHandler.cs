using ConsoleMud.Core;

namespace ConsoleMud.Core.Skills.Handlers.Cleric;

public class MinorHealHandler : ISkillHandler
{
    public string SkillId => "minor_heal";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveFriendlyTarget();

        int amount = DiceRoller.Roll(ctx.Definition.DiceNotation ?? "1d8")
                     + ctx.AttributeModifier(ctx.Definition.AttributeBonus)
                     + ctx.HealScaleBonus();
        if (amount < 1) amount = 1;

        int healed = Math.Min(amount, target.MaxHealth - target.Health);
        target.Health += healed;

        bool self = ReferenceEquals(target, ctx.Caster);
        string who = self ? "yourself" : target.Name;
        if (healed > 0)
            Helpers.ColorConsole.WriteLine(
                $"Divine light mends {who} for {healed} health. -> [{target.Name} HP: {target.Health}/{target.MaxHealth}]",
                ConsoleColor.Gray);
        else
            Helpers.ColorConsole.WriteLine(
                self ? "You are already at full health." : $"{target.Name} is already at full health.",
                ConsoleColor.Gray);
    }
}
