using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Mage;

/// <summary>
/// Approximation of stopping time: every other foe in the room is frozen (stunned)
/// for a few rounds, letting the mage act unopposed. A true world-freeze that also
/// suspends ticks is deferred with the channeling/room-freeze subsystem.
/// </summary>
public class TimeStopHandler : ISkillHandler
{
    public string SkillId => "time_stop";

    public void Execute(SkillContext ctx)
    {
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room)) return;
        int rounds = Math.Max(1, (int)ctx.Param("freezeRounds", 3));

        var foes = room.Characters.OfType<NonPlayerCharacter>().Where(n => n.Health > 0).ToList();
        foreach (var foe in foes)
            foe.StatusEffects.Add(new StatusEffect
            {
                Name = "time-stopped", Modifier = EffectModifier.Stun, Polarity = EffectPolarity.Negative,
                Type = EffectType.Magic, TicksRemaining = rounds
            });

        Helpers.ColorConsole.WriteLine(
            foes.Count > 0 ? "Time freezes — your foes hang motionless!" : "Time freezes, but no one is here to notice.",
            ConsoleColor.Yellow);
    }
}
