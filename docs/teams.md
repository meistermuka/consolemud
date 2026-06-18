# Teams (Multi-Body Spirit) — Design

The defining feature of this MUD: the human player is a **spirit** that inhabits
and controls more than one body at a time. Team composition, positioning, and
per-body orders are the core strategy for tackling problems a lone character can't.

> Status: **design only.** No code yet. Implementation is phased (see the checklist).

## Concept

- **The spirit** is the save/account — a named owner that holds a roster of bodies,
  tracks which body is **active** and which is the **main** body, and exposes the
  unlocked-slot count. The spirit has no stats, level, or abilities of its own.
- **A body** is a full player-grade character: its own species, class, level, skills,
  gear, HP/mana, inventory, and position. Bodies reuse the existing `Player`
  capabilities. Resources are fully separate per body.
- One human controls the whole roster. (Multiplayer networking is unrelated and future.)

## Slots and the main body

Body slots unlock by the **main body's level**, and the main body is fixed for the
life of the spirit:

| Main body level | Bodies allowed |
|---|---|
| 1 | 1 |
| 26 | 2 |
| 51 | 3 |
| 76 | 4 |
| 101 | 5 |

Only the main body's level moves this gate; other bodies leveling does not. Crossing
a threshold unlocks a slot, and filling it runs the **full character creation flow**
(species, class, rolled stats), so every body is its own build.

## Control model

- **Active body** — the one you perceive through (look/prompt) and that bare commands target.
- **`switch <body>`** — change the active body.
- **Per-body prefix** — direct a command to one named body (e.g. `grog: kill rat`).
- **Group keyword** — broadcast to all bodies (e.g. `all north`).
- **Standing orders** — group/per-body orders persist across ticks; commands also work as immediate "everyone do X now".
- **Default non-active behavior** — follow the active body and auto-assist its combat.
  A strict order (`stay`, etc.) makes a body **independent** until told to follow again.
- **Follow + switch** — followers move step-by-step with whoever is active; switching
  the active body never teleports the others. **Splitting the party across rooms is
  intentional and useful** (e.g. one body holds a lever while another opens a door).

## Combat, death, and XP

- **Friend/foe** — all bodies (and their pets) are mutually friendly: excluded from
  each other's AoE and from pet targeting. Hostile NPCs aggro any body in their room.
- **Body death** — recalled at 1 HP (same as the current player-death recall). The
  spirit persists while any body lives.
- **XP from a kill** — split among bodies that were **involved** (dealt or took damage
  to/from the slain enemy) **and present in the room**. The body that landed the kill
  gets a **double share**; the rest split the remainder evenly. Uninvolved or absent
  bodies get nothing.

## How it reshapes existing systems

- **Player model** — today `Player : Character` is the single controlled entity. Teams
  add a `Spirit` owner above a roster of `Player` bodies; the "active body" replaces the
  lone `player` reference in `Program` and `CommandParser`.
- **Combat tick** — `CombatSystem` already resolves all characters with a target each
  pulse, and the Ranger pet is the precedent for a player-owned auto-acting ally. Bodies
  generalize that to full controllable characters. Pets belong to a body, not the spirit,
  so a team can include both.
- **Leveling** — `LevelingService` already levels each `Character` independently, so
  per-body leveling is natural. The team XP split (above) feeds it.
- **Persistence** — `SaveService` becomes a `SpiritSave` roster keyed by the spirit's
  name: every body's full state, location, control mode, standing orders, plus the
  active and main references.
- **Look / presence** — "where am I" becomes "where is my active body"; other bodies
  exist elsewhere in the world simultaneously.

## Decisions locked

- Spirit is purely organizational (no stats/level).
- Main body is fixed at creation and gates slots by its level (even when recalled/idle).
- New bodies are created via full character creation; each can be a different class/species.
- Bodies can occupy different rooms; followers move with the active body.
- Body death = recall at 1 HP (for now).
- Resources fully separate; the spirit can inspect any/all bodies.
- "Involved" = dealt or took damage to/from the slain enemy.
- Save is keyed by a spirit name (entered once); bodies are named at their creation.
- XP: killer double share, others involved split the rest evenly.

## Open / deferred

- All-bodies-down handling (spirit does not "die" for now; revisit with a fuller death model).
- Re-designating the main body (currently not allowed).
- Body dismissal / replacing a body in a slot.
