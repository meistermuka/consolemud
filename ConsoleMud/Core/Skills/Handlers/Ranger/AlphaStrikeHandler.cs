using ConsoleMud.Core;
using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Ranger;

public class AlphaStrikeHandler : ISkillHandler
{
    public string SkillId => "alpha_strike";

    public void Execute(SkillContext ctx)
    {
        if (ctx.Caster is not Player ranger || ranger.Pet is not { Health: > 0 } pet)
        {
            ColorConsole.WriteLine("You need a living companion to pin the target.");
            return;
        }
        var bow = ctx.Caster.MainHandWeapon;
        if (bow == null || bow.WeaponType != WeaponType.Bow)
        {
            ColorConsole.WriteLine("You need a bow for the alpha strike.");
            return;
        }

        var target = ctx.ResolveNpcTarget();
        if (target == null) { ColorConsole.WriteLine("Alpha strike on what?"); return; }

        ctx.Engage(target);

        // The pet pins the target, then four guaranteed critical point-blank shots.
        target.StatusEffects.Add(new StatusEffect { Name = "pinned", Modifier = EffectModifier.Stun, Polarity = EffectPolarity.Negative, Type = EffectType.Physical, TicksRemaining = 2 });
        Helpers.ColorConsole.WriteLine($"{pet.Name} pins {target.Name} as you take aim!", ConsoleColor.Gray);

        int shots = Math.Max(1, (int)ctx.Param("shots", 4));
        for (int i = 0; i < shots && target.Health > 0; i++)
        {
            int raw = DiceRoller.Roll(bow.DiceNotation ?? "2d4") * 2; // guaranteed crit
            int afterArmor = Math.Max(0, raw - target.TotalArmourRating);
            double mult = DamageResolver.GetDamageMultiplier(target, DamageType.Physical);
            int dmg = mult <= 0 ? 0 : Math.Max(1, (int)Math.Round(afterArmor * mult));
            target.Health -= dmg;
            Helpers.ColorConsole.WriteLine($"  critical shot {i + 1}: {dmg}! -> [HP: {Math.Max(0, target.Health)}]", ConsoleColor.Gray);
            if (target.Health <= 0) { DeathService.HandleDeath(target, ctx.World, ctx.Caster); break; }
        }
    }
}
