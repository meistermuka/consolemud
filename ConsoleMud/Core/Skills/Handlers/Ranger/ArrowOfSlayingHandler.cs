using ConsoleMud.Core;
using ConsoleMud.Core.Combat;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Ranger;

public class ArrowOfSlayingHandler : ISkillHandler
{
    public string SkillId => "arrow_of_slaying";

    public void Execute(SkillContext ctx)
    {
        var bow = ctx.Caster.MainHandWeapon;
        if (bow == null || bow.WeaponType != WeaponType.Bow)
        {
            ColorConsole.WriteLine("You need a bow to loose an arrow of slaying.");
            return;
        }

        var target = ctx.ResolveNpcTarget();
        if (target == null) { ColorConsole.WriteLine("Loose your arrow at what?"); return; }

        ctx.Engage(target);

        // A focused, armor-ignoring sniper shot.
        int raw = DiceRoller.Roll(ctx.Definition.DiceNotation ?? "8d8");
        double mult = DamageResolver.GetDamageMultiplier(target, DamageType.Physical);
        int dmg = mult <= 0 ? 0 : Math.Max(1, (int)Math.Round(raw * mult));
        target.Health -= dmg;

        Helpers.ColorConsole.WriteLine(
            $"Your arrow of slaying punches clean through {target.Name} for {dmg}! -> [{target.Name} HP: {Math.Max(0, target.Health)}]",
            ConsoleColor.Gray);

        if (target.Health <= 0)
            DeathService.HandleDeath(target, ctx.World, ctx.Caster);
    }
}
