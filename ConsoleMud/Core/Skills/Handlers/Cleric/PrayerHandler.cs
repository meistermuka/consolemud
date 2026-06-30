using ConsoleMud.Core;
using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Cleric;

public class PrayerHandler : ISkillHandler
{
    public string SkillId => "prayer";

    public void Execute(SkillContext ctx)
    {
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room))
            return;

        int baseHeal = DiceRoller.Roll(ctx.Definition.DiceNotation ?? "2d8")
                       + ctx.AttributeModifier(ctx.Definition.AttributeBonus)
                       + ctx.HealScaleBonus();
        int mana = (int)ctx.Param("manaRestore", 5);

        // All friendly players in the room (covers solo play; party-aware later).
        foreach (var ally in room.Characters.OfType<Player>())
        {
            int healed = Math.Min(Math.Max(1, baseHeal), ally.MaxHealth - ally.Health);
            ally.Health += healed;
            ally.Mana = Math.Min(ally.MaxMana, ally.Mana + mana);
        }

        Helpers.ColorConsole.WriteLine(
            $"A pulse of holy light washes over the room, mending the faithful (+{Math.Max(1, baseHeal)} HP, +{mana} mana).",
            ConsoleColor.Gray);
    }
}
