using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Druid;

public class WrathOfNatureHandler : ISkillHandler
{
    public string SkillId => "wrath_of_nature";

    public void Execute(SkillContext ctx)
    {
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room)) return;
        var foes = room.Characters.OfType<NonPlayerCharacter>().Where(n => n.Health > 0).ToList();
        if (foes.Count == 0) { Helpers.ColorConsole.WriteLine("Nature stirs, but there are no foes to punish.", ConsoleColor.Gray); return; }

        ctx.Caster.BreakHidden();
        int rootRounds = Math.Max(1, (int)ctx.Param("rootRounds", 2));
        int blindRounds = Math.Max(1, (int)ctx.Param("blindRounds", 2));
        Helpers.ColorConsole.WriteLine("The wilds erupt in fury — roots, wind, and lightning rip through the room!", ConsoleColor.Green);

        foreach (var foe in foes)
        {
            var outcome = AttackResolver.Resolve(ctx.Caster, foe, ctx.Definition.DiceNotation ?? "6d8", DamageType.Nature);
            if (outcome.Hit)
            {
                foe.Health -= outcome.Damage;
                Helpers.ColorConsole.WriteLine($"  {foe.Name} is ravaged for {outcome.Damage}! -> [HP: {Math.Max(0, foe.Health)}]", ConsoleColor.Gray);
            }

            if (foe.Health <= 0) { DeathService.HandleDeath(foe, ctx.World, ctx.Caster); continue; }

            foe.StatusEffects.Add(new StatusEffect { Name = "entangled", Modifier = EffectModifier.Root, Polarity = EffectPolarity.Negative, Type = EffectType.Magic, TicksRemaining = rootRounds });
            foe.StatusEffects.Add(new StatusEffect { Name = "wind-blinded", Modifier = EffectModifier.Blind, Polarity = EffectPolarity.Negative, Type = EffectType.Magic, TicksRemaining = blindRounds });
        }
    }
}
