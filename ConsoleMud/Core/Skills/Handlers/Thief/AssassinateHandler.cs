using ConsoleMud.Core;
using ConsoleMud.Core.Combat;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Thief;

public class AssassinateHandler : ISkillHandler
{
    public string SkillId => "assassinate";

    public void Execute(SkillContext ctx)
    {
        if (!ctx.Caster.IsHidden)
        {
            ColorConsole.WriteLine("You must be hidden and unseen to assassinate.");
            return;
        }

        var target = ctx.ResolveNpcTarget();
        if (target == null) { ColorConsole.WriteLine("Assassinate whom?"); return; }

        ctx.Caster.BreakHidden();

        // A weaker mark dies outright; a stronger one is crippled.
        if (target.Level < ctx.Caster.Level)
        {
            target.Health = 0;
            Helpers.ColorConsole.WriteLine($"You slip your blade across {target.Name}'s throat. It dies instantly!", ConsoleColor.Red);
            DeathService.HandleDeath(target, ctx.World, ctx.Caster);
            return;
        }

        int raw = DiceRoller.Roll(ctx.Definition.DiceNotation ?? "10d8");
        double mult = DamageResolver.GetDamageMultiplier(target, DamageType.Physical);
        int dmg = mult <= 0 ? 0 : Math.Max(1, (int)Math.Round(raw * mult));
        target.Health -= dmg;

        // Permanent (this-fight) crippling: halve the survivor's max health.
        target.MaxHealth = Math.Max(1, target.MaxHealth / 2);
        if (target.Health > target.MaxHealth) target.Health = target.MaxHealth;

        Helpers.ColorConsole.WriteLine(
            $"You strike a vital blow on {target.Name} for {dmg}, crippling it! -> [{target.Name} HP: {Math.Max(0, target.Health)}/{target.MaxHealth}]",
            ConsoleColor.Red);

        if (target.Health <= 0)
            DeathService.HandleDeath(target, ctx.World, ctx.Caster);
        else
            ctx.Engage(target);
    }
}
