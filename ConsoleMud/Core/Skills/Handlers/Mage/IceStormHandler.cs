using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Mage;

public class IceStormHandler : ISkillHandler
{
    public string SkillId => "ice_storm";

    public void Execute(SkillContext ctx)
    {
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room)) return;
        var foes = room.Characters.OfType<NonPlayerCharacter>().Where(n => n.Health > 0).ToList();
        if (foes.Count == 0) { Helpers.ColorConsole.WriteLine("Ice rains down on an empty room.", ConsoleColor.Gray); return; }

        ctx.Caster.BreakHidden();
        int duration = ctx.Definition.DurationTicks > 0 ? ctx.Definition.DurationTicks : 4;
        Helpers.ColorConsole.WriteLine("A freezing storm of razor ice tears through the room!", ConsoleColor.Cyan);

        foreach (var foe in foes)
        {
            var outcome = AttackResolver.Resolve(ctx.Caster, foe, ctx.Definition.DiceNotation ?? "4d6", DamageType.Cold);
            if (outcome.Hit)
            {
                int dmg = outcome.Damage + ctx.SpellPowerBonus();
                foe.Health -= dmg;
                Helpers.ColorConsole.WriteLine($"  {foe.Name} is frozen for {dmg}! -> [HP: {Math.Max(0, foe.Health)}]", ConsoleColor.Gray);
            }

            if (foe.Health <= 0) { DeathService.HandleDeath(foe, ctx.World, ctx.Caster); continue; }

            // Slow: fewer attacks per round.
            foe.StatusEffects.Add(new StatusEffect
            {
                Name = "chilled", Modifier = EffectModifier.AttackRateMod, Magnitude = -1,
                Polarity = EffectPolarity.Negative, Type = EffectType.Magic, TicksRemaining = duration
            });
        }
    }
}
