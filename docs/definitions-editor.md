# Definitions Editor

**File:** [`tools/definitions-editor.html`](../tools/definitions-editor.html)

A browser-based tool for editing the five `Definitions/*.json` files — classes,
skills, species, forms, and tuning. Open the HTML file directly in any modern
browser; no build step, no server, no dependencies.

## What it edits

| Tab | File | Records |
|---|---|---|
| Classes | `Definitions/classes.json` | `ClassDefinition` — HP/mana bonuses, per-level growth, skill progression |
| Skills | `Definitions/skills.json` | `SkillDefinition` — kind, spell flag, cost, cooldown, dice, tags, parameters |
| Species | `Definitions/species.json` | `SpeciesDefinition` — attribute modifiers, damage multipliers, darkvision |
| Forms | `Definitions/forms.json` | `FormDefinition` — shapeshift stats, attack dice/verb, transform message |
| Tuning | `Definitions/tuning.json` | Global balance constants (`key → { value, desc }`) |

---

## Layout

Each tab (except Tuning) follows the same two-panel layout:

- **Left panel** — scrollable list of entries, each showing name and id. Controls: **+ Add** (new entry with default values), **Duplicate** (copy of the selected entry with a unique id), and per-entry **Delete**.
- **Right panel** — typed form for the selected entry. A toolbar offers **Download \<file\>.json** (export this tab's data) and **Delete**.

**Tuning** is special-cased: it renders as a flat table of rows, one per key, with editable key name, numeric value, and description. **+ Add key** appends a new row.

---

## Field widgets

The form renderer maps each field to the appropriate control:

| Widget | Used for |
|---|---|
| Text input | Id, Name, most string fields |
| Textarea | Description, TransformMessage |
| Number / integer | HP/mana bonuses, cooldowns, mana cost, armor rating |
| Checkbox | `IsSpell`, `IsShield`, `HasDarkvision`, `LocksCasting`, etc. |
| Enum dropdown | `Kind` (Active/Passive), `DamageType`, `Attribute`, `TargetSlot` |
| String list | `Tags` — add/remove chip rows |
| Number map (free-text keys) | `Parameters` — key + numeric value rows |
| Number map (enum keys) | `DamageMultipliers` — DamageType select + numeric value |
| Object list | Class `Skills` — SkillId picker + Level, with reorder arrows |

Optional enum fields include a `(none)` blank option. If a loaded value is not in the known enum list it is preserved and shown as a `(custom)` option rather than silently dropped.

---

## Cross-references

The class `Skills` object list uses a dropdown for `SkillId` that is populated from
the currently loaded skills data. If skills have not been loaded the field falls back
to a free-text input. Any class `SkillId` not present in the skills list is
highlighted in red and reported in the warnings panel.

---

## Validation

A non-blocking warnings panel appears below the header when issues exist. The
**Validate** button re-runs the check on demand. Issues flagged:

- Empty or duplicate `Id` within any file.
- Class `SkillId` referencing a skill id not defined in the Skills tab.

Download is allowed regardless of warnings — the panel is advisory only.

---

## Upload and download

Each tab's toolbar has a **Upload** button that replaces that tab's data with a
JSON file from disk (the other tabs are unaffected). A global **Download all**
button in the header downloads all five files at once. Each download uses 2-space
indentation and preserves the field presence of the original source files (e.g.
a skill that had `"ManaCost": 0` in the source will keep it; a skill that omitted
it will continue to omit it).

---

## Embedded seed data

The tool opens pre-populated with a static snapshot of the five definition files
embedded in the HTML. The snapshot is generated at build time and will not update
automatically when you edit the JSON files directly. After making significant
changes to the source files, regenerate the snapshot:

```bash
python3 - << 'EOF'
import re, json

html = open("tools/definitions-editor.html").read()
for key, path in [("classes",  "ConsoleMud/Definitions/classes.json"),
                  ("skills",   "ConsoleMud/Definitions/skills.json"),
                  ("species",  "ConsoleMud/Definitions/species.json"),
                  ("forms",    "ConsoleMud/Definitions/forms.json"),
                  ("tuning",   "ConsoleMud/Definitions/tuning.json")]:
    raw = open(path).read().strip()
    json.loads(raw)   # validate
    html = html.replace(f"/*__SEED_{key}__*/", raw)

open("tools/definitions-editor.html", "w").write(html)
print("done")
EOF
```

---

## Workflow

The tool is an **authoring aid**, not a required step. The normal edit loop is:

1. Open `tools/definitions-editor.html` in a browser.
2. Edit entries using the typed forms.
3. Click **Download \<file\>.json** (or **Download all**) to save.
4. Move the downloaded file(s) to `ConsoleMud/Definitions/`.
5. Rebuild and run the game to pick up the changes.

Alternatively, hand-editing the JSON files directly and reloading the tool is
equally valid — use **Upload** to load the updated file into the session.

---

## Serialization notes

The downloaded JSON matches the format the game's `System.Text.Json` deserializer
expects (property names preserved exactly, 2-space indentation). Field ordering
follows the schema definition order, which matches the C# class declaration order
for the majority of records. The few source files where the original ordering
differed are preserved on a field-presence basis: fields present in the loaded data
are always included in the output even when their value is a default, so round-trip
downloads do not lose fields that the original author explicitly set.
