namespace ConsoleMud.Core.Skills;

/// <summary>
/// The code behind an active skill. Resource, cooldown, knowledge, and
/// proficiency checks all happen in SkillExecutor before Execute is called,
/// so a handler only implements the effect.
/// </summary>
public interface ISkillHandler
{
    string SkillId { get; }
    void Execute(SkillContext context);
}
