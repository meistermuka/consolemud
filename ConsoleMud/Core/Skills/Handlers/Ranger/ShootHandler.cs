using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Ranger;

public class ShootHandler : ISkillHandler
{
    public string SkillId => "shoot";

    public void Execute(SkillContext ctx)
    {
        var bow = ctx.Caster.MainHandWeapon;
        if (bow == null || bow.WeaponType != WeaponType.Bow)
        {
            Console.WriteLine("You need a bow equipped to shoot.");
            return;
        }
        if (ctx.Args.Length == 0)
        {
            Console.WriteLine("Shoot what?");
            return;
        }

        // Prefer a target in the current room; otherwise look one exit away.
        var target = ctx.ResolveNpcTarget();
        bool fromAfar = false;
        if (target == null)
        {
            target = FindAdjacent(ctx);
            fromAfar = target != null;
        }

        if (target == null)
        {
            Console.WriteLine($"You don't see a '{ctx.TargetName}' to shoot.");
            return;
        }

        var outcome = AttackResolver.Resolve(ctx.Caster, target, bow.DiceNotation ?? "2d4", DamageType.Physical);
        if (!outcome.Hit)
        {
            ColorConsole.WriteLine($"Your arrow flies wide of {target.Name}.", ConsoleColor.Gray);
        }
        else
        {
            target.Health -= outcome.Damage;
            ColorConsole.WriteLine(
                $"Your arrow strikes {target.Name} for {outcome.Damage}! -> [{target.Name} HP: {Math.Max(0, target.Health)}]",
                ConsoleColor.Gray);
        }

        if (target.Health <= 0)
        {
            DeathService.HandleDeath(target, ctx.World, ctx.Caster);
            return;
        }

        // A shot from afar drags the target into the shooter's room (pulling aggro safely).
        if (fromAfar)
        {
            ctx.World.MoveCharacter(target, ctx.Caster.CurrentRoomId);
            ColorConsole.WriteLine($"Your shot draws {target.Name} into the room!", ConsoleColor.Gray);
        }

        ctx.Engage(target);

        // Arrow volley: extra projectiles strike other foes in the room.
        if (ctx.Caster.KnownSkills.ContainsKey("arrow_volley")
            && ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var here))
        {
            var others = here.Characters.OfType<NonPlayerCharacter>()
                .Where(n => n != target && n.Health > 0 && !n.IsPet).Take(2).ToList();
            foreach (var extra in others)
            {
                var o = AttackResolver.Resolve(ctx.Caster, extra, bow.DiceNotation ?? "2d4", DamageType.Physical);
                if (!o.Hit) continue;
                extra.Health -= o.Damage;
                ColorConsole.WriteLine($"A second arrow hits {extra.Name} for {o.Damage}!", ConsoleColor.Gray);
                if (extra.Health <= 0) DeathService.HandleDeath(extra, ctx.World, ctx.Caster);
            }
        }
    }

    private static NonPlayerCharacter FindAdjacent(SkillContext ctx)
    {
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room))
            return null;

        var (_, keyword) = KeywordParser.ExtractIndex(string.Join(" ", ctx.Args));
        foreach (var exitId in room.Exits.Values)
            if (ctx.World.Rooms.TryGetValue(exitId, out var adj))
            {
                var npc = adj.Characters.OfType<NonPlayerCharacter>()
                    .FirstOrDefault(n => n.Health > 0 && !n.IsPet && n.MatchesKeyword(keyword));
                if (npc != null)
                    return npc;
            }
        return null;
    }
}
