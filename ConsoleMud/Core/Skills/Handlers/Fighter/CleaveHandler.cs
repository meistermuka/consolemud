using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Fighter;

public class CleaveHandler : ISkillHandler
{
    public string SkillId => "cleave";

    public void Execute(SkillContext ctx)
    {
        var primary = ctx.ResolveNpcTarget();
        if (primary == null)
        {
            ColorConsole.WriteLine("Cleave what?");
            return;
        }

        ctx.Engage(primary);

        string dice = ctx.Caster.MainHandWeapon?.DiceNotation ?? "1d4";
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room))
            return;

        double splash = ctx.Param("splashFraction", 0.5);

        // Full damage to the primary target.
        Strike(ctx, primary, dice, 1.0);

        // Half damage to every other living NPC in the room.
        foreach (var other in room.Characters.OfType<NonPlayerCharacter>().Where(n => n != primary && n.Health > 0).ToList())
            Strike(ctx, other, dice, splash);
    }

    private static void Strike(SkillContext ctx, NonPlayerCharacter target, string dice, double scale)
    {
        var outcome = AttackResolver.Resolve(ctx.Caster, target, dice, DamageType.Physical);
        if (!outcome.Hit)
            return;

        int dmg = Math.Max(1, (int)(outcome.Damage * scale));
        target.Health -= dmg;
        Helpers.ColorConsole.WriteLine(
            $"Your cleave hits {target.Name} for {dmg}! -> [{target.Name} HP: {Math.Max(0, target.Health)}]", ConsoleColor.Gray);

        if (target.Health <= 0)
            DeathService.HandleDeath(target, ctx.World, ctx.Caster);
    }
}
