using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Ranger;

public class TameHandler : ISkillHandler
{
    public string SkillId => "tame";

    public void Execute(SkillContext ctx)
    {
        if (ctx.Caster is not Player owner)
            return;

        var beast = ctx.ResolveNpcTarget();
        if (beast == null)
        {
            ColorConsole.WriteLine("Tame what?");
            return;
        }
        if (beast.IsPet)
        {
            ColorConsole.WriteLine($"{beast.Name} is already a companion.");
            return;
        }
        if (!beast.Archetypes.Contains(Archetype.Animal) && !beast.Archetypes.Contains(Archetype.Beast))
        {
            Helpers.ColorConsole.WriteLine($"{beast.Name} is not a wild animal you can tame.", ConsoleColor.Gray);
            return;
        }

        PetSystem.Tame(owner, beast, ctx.World);
    }
}
