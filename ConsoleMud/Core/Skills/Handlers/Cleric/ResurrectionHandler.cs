namespace ConsoleMud.Core.Skills.Handlers.Cleric;

/// <summary>
/// Blocked: resurrection needs the fuller death/corpse model (player corpses,
/// an ethereal state). Until that exists it reports its unavailability.
/// </summary>
public class ResurrectionHandler : ISkillHandler
{
    public string SkillId => "resurrection";

    public void Execute(SkillContext ctx)
    {
        Helpers.ColorConsole.WriteLine(
            "You call upon your deity, but the art of resurrection awaits a world that remembers the dead.",
            ConsoleColor.DarkGray);
    }
}
