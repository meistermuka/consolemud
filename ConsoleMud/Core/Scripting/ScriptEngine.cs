using ConsoleMud.Core.Services;
using ConsoleMud.Core.Skills;
using ConsoleMud.Entities;
using ConsoleMud.Helpers;
using MoonSharp.Interpreter;

namespace ConsoleMud.Core.Scripting;

/// <summary>
/// Central scripting service. Loads all *.lua files from the Scripts/ directory
/// at startup, sandboxes them, and dispatches function calls on behalf of game
/// systems (combat tick, NPC AI, room entry, etc.).
///
/// Pattern: static singleton, modelled after TuningRegistry / PassiveService.
///
/// Thread-safety: individual Script instances are NOT thread-safe. All calls into
/// RunFunction must occur on the game's tick-thread (i.e. inside TimeEngine callbacks
/// that already run serially). Do NOT call from multiple concurrent threads.
/// </summary>
public static class ScriptEngine
{
    // script name (filename without extension, relative to Scripts/) → loaded Script
    private static readonly Dictionary<string, Script> _scripts =
        new(StringComparer.OrdinalIgnoreCase);

    private static ScriptApi? _api;
    private static bool _loaded;

    /// <summary>
    /// Scans <paramref name="scriptsRoot"/> recursively for *.lua files, compiles
    /// each one into a sandboxed MoonSharp Script, and injects the game API.
    /// Throws <see cref="SyntaxErrorException"/> on the first syntax error found,
    /// so bad scripts are caught at startup rather than silently at runtime.
    /// </summary>
    public static void Load(string scriptsRoot, WorldState world, SkillExecutor executor, DefinitionRegistry definitions)
    {
        _api = new ScriptApi(world, executor, definitions);
        _scripts.Clear();

        if (!Directory.Exists(scriptsRoot))
        {
            Console.WriteLine($"[ScriptEngine] Scripts directory not found: '{scriptsRoot}' — scripting disabled.");
            _loaded = true;
            return;
        }

        // Register ScriptApi so MoonSharp can marshal it as a Lua userdata object.
        UserData.RegisterType<ScriptApi>();
        UserData.RegisterType<LuaCharacterProxy>();
        UserData.RegisterType<LuaRoomProxy>();
        UserData.RegisterType<LuaSkillContext>();
        UserData.RegisterType<LuaItemProxy>();

        int count = 0;
        foreach (var file in Directory.EnumerateFiles(scriptsRoot, "*.lua", SearchOption.AllDirectories))
        {
            // Key = relative path from scriptsRoot, forward-slashes, no extension.
            // e.g. "npcs/goblin_shaman" or "skills/fire_bolt"
            string key = Path.GetRelativePath(scriptsRoot, file)
                             .Replace('\\', '/')
                             .Replace(".lua", "", StringComparison.OrdinalIgnoreCase);

            var script = new Script(CoreModules.Preset_SoftSandbox);
            script.Globals["game"] = _api;

            // Compile and execute the script body (defines functions, sets globals).
            // Throws SyntaxErrorException immediately if the file is malformed.
            script.DoFile(file);

            _scripts[key] = script;
            count++;
            Console.WriteLine($"[ScriptEngine] Loaded: {key}");
        }

        Console.WriteLine($"[ScriptEngine] {count} script(s) loaded.");
        _loaded = true;
    }

    /// <summary>
    /// Calls a named function in a loaded script, passing the supplied arguments.
    /// If the script or function does not exist the call is silently skipped.
    /// Any Lua runtime error is caught, printed to the game output, and swallowed
    /// so a misbehaving script cannot crash the tick loop.
    /// </summary>
    /// <param name="scriptId">The script key as returned by <see cref="Load"/>
    /// (e.g. "npcs/goblin_shaman").</param>
    /// <param name="fnName">The Lua function name to call (e.g. "on_tick").</param>
    /// <param name="args">Arguments forwarded to the Lua function.</param>
    public static void RunFunction(string scriptId, string fnName, params object[] args)
    {
        if (!_loaded || !_scripts.TryGetValue(scriptId, out var script))
            return;

        var fn = script.Globals.Get(fnName);
        if (fn.IsNil())
            return;

        try
        {
            script.Call(fn, args);
        }
        catch (ScriptRuntimeException ex)
        {
            ColorConsole.WriteLine(
                $"\n[ScriptEngine] Error in '{scriptId}.{fnName}': {ex}",
                ConsoleColor.DarkYellow);
        }
        catch (Exception ex)
        {
            ColorConsole.WriteLine(
                $"\n[ScriptEngine] Unexpected error in '{scriptId}.{fnName}': {ex.Message}",
                ConsoleColor.DarkYellow);
        }
    }

    /// <summary>
    /// Returns true if a script with the given id has been loaded.
    /// Useful for guards before calling <see cref="RunFunction"/>.
    /// </summary>
    public static bool HasScript(string scriptId)
        => _loaded && _scripts.ContainsKey(scriptId);

    /// <summary>
    /// Enumerates all loaded scripts under the <c>skills/</c> prefix and
    /// yields (scriptKey, skillId) pairs for each file that exports a
    /// <c>skill_id</c> string global.
    ///
    /// Scripts without a valid <c>skill_id</c> are skipped with a console warning.
    /// Called by <see cref="Skills.SkillHandlerRegistry.RegisterScriptedSkills"/>.
    /// </summary>
    public static IEnumerable<(string Key, string SkillId)> GetSkillScriptIds()
    {
        foreach (var (key, script) in _scripts)
        {
            if (!key.StartsWith("skills/", StringComparison.OrdinalIgnoreCase))
                continue;

            var val = script.Globals.Get("skill_id");
            if (val.IsNil() || val.Type != MoonSharp.Interpreter.DataType.String)
            {
                Console.WriteLine($"[ScriptEngine] Warning: '{key}.lua' has no skill_id global — skipped.");
                continue;
            }

            yield return (key, val.String);
        }
    }
}
