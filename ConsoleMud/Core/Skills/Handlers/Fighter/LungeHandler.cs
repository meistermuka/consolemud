using ConsoleMud.Core.Combat;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Fighter;

public class LungeHandler : ISkillHandler
{
    public string SkillId => "lunge";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveNpcTarget();
        if (target == null)
        {
            ColorConsole.WriteLine("Lunge at what?");
            return;
        }

        ctx.Engage(target);

        // Automatic maximum weapon damage on the opening strike.
        string dice = ctx.Caster.MainHandWeapon?.DiceNotation ?? "1d4";
        int max = DiceRoller.Max(dice);
        int afterArmor = Math.Max(0, max - target.TotalArmourRating);
        double mult = DamageResolver.GetDamageMultiplier(target, DamageType.Physical);
        int dmg = mult <= 0 ? 0 : Math.Max(1, (int)Math.Round(afterArmor * mult));

        target.Health -= dmg;
        Helpers.ColorConsole.WriteLine(
            $"You lunge in with a perfect strike on {target.Name} for {dmg}! -> [{target.Name} HP: {Math.Max(0, target.Health)}]",
            ConsoleColor.Gray);

        if (target.Health <= 0)
            DeathService.HandleDeath(target, ctx.World, ctx.Caster);
    }
}
