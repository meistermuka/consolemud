using ConsoleMud.Core.Skills;

namespace ConsoleMud.Core.Scripting;

/// <summary>
/// Bridges the C# skill pipeline to a Lua script file.
/// Implements <see cref="ISkillHandler"/> so it slots into
/// <see cref="SkillHandlerRegistry"/> identically to any C# handler.
///
/// The script file must export:
///   skill_id = "my_skill"        -- matched to skills.json Id
///   function execute(ctx) ... end -- called with a LuaSkillContext
/// </summary>
public class LuaSkillHandler : ISkillHandler
{
    /// <inheritdoc/>
    public string SkillId { get; }

    // Key used by ScriptEngine to locate the script, e.g. "skills/thunder_bolt".
    private readonly string _scriptId;

    public LuaSkillHandler(string skillId, string scriptId)
    {
        SkillId   = skillId;
        _scriptId = scriptId;
    }

    /// <inheritdoc/>
    public void Execute(SkillContext ctx)
    {
        // Build the Lua-safe proxy; pre-resolves the NPC target so the script
        // gets a target_id string rather than a raw C# object.
        var luaCtx = new LuaSkillContext(ctx);
        ScriptEngine.RunFunction(_scriptId, "execute", luaCtx);
    }
}
