namespace ConsoleMud.Core.Skills.Handlers.Druid;

/// <summary>Needs hidden exits / caches / boss-location data (not yet built).</summary>
public class NaturesSpeechHandler : ISkillHandler
{
    public string SkillId => "natures_speech";

    public void Execute(SkillContext ctx) =>
        Helpers.ColorConsole.WriteLine(
            "You commune with the wild things, but they have no secrets to share yet.", ConsoleColor.DarkGray);
}
