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
        if (args.Length == 0) { ColorConsole.WriteLine("Wear what?"); return; }

        if (args[0].Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            WearAll(player);
            return;
        }

        string targetName = string.Join(" ", args);
        var item = player.Inventory.FirstOrDefault(i => i.MatchesKeyword(targetName));

        if (item == null)
        {
            ColorConsole.WriteLine($"You aren't carrying a '{targetName}'.");
            return;
        }

        if (!item.IsEquippable)
        {
            ColorConsole.WriteLine($"You cannot equip the {item.Name}.");
            return;
        }

        if (item.TargetSlot == EquipmentSlot.OffHand && item.IsWeapon && player.MainHandWeapon == null)
        {
            ColorConsole.WriteLine("You must wield a weapon in your main hand before equipping an off-hand weapon.");
            return;
        }

        var resolved = SlotResolver.Resolve(player, item.TargetSlot, allowReplace: true);
        if (resolved is not { } slot)
        {
            ColorConsole.WriteLine($"You have no free slot for the {item.Name}.");
            return;
        }

        if (player.Equipment.TryGetValue(slot, out var oldItem))
        {
            player.Inventory.Add(oldItem);
            player.Equipment.Remove(slot);
            ColorConsole.WriteLine($"You stop using the {oldItem.Name}.");
        }

        player.Inventory.Remove(item);
        player.Equipment[slot] = item;

        ColorConsole.WriteLine($"You equip the {item.Name} to your {slot}. " +
                          $"(Armor: +{item.ArmourRating}) [Total Defense: {player.TotalArmourRating}]");
    }

    private static void WearAll(Player player)
    {
        var candidates = player.Inventory
            .Where(i => i.IsEquippable
                        && i.TargetSlot != EquipmentSlot.MainHand
                        && i.TargetSlot != EquipmentSlot.OffHand)
            .ToList();

        if (candidates.Count == 0)
        {
            ColorConsole.WriteLine("You have nothing to wear. (Weapons and shields are wielded, not worn.)");
            return;
        }

        int worn = 0;
        var skipped = new List<string>();
        foreach (var item in candidates)
        {
            var resolved = SlotResolver.Resolve(player, item.TargetSlot, allowReplace: false);
            if (resolved is not { } slot)
            {
                skipped.Add(ColorMarkup.Strip(item.Name));
                continue;
            }

            player.Inventory.Remove(item);
            player.Equipment[slot] = item;
            worn++;
            ColorConsole.WriteLine($"You wear the {item.Name}.", ConsoleColor.Gray);
        }

        ColorConsole.WriteLine($"({worn} worn{(skipped.Count > 0 ? $", {skipped.Count} skipped: {string.Join(", ", skipped)}" : "")}.) " +
                          $"[Total Defense: {player.TotalArmourRating}]");
    }
}
