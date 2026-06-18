using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class WearCommand : ICommand
{
    public string Description => "Wear a piece of armor or equipment.";
    public string Usage => "wear <item>";
    public string Example => "wear iron helmet";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0) { Console.WriteLine("Wear what?"); return; }

        string targetName = string.Join(" ", args);
        var item = player.Inventory.FirstOrDefault(i => i.MatchesKeyword(targetName));

        if (item == null)
        {
            Console.WriteLine($"You aren't carrying a '{targetName}'.");
            return;
        }

        if (!item.IsEquippable)
        {
            Console.WriteLine($"You cannot equip the {item.Name}.");
            return;
        }

        // A weapon in the off-hand requires a main-hand weapon first (dual-wield rule).
        if (item.TargetSlot == EquipmentSlot.OffHand && item.IsWeapon && player.MainHandWeapon == null)
        {
            Console.WriteLine("You must wield a weapon in your main hand before equipping an off-hand weapon.");
            return;
        }

        // Resolve to the first free physical slot in the item's family, replacing the oldest if full.
        var resolved = SlotResolver.Resolve(player, item.TargetSlot, allowReplace: true);
        if (resolved is not { } slot)
        {
            Console.WriteLine($"You have no free slot for the {item.Name}.");
            return;
        }

        if (player.Equipment.TryGetValue(slot, out var oldItem))
        {
            player.Inventory.Add(oldItem);
            player.Equipment.Remove(slot);
            Console.WriteLine($"You stop using the {oldItem.Name}.");
        }

        player.Inventory.Remove(item);
        player.Equipment[slot] = item;

        Helpers.ColorConsole.WriteLine($"You equip the {item.Name} to your {slot}. " +
                          $"(Armor: +{item.ArmourRating}) [Total Defense: {player.TotalArmourRating}]");
    }
}