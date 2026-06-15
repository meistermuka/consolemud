using ConsoleMud.Core.Services;
using ConsoleMud.Entities;

namespace ConsoleMud.Core.Skills;

/// <summary>
/// Drives an active skill use: knowledge, cooldown, and mana checks, then a
/// proficiency roll (gaining on every attempt) before invoking the handler.
/// </summary>
public class SkillExecutor
{
    private readonly DefinitionRegistry _definitions;
    private readonly SkillHandlerRegistry _handlers;

    public SkillExecutor(DefinitionRegistry definitions, SkillHandlerRegistry handlers)
    {
        _definitions = definitions;
        _handlers = handlers;
    }

    /// <summary>
    /// Attempts to use a skill. Returns false only when the id isn't a real
    /// skill, so the caller can fall through to "unknown command". Any real
    /// skill (known or not, ready or not) returns true after messaging.
    /// </summary>
    public bool TryUse(Character caster, string skillId, string[] args, WorldState world)
    {
        if (!_definitions.Skills.TryGetValue(skillId, out var def))
            return false; // not a skill at all

        if (!string.Equals(def.Kind, "Active", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"{def.Name} is a passive skill; it works on its own.");
            return true;
        }

        if (!caster.KnownSkills.ContainsKey(skillId))
        {
            Console.WriteLine($"You haven't learned {def.Name}.");
            return true;
        }

        if (caster.IsStunned)
        {
            Console.WriteLine("You are stunned and cannot act.");
            return true;
        }

        if (def.IsSpell && caster.IsBlinded)
        {
            Console.WriteLine("You are blinded and cannot focus a spell.");
            return true;
        }

        if (IsOnCooldown(caster, skillId, out double secondsLeft))
        {
            Console.WriteLine($"{def.Name} is on cooldown. Wait {secondsLeft:F1}s.");
            return true;
        }

        if (def.IsSpell && caster.Mana < def.ManaCost)
        {
            Console.WriteLine($"You don't have enough mana for {def.Name} ({def.ManaCost} needed, {caster.Mana} available).");
            return true;
        }

        if (!_handlers.TryGet(skillId, out var handler))
        {
            Console.WriteLine($"You know of {def.Name}, but haven't learned to wield its full effect yet.");
            return true;
        }

        // Attempt committed: using a skill (other than hiding) breaks stealth.
        if (!string.Equals(skillId, "hide", StringComparison.OrdinalIgnoreCase))
            caster.BreakHidden();

        // Spend resources, start cooldown, train proficiency.
        if (def.IsSpell && def.ManaCost > 0)
            caster.Mana -= def.ManaCost;
        if (def.CooldownSeconds > 0)
            caster.Cooldowns[skillId] = DateTime.UtcNow.AddSeconds(def.CooldownSeconds);

        double proficiency = caster.KnownSkills[skillId];
        bool success = ProficiencyMath.RollSuccess(proficiency);
        caster.KnownSkills[skillId] = ProficiencyMath.Gain(proficiency);

        if (!success)
        {
            Console.WriteLine($"You fail to execute {def.Name}.");
            return true;
        }

        handler.Execute(new SkillContext(caster, world, def, args, _definitions));
        return true;
    }

    private static bool IsOnCooldown(Character caster, string skillId, out double secondsLeft)
    {
        secondsLeft = 0;
        if (caster.Cooldowns.TryGetValue(skillId, out var readyAt) && DateTime.UtcNow < readyAt)
        {
            secondsLeft = (readyAt - DateTime.UtcNow).TotalSeconds;
            return true;
        }
        return false;
    }
}
