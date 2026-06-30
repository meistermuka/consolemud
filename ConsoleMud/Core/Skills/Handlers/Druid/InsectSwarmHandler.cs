using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Druid;

public class InsectSwarmHandler : ISkillHandler
{
    public string SkillId => "insect_swarm";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveNpcTarget();
        if (target == null) { ColorConsole.WriteLine("Swarm what?"); return; }

        ctx.Engage(target);
        int duration = ctx.Definition.DurationTicks > 0 ? ctx.Definition.DurationTicks : 4;

        target.StatusEffects.Add(new StatusEffect
        {
            Name = "insect swarm (dot)", Modifier = EffectModifier.DamageOverTime,
            Magnitude = ctx.Param("tickDamage", 3), DamageType = DamageType.Nature,
            Polarity = EffectPolarity.Negative, Type = EffectType.Generic, TicksRemaining = duration
        });
        target.StatusEffects.Add(new StatusEffect
        {
            Name = "insect swarm (blind aim)", Modifier = EffectModifier.AccuracyMod,
            Magnitude = -ctx.Param("accuracyDebuff", 30), Polarity = EffectPolarity.Negative,
            Type = EffectType.Generic, TicksRemaining = duration
        });

        Helpers.ColorConsole.WriteLine($"A cloud of biting insects engulfs {target.Name}!", ConsoleColor.Gray);
    }
}
