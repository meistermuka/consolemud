using ConsoleMud.Core.Services;
using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class SkillsCommand : ICommand
{
    private readonly DefinitionRegistry _definitions;

    public SkillsCommand(DefinitionRegistry definitions) => _definitions = definitions;

    public string Description => "List your class skills, their unlock level, and your proficiency.";
    public string Usage => "skills";
    public string Example => "skills";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (!_definitions.Classes.TryGetValue(player.Class.ToString(), out var cls))
        {
            Console.WriteLine("You have no class skills.");
            return;
        }

        ColorConsole.WriteLine($"\n=== Skills: {cls.Name} (Level {player.Level}) ===", ConsoleColor.Cyan);

        foreach (var entry in cls.Skills.OrderBy(s => s.Level))
        {
            string name = _definitions.Skills.TryGetValue(entry.SkillId, out var def) ? def.Name : entry.SkillId;

            if (player.KnownSkills.TryGetValue(entry.SkillId, out var proficiency))
            {
                ColorConsole.WriteLine($"  Lv {entry.Level,3}  {name,-26} {proficiency,5:F1}%", ConsoleColor.Green);
            }
            else
            {
                string status = player.Level >= entry.Level ? "not learned" : $"unlocks at Lv {entry.Level}";
                ColorConsole.WriteLine($"  Lv {entry.Level,3}  {name,-26} {status}", ConsoleColor.DarkGray);
            }
        }

        Console.WriteLine();
    }
}
