using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class StatusCommand : ICommand
{
    public void Execute(Player player, string[] args, WorldState world)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n================== {player.Name.ToUpper()} THE {player.Class.ToString().ToUpper()} ==================");
        Console.ResetColor();
        Console.WriteLine($"  Health: {player.Health,3} / {player.MaxHealth,-3}  |  Mana: {player.Mana,3} / {player.MaxMana,-3}");
        Console.WriteLine("---------------------------------------------------------");
        Console.WriteLine("  [CORE ATTRIBUTES]");
        Console.WriteLine($"  STR (Strength):     {player.Strength,-2}  |  INT (Intelligence): {player.Intelligence,-2}");
        Console.WriteLine($"  DEX (Dexterity):    {player.Dexterity,-2}  |  WIS (Wisdom):       {player.Wisdom,-2}");
        Console.WriteLine($"  CON (Constitution): {player.Constitution,-2}  |  CHA (Charisma):     {player.Charisma,-2}");
        Console.WriteLine("---------------------------------------------------------");
        
        if (player.MainHandWeapon != null)
            Console.WriteLine($"  Weapon: {player.MainHandWeapon.Name} ({player.MainHandWeapon.DiceNotation})");
        Console.WriteLine($"  Total Armor Mitigation: {player.TotalArmourRating}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=========================================================\n");
        Console.ResetColor();
    }
}