using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class WearCommand : ICommand
{
    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0) { Console.WriteLine("Wear what?"); return; }

        string targetName = string.Join(" ", args);
        var armor = player.Inventory.FirstOrDefault(i => i.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));

        if (armor == null) { Console.WriteLine($"You aren't carrying a '{targetName}'."); return; }
        if (!armor.IsArmour) { Console.WriteLine($"You can't wear a {armor.Name}. It offers no protection."); return; }

        if (player.EquippedArmour != null)
        {
            player.Inventory.Add(player.EquippedArmour);
            Console.WriteLine($"You remove your {player.EquippedArmour.Name}.");
        }

        player.Inventory.Remove(armor);
        player.EquippedArmour = armor;
        Console.WriteLine($"You wear the {armor.Name}. [Armor Rating: +{armor.ArmourRating}]");
    }
}