using ConsoleMud.Core;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Helpers;

public static class CharacterGenerator
{
    public static Player CreateNewPlayer(Guid startingRoomId)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("=== CHARACTER CREATION ===");
        Console.ResetColor();

        Console.Write("Enter your character's name: ");
        string name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name)) name = "Hero";

        // 1. Roll attributes using 3d6 (Range 3 to 18)
        int str = DiceRoller.Roll("3d6");
        int dex = DiceRoller.Roll("3d6");
        int con = DiceRoller.Roll("3d6");
        int intel = DiceRoller.Roll("3d6");
        int wis = DiceRoller.Roll("3d6");
        int cha = DiceRoller.Roll("3d6");

        Console.WriteLine("\nYour attributes have been rolled (3d6):");
        Console.WriteLine($"  STR: {str}  |  INT: {intel}");
        Console.WriteLine($"  DEX: {dex}  |  WIS: {wis}");
        Console.WriteLine($"  CON: {con}  |  CHA: {cha}\n");

        // 2. Select Class
        Console.WriteLine("Select your Class:");
        Console.WriteLine("  1) Fighter   2) Thief   3) Ranger");
        Console.WriteLine("  4) Cleric    5) Mage    6) Druid");
        
        CharacterClass selectedClass = CharacterClass.Fighter;
        bool validSelection = false;
        
        while (!validSelection)
        {
            Console.Write("Choose a number (1-6): ");
            string choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1": selectedClass = CharacterClass.Fighter; validSelection = true; break;
                case "2": selectedClass = CharacterClass.Thief; validSelection = true; break;
                case "3": selectedClass = CharacterClass.Ranger; validSelection = true; break;
                case "4": selectedClass = CharacterClass.Cleric; validSelection = true; break;
                case "5": selectedClass = CharacterClass.Mage; validSelection = true; break;
                case "6": selectedClass = CharacterClass.Druid; validSelection = true; break;
                default: Console.WriteLine("Invalid choice."); break;
            }
        }

        // 3. Scale Base Vitals off of Attributes and Class Choice
        int baseHp = 50 + (con * 3);  // Constitution scales raw survival capacity
        int baseMana = 10 + (intel * 2); // Intelligence acts as base mana threshold

        switch (selectedClass)
        {
            case CharacterClass.Fighter: baseHp += 30; baseMana = 5; break;   // Pure martial tank
            case CharacterClass.Mage:    baseHp -= 10; baseMana += 40; break; // Fragile magic artillery
            case CharacterClass.Ranger:  baseHp += 15; baseMana += 10; break; 
            case CharacterClass.Druid:   baseHp += 5;  baseMana += 25; break;
            case CharacterClass.Cleric:  baseHp += 10; baseMana += 20; break;
            case CharacterClass.Thief:   baseHp += 5;  baseMana += 10; break;
        }

        // 4. Construct and return final fully formed object
        return new Player
        {
            Name = name,
            Class = selectedClass,
            Strength = str,
            Dexterity = dex,
            Constitution = con,
            Intelligence = intel,
            Wisdom = wis,
            Charisma = cha,
            MaxHealth = baseHp,
            Health = baseHp,
            MaxMana = baseMana,
            Mana = baseMana,
            CurrentRoomId = startingRoomId
        };
    }
}