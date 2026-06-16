using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Thief;

public class BlindsideHandler : ISkillHandler
{
    public string SkillId => "blindside";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveNpcTarget();
        if (target == null) { Console.WriteLine("Blindside what?"); return; }

        ctx.Engage(target);
        target.StatusEffects.Add(new StatusEffect
        {
            Name = "blindsided", Modifier = EffectModifier.Stun, Polarity = EffectPolarity.Negative,
            Type = EffectType.Physical, TicksRemaining = Math.Max(1, (int)ctx.Param("stunRounds", 2))
        });

        Helpers.ColorConsole.WriteLine($"You feint and {target.Name} reels, stunned and helpless!", ConsoleColor.Gray);
    }
}
