using ConsoleMud.Entities;
using ConsoleMud.Enums;

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
            Console.WriteLine("Wield what in your off hand?");
            return;
        }

        string targetName = string.Join(" ", args);
        var item = player.Inventory.FirstOrDefault(i => i.MatchesKeyword(targetName));

        if (item == null)
        {
            Console.WriteLine($"You aren't carrying a '{targetName}'.");
            return;
        }

        // The off hand only accepts a weapon or a shield
        if (!item.IsWeapon && !item.IsShield)
        {
            Console.WriteLine($"You can't hold the {item.Name} in your off hand.");
            return;
        }

        // Enforce the main-hand-first rule
        if (!player.Equipment.ContainsKey(EquipmentSlot.MainHand))
        {
            Console.WriteLine("You must wield a weapon in your main hand before using your off hand.");
            return;
        }

        // Swap out whatever currently occupies the off hand (weapon or shield)
        if (player.Equipment.TryGetValue(EquipmentSlot.OffHand, out var oldItem))
        {
            player.Inventory.Add(oldItem);
            player.Equipment.Remove(EquipmentSlot.OffHand);
            Console.WriteLine($"You stop using your {oldItem.Name}.");
        }

        player.Inventory.Remove(item);
        player.Equipment[EquipmentSlot.OffHand] = item;

        if (item.IsWeapon)
            Helpers.ColorConsole.WriteLine($"You wield the {item.Name} in your off hand! ({item.DiceNotation})", ConsoleColor.Gray);
        else
            Helpers.ColorConsole.WriteLine($"You ready the {item.Name} in your off hand. " +
                              $"(Armor: +{item.ArmourRating}) [Total Defense: {player.TotalArmourRating}]", ConsoleColor.Gray);
    }
}
