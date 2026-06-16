using ConsoleMud.Core;
using ConsoleMud.Core.Combat;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Thief;

public class BackstabHandler : ISkillHandler
{
    public string SkillId => "backstab";

    public void Execute(SkillContext ctx)
    {
        if (!ctx.Caster.IsHidden)
        {
            Console.WriteLine("You must be hidden to backstab.");
            return;
        }

        var target = ctx.ResolveNpcTarget();
        if (target == null) { Console.WriteLine("Backstab what?"); return; }

        var weapon = ctx.Caster.MainHandWeapon;
        if (weapon == null) { Console.WriteLine("You need a weapon to backstab."); return; }

        double mult = ctx.Caster.KnownSkills.ContainsKey("anatomic_precision") ? 5.0 : ctx.Param("multiplier", 3);

        // A surprise strike lands automatically.
        int raw = (int)(DiceRoller.Roll(weapon.DiceNotation ?? "1d4") * mult);
        int afterArmor = Math.Max(0, raw - target.TotalArmourRating);
        double typeMult = DamageResolver.GetDamageMultiplier(target, DamageType.Physical);
        int dmg = typeMult <= 0 ? 0 : Math.Max(1, (int)Math.Round(afterArmor * typeMult));

        target.Health -= dmg;
        Helpers.ColorConsole.WriteLine(
            $"You drive your blade into {target.Name}'s back for {dmg}! -> [{target.Name} HP: {Math.Max(0, target.Health)}]",
            ConsoleColor.Gray);

        ctx.Caster.BreakHidden();
        if (target.Health <= 0)
            DeathService.HandleDeath(target, ctx.World, ctx.Caster);
        else
            ctx.Engage(target);
    }
}
