using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Skills.Handlers.Cleric;

/// <summary>Mace/club auto-attacks have a chance to stun the target for a round.</summary>
public class HolyFervorPassive : IPassiveHandler
{
    public string SkillId => "holy_fervor";
    public SkillTrigger Trigger => SkillTrigger.OnOutgoingHit;

    public void OnTrigger(TriggerContext ctx)
    {
        var target = ctx.Other;
        var weapon = ctx.Owner.MainHandWeapon;
        if (target == null || target.Health <= 0 || weapon == null)
            return;
        if (weapon.WeaponType != WeaponType.Mace && weapon.WeaponType != WeaponType.Club)
            return;

        if (Random.Shared.NextDouble() >= PassiveService.SkillParam("holy_fervor", "stunChance", 0.15))
            return;

        target.StatusEffects.Add(new StatusEffect
        {
            Name = "holy stun",
            Modifier = EffectModifier.Stun,
            Polarity = EffectPolarity.Negative,
            Type = EffectType.Physical,
            TicksRemaining = 1
        });
        Helpers.ColorConsole.WriteLine($"Your blessed weapon stuns {target.Name}!", ConsoleColor.Gray);
    }
}
