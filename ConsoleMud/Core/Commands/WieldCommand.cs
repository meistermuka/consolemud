using ConsoleMud.Entities;
using ConsoleMud.Enums;

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
        
        var targetSlot = weapon.TargetSlot;

        if (targetSlot != EquipmentSlot.MainHand && targetSlot != EquipmentSlot.OffHand)
        {
            targetSlot = EquipmentSlot.MainHand;
        }

        if (targetSlot == EquipmentSlot.OffHand && player.MainHandWeapon == null)
        {
            Console.WriteLine("You must wield a weapon in your MainHand before dual-wielding an OffHand weapon.");
            return;
        }

        // Swap out old weapon if necessary
        if (player.Equipment.TryGetValue(targetSlot, out var oldWeapon))
        {
            player.Inventory.Add(oldWeapon);
            player.Equipment.Remove(targetSlot);
            Console.WriteLine($"You stop wielding your {oldWeapon.Name}.");
        }

        player.Inventory.Remove(weapon);
        player.Equipment[targetSlot] = weapon;

        Console.WriteLine($"You wield the {weapon.Name}! ({weapon.DiceNotation})");
    }
}