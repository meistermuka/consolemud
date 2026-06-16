namespace ConsoleMud.Core.Skills.Handlers.Thief;

/// <summary>Needs the lock system on doors/containers (not yet built).</summary>
public class PicklockHandler : ISkillHandler
{
    public string SkillId => "picklock";
    public void Execute(SkillContext ctx) =>
        Helpers.ColorConsole.WriteLine("You ready your picks, but nothing here is locked yet.", ConsoleColor.DarkGray);
}

/// <summary>Needs NPC exit-blocking (not yet built).</summary>
public class ShadowstepHandler : ISkillHandler
{
    public string SkillId => "shadowstep";
    public void Execute(SkillContext ctx) =>
        Helpers.ColorConsole.WriteLine("There is no one blocking a path to slip past.", ConsoleColor.DarkGray);
}
