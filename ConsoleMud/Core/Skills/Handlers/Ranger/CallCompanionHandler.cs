using ConsoleMud.Entities;

namespace ConsoleMud.Core.Skills.Handlers.Ranger;

public class CallCompanionHandler : ISkillHandler
{
    public string SkillId => "call_companion";

    public void Execute(SkillContext ctx)
    {
        if (ctx.Caster is Player owner)
            PetSystem.Recall(owner, ctx.World);
    }
}
