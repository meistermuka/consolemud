namespace ConsoleMud.Core.Skills.Handlers.Ranger;

public class ForestCamouflageHandler : ISkillHandler
{
    public string SkillId => "forest_camouflage";

    public void Execute(SkillContext ctx)
    {
        if (ctx.Caster.CombatTarget != null)
        {
            Console.WriteLine("You can't melt into cover while fighting.");
            return;
        }
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room) || !room.IsOutside)
        {
            Console.WriteLine("There is no natural cover to blend into here.");
            return;
        }

        ctx.Caster.IsHidden = true;
        Helpers.ColorConsole.WriteLine("You melt into the foliage, hidden from sight.", ConsoleColor.DarkGray);
    }
}
