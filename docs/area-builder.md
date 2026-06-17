# Area Builder (interactive)

An offline wizard that writes a valid area JSON file. It assembles the real
`AreaBlueprint`/`ItemBlueprint`/`NpcBlueprint`/`RoomBlueprint` objects and
serializes them with the same `System.Text.Json` the loader uses, so the output
cannot drift from the schema. After writing, it reloads the file through
`AreaLoaderService` as a final check.

## Run it

```bash
dotnet run -- build-area
```

(`Helpers/AreaBuilder.cs`, launched from `Program.Main` when the first arg is `build-area`.)

## Flow

1. **Area meta** — name, description, output filename (sanitized).
2. **Item templates** — id, name, description, getable; guided weapon (type/dice/verbs/slot), armor (rating/slot/shield), and container branches.
3. **NPC templates** — id, name, description, health, level, XP reward, aggressive, archetypes (multi-select), optional equipped weapon (picked from defined items).
4. **Rooms** — a count, then per room: id, name, description, outdoor flag, and item/NPC spawns picked from the templates with counts.
5. **Exits** — added after all rooms exist, so destinations are picks from the room list; offers to auto-create the reciprocal exit.
6. **Review → validate → write → load-back.**

## Why this order

Templates first so room spawns are menu picks (no typos); exits last so every
destination is a pick from the finished room list.

## Validation before writing

- Unique VirtualIds across rooms, items, and NPCs.
- Every exit targets a real room; every spawn / equipped-weapon id is a defined template.
- Dice notation matches `NdM`; enum choices come from menus so they always parse.

If problems are found it lists them and asks before writing. The trailing
load-back surfaces any loader warning.

## Notes & limits

- Create-oriented: editing an existing area means re-running or hand-editing the JSON.
- Colour codes are allowed in names/descriptions (see [colour.md](colour.md)).
- A complementary `validate-area <file>` command (for hand-edited files) and in-game
  OLC are the natural next steps; see the design discussion.
