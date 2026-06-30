using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Thief;

public class DeathDanceHandler : ISkillHandler
{
    public string SkillId => "death_dance";

    public void Execute(SkillContext ctx)
    {
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room)) return;
        var foes = room.Characters.OfType<NonPlayerCharacter>().Where(n => n.Health > 0).ToList();
        if (foes.Count == 0) { Helpers.ColorConsole.WriteLine("You whirl your blades through empty air.", ConsoleColor.Gray); return; }

        // Strike every foe several times with the off-hand profile.
        string dice = ctx.Caster.OffHandWeapon?.DiceNotation ?? ctx.Caster.MainHandWeapon?.DiceNotation ?? "1d4";
        int hits = Math.Max(1, (int)ctx.Param("hits", 3));
        Helpers.ColorConsole.WriteLine("You become a whirlwind of blades!", ConsoleColor.Gray);

        foreach (var foe in foes)
        {
            for (int i = 0; i < hits && foe.Health > 0; i++)
            {
                var outcome = AttackResolver.Resolve(ctx.Caster, foe, dice, DamageType.Physical);
                if (!outcome.Hit) continue;
                foe.Health -= outcome.Damage;
                if (foe.Health <= 0) { DeathService.HandleDeath(foe, ctx.World, ctx.Caster); break; }
            }
            if (foe.Health > 0)
                Helpers.ColorConsole.WriteLine($"  {foe.Name} is cut to ribbons -> [HP: {Math.Max(0, foe.Health)}]", ConsoleColor.Gray);
        }
        // Stealth is retained (the executor's stealth-break exempts death_dance).
    }
}
