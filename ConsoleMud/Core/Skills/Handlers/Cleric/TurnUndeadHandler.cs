using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Cleric;

public class TurnUndeadHandler : ISkillHandler
{
    public string SkillId => "turn_undead";

    public void Execute(SkillContext ctx)
    {
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room))
            return;

        var undead = room.Characters.OfType<NonPlayerCharacter>()
            .Where(n => n.Health > 0 && n.Archetypes.Contains(Archetype.Undead))
            .ToList();

        if (undead.Count == 0)
        {
            Helpers.ColorConsole.WriteLine("Holy light flares, but no undead are here to flee it.", ConsoleColor.Gray);
            return;
        }

        var exits = room.Exits.Values.ToList();
        foreach (var npc in undead)
        {
            npc.CombatTarget = null;
            foreach (var c in ctx.World.Characters.Values.Where(c => c.CombatTarget == npc))
                c.CombatTarget = null;

            Helpers.ColorConsole.WriteLine($"{npc.Name} recoils from the holy light and flees!", ConsoleColor.Gray);
            if (exits.Count > 0)
                ctx.World.MoveCharacter(npc, exits[Random.Shared.Next(exits.Count)]);
        }
    }
}
