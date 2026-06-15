using ConsoleMud.Entities;

namespace ConsoleMud.Core.Skills.Handlers.Fighter;

public class RescueHandler : ISkillHandler
{
    public string SkillId => "rescue";

    public void Execute(SkillContext ctx)
    {
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room))
            return;

        // Find a foe attacking someone other than the rescuer.
        var attacker = room.Characters.OfType<NonPlayerCharacter>()
            .FirstOrDefault(n => n.Health > 0 && n.CombatTarget != null && n.CombatTarget != ctx.Caster);

        if (attacker == null)
        {
            Console.WriteLine("There is no one here who needs rescuing.");
            return;
        }

        var saved = attacker.CombatTarget;
        attacker.CombatTarget = ctx.Caster;
        if (ctx.Caster.CombatTarget == null)
            ctx.Caster.CombatTarget = attacker;

        Helpers.ColorConsole.WriteLine(
            $"You intercept {attacker.Name}, dragging its fury away from {saved.Name}!", ConsoleColor.Gray);
    }
}
