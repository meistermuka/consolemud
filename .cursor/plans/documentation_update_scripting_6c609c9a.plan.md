---
name: Documentation update scripting
overview: Create a new scripting.md reference and update README.md, skills.md, area-builder.md, and tick-system.md to reflect the Lua scripting system (Layers 1–5) implemented this session, plus the colour preview feature added to the area editor.
todos:
  - id: doc-scripting-md
    content: Create docs/scripting.md — comprehensive Lua scripting reference covering all four layers, the game API, proxy field tables, and the three script contracts (skill/NPC/room).
    status: completed
  - id: doc-readme
    content: Update docs/README.md — add three rows to the How to extend table and scripting.md to Other references.
    status: completed
  - id: doc-skills
    content: Update docs/skills.md — add Lua skill handler alternative section after the existing checklist.
    status: completed
  - id: doc-area-builder
    content: Update docs/area-builder.md — add ScriptId field descriptions under NPCs and Rooms tabs, and a Colour preview subsection.
    status: completed
  - id: doc-tick
    content: Update docs/tick-system.md — note that UpdateNpcIntelligence now fires NPC scripts for scripted NPCs.
    status: completed
isProject: false
---

# Documentation Update

## What changed in this session

- **Area editor colour preview** — live colour chip toolbar and rendered preview for Name/Description fields in Items, NPCs, and Rooms.
- **Lua scripting system (Layers 1–5)**:
  - `ScriptEngine`, `ScriptApi`, sandboxed script loading from `Scripts/`
  - Lua-backed active skill handlers (`LuaSkillHandler`, `LuaSkillContext`)
  - NPC per-tick behaviour scripts (`Scripts/npcs/`, `NpcBlueprint.ScriptId`)
  - Room entry event scripts (`Scripts/rooms/`, `RoomBlueprint.ScriptId`, `Room.ScriptId`)
  - `ScriptId` fields in the area editor web tool for NPCs and Rooms

---

## Files to change

### 1. New `docs/scripting.md`

The main reference for the entire Lua system. Covers:

- **Overview** — the four layers (foundation, skills, NPC behaviour, room events) and the dependency graph
- **Security model** — `SoftSandbox`: what is removed (`io`, `os`, `debug`, `loadfile`, `require`), what is kept (`math`, `string`, `table`)
- **The `game` API** — table of all six methods: `print`, `roll_dice`, `damage`, `heal`, `engage`, `teleport`
- **Character proxy fields** (`LuaCharacterProxy`) — `id`, `name`, `health`, `max_health`, `health_pct`, `level`, `is_player`, `is_in_combat`
- **Room proxy fields** (`LuaRoomProxy`) — `id`, `virtual_id`, `name`, `is_outside`, `is_dark`
- **NPC behaviour scripts** — file location (`Scripts/npcs/`), the `on_tick(npc, room, player)` contract, assigning via `"ScriptId"` in the area JSON NPC template
- **Room entry scripts** — file location (`Scripts/rooms/`), the `on_enter(character, room)` contract, assigning via `"ScriptId"` in the area JSON room blueprint
- **Lua skill handlers** — file location (`Scripts/skills/`), the `skill_id` global + `execute(ctx)` function, the `LuaSkillContext` fields (`caster_id`, `target_id`, `target_name`, `spell_power`, `heal_bonus`, `ctx.param(key)`), plus the required `skills.json` metadata entry

### 2. `docs/README.md`

- Add three rows to the "How to extend" table:

| I want to add a... | Read |
|---|---|
| Scripted NPC behaviour (Lua) | scripting.md |
| Room entry event (Lua) | scripting.md |
| Skill effect in Lua instead of C# | scripting.md |

- Add `scripting.md` to the "Other references" paragraph.

### 3. `docs/skills.md`

Add a new final section **"Alternative: Lua skill handlers"** after the existing checklist. Key points:
- Create `Scripts/skills/my_skill.lua` with `skill_id = "my_skill"` and `function execute(ctx)`
- Still requires the `skills.json` metadata entry (cost, cooldown, dice)
- No C# recompile — engine auto-scans and registers at startup
- Cross-reference to scripting.md for the full ctx field table

### 4. `docs/area-builder.md`

Under the **NPCs** tab section, add:
- `ScriptId` (optional) — the relative path to a Lua behaviour script (e.g. `npcs/goblin_shaman`). Assigns custom per-tick AI. See `scripting.md`.

Under the **Rooms** tab section, add:
- `ScriptId` (optional) — the relative path to a Lua entry-event script (e.g. `rooms/throne_room`). Fires `on_enter` when any character steps into the room. See `scripting.md`.

Under the **Colour preview** section (new subsection), explain:
- `Name` and `Description` fields for Items, NPCs, and Rooms now show a live preview panel and a colour chip toolbar for inserting colour codes. Clicking a chip inserts the code at the cursor position in the input.

### 5. `docs/tick-system.md`

In the `AiInterval` row and the AI description, add a note:
- `UpdateNpcIntelligence` now also fires `ScriptEngine.RunFunction(npc.ScriptId, "on_tick", ...)` after the default aggressive check, for any NPC whose blueprint includes a `ScriptId`. See `scripting.md`.
