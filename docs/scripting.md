# Lua Scripting

ConsoleMud embeds [MoonSharp](https://www.moonsharp.org/) (a Lua 5.2 interpreter
written in C#) to let area authors add custom behaviour to NPCs, rooms, and skills
without touching or recompiling any C# code.

---

## Architecture overview

The scripting system is split into four layers, each independently optional:

```
Layer 1 — Foundation   ScriptEngine + ScriptApi
Layer 2 — Skills       LuaSkillHandler, Scripts/skills/*.lua
Layer 3 — NPC AI       on_tick, Scripts/npcs/*.lua
Layer 4 — Room events  on_enter, Scripts/rooms/*.lua
```

Layers 2, 3, and 4 all depend on Layer 1. A script file belongs to exactly one
layer, determined by which subdirectory it lives in.

### Files and classes

| File / folder | Role |
|---|---|
| `ConsoleMud/Scripts/` | Root for all Lua files (copied to output directory) |
| `Scripts/skills/*.lua` | Active skill effect scripts (Layer 2) |
| `Scripts/npcs/*.lua` | NPC per-tick behaviour scripts (Layer 3) |
| `Scripts/rooms/*.lua` | Room entry event scripts (Layer 4) |
| `Core/Scripting/ScriptEngine.cs` | Loads, caches, and dispatches all scripts |
| `Core/Scripting/ScriptApi.cs` | The safe API object injected as `game` in every script |
| `Core/Scripting/LuaSkillHandler.cs` | `ISkillHandler` adapter that calls a Lua `execute` function |
| `Core/Scripting/LuaSkillContext.cs` | Read-only proxy passed to skill `execute(ctx)` |
| `Core/Scripting/LuaCharacterProxy.cs` | Read-only proxy for a character (player or NPC) |
| `Core/Scripting/LuaRoomProxy.cs` | Read-only proxy for a room |

---

## Security model

Every script is loaded into its own `MoonSharp.Interpreter.Script` instance with
`CoreModules.Preset_SoftSandbox`. This removes:

- `io` — no file system access
- `os` — no OS access or timing
- `debug` — no reflection or introspection
- `loadfile`, `dofile`, `require` — no importing other files

Standard Lua modules kept: `math`, `string`, `table`, `coroutine`, `bit32`.

The only game-side entry point is the `game` global (a `ScriptApi` instance). Raw
C# objects (`WorldState`, `Character`, `Room`) are never handed to scripts.

Syntax errors abort startup with a clear message. Runtime errors inside
`RunFunction` are caught, printed to the game output as a warning, and swallowed —
a broken script cannot crash the tick loop.

---

## The `game` API

All Lua scripts access the game through the `game` global:

| Method | Signature | Description |
|---|---|---|
| `print` | `game.print(msg)` | Output colour-markup text to the game pane |
| `roll_dice` | `game.roll_dice(notation)` | Roll a dice expression; returns an integer (e.g. `"2d6+3"`) |
| `damage` | `game.damage(char_id, amount)` | Deal raw damage; triggers NPC death if health reaches 0 |
| `heal` | `game.heal(char_id, amount)` | Restore health, capped at `max_health` |
| `engage` | `game.engage(attacker_id, target_id)` | Set mutual combat between two characters; breaks attacker stealth |
| `teleport` | `game.teleport(char_id, virtual_room_id)` | Move a character to a room by its `VirtualId` |

`char_id` and `attacker_id` / `target_id` are always **GUID strings** as provided
by the proxy objects below (e.g. `npc.id`, `player.id`).

---

## Proxy fields

Scripts receive proxy objects, not raw C# references. All fields are read-only.

### `LuaCharacterProxy` — passed as `npc`, `player`, or `character`

| Field | Type | Description |
|---|---|---|
| `id` | string | Runtime GUID (use with `game.*` methods) |
| `name` | string | Plain name (colour codes stripped) |
| `health` | int | Current health |
| `max_health` | int | Maximum health |
| `health_pct` | number | `health / max_health` (0.0 – 1.0) |
| `level` | int | Character level |
| `is_player` | bool | `true` when the character is a player |
| `is_in_combat` | bool | `true` when the character has a combat target |

### `LuaRoomProxy` — passed as `room`

| Field | Type | Description |
|---|---|---|
| `id` | string | Runtime GUID |
| `virtual_id` | string | Stable id from the area file (e.g. `"forest_entrance"`) |
| `name` | string | Plain name (colour codes stripped) |
| `is_outside` | bool | Room has the `IsOutside` flag |
| `is_dark` | bool | Room has the `IsDark` flag |

---

## Layer 2 — Lua skill handlers

A Lua skill replaces a C# `ISkillHandler`. The skill's metadata (mana cost,
cooldown, dice notation) must still exist in `Definitions/skills.json`; only the
runtime effect is in Lua.

### File location

`Scripts/skills/<skill_id>.lua`

### Contract

```lua
skill_id = "thunder_bolt"   -- must match the Id in skills.json

function execute(ctx)
    if ctx.target_id == nil then
        game.print("{YStrike what?{x")
        return
    end
    local dmg = game.roll_dice("2d6") + ctx.spell_power
    game.damage(ctx.target_id, dmg)
    game.print("{YLightning arcs into " .. ctx.target_name .. " for " .. dmg .. " damage!{x")
end
```

### `LuaSkillContext` fields (the `ctx` argument)

| Field / method | Description |
|---|---|
| `ctx.caster_id` | GUID string of the caster |
| `ctx.target_id` | GUID string of the resolved NPC target, or `nil` if none |
| `ctx.target_name` | Raw target argument string (e.g. `"wolf"`) |
| `ctx.spell_power` | Integer bonus from `sage_insight` passive |
| `ctx.heal_bonus` | Integer bonus from `divine_grace` passive |
| `ctx.param(key)` | Numeric tunable from the skill's `Parameters` bag in `skills.json` |

The target is pre-resolved by the engine. If there is no valid target in the room
or combat, `ctx.target_id` is `nil` and the script should return early.

### Registration

Skills are auto-registered. At startup `ScriptEngine` scans every file in
`Scripts/skills/`, reads the `skill_id` global, and registers a `LuaSkillHandler`
in `SkillHandlerRegistry`. No C# change required.

### Checklist

1. Add the skill metadata to `Definitions/skills.json` (name, mana cost, cooldown, dice).
2. Reference the skill id in the class's `Skills` list in `Definitions/classes.json`.
3. Create `Scripts/skills/<skill_id>.lua` with a `skill_id` global and `execute(ctx)`.

---

## Layer 3 — NPC behaviour scripts

Custom per-tick AI runs after the default aggressive-attack check. Any NPC with a
`ScriptId` in its blueprint gets `on_tick` called every AI pulse (every 2 seconds).

### File location

`Scripts/npcs/<any_name>.lua`

### Contract

```lua
function on_tick(npc, room, player)
    -- player is nil when no player is in the room
    if player == nil then return end

    if npc.health_pct < 0.25 then
        game.print("{MThe shaman calls upon dark powers!{x")
        return
    end

    if not npc.is_in_combat then
        game.engage(npc.id, player.id)
    end
end
```

`npc` and `player` are `LuaCharacterProxy` instances. `room` is a `LuaRoomProxy`.
`player` is `nil` when no player is present in the room.

### Assigning to an NPC (area JSON)

```json
{
  "VirtualId": "goblin_shaman",
  "Name": "goblin shaman",
  "ScriptId": "npcs/example_shaman",
  "IsAggressive": false,
  ...
}
```

`ScriptId` is the path to the `.lua` file relative to `Scripts/`, without the
extension. The default aggressive AI still runs for NPCs that also have
`"IsAggressive": true`; the script fires immediately after.

---

## Layer 4 — Room entry scripts

A room script's `on_enter` function fires whenever any character (player or NPC)
enters the room. It fires after floor traps are checked.

### File location

`Scripts/rooms/<any_name>.lua`

### Contract

```lua
function on_enter(character, room)
    if not character.is_player then return end
    game.print("{YA cold wind sweeps through the throne room as you enter...{x")
end
```

`character` is a `LuaCharacterProxy`. `room` is a `LuaRoomProxy`.

### Assigning to a room (area JSON)

```json
{
  "VirtualId": "throne_room",
  "Name": "The Throne Room",
  "ScriptId": "rooms/example_throne_room",
  ...
}
```

All commands that move characters (`move`, `flee`, shapeshift, pet teleport,
player death recall) route through `WorldState.MoveCharacter` and therefore
trigger room scripts automatically.

---

## Colour markup in scripts

The same colour codes used in area file names and descriptions work inside
`game.print`:

```lua
game.print("{Rred text{x normal {Ggreen{x")
```

Full colour reference: [colour.md](colour.md)

---

## Adding a new script (quick reference)

| Goal | File | Required globals |
|---|---|---|
| New Lua skill | `Scripts/skills/my_id.lua` | `skill_id = "my_id"`, `function execute(ctx)` |
| Custom NPC AI | `Scripts/npcs/my_npc.lua` | `function on_tick(npc, room, player)` |
| Room entry event | `Scripts/rooms/my_room.lua` | `function on_enter(character, room)` |

Skills are auto-registered by id. NPC and room scripts are assigned via `"ScriptId"`
in the area JSON (or in the area editor web tool — see [area-builder.md](area-builder.md)).
