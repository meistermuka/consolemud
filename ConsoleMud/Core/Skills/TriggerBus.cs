using ConsoleMud.Entities;

namespace ConsoleMud.Core.Skills;

public enum SkillTrigger
{
    OnIncomingHit,
    OnOutgoingHit,
    OnCast,
    OnIncomingSpell,
    OnLook,
    OnMaxRoll,
    OnLowHealth
}

/// <summary>
/// Context handed to a passive when its trigger fires.
/// </summary>
public class TriggerContext
{
    public Character Owner { get; init; }
    public Character Other { get; init; }   // attacker/defender/target, depending on trigger
    public WorldState World { get; init; }
    public object Payload { get; init; }     // trigger-specific data (e.g. damage amount)
}

/// <summary>
/// A passive that reacts to a game event. Phase 8 implements these; the bus
/// is wired now so combat/look/cast fire points have somewhere to call.
/// </summary>
public interface IPassiveHandler
{
    string SkillId { get; }
    SkillTrigger Trigger { get; }
    void OnTrigger(TriggerContext context);
}

/// <summary>
/// Routes game events to subscribed passives. A passive only fires for an owner
/// who has learned its skill; proficiency scaling happens inside the handler.
/// </summary>
public class TriggerBus
{
    private readonly Dictionary<SkillTrigger, List<IPassiveHandler>> _subscribers = new();

    public void Register(IPassiveHandler handler)
    {
        if (!_subscribers.TryGetValue(handler.Trigger, out var list))
        {
            list = new List<IPassiveHandler>();
            _subscribers[handler.Trigger] = list;
        }
        list.Add(handler);
    }

    public void Fire(SkillTrigger trigger, TriggerContext context)
    {
        if (context.Owner == null || !_subscribers.TryGetValue(trigger, out var list))
            return;

        foreach (var handler in list)
            if (context.Owner.KnownSkills.ContainsKey(handler.SkillId))
                handler.OnTrigger(context);
    }
}
