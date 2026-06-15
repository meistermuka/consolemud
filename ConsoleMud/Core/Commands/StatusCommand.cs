using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class StatusCommand : ICommand
{
    public string Description => "Show your health, mana, attributes, and class.";
    public string Usage => "status";
    public string Example => "status";

    public void Execute(Player player, string[] args, WorldState world)
    {
        Helpers.ColorConsole.WriteLine($"\n================== {player.Name.ToUpper()} THE {player.Class.ToString().ToUpper()} ==================", ConsoleColor.Cyan);
        Helpers.ColorConsole.WriteLine($"  Health: {player.Health,3} / {player.MaxHealth,-3}  |  Mana: {player.Mana,3} / {player.MaxMana,-3}");
        Helpers.ColorConsole.WriteLine("---------------------------------------------------------");
        Helpers.ColorConsole.WriteLine("  [CORE ATTRIBUTES]");
        Helpers.ColorConsole.WriteLine($"  STR (Strength):     {player.Strength,-2}  |  INT (Intelligence): {player.Intelligence,-2}");
        Helpers.ColorConsole.WriteLine($"  DEX (Dexterity):    {player.Dexterity,-2}  |  WIS (Wisdom):       {player.Wisdom,-2}");
        Helpers.ColorConsole.WriteLine($"  CON (Constitution): {player.Constitution,-2}  |  CHA (Charisma):     {player.Charisma,-2}");
        Helpers.ColorConsole.WriteLine("---------------------------------------------------------");
        
        if (player.MainHandWeapon != null)
            Helpers.ColorConsole.WriteLine($"  Weapon: {player.MainHandWeapon.Name} ({player.MainHandWeapon.DiceNotation})");
        Helpers.ColorConsole.WriteLine($"  Total Armor Mitigation: {player.TotalArmourRating}");
        Helpers.ColorConsole.WriteLine("=========================================================\n", ConsoleColor.Cyan);
    }
}