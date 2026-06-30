using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Fighter;

public class TauntHandler : ISkillHandler
{
    public string SkillId => "taunt";

    public void Execute(SkillContext ctx)
    {
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room))
            return;

        bool any = false;
        foreach (var npc in room.Characters.OfType<NonPlayerCharacter>().Where(n => n.Health > 0))
        {
            npc.CombatTarget = ctx.Caster;
            if (ctx.Caster.CombatTarget == null)
                ctx.Caster.CombatTarget = npc;
            any = true;
        }

        Helpers.ColorConsole.WriteLine(
            any ? "You bellow a challenge; every foe here turns to face you!" : "No enemies heed your taunt.",
            ConsoleColor.Gray);
    }
}
