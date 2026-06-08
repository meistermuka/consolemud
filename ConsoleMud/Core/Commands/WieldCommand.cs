using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class WieldCommand : ICommand
{
    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Wield what?");
            return;
        }

        string targetWeaponName = string.Join(" ", args);
        var weapon = player.Inventory.FirstOrDefault(i => i.Name.Equals(targetWeaponName, StringComparison.OrdinalIgnoreCase));

        if (weapon == null)
        {
            Console.WriteLine($"You aren't carrying a '{targetWeaponName}'.");
            return;
        }

        if (!weapon.IsWeapon)
        {
            Console.WriteLine($"You can't wield a {weapon.Name}. It's not a weapon.");
            return;
        }

        // Swap out old weapon if necessary
        if (player.EquippedWeapon != null)
        {
            player.Inventory.Add(player.EquippedWeapon);
            Console.WriteLine($"You stop wielding your {player.EquippedWeapon.Name}.");
        }

        player.Inventory.Remove(weapon);
        player.EquippedWeapon = weapon;

        Console.WriteLine($"You wield the {weapon.Name}! ({weapon.DiceNotation})");
    }
}