using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Commands;

public class WieldCommand : ICommand
{
    public string Description => "Wield a weapon in your main hand.";
    public string Usage => "wield <weapon>";
    public string Example => "wield sword";

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
        
        // Wield always targets the main hand. The off hand is handled by "second".
        var targetSlot = EquipmentSlot.MainHand;

        // Swap out the current main-hand weapon if there is one
        if (player.Equipment.TryGetValue(targetSlot, out var oldWeapon))
        {
            player.Inventory.Add(oldWeapon);
            player.Equipment.Remove(targetSlot);
            Console.WriteLine($"You stop wielding your {oldWeapon.Name}.");
        }

        player.Inventory.Remove(weapon);
        player.Equipment[targetSlot] = weapon;

        Helpers.ColorConsole.WriteLine($"You wield the {weapon.Name} in your main hand! ({weapon.DiceNotation})", ConsoleColor.Gray);
    }
}