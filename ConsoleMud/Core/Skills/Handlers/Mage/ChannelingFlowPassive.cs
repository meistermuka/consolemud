using ConsoleMud.Entities.Definitions;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Mage;

/// <summary>A cast sometimes refunds half its mana cost.</summary>
public class ChannelingFlowPassive : IPassiveHandler
{
    public string SkillId => "channeling_flow";
    public SkillTrigger Trigger => SkillTrigger.OnCast;

    public void OnTrigger(TriggerContext ctx)
    {
        if (ctx.Payload is not SkillDefinition def || def.ManaCost <= 0)
            return;

        if (Random.Shared.NextDouble() >= PassiveService.SkillParam("channeling_flow", "refundChance", 0.20))
            return;

        int refund = (int)(def.ManaCost * PassiveService.SkillParam("channeling_flow", "refundFraction", 0.5));
        if (refund <= 0)
            return;

        ctx.Owner.Mana = Math.Min(ctx.Owner.MaxMana, ctx.Owner.Mana + refund);
        Helpers.ColorConsole.WriteLine($"Arcane efficiency refunds {refund} mana.", ConsoleColor.DarkGray);
    }
}
