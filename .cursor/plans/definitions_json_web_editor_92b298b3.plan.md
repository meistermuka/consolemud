---
name: Definitions JSON web editor
overview: A single self-contained HTML file (inline CSS + JS, no dependencies) that edits the five Definitions JSON files through typed, schema-aware forms organized in tabs, with the current data embedded as a starting point, cross-file skill-reference validation, and per-file upload/download.
todos:
  - id: scaffold
    content: "Create tools/definitions-editor.html shell: inline CSS/JS, header with tab bar (Classes/Skills/Species/Forms/Tuning), main split layout, and embedded SEED snapshot of the 5 current Definitions files."
    status: completed
  - id: schema-registry
    content: "Define schema registry: per-file field descriptors (type, label, optional, ordering, inclusion rules) and embedded enum value lists (DamageType, Kind, Attribute)."
    status: completed
  - id: form-renderer
    content: "Implement generic schema-driven widget renderer: text, textarea, int, number, bool, enum select, stringList (Tags), numberMap (Parameters/DamageMultipliers), objectList (class Skills)."
    status: completed
  - id: list-crud
    content: Implement per-tab entry list with add/duplicate/delete/select wired to the form; special-case Tuning as key->{value,desc} rows.
    status: completed
  - id: cross-ref
    content: Populate class Skills SkillId dropdown from loaded skills data with free-text fallback; flag unknown skill references.
    status: completed
  - id: validate-io
    content: Add validation (unique/required Id, numeric inputs) with a non-blocking warnings panel, JSON serialization matching existing formatting, and per-file upload + per-file/all download.
    status: completed
isProject: false
---

# Definitions JSON Web Editor

## Goal

One self-contained file, `tools/definitions-editor.html` (inline CSS + JS, zero dependencies), to add/remove/update entries across the five files in [`ConsoleMud/Definitions/`](ConsoleMud/Definitions/). Loads pre-populated from an embedded snapshot; user uploads to override and downloads to save.

## Layout

- Top tab bar: `Classes`, `Skills`, `Species`, `Forms`, `Tuning`.
- Each tab: left = list of entries (select / add / duplicate / delete); right = typed form for the selected entry.
- Per-tab toolbar: `Upload JSON` (replace this file's data), `Download JSON` (export edited file). Global `Download all` in the header.
- `Tuning` is special-cased: it is a key->{value, desc} map, rendered as a flat list of rows rather than entry+form.

## Schemas (derived from `ConsoleMud/Entities/Definitions/*.cs`)

- Classes ([`ClassDefinition.cs`](ConsoleMud/Entities/Definitions/ClassDefinition.cs)): `Id`, `Name`, `Description`(textarea), `HpBonus`, `ManaBonus`, `HpPerLevel`, `ManaPerLevel` (ints), `Skills` = list of `{SkillId, Level}`.
- Skills ([`SkillDefinition.cs`](ConsoleMud/Entities/Definitions/SkillDefinition.cs)): `Id`, `Name`, `Description`, `Kind`(enum), `IsSpell`(bool), `ManaCost`, `CooldownSeconds`, `DurationTicks` (ints), `DamageType`(enum, optional), `DiceNotation`(string, optional), `AttributeBonus`(enum, optional), `StartingProficiency`(number, default 1.0), `Tags`(string list), `Parameters`(string->number map).
- Species ([`SpeciesDefinition.cs`](ConsoleMud/Entities/Definitions/SpeciesDefinition.cs)): `Id`, `Name`, `Description`, `HasDarkvision`(bool), `Modifiers`{`Str`,`Dex`,`Con`,`Int`,`Wis`,`Cha`} (ints), `DamageMultipliers`(DamageType->number map).
- Forms ([`FormDefinition.cs`](ConsoleMud/Entities/Definitions/FormDefinition.cs)): `Id`, `Name`, `HpBonus`, `ArmorBonus` (ints), `AttackDice`, `AttackVerb` (strings), `AttackAttribute`(enum), `LocksCasting`, `LocksPhysical`(bools), `BreathDice`(string, optional), `TransformMessage`(textarea).
- Tuning ([`tuning.json`](ConsoleMud/Definitions/tuning.json)): map of `key -> { value:number, desc:string }`.

## Enum dropdown values (from `ConsoleMud/Enums/`)

- DamageType: Physical, Poison, Magic, Psychic, Charm, Sleep, Fear, Fire, Cold, Lightning, Force, Holy, Nature.
- Kind: Active, Passive.
- Attribute (AttributeBonus / AttackAttribute): Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma.

Enum selects include a blank option for optional fields, and tolerate an out-of-list value loaded from data (shown as a flagged custom option) rather than silently dropping it.

## Field widgets (generic renderer)

A small schema-driven renderer maps a field descriptor to an input: `text`, `textarea`, `int`, `number`, `bool`(checkbox), `enum`(select), `stringList`(Tags: add/remove chips), `numberMap`(Parameters/DamageMultipliers: key + numeric value rows; key is enum-select for DamageMultipliers, free text for Parameters), and `objectList`(class Skills rows).

## Cross-file behavior

- The class `Skills` editor's `SkillId` is a `<select>` populated from the currently loaded skills data; if skills aren't loaded it falls back to free text.
- Validation pass flags any class `SkillId` not present in skills, and any duplicate/empty `Id` within a file. Issues show as a non-blocking warnings panel; download is still allowed.

## Serialization (round-trip fidelity)

- 2-space indentation to match existing files.
- Per-file field ordering and inclusion rules so output closely matches the originals:
  - Always include `Id`/`Name`; include core numeric fields even when 0 where the source does.
  - Omit optional empties: empty `DiceNotation`/`DamageType`/`AttributeBonus`/`BreathDice` strings, empty `Tags`, empty `Parameters`.
  - Species `DamageMultipliers` is kept even when empty (`{}`), matching the source (e.g. Human).
- Tuning preserves `{ value, desc }` shape and numeric types (no quoting numbers).

## Embedded seed data

- A `const SEED = { classes:[...], skills:[...], species:[...], forms:[...], tuning:{...} }` block holds a snapshot of the current five files so the tool opens populated. A short comment notes the snapshot is static.

## Implementation order

Build the shell + schema registry first, then the generic renderer, then list/CRUD, then cross-references, then validation + serialization + upload/download.
