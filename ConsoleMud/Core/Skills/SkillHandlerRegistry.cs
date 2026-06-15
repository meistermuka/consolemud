using ConsoleMud.Core.Skills.Handlers.Fighter;

namespace ConsoleMud.Core.Skills;

/// <summary>
/// Maps skill ids to their code handlers. Skills with no handler yet are
/// "known but inert" and report that they aren't fully learned. Phase 8 fills
/// this out class by class.
/// </summary>
public class SkillHandlerRegistry
{
    private readonly Dictionary<string, ISkillHandler> _handlers = new(StringComparer.OrdinalIgnoreCase);

    public SkillHandlerRegistry()
    {
        // Vertical-slice handlers proving the pipeline. More arrive in Phase 8.
        Register(new KickHandler());
        Register(new BashHandler());
    }

    public void Register(ISkillHandler handler) => _handlers[handler.SkillId] = handler;

    public bool TryGet(string skillId, out ISkillHandler handler) => _handlers.TryGetValue(skillId, out handler);
}
