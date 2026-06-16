using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Mage;

/// <summary>
/// Cataclysmic AoE on the current room. The original design also hits adjacent
/// rooms; that multi-room reach is deferred until the multi-room AoE subsystem.
/// </summary>
public class MeteorSwarmHandler : ISkillHandler
{
    public string SkillId => "meteor_swarm";

    public void Execute(SkillContext ctx)
    {
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room)) return;
        var foes = room.Characters.OfType<NonPlayerCharacter>().Where(n => n.Health > 0).ToList();
        if (foes.Count == 0) { Helpers.ColorConsole.WriteLine("Meteors crater an empty room.", ConsoleColor.Gray); return; }

        ctx.Caster.BreakHidden();
        Helpers.ColorConsole.WriteLine("The sky splits and a rain of meteors hammers the room!", ConsoleColor.Red);

        foreach (var foe in foes)
        {
            var outcome = AttackResolver.Resolve(ctx.Caster, foe, ctx.Definition.DiceNotation ?? "8d6", DamageType.Fire);
            if (outcome.Hit)
            {
                int dmg = outcome.Damage + ctx.SpellPowerBonus();
                foe.Health -= dmg;
                Helpers.ColorConsole.WriteLine($"  {foe.Name} is crushed for {dmg}! -> [HP: {Math.Max(0, foe.Health)}]", ConsoleColor.Gray);
            }
            if (foe.Health <= 0) DeathService.HandleDeath(foe, ctx.World, ctx.Caster);
        }
    }
}
