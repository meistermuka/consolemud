using ConsoleMud.Core;
using ConsoleMud.Core.Combat;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Fighter;

public class DecapitateHandler : ISkillHandler
{
    public string SkillId => "decapitate";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveNpcTarget();
        if (target == null)
        {
            ColorConsole.WriteLine("Decapitate what?");
            return;
        }

        ctx.Engage(target);

        double threshold = ctx.Param("executeThresholdPct", 15) / 100.0;
        if (target.Health > target.MaxHealth * threshold)
        {
            Helpers.ColorConsole.WriteLine(
                $"{target.Name} is too strong to decapitate. Wound it further first.", ConsoleColor.Gray);
            return;
        }

        int rawDamage = DiceRoller.Roll(ctx.Definition.DiceNotation ?? "10d10");
        double mult = DamageResolver.GetDamageMultiplier(target, DamageType.Physical);
        int dmg = mult <= 0 ? 0 : Math.Max(1, (int)Math.Round(rawDamage * mult));

        target.Health -= dmg;
        Helpers.ColorConsole.WriteLine($"You bring your weapon down on {target.Name} for {dmg}!", ConsoleColor.Red);

        if (target.Health <= 0)
        {
            // A clean execution destroys the victim's gear (no lootable corpse).
            target.Equipment.Clear();
            target.Inventory.Clear();
            Helpers.ColorConsole.WriteLine($"{target.Name}'s head rolls free. Nothing usable remains.", ConsoleColor.Red);
            DeathService.HandleDeath(target, ctx.World, ctx.Caster);
        }
    }
}
