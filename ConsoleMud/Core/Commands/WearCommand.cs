using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class WearCommand : ICommand
{
    public string Description => "Wear a piece of armor or equipment, or 'wear all'.";
    public string Usage => "wear <item|all>";
    public string Example => "wear all";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0) { Console.WriteLine("Wear what?"); return; }

        if (args[0].Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            WearAll(player);
            return;
        }

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

    // Wears all armor/accessories (not weapons or shields), filling empty slots only.
    private static void WearAll(Player player)
    {
        var candidates = player.Inventory
            .Where(i => i.IsEquippable
                        && i.TargetSlot != EquipmentSlot.MainHand
                        && i.TargetSlot != EquipmentSlot.OffHand)
            .ToList();

        if (candidates.Count == 0)
        {
            Console.WriteLine("You have nothing to wear. (Weapons and shields are wielded, not worn.)");
            return;
        }

        int worn = 0;
        var skipped = new List<string>();
        foreach (var item in candidates)
        {
            // allowReplace: false -> a full family is skipped, not swapped.
            var resolved = SlotResolver.Resolve(player, item.TargetSlot, allowReplace: false);
            if (resolved is not { } slot)
            {
                skipped.Add(Helpers.ColorMarkup.Strip(item.Name));
                continue;
            }

            player.Inventory.Remove(item);
            player.Equipment[slot] = item;
            worn++;
            Helpers.ColorConsole.WriteLine($"You wear the {item.Name}.", ConsoleColor.Gray);
        }

        Console.WriteLine($"({worn} worn{(skipped.Count > 0 ? $", {skipped.Count} skipped: {string.Join(", ", skipped)}" : "")}.) " +
                          $"[Total Defense: {player.TotalArmourRating}]");
    }
}