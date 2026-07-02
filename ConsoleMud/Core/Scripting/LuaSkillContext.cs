using ConsoleMud.Core.Skills;
using MoonSharp.Interpreter;

namespace ConsoleMud.Core.Scripting;

/// <summary>
/// Read-only proxy passed to Lua skill scripts. Exposes the fields a script
/// needs to carry out its effect without giving it access to raw C# objects.
///
/// The [MoonSharpUserData] attribute auto-registers the type with MoonSharp so
/// it can be handed directly to scripts as a userdata value.
///
/// Lua usage:
///   ctx.caster_id     -- string GUID of the caster
///   ctx.target_id     -- string GUID of the resolved NPC target, or nil
///   ctx.target_name   -- the raw argument string, e.g. "wolf"
///   ctx.spell_power   -- integer bonus from sage_insight passive
///   ctx.heal_bonus    -- integer bonus from divine_grace passive
///   ctx.param("key")  -- numeric tunable from skills.json Parameters bag
/// </summary>
[MoonSharpUserData]
public class LuaSkillContext
{
    private readonly SkillContext _ctx;

    public string caster_id   { get; }
    public string? target_id  { get; }
    public string target_name { get; }
    public int    spell_power { get; }
    public int    heal_bonus  { get; }

    public LuaSkillContext(SkillContext ctx)
    {
        _ctx = ctx;

        caster_id   = ctx.Caster.Id.ToString();
        target_name = ctx.TargetName;
        spell_power = ctx.SpellPowerBonus();
        heal_bonus  = ctx.HealScaleBonus();

        // Pre-resolve so scripts don't need to find the target themselves.
        var npc = ctx.ResolveNpcTarget();
        target_id = npc?.Id.ToString();
    }

    /// <summary>Read a numeric tunable from the skill's Parameters bag.</summary>
    public double param(string key) => _ctx.Param(key);
}
