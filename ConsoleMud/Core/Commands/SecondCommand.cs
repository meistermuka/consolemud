using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class SecondCommand : ICommand
{
    public string Description => "Hold a weapon or shield in your off hand.";
    public string Usage => "second <weapon|shield>";
    public string Example => "second shield";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0)
        {
            ColorConsole.WriteLine("Wield what in your off hand?");
            return;
        }

        string targetName = string.Join(" ", args);
        var item = player.Inventory.FirstOrDefault(i => i.MatchesKeyword(targetName));

        if (item == null)
        {
            ColorConsole.WriteLine($"You aren't carrying a '{targetName}'.");
            return;
        }

        if (!item.IsWeapon && !item.IsShield)
        {
            ColorConsole.WriteLine($"You can't hold the {item.Name} in your off hand.");
            return;
        }

        if (!player.Equipment.ContainsKey(EquipmentSlot.MainHand))
        {
            ColorConsole.WriteLine("You must wield a weapon in your main hand before using your off hand.");
            return;
        }

        if (player.Equipment.TryGetValue(EquipmentSlot.OffHand, out var oldItem))
        {
            player.Inventory.Add(oldItem);
            player.Equipment.Remove(EquipmentSlot.OffHand);
            ColorConsole.WriteLine($"You stop using your {oldItem.Name}.");
        }

        player.Inventory.Remove(item);
        player.Equipment[EquipmentSlot.OffHand] = item;

        if (item.IsWeapon)
            ColorConsole.WriteLine($"You wield the {item.Name} in your off hand! ({item.DiceNotation})", ConsoleColor.Gray);
        else
            ColorConsole.WriteLine($"You ready the {item.Name} in your off hand. " +
                              $"(Armor: +{item.ArmourRating}) [Total Defense: {player.TotalArmourRating}]", ConsoleColor.Gray);
    }
}
