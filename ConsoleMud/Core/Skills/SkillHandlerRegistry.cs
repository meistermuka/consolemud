using ConsoleMud.Core.Skills.Handlers.Cleric;
using ConsoleMud.Core.Skills.Handlers.Druid;
using ConsoleMud.Core.Skills.Handlers.Fighter;
using ConsoleMud.Core.Skills.Handlers.Mage;
using ConsoleMud.Core.Skills.Handlers.Ranger;

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
        // Fighter
        Register(new KickHandler());
        Register(new BashHandler());
        Register(new RescueHandler());
        Register(new DisarmHandler());
        Register(new TauntHandler());
        Register(new CleaveHandler());
        Register(new LungeHandler());
        Register(new BerserkHandler());
        Register(new DecapitateHandler());
        Register(new OnslaughtHandler());
        // Fighter passives (armor_optimization, parry, indomitable_will) are applied by
        // PassiveService; critical_mastery and second_wind are wired into combat.

        // Caster tier-1 actives.
        Register(new MagicMissileHandler());
        Register(new MinorHealHandler());
        Register(new EntangleHandler());

        // Cleric actives (passives applied by PassiveService / fired via TriggerBus).
        Register(new BlessHandler());
        Register(new TurnUndeadHandler());
        Register(new CurePoisonHandler());
        Register(new SmiteHandler());
        Register(new SanctuaryHandler());
        Register(new MajorHealHandler());
        Register(new PrayerHandler());
        Register(new DispelMagicHandler());
        Register(new DivineInterventionHandler());
        Register(new JudgmentHandler());
        Register(new ResurrectionHandler());

        // Mage actives (sage_insight/channeling_flow/elemental_mastery/arcane_meditation are passive).
        Register(new ShieldHandler());
        Register(new ShockingGraspHandler());
        Register(new FireballHandler());
        Register(new BlinkHandler());
        Register(new HasteHandler());
        Register(new IceStormHandler());
        Register(new DisintegrateHandler());
        Register(new TimeStopHandler());
        Register(new MeteorSwarmHandler());
        Register(new DetectMagicHandler());
        Register(new TeleportHandler());

        // Ranger (traps + pet subsystems; remaining ranger skills land with that class batch).
        Register(new SetTrapHandler());
        Register(new TameHandler());
        Register(new CallCompanionHandler());

        // Druid shapeshift forms (rest of the druid lands with that class batch).
        Register(new ShapeshiftHandler("shapeshift_bear", Enums.Form.Bear));
        Register(new ShapeshiftHandler("shapeshift_wolf", Enums.Form.Wolf));
        Register(new ShapeshiftHandler("shapeshift_owl", Enums.Form.Owl));
        Register(new ShapeshiftHandler("shapeshift_dragon", Enums.Form.Dragon));
    }

    public void Register(ISkillHandler handler) => _handlers[handler.SkillId] = handler;

    public bool TryGet(string skillId, out ISkillHandler handler) => _handlers.TryGetValue(skillId, out handler);
}
