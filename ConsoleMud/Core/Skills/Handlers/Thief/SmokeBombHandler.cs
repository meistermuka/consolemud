using ConsoleMud.Core.Commands;
using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Thief;

public class SmokeBombHandler : ISkillHandler
{
    public string SkillId => "smoke_bomb";

    public void Execute(SkillContext ctx)
    {
        if (ctx.Caster is not Player thief) return;
        if (!ctx.World.Rooms.TryGetValue(thief.CurrentRoomId, out var room)) return;

        int blindRounds = Math.Max(1, (int)ctx.Param("blindRounds", 2));

        // Blind everything in the room.
        foreach (var npc in room.Characters.OfType<NonPlayerCharacter>().Where(n => n.Health > 0))
            npc.StatusEffects.Add(new StatusEffect
            {
                Name = "smoke-blinded", Modifier = EffectModifier.Blind, Polarity = EffectPolarity.Negative,
                Type = EffectType.Generic, TicksRemaining = blindRounds
            });

        // Break combat and bolt to a random exit.
        thief.CombatTarget = null;
        foreach (var c in ctx.World.Characters.Values.Where(c => c.CombatTarget == thief))
            c.CombatTarget = null;

        Helpers.ColorConsole.WriteLine("\nYou hurl a smoke bomb and vanish in the chaos!", ConsoleColor.Gray);

        if (room.Exits.Count > 0)
        {
            var chosen = room.Exits.ToList()[Random.Shared.Next(room.Exits.Count)];
            thief.LastExit = chosen.Key;
            thief.Position = Position.Standing;
            ctx.World.MoveCharacter(thief, chosen.Value);
            new LookCommand().Execute(thief, Array.Empty<string>(), ctx.World);
        }
    }
}
