using ConsoleMud.Core.Services;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Ranger;

public class ScoutHandler : ISkillHandler
{
    public string SkillId => "scout";

    public void Execute(SkillContext ctx)
    {
        if (ctx.Args.Length == 0 || !Enum.TryParse<Direction>(ctx.Args[0], true, out var dir))
        {
            ColorConsole.WriteLine("Scout which direction? (north/south/east/west/up/down)");
            return;
        }
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room))
            return;

        Helpers.ColorConsole.WriteLine(PerceptionService.DescribeAdjacent(ctx.World, room, dir), ConsoleColor.Gray);
    }
}
