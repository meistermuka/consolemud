using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Ranger;

public class SetTrapHandler : ISkillHandler
{
    public string SkillId => "set_trap";

    public void Execute(SkillContext ctx)
    {
        if (ctx.Caster.CombatTarget != null)
        {
            ColorConsole.WriteLine("You can't set a trap in the middle of a fight.");
            return;
        }

        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room))
            return;

        int rootRounds = Math.Max(1, (int)ctx.Param("rootRounds", 3));
        TrapSystem.Place(room, ctx.Caster, ctx.Definition.DiceNotation ?? "2d6", DamageType.Physical, rootRounds);

        Helpers.ColorConsole.WriteLine("You carefully conceal a snare on the ground here.", ConsoleColor.Gray);
    }
}
