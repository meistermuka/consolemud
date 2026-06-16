using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Cleric;

public class JudgmentHandler : ISkillHandler
{
    public string SkillId => "judgment";

    public void Execute(SkillContext ctx)
    {
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room))
            return;

        var foes = room.Characters.OfType<NonPlayerCharacter>().Where(n => n.Health > 0).ToList();
        if (foes.Count == 0)
        {
            Helpers.ColorConsole.WriteLine("A pillar of light descends, but there are no foes to judge.", ConsoleColor.Gray);
            return;
        }

        int blindRounds = Math.Max(1, (int)ctx.Param("blindRounds", 3));
        Helpers.ColorConsole.WriteLine("A blinding pillar of holy light crashes down!", ConsoleColor.Yellow);

        foreach (var foe in foes)
        {
            var outcome = AttackResolver.Resolve(ctx.Caster, foe, ctx.Definition.DiceNotation ?? "6d8",
                                                 DamageType.Holy, ignoresArmor: true);
            if (outcome.Hit)
            {
                foe.Health -= outcome.Damage;
                Helpers.ColorConsole.WriteLine(
                    $"  {foe.Name} is judged for {outcome.Damage} holy damage! -> [HP: {Math.Max(0, foe.Health)}]", ConsoleColor.Gray);
            }

            if (foe.Health <= 0)
            {
                DeathService.HandleDeath(foe, ctx.World, ctx.Caster);
                continue;
            }

            foe.StatusEffects.Add(new StatusEffect
            {
                Name = "blinded", Modifier = EffectModifier.Blind, Polarity = EffectPolarity.Negative,
                Type = EffectType.Magic, TicksRemaining = blindRounds
            });
        }
    }
}
