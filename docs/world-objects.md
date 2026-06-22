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

## Doors (Phase 3 — planned)

Closable/lockable exits, declared on both sides and synced as one shared door.

## Light & darkness (Phase 2 — planned)

A room `IsDark` flag; light from a held or floor light source (not from inside a
container); darkvision from species (Dwarf/Elf/Orc), a magical item, or a
`darkvision` spell (Mage/Cleric). Darkness blocks sight and makes fighting blind
harder, easing as the eyes adjust.
