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
        ColorConsole.WriteLine("\n================== YOUR EQUIPPED GEAR ==================", ConsoleColor.Cyan);

        foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
        {
            string slotLabel = FormatSlotLabel(slot.ToString());

            if (player.Equipment.TryGetValue(slot, out var item))
            {
                string statBonus = "";
                if (item.IsWeapon)
                    statBonus = $" ({item.DiceNotation})";
                else if (item.ArmourRating > 0)
                    statBonus = $" [DR: +{item.ArmourRating}]";

                ColorConsole.Write($"  <{slotLabel,-15}> ");
                ColorConsole.WriteLine($"{item.Name}{statBonus}");
            }
            else
            {
                ColorConsole.Write($"  <{slotLabel,-15}> ");
                ColorConsole.WriteLine("empty", ConsoleColor.DarkGray);
            }
        }

        ColorConsole.WriteLine("--------------------------------------------------------", ConsoleColor.Cyan);
        ColorConsole.WriteLine($"  Total Armor Mitigation: {player.TotalArmourRating}");
        ColorConsole.WriteLine("========================================================\n", ConsoleColor.Cyan);
    }

    private string FormatSlotLabel(string slotName)
    {
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
