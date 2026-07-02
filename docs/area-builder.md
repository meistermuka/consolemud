# Area Builder

The recommended way to create and edit area files is the browser-based web tool.
A legacy console wizard is also available and described at the bottom of this page.

---

## Web tool (recommended)

**File:** [`tools/area-editor.html`](../tools/area-editor.html)

Open the file directly in any modern browser — no build step, no server required.

### Opening an area

The header offers three ways to load content:

| Control | What it does |
|---|---|
| **New area** | Starts a blank area with empty item/NPC/room lists |
| **Load example** dropdown | Loads one of the two embedded example areas (The Emerald Forest, fanatics_tower) — useful as a starting point |
| **Upload** | Replaces the current session with an existing area JSON file from disk |

The area name is always shown in the header. Click **Download area** at any time to save the current state; the filename is derived from the area name (e.g. `the_emerald_forest.json`).

### Tabs

#### Area Info

Fields for the area's `Name`, `Description`, and the optional `Author` field (preserved on round-trip; ignored by the game loader). A summary line shows how many items, NPCs, and rooms are currently defined.

#### Items

Left panel: list of all item templates with add / duplicate / delete controls.
Right panel: a typed form for the selected item covering every `ItemBlueprint` field:

- Basic: `VirtualId`, `Name`, `Description`.
- Boolean flags (grouped): `IsGetable`, `IsEquippable`, `IsWeapon`, `IsArmor`, `IsShield`, `IsContainer`, `IsCloseable`, `StartsLocked`, `IsLightSource`, `GrantsDarkvision`.
- Equipment: `TargetSlot` dropdown (all `EquipmentSlot` values), `ArmorRating`.
- Weapon: `WeaponType` dropdown, `DiceNotation`, `AttackVerbs` (add/remove list).
- Container/lock: `KeyId` (marks this item as a key), `LockKeyId` (the key that opens this container).

#### NPCs

Left panel: list of NPC templates.
Right panel: form for the selected NPC:

- Basic: `VirtualId`, `Name`, `Description`.
- Combat stats: `Health`, `MaxHealth`, `Level` (default 1), `XpReward` (0 = auto-formula).
- **`EquippedWeaponTemplateId`** — dropdown populated from the item templates defined in this area; unknown references are shown in red.
- Boolean flags: `IsAggressive`, `HasDarkvision`.
- `Archetypes` — multi-select checkboxes (Humanoid, Animal, Beast, Undead, Fiend, Dragon, Elemental).
- **`ScriptId`** (optional) — relative path to a Lua behaviour script under `Scripts/`, without the extension (e.g. `npcs/goblin_shaman`). Assigns custom per-tick AI that fires every 2 seconds after the default aggressive check. See [scripting.md](scripting.md).

#### Rooms

Left panel: list of rooms.
Right panel: form for the selected room:

- Basic: `VirtualId`, `Name`, `Description`.
- Boolean flags: `IsOutside`, `IsDark`.
- **`ScriptId`** (optional) — relative path to a Lua entry-event script under `Scripts/`, without the extension (e.g. `rooms/throne_room`). The script's `on_enter(character, room)` function fires whenever any character steps into the room, after floor traps are checked. See [scripting.md](scripting.md).
- **Exits editor** — each exit is a row of two dropdowns: direction (North / South / East / West / Up / Down) and target room (populated from the rooms in this area). The **Auto-add reciprocal exit** toggle automatically writes the opposite exit on the target room (N↔S, E↔W, Up↔Down) and removes it when the exit is deleted or retargeted.
- **Spawns editor** — separate sections for item spawns and NPC spawns. Each spawn row is a template dropdown (populated from item/NPC templates) and a count.

#### Map

A read-only SVG diagram auto-laid-out from the exit directions: N/S/E/W exits determine grid position; Up/Down exits appear as small badges on the room node. Click any room node to jump directly to that room in the Rooms tab.

### Colour preview

The `Name` and `Description` fields for Items, NPCs, and Rooms support ConsoleMud
colour markup (e.g. `{Rdire wolf{x`). Each of these fields shows:

- **Colour chip toolbar** — a row of 17 small coloured circles (one per code: `r R g G y Y b B m M c C w W d k x`). Click any chip to insert the corresponding code at the current cursor position in the input. Hover a chip to see its code letter and colour name.
- **Preview pane** — a dark-background read-only div below the input that renders the markup in real time. Plain text with no codes displays unchanged.

The `{x` (reset) chip is shown as a labelled outline circle rather than a filled colour. Full colour code reference: [colour.md](colour.md).

### Validation

A non-blocking warnings panel appears below the header whenever issues are detected. The **Validate** button re-runs the check on demand. Issues flagged:

- Empty or duplicate `VirtualId` within items, NPCs, or rooms.
- Exit targeting a room that does not exist in this area.
- Spawn or NPC `EquippedWeaponTemplateId` referencing a template not defined in this area.
- `LockKeyId` with no matching item `KeyId`.

Download is always allowed regardless of warnings.

### Serialization

The downloaded JSON uses a minimal style matching `emerald_forest.json`: boolean fields are omitted when `false`, strings and arrays when empty, `ArmorRating` when zero, NPC `Level` when 1, `XpReward` when 0. The output is always loadable by `AreaLoaderService` without modification.

### Updating the embedded examples

The two example areas are a static snapshot embedded in the HTML at build time. After making significant changes to the live area files, regenerate the snapshot:

```bash
python3 - << 'EOF'
import re, json

html = open("tools/area-editor.html").read()
for key, path in [("emerald_forest", "ConsoleMud/Areas/emerald_forest.json"),
                  ("fanatics_tower",  "ConsoleMud/Areas/fanatics_tower.json")]:
    raw = open(path).read().strip()
    json.loads(raw)   # validate
    html = html.replace(f"/*__SEED_{key}__*/", raw)

open("tools/area-editor.html", "w").write(html)
print("done")
EOF
```

---

## CLI wizard (legacy)

The original console-mode area builder is still available. It writes a valid area JSON using the real C# blueprint types, so schema drift is impossible.

```bash
dotnet run -- build-area
```

(`Helpers/AreaBuilder.cs`, launched from `Program.Main` when the first argument is `build-area`.)

### Flow

1. **Area meta** — name, description, output filename (sanitized).
2. **Item templates** — id, name, description, getable; guided weapon, armor, and container branches.
3. **NPC templates** — id, name, health, level, XP reward, aggressive, archetypes, optional equipped weapon.
4. **Rooms** — count, then per room: id, name, description, outdoor flag, item/NPC spawns with counts.
5. **Exits** — added after all rooms exist, so destinations are menu picks.
6. **Review → validate → write → load-back.**

### Limits

- Create-oriented: editing an existing area requires re-running the wizard or using the web tool.
- Colour codes are allowed in names/descriptions (see [colour.md](colour.md)).
