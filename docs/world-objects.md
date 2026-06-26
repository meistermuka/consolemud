# Containers, Keys, Doors & Light

Interactive world state. Built in phases; this doc grows with each.

> Persistence note: room/container/door/light state is rebuilt from area files
> every boot and is **not** saved. Opening/looting/unlocking is per-session, like
> dropped items and corpses.

## Keys (shared)

A key is an `Item` with a `KeyId` string. A lock (container or, later, a door)
stores the `LockKeyId` it accepts. A character "has the key" when a carried item's
`KeyId` matches the lock's `LockKeyId` (`Character.HasKey`). Keys are never consumed.

## Containers (Phase 1 — done)

`Item` carries the container state: `IsContainer`, `IsCloseable`, `IsOpen`
(default `true`), `IsLocked`, `LockKeyId`.

| Command | Effect |
|---|---|
| `open <container>` | opens it (fails if locked or already open) |
| `close <container>` | closes it (only if closeable) |
| `lock <container>` | locks a closed container — requires the matching key |
| `unlock <container>` | unlocks it — requires the matching key |

`get`, `get all from`, `put`, and `look in` all require the container to be **open**
("the chest is closed"). Corpses are always-open and not closeable.

### Authoring (area JSON / `ItemBlueprint`)

```json
{ "VirtualId": "oak_chest", "Name": "oak chest", "IsContainer": true,
  "IsCloseable": true, "StartsLocked": true, "LockKeyId": "brass", "IsGetable": false }
{ "VirtualId": "brass_key", "Name": "brass key", "KeyId": "brass" }
```

`StartsLocked` implies the container starts closed. The area builder prompts for
these on container items and for `KeyId` on any item.

## Light & darkness (Phase 2 — done)

### Room darkness

A room sets `IsDark: true` in its area blueprint. A dark room blocks sight
entirely unless the viewer has darkvision or a light source.

### Light sources

An item with `IsLightSource: true` illuminates a dark room for its carrier while
held in the inventory or equipped, or while lying on the floor. Light inside a
closed container does **not** count.

### Darkvision

Darkvision lets a character see in a dark room regardless of light. There are
three sources, stacking in `Character.HasDarkvision`:

| Source | How it works |
|---|---|
| Species | Elf, Dwarf, Orc have `HasDarkvision: true` in `species.json`; `CharacterGenerator` sets `InnateDarkvision`. Persisted in `PlayerSave`. |
| Magical item | An item with `GrantsDarkvision: true` grants sight while carried or worn. |
| `darkvision` spell | Mage (L18) and Cleric (L22) cast this active; it applies `EffectModifier.Darkvision` for `DurationTicks: 40` status ticks. `DarkvisionHandler` refreshes any existing effect. |

### Vision gate — `CanSee(room)`

`Character.CanSee(room)` returns true when:

1. The room is not dark, **or**
2. The character has darkvision (`InnateDarkvision`, a darkvision item, or the spell effect), **or**
3. A light source is on the room floor, in the character's inventory, or in their equipment.

### What `CanSee` gates

| Gate | Effect when blind |
|---|---|
| `LookCommand` (room) | "It is pitch black. You can't see a thing." |
| `LookCommand` (target) | "It is too dark to make anything out." |
| `KillCommand` | "It's too dark to make out anything to attack." |
| `SkillContext.ResolveNpcTarget()` | Hostile skill targets cannot be selected. |
| NPC aggro (`TimeEngine`) | Aggressive NPCs that lack darkvision and have no floor light do not initiate combat. |

### Dark combat penalty

When `attacker.CanSee(room)` is false, `CombatSystem.DarknessHitModifier()` applies
a negative to-hit modifier drawn from `tuning.json`:

| Tuning key | Default | Meaning |
|---|---|---|
| `combat.darkMissPenalty` | 50 | Initial to-hit penalty (%) |
| `combat.darkAdaptationPerRound` | 5 | Penalty eased per combat round |
| `combat.darkAdaptationCap` | 30 | Maximum total easing |

After one fight at 50% penalty, eyes adjust by up to 30 points (cap), leaving a
floor of −20%. `DarknessAdaptation` resets to 0 when the combatant leaves combat.

### Authoring (area JSON / `ItemBlueprint` / `RoomBlueprint`)

```json
// Dark room
{ "VirtualId": "cave_entrance", "IsDark": true, ... }

// Light source item
{ "VirtualId": "torch", "Name": "torch", "IsLightSource": true, "IsGetable": true }

// Darkvision item
{ "VirtualId": "goggles_of_night", "GrantsDarkvision": true, "IsEquippable": true, "TargetSlot": "Head" }

// NPC that can see in the dark
{ "VirtualId": "cave_bat", "HasDarkvision": true, "IsAggressive": true, ... }
```

The area builder (`dotnet run -- build-area`) prompts for `IsDark` on rooms,
`IsLightSource`/`GrantsDarkvision` on items, and `HasDarkvision` on NPCs.

### Testing in-game

The `cave_entrance` room in The Emerald Forest (`deep_woods` → north) is a dark
room with a torch and goggles on the floor and an aggressive `cave_bat` (has
darkvision) that aggroes regardless of light. Human/Halfling/Gnome players must
pick up the torch, wear the goggles, or cast `darkvision` to see and fight.
Elf/Dwarf/Orc characters see without any item.

---

## Doors (Phase 3 — planned)

Closable/lockable exits, declared on both sides and synced as one shared door.
