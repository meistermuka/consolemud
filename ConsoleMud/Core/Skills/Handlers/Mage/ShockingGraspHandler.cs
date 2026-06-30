using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Mage;

public class ShockingGraspHandler : ISkillHandler
{
    public string SkillId => "shocking_grasp";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveNpcTarget();
        if (target == null) { ColorConsole.WriteLine("Shock what?"); return; }

        ctx.Engage(target);
        var outcome = AttackResolver.Resolve(ctx.Caster, target, ctx.Definition.DiceNotation ?? "2d6", DamageType.Lightning);
        if (!outcome.Hit) { Helpers.ColorConsole.WriteLine($"Your jolt crackles past {target.Name}.", ConsoleColor.Gray); return; }

        int dmg = outcome.Damage + ctx.SpellPowerBonus();
        target.Health -= dmg;
        Helpers.ColorConsole.WriteLine(
            $"Lightning arcs into {target.Name} for {dmg}! -> [{target.Name} HP: {Math.Max(0, target.Health)}]", ConsoleColor.Gray);

        if (target.Health <= 0) { DeathService.HandleDeath(target, ctx.World, ctx.Caster); return; }

        target.StatusEffects.Add(new StatusEffect
        {
            Name = "shocked", Modifier = EffectModifier.Stun, Polarity = EffectPolarity.Negative,
            Type = EffectType.Magic, TicksRemaining = Math.Max(1, (int)ctx.Param("stunRounds", 1))
        });
    }
}
