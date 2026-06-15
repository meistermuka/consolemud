using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Druid;

public class EntangleHandler : ISkillHandler
{
    public string SkillId => "entangle";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveNpcTarget();
        if (target == null)
        {
            Console.WriteLine("Entangle what?");
            return;
        }

        ctx.Engage(target);

        int rounds = Math.Max(1, (int)ctx.Param("rootRounds", 2));
        target.StatusEffects.Add(new StatusEffect
        {
            Name = "entangling roots",
            Modifier = EffectModifier.Root,
            Polarity = EffectPolarity.Negative,
            Type = EffectType.Magic,
            DamageType = DamageType.Nature,
            TicksRemaining = rounds
        });

        Helpers.ColorConsole.WriteLine(
            $"Roots erupt and bind {target.Name} in place for {rounds} round(s)!", ConsoleColor.Gray);
    }
}
