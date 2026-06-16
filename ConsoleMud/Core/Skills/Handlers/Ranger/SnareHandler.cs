using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Ranger;

public class SnareHandler : ISkillHandler
{
    public string SkillId => "snare";

    public void Execute(SkillContext ctx)
    {
        var bow = ctx.Caster.MainHandWeapon;
        if (bow == null || bow.WeaponType != WeaponType.Bow)
        {
            Console.WriteLine("You need a bow to fire a snaring shot.");
            return;
        }

        var target = ctx.ResolveNpcTarget();
        if (target == null) { Console.WriteLine("Snare what?"); return; }

        ctx.Engage(target);
        var outcome = AttackResolver.Resolve(ctx.Caster, target, ctx.Definition.DiceNotation ?? "2d6", DamageType.Physical);
        if (outcome.Hit)
        {
            target.Health -= outcome.Damage;
            Helpers.ColorConsole.WriteLine($"Your snaring shot hits {target.Name} for {outcome.Damage}!", ConsoleColor.Gray);
        }

        if (target.Health <= 0) { DeathService.HandleDeath(target, ctx.World, ctx.Caster); return; }

        target.StatusEffects.Add(new StatusEffect
        {
            Name = "snared", Modifier = EffectModifier.Root, Polarity = EffectPolarity.Negative,
            Type = EffectType.Physical, TicksRemaining = Math.Max(1, (int)ctx.Param("rootRounds", 4))
        });
        Helpers.ColorConsole.WriteLine($"{target.Name} is snared and cannot give chase!", ConsoleColor.Gray);
    }
}
