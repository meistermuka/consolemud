using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

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
            ColorConsole.WriteLine("Wield what?");
            return;
        }

        string targetWeaponName = string.Join(" ", args);
        var weapon = player.Inventory.FirstOrDefault(i => i.MatchesKeyword(targetWeaponName));

        if (weapon == null)
        {
            ColorConsole.WriteLine($"You aren't carrying a '{targetWeaponName}'.");
            return;
        }

        if (!weapon.IsWeapon)
        {
            ColorConsole.WriteLine($"You can't wield a {weapon.Name}. It's not a weapon.");
            return;
        }

        var targetSlot = EquipmentSlot.MainHand;

        if (player.Equipment.TryGetValue(targetSlot, out var oldWeapon))
        {
            player.Inventory.Add(oldWeapon);
            player.Equipment.Remove(targetSlot);
            ColorConsole.WriteLine($"You stop wielding your {oldWeapon.Name}.");
        }

        player.Inventory.Remove(weapon);
        player.Equipment[targetSlot] = weapon;

        ColorConsole.WriteLine($"You wield the {weapon.Name} in your main hand! ({weapon.DiceNotation})", ConsoleColor.Gray);
    }
}
