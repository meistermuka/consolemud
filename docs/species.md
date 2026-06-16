# How to Add a Species

A species supplies two things at creation: **attribute modifiers** and a **damage
matrix** (resistances/immunities/vulnerabilities). Both are pure data.

## 1. Add the enum value

`Enums/Species.cs`:

```csharp
public enum Species { Human, Elf, Dwarf, Halfling, Gnome, Orc, /* Troll */ }
```

The enum name (case-insensitive) must match the species `Id` in JSON.

## 2. Add the definition

`Definitions/species.json` (`Entities/Definitions/SpeciesDefinition.cs`):

```json
{
  "Id": "troll",
  "Name": "Troll",
  "Description": "Huge and regenerating, but dim and flammable.",
  "Modifiers": { "Str": 3, "Dex": -1, "Con": 2, "Int": -3, "Wis": 0, "Cha": -1 },
  "DamageMultipliers": { "Fire": 2.0, "Poison": 0.5 }
}
```

- **`Modifiers`** are applied on top of the assigned 3d6 rolls during creation (`CreationSteps.ApplyModifiers`).
- **`DamageMultipliers`** map a `DamageType` name to a multiplier: `0` = immune, `0.5` = resistant, `2.0` = vulnerable. Anything unlisted defaults to `1.0`.

## How it flows in

At creation, `CharacterGenerator.BuildPlayer`:

```csharp
foreach (var kv in species.DamageMultipliers)
    if (Enum.TryParse<DamageType>(kv.Key, true, out var dt))
        player.DamageMultipliers[dt] = kv.Value;
```

That `Character.DamageMultipliers` dictionary is exactly what
`DamageResolver.GetDamageMultiplier` reads first when resolving incoming damage
(see [combat.md](combat.md)). So a Troll taking a `Fire` hit is multiplied ×2
before armour-independent reductions apply.

## That's it

Creation lists the species automatically (it enumerates
`DefinitionRegistry.Species`) and prints the modifier and resistance lines on the
selection screen. No code beyond the enum value.

## Notes

- `DamageType` names in JSON must match the `Enums/DamageType.cs` values: Physical, Poison, Magic, Psychic, Charm, Sleep, Fear, Fire, Cold, Lightning, Force, Holy, Nature.
- NPCs can carry their own `DamageMultipliers` too (the resolver is identical for any `Character`); populating them from NPC blueprints is future work.
