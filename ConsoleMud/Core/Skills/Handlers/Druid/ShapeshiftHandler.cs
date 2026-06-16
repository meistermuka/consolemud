using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Druid;

/// <summary>One handler shared by all shapeshift skills; bound to a form at registration.</summary>
public class ShapeshiftHandler : ISkillHandler
{
    public string SkillId { get; }
    private readonly Form _form;

    public ShapeshiftHandler(string skillId, Form form)
    {
        SkillId = skillId;
        _form = form;
    }

    public void Execute(SkillContext ctx) => ShapeshiftService.Enter(ctx.Caster, _form, ctx.World);
}
