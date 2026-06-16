using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Thief;

public class TripHandler : ISkillHandler
{
    public string SkillId => "trip";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveNpcTarget();
        if (target == null) { Console.WriteLine("Trip what?"); return; }

        ctx.Engage(target);
        target.StatusEffects.Add(new StatusEffect { Name = "tripped", Modifier = EffectModifier.Stun, Polarity = EffectPolarity.Negative, Type = EffectType.Physical, TicksRemaining = Math.Max(1, (int)ctx.Param("stunRounds", 1)) });
        target.StatusEffects.Add(new StatusEffect { Name = "pinned", Modifier = EffectModifier.Root, Polarity = EffectPolarity.Negative, Type = EffectType.Physical, TicksRemaining = Math.Max(1, (int)ctx.Param("rootRounds", 3)) });

        Helpers.ColorConsole.WriteLine($"You sweep {target.Name}'s legs, knocking it flat!", ConsoleColor.Gray);
    }
}
