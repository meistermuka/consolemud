using ConsoleMud.Core.Combat;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Cleric;

/// <summary>Attackers take holy damage when they land a hit on the warded cleric.</summary>
public class RetributionAuraPassive : IPassiveHandler
{
    public string SkillId => "retribution_aura";
    public SkillTrigger Trigger => SkillTrigger.OnIncomingHit;

    public void OnTrigger(TriggerContext ctx)
    {
        var attacker = ctx.Other;
        if (attacker == null || attacker.Health <= 0)
            return;

        int thorns = (int)PassiveService.SkillParam("retribution_aura", "thornsDamage", 2);
        int dmg = DamageResolver.Apply(attacker, DamageType.Holy, thorns);
        attacker.Health -= dmg;
        Helpers.ColorConsole.WriteLine(
            $"{attacker.Name} is seared for {dmg} holy damage by your retribution aura!", ConsoleColor.Gray);

        if (attacker.Health <= 0)
            DeathService.HandleDeath(attacker, ctx.World, ctx.Owner);
    }
}
