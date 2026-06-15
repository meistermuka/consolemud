using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class EquipmentCommand : ICommand
{
    public string Description => "Show everything you have equipped.";
    public string Usage => "equipment";
    public string Example => "eq";

    public void Execute(Player player, string[] args, WorldState world)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n================== YOUR EQUIPPED GEAR ==================");
        Console.ResetColor();

        // Loop through every possible slot defined in the enum
        foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
        {
            // Format the enum name slightly for prettier printing (e.g., "LeftForearm" -> "Left Forearm")
            string slotLabel = FormatSlotLabel(slot.ToString());

            // Check if the character actually has something equipped in this slot
            if (player.Equipment.TryGetValue(slot, out var item))
            {
                // Determine item attributes for display
                string statBonus = "";
                if (item.IsWeapon) 
                    statBonus = $" ({item.DiceNotation})";
                else if (item.ArmourRating > 0) 
                    statBonus = $" [DR: +{item.ArmourRating}]";

                Console.Write($"  <{slotLabel,-15}> ");
                ColorConsole.WriteLine($"{item.Name}{statBonus}");
            }
            else
            {
                // Print a clean, empty placeholder
                Console.Write($"  <{slotLabel,-15}> ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("empty");
                Console.ResetColor();
            }
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("--------------------------------------------------------");
        Console.ResetColor();
        Console.WriteLine($"  Total Armor Mitigation: {player.TotalArmourRating}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("========================================================\n");
        Console.ResetColor();
    }

    // Helper to add clean spacing into camelCase enum strings for display
    private string FormatSlotLabel(string slotName)
    {
        // Simple replacements for readable spacing
        return slotName
            .Replace("Earring1", "Earring (L)")
            .Replace("Earring2", "Earring (R)")
            .Replace("LeftArm", "Left Arm")
            .Replace("RightArm", "Right Arm")
            .Replace("LeftForearm", "L Forearm")
            .Replace("RightForearm", "R Forearm")
            .Replace("Ring1", "Ring (L)")
            .Replace("Ring2", "Ring (R)")
            .Replace("MainHand", "Main Hand")
            .Replace("OffHand", "Off Hand");
    }
}