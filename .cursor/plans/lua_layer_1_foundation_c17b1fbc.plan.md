---
name: Lua Layer 1 Foundation
overview: Add MoonSharp to the project, create the sandboxed ScriptEngine service and ScriptApi surface, and set up the Scripts/ directory. This is the prerequisite for all other scripting layers.
todos:
  - id: l1-package
    content: Add MoonSharp NuGet package reference to ConsoleMud.csproj.
    status: completed
  - id: l1-scriptapi
    content: Create ConsoleMud/Core/Scripting/ScriptApi.cs with print, roll_dice, damage, heal, engage, teleport methods.
    status: completed
  - id: l1-scriptengine
    content: "Create ConsoleMud/Core/Scripting/ScriptEngine.cs: load all *.lua from Scripts/, sandbox with SoftSandbox, cache by name, expose RunFunction with exception guard."
    status: completed
  - id: l1-scripts-dir
    content: Create Scripts/ directory with npcs/, rooms/, skills/ subdirectories and a README.md. Configure *.lua files to copy to output directory in .csproj.
    status: completed
  - id: l1-program
    content: Call ScriptEngine.Load("Scripts", world) in Program.cs after world and all areas are loaded.
    status: completed
isProject: false
---

# Layer 1 — Lua Foundation

## Prerequisites
None. This layer is the base for Layers 2, 3, and 4.

## Package

Add to [`ConsoleMud/ConsoleMud.csproj`](ConsoleMud/ConsoleMud.csproj):

```xml
<PackageReference Include="MoonSharp" Version="2.*" />
```

## New files

### `ConsoleMud/Core/Scripting/ScriptApi.cs`

The only object Lua scripts can call. Wraps `WorldState` and `ColorConsole` behind intentional, safe methods. Raw C# objects are never handed to scripts.

```csharp
public class ScriptApi
{
    private readonly WorldState _world;
    public ScriptApi(WorldState world) => _world = world;

    public void print(string msg) => ColorConsole.WriteLine(msg);
    public int roll_dice(string notation) => DiceRoller.Roll(notation);
    public void damage(string charId, int amount) { /* look up by Id string, apply */ }
    public void heal(string charId, int amount)   { /* look up by Id string, apply */ }
    public void engage(string attackerId, string targetId) { /* set CombatTarget both ways */ }
    public void teleport(string charId, string virtualRoomId)
        { /* _world.TryGetRoomByVirtualId → MoveCharacter */ }
}
```

### `ConsoleMud/Core/Scripting/ScriptEngine.cs`

Singleton service (static class, similar to `TuningRegistry`) that:
- Loads all `*.lua` files under the `Scripts/` directory at startup
- Creates one `MoonSharp.Interpreter.Script` per file, sandboxed with `CoreModules.Preset_SoftSandbox` (removes `io`, `os`, `debug`, `loadfile`, `require`)
- Injects `game = new ScriptApi(world)` as the only global
- Caches by script name (`Dictionary<string, Script>`)
- Throws on syntax errors at load time (fail-loud)
- Exposes `RunFunction(string scriptId, string fnName, params object[] args)` which catches all `ScriptRuntimeException`s, logs them with `ColorConsole`, and returns without crashing the tick loop

```csharp
public static class ScriptEngine
{
    public static void Load(string scriptsRoot, WorldState world) { … }
    public static void RunFunction(string scriptId, string fnName, params object[] args) { … }
}
```

Called once from `Program.cs` after `world` is fully loaded:

```csharp
ScriptEngine.Load("Scripts", world);
```

### `Scripts/` folder structure

```
Scripts/
  npcs/        ← Layer 3
  rooms/       ← Layer 4
  skills/      ← Layer 2
  README.md    ← brief authoring guide
```

Set all `*.lua` files to **Copy to Output Directory: Always** in the `.csproj`.

## What this layer does NOT do

No game behaviour changes. No hooks into the tick loop or commands. The engine loads silently and sits idle until later layers use it.
