using ConsoleMud.Core.Combat;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Fighter;

public class BashHandler : ISkillHandler
{
    public string SkillId => "bash";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.Caster.CombatTarget;
        if (target == null)
        {
            ColorConsole.WriteLine("You can only bash a foe you are fighting.");
            return;
        }

        var outcome = AttackResolver.Resolve(
            ctx.Caster, target,
            ctx.Definition.DiceNotation ?? "1d6",
            DamageType.Physical);

        if (!outcome.Hit)
        {
            Helpers.ColorConsole.WriteLine($"You lunge to bash {target.Name} but miss!", ConsoleColor.Gray);
            return;
        }

        target.Health -= outcome.Damage;
        Helpers.ColorConsole.WriteLine($"You bash {target.Name} for {outcome.Damage} damage and stagger it! " +
                          $"-> [{target.Name} HP: {Math.Max(0, target.Health)}]", ConsoleColor.Gray);

        if (target.Health <= 0)
        {
            DeathService.HandleDeath(target, ctx.World, ctx.Caster);
            return;
        }

        // Stagger: a brief stun measured in combat rounds.
        int staggerRounds = Math.Max(1, (int)ctx.Param("staggerPulses", 1));
        target.StatusEffects.Add(new Entities.StatusEffect
        {
            Name = "stagger",
            Modifier = Enums.EffectModifier.Stun,
            Polarity = Enums.EffectPolarity.Negative,
            Type = Enums.EffectType.Physical,
            TicksRemaining = staggerRounds
        });
    }
}
