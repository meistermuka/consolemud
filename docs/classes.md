# How to Add a Class

A class is data plus an enum value. Skills supply its behaviour (see
[skills.md](skills.md)), so a new class is mostly a JSON entry and a progression table.

## 1. Add the enum value

`Enums/CharacterClass.cs`:

```csharp
public enum CharacterClass { Fighter, Thief, Ranger, Cleric, Mage, Druid, /* Paladin */ }
```

The enum name (case-insensitive) must equal the class `Id` in JSON — creation maps
`ClassDefinition.Id` to the enum with `Enum.TryParse`.

## 2. Add the definition

`Definitions/classes.json` (`Entities/Definitions/ClassDefinition.cs`):

```json
{
  "Id": "paladin",
  "Name": "Paladin",
  "Description": "A holy warrior.",
  "HpBonus": 25,          // one-time creation bonus
  "ManaBonus": 10,
  "HpPerLevel": 10,        // added each level-up (plus CON modifier)
  "ManaPerLevel": 4,       // added each level-up (plus INT modifier)
  "Skills": [
    { "SkillId": "kick",  "Level": 1 },
    { "SkillId": "smite", "Level": 5 }
  ]
}
```

- **Vitals at creation** (`Helpers/CharacterGenerator.cs`): `50 + CON*3 + HpBonus` HP, `10 + INT*2 + ManaBonus` mana.
- **Vitals on level-up** (`Core/Services/LevelingService.cs`): `+HpPerLevel + (CON-10)/2` HP, `+ManaPerLevel + (INT-10)/2` mana, and the character fully heals.
- **Skills** unlock at the listed `Level`. A skill id with no definition in `skills.json` triggers a load-time warning (a handy to-do list).

## 3. Reuse or add skills

Classes can share skills by id (e.g. several share `kick`). For new behaviour, add
the skill per [skills.md](skills.md). The level-1 skill is what a freshly created
character of this class starts with.

## That's it

Creation lists the class automatically (it enumerates `DefinitionRegistry.Classes`),
seeds the level-1 skills into `KnownSkills`, and the `skills` command shows the full
table. No code changes beyond the enum value and any genuinely new skill handlers.

## Notes

- **Mage specialization** (Fire/Cold/Lightning) is stored on `Player.Specialization`; the level-gated prompt that sets it is part of the progression work.
- Class-restricted gear or species/class gating do not exist yet — every species/class combo is legal.
