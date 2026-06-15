using ConsoleMud.Entities;
using ConsoleMud.Enums;

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
        
        // 1. Resolve the specific slot destination based on item type and duplicates
        EquipmentSlot resolvedSlot = DetermineTargetSlot(player, item);

        // 2. Dual-wielding safety validation checks
        if (!ValidateGripRules(player, item, resolvedSlot))
        {
            return; // Error message handled inside validation method
        }

        // 3. Remove existing item in that slot if it's occupied
        if (player.Equipment.TryGetValue(resolvedSlot, out var oldItem))
        {
            player.Inventory.Add(oldItem);
            player.Equipment.Remove(resolvedSlot);
            Console.WriteLine($"You stop using the {oldItem.Name}.");
        }

        // 4. Equip the new item
        player.Inventory.Remove(item);
        player.Equipment[resolvedSlot] = item;

        Helpers.ColorConsole.WriteLine($"You equip the {item.Name} to your {resolvedSlot}. " +
                          $"(Armor: +{item.ArmourRating}) [Total Defense: {player.TotalArmourRating}]");
    }
    
    private EquipmentSlot DetermineTargetSlot(Player player, Item item)
    {
        // Special case: Rings shift to slot 2 if slot 1 is full
        if (item.TargetSlot == EquipmentSlot.Ring1 && player.Equipment.ContainsKey(EquipmentSlot.Ring1))
        {
            if (!player.Equipment.ContainsKey(EquipmentSlot.Ring2)) return EquipmentSlot.Ring2;
        }

        // Special case: Earrings shift to slot 2 if slot 1 is full
        if (item.TargetSlot == EquipmentSlot.Earring1 && player.Equipment.ContainsKey(EquipmentSlot.Earring1))
        {
            if (!player.Equipment.ContainsKey(EquipmentSlot.Earring2)) return EquipmentSlot.Earring2;
        }

        return item.TargetSlot;
    }
    
    private bool ValidateGripRules(Player player, Item item, EquipmentSlot slot)
    {
        // Trying to put a weapon in the off-hand to dual-wield
        if (slot == EquipmentSlot.OffHand && item.IsWeapon)
        {
            var mainHand = player.MainHandWeapon;
            if (mainHand == null)
            {
                Console.WriteLine("You must equip a weapon in your MainHand before dual-wielding an OffHand weapon.");
                return false;
            }
        }

        // Trying to put a shield in the off-hand
        if (slot == EquipmentSlot.OffHand && item.IsShield)
        {
            // Allowed completely as long as the hand is open (handled by standard replacement logic)
            return true;
        }

        return true;
    }
}