using ConsoleMud.Core.Services;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Commands;

public class SpecializeCommand : ICommand
{
    public string Description => "Choose your Mage elemental specialization (fire, cold, lightning).";
    public string Usage => "specialize <fire|cold|lightning>";
    public string Example => "specialize fire";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (player.Class != CharacterClass.Mage)
        {
            Console.WriteLine("Only mages can specialize in an element.");
            return;
        }

        if (args.Length == 0)
        {
            string current = player.Specialization?.ToString() ?? "none";
            Console.WriteLine($"Specialize in what? (fire / cold / lightning). Current: {current}");
            return;
        }

        DamageType chosen = args[0].ToLower() switch
        {
            "fire" => DamageType.Fire,
            "cold" or "ice" => DamageType.Cold,
            "lightning" => DamageType.Lightning,
            _ => DamageType.Physical
        };

        if (chosen == DamageType.Physical)
        {
            Console.WriteLine("Choose one of: fire, cold, lightning.");
            return;
        }

        player.Specialization = chosen;
        Skills.PassiveService.Refresh(player); // (re)applies elemental_mastery immunity if known
        Console.WriteLine($"You attune yourself to {chosen}.");
    }
}
