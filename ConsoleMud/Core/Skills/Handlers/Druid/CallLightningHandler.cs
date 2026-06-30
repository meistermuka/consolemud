using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Druid;

public class CallLightningHandler : ISkillHandler
{
    public string SkillId => "call_lightning";

    public void Execute(SkillContext ctx)
    {
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room)) return;
        var foes = room.Characters.OfType<NonPlayerCharacter>().Where(n => n.Health > 0).ToList();
        if (foes.Count == 0) { Helpers.ColorConsole.WriteLine("Lightning forks down on an empty room.", ConsoleColor.Gray); return; }

        ctx.Caster.BreakHidden();
        int max = Math.Max(1, (int)ctx.Param("targets", 4));
        bool storm = ctx.World.IsStormy;
        Helpers.ColorConsole.WriteLine("A storm cloud gathers and lightning lashes down!", ConsoleColor.Cyan);

        foreach (var foe in foes.OrderBy(_ => Random.Shared.Next()).Take(max))
        {
            var outcome = AttackResolver.Resolve(ctx.Caster, foe, ctx.Definition.DiceNotation ?? "4d6", DamageType.Nature);
            if (!outcome.Hit) continue;
            int dmg = storm ? outcome.Damage * 2 : outcome.Damage;
            foe.Health -= dmg;
            Helpers.ColorConsole.WriteLine($"  {foe.Name} is struck for {dmg}! -> [HP: {Math.Max(0, foe.Health)}]", ConsoleColor.Gray);
            if (foe.Health <= 0) DeathService.HandleDeath(foe, ctx.World, ctx.Caster);
        }
    }
}
