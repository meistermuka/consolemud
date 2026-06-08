using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class StatusCommand : ICommand
{
    public void Execute(Player player, string[] args, WorldState world)
    {
        Console.WriteLine("\n=== Character Status ===");
        Console.WriteLine($"Name:    {player.Name}");
        Console.WriteLine($"Health:  {player.Health} / {player.MaxHealth}");
        Console.WriteLine($"Mana:    {player.Mana} / {player.MaxMana}");
        
        if (player.EquippedWeapon != null)
            Console.WriteLine($"Weapon:  {player.EquippedWeapon.Name} ({player.EquippedWeapon.DiceNotation})");
        if (player.EquippedArmour != null)
            Console.WriteLine($"Armor:   {player.EquippedArmour.Name} (DR: {player.EquippedArmour.ArmourRating})");
        
        Console.WriteLine("========================");
    }
}