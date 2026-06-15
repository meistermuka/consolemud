using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Fighter;

public class DisarmHandler : ISkillHandler
{
    public string SkillId => "disarm";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveNpcTarget();
        if (target == null)
        {
            Console.WriteLine("Disarm what?");
            return;
        }

        ctx.Engage(target);

        if (!target.Equipment.TryGetValue(EquipmentSlot.MainHand, out var weapon) || weapon == null)
        {
            Helpers.ColorConsole.WriteLine($"{target.Name} has no weapon to disarm.", ConsoleColor.Gray);
            return;
        }

        if (Random.Shared.NextDouble() < ctx.Param("dropChance", 0.4))
        {
            target.Equipment.Remove(EquipmentSlot.MainHand);
            if (ctx.World.Rooms.TryGetValue(target.CurrentRoomId, out var room))
                room.Items.Add(weapon);
            Helpers.ColorConsole.WriteLine($"You knock the {weapon.Name} from {target.Name}'s grip!", ConsoleColor.Gray);
        }
        else
        {
            Helpers.ColorConsole.WriteLine($"You fail to disarm {target.Name}.", ConsoleColor.Gray);
        }
    }
}
