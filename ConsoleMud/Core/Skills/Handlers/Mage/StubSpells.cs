namespace ConsoleMud.Core.Skills.Handlers.Mage;

/// <summary>Needs hidden doors / curses / item-property inspection (not yet built).</summary>
public class DetectMagicHandler : ISkillHandler
{
    public string SkillId => "detect_magic";
    public void Execute(SkillContext ctx) =>
        Helpers.ColorConsole.WriteLine(
            "You sense for magic, but the world has no hidden auras to reveal yet.", ConsoleColor.DarkGray);
}

/// <summary>Needs a grouping/party system and discovered recall points (not yet built).</summary>
public class TeleportHandler : ISkillHandler
{
    public string SkillId => "teleport";
    public void Execute(SkillContext ctx) =>
        Helpers.ColorConsole.WriteLine(
            "You reach for distant coordinates, but no recall destinations exist yet.", ConsoleColor.DarkGray);
}
