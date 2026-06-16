using ConsoleMud.Core.Combat;
using ConsoleMud.Core.Skills;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Commands;

/// <summary>The dragon-form breath weapon: un-mitigated elemental AoE on the room.</summary>
public class BreathCommand : ICommand
{
    public string Description => "Unleash a dragon's breath weapon (dragon form only).";
    public string Usage => "breath";
    public string Example => "breath";

    public void Execute(Player player, string[] args, WorldState world)
    {
        var form = ShapeshiftService.GetForm(player);
        if (form == null || string.IsNullOrWhiteSpace(form.BreathDice))
        {
            Console.WriteLine("You have no breath weapon in this form.");
            return;
        }

        if (!world.Rooms.TryGetValue(player.CurrentRoomId, out var room))
            return;

        var foes = room.Characters.OfType<NonPlayerCharacter>().Where(n => n.Health > 0).ToList();
        if (foes.Count == 0)
        {
            Console.WriteLine("You breathe, but there is nothing here to scorch.");
            return;
        }

        Helpers.ColorConsole.WriteLine("You unleash a searing torrent of elemental breath!", ConsoleColor.Red);
        foreach (var foe in foes)
        {
            int dmg = DamageResolver.Apply(foe, DamageType.Fire, DiceRoller.Roll(form.BreathDice));
            foe.Health -= dmg;
            Helpers.ColorConsole.WriteLine($"  {foe.Name} is engulfed for {dmg}! -> [HP: {Math.Max(0, foe.Health)}]", ConsoleColor.Gray);
            if (foe.Health <= 0)
                DeathService.HandleDeath(foe, world, player);
        }
    }
}
