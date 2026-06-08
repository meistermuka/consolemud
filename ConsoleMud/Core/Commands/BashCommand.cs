using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class BashCommand : ICommand
{
    public void Execute(Player player, string[] args, WorldState world)
    {
        if (player.CombatTarget == null)
        {
            Console.WriteLine("You can only bash while actively engaged in combat!");
            return;
        }

        if (player.EquippedWeapon == null)
        {
            Console.WriteLine("You need to have a weapon equipped to deliver a devastating bash!");
            return;
        }

        var target = player.CombatTarget;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n[SKILL] You violently drive the hilt of your weapon into the {target.Name}!");
        Console.ResetColor();

        // Skill properties: heavy 2d6 roll + ignores up to 2 points of armor
        int rawDamage = DiceRoller.Roll("2d6");
        int enemyArmor = target.EquippedArmour?.ArmourRating ?? 0;
        int penetratedArmor = Math.Max(0, enemyArmor - 2); 

        int finalDamage = Math.Max(2, rawDamage - penetratedArmor);
        target.Health -= finalDamage;

        Console.WriteLine($"Your brute-force Bash slams {target.Name} for {finalDamage} damage! (Bypassed armor!)");

        if (target.Health <= 0)
        {
            // Fallback safety if the skill deals the killing blow
            player.CombatTarget = null;
            var room = world.Rooms[target.CurrentRoomId];
            room.Characters.Remove(target);
            world.Characters.Remove(target.Id);
            room.Items.Add(new Item { Name = $"pulverized corpse of a {target.Name}", IsContainer = true });
            Console.WriteLine($"The {target.Name} is completely crushed by the impact!");
        }
    }
}