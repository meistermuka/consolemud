# Combat, Status Effects & Damage Resolution

## The combat round

`CombatSystem.Tick` (`Core/CombatSystem.cs`) runs once per combat pulse (1 s). It
ages crowd-control effects, then for every character with a `CombatTarget` it
validates the target, skips stunned attackers, and resolves an attack.

![Combat pipeline](diagrams/combat-pipeline.png)

`ExecuteAttack` swings the main hand `AttackRate` times (haste/slow change the
count) plus one off-hand swing when dual-wielding. Every hit — auto-attack or
skill — flows through one pipeline.

## AttackResolver

`Core/Combat/AttackResolver.cs` is the single attack pipeline:

1. **To-hit:** `85 + attacker.AccuracyBonus - defender.AvoidanceChance` vs d100.
2. **Roll + attribute bonus:** dice plus `(attr-10)/2` if an `AttributeBonus` is given.
3. **Crit:** a natural max roll (when the attacker qualifies, e.g. `critical_mastery`) or a buff-driven `CritChanceBonus`; doubles damage.
4. **Outgoing buffs:** `× DamageDealtMultiplier` (e.g. berserk +50%).
5. **Armor:** physical damage is reduced by `TotalArmourRating` unless `ignoresArmor`.
6. **Type multiplier:** `DamageResolver.GetDamageMultiplier` (below).

```csharp
var outcome = AttackResolver.Resolve(attacker, defender, dice, DamageType.Physical,
                                     attributeBonus: "Strength", critOnMaxRoll: hasCritMastery);
if (outcome.Hit) { defender.Health -= outcome.Damage; ... }
```

## StatusEffect — the modifier framework

`Entities/StatusEffect.cs` is one timed modifier. The `EffectModifier` it carries
decides what it does:

![StatusEffect](diagrams/statuseffect.png)

| Modifier | Effect |
|---|---|
| `DamageOverTime` / `HealOverTime` | HP change per status tick |
| `ArmorMod` | adds to `TotalArmourRating` |
| `AccuracyMod` / `AvoidanceMod` | to-hit / dodge |
| `AttackRateMod` | extra/fewer swings per round |
| `DamageDealtMod` | % change to outgoing damage |
| `ImmunityOverride` | immune to a `DamageType` while active |
| `FlatDamageReduction` | % cut to a `DamageType` |
| `CritChanceMod` | added crit % |
| `Stun` / `Root` / `Blind` | lose turn / can't move / can't cast |
| `Thorns` | reflects Magnitude to melee attackers (druid thorns) |
| `WeaponCoat` | charges of a hit-proc coating (thief poison) |

`Character` exposes computed accessors that fold active effects:
`TotalArmourRating`, `AttackRate`, `AccuracyBonus`, `AvoidanceChance`,
`DamageDealtMultiplier`, `IsStunned`, `IsRooted`, `IsBlinded`.

Polarity (`Positive`/`Negative`) and `EffectType` (Poison, Curse, Magic, Mental...)
support cleanse/dispel: remove negatives by type without stripping the target's buffs.

## DamageResolver

`Core/Combat/DamageResolver.cs` layers three things, in order:

1. **Species matrix** — `Character.DamageMultipliers[type]` (0 immune, 0.5 resist, 2 vulnerable, default 1). Populated from `species.json` at creation.
2. **Immunity overrides** — any active `ImmunityOverride` for the type returns 0.
3. **Flat reductions** — each `FlatDamageReduction` multiplies by `(1 - pct/100)`.

Immunity yields 0 damage; otherwise the result floors at 1.

## Death

`Core/Combat/DeathService.HandleDeath(dead, world, killer)`:
- **Player:** clears effects, recalls to `WorldState.SafeRoomId` at 1 HP (no `Environment.Exit`).
- **NPC:** removes it, spawns a corpse container holding its gear + inventory, breaks others' targeting, and awards `XpReward` to a player killer via `LevelingService`.

Call it from any handler that can reduce a target to 0, passing `ctx.Caster` as the killer so XP is attributed.

## Passive triggers in combat

After a hit lands and the defender survives, `CombatSystem.ResolveSingleHit` fires
two passive events through `PassiveService.Fire`:

- `OnOutgoingHit` (owner = attacker) — e.g. `holy_fervor` chance-stuns with a mace/club.
- `OnIncomingHit` (owner = defender) — e.g. `retribution_aura` sears the attacker with holy damage.

A passive only runs for an owner who knows its skill (the `TriggerBus` checks
`KnownSkills`). Other fire points (`OnCast`, `OnLook`, `OnMaxRoll`, `OnLowHealth`,
`OnIncomingSpell`) exist on the bus but are not all wired yet. To add an event
passive: implement `IPassiveHandler`, register it in `PassiveService.Initialize`,
and ensure the relevant `Fire` call exists at the event site.

## Shapeshift forms

`Core/Skills/ShapeshiftService.cs` + `Definitions/forms.json` (`FormDefinition`).
`ShapeshiftService.Enter`/`Revert` apply a form's armor bonus (a tagged permanent
`StatusEffect`) and a tracked temporary max-HP bump, restoring cleanly on revert.
`Character.Form` holds the active form.

- **Combat:** `ExecuteAttack` calls `ShapeshiftService.GetForm`; a form with an attack
  profile swings its natural attack (dice + verb + scaling attribute) instead of weapons,
  and a form that `LocksPhysical` (owl) cannot melee.
- **Skills:** `SkillExecutor` blocks spells when the form `LocksCasting` (bear/wolf) and
  physical skills when it `LocksPhysical` (owl).
- **Commands:** `shapeshift <bear|wolf|owl|dragon|human>` (reverting to human is free;
  entering a form routes through the `shapeshift_<form>` skill for mana/cooldown). Dragon
  form unlocks `breath` (un-mitigated elemental AoE).

## Ranged & cross-room attacks

The ranger `shoot` skill requires a `WeaponType.Bow` in the main hand (item blueprints
carry `WeaponType`). It strikes a target in the current room, or — if the target is one
exit away — hits it from afar and pulls it into the shooter's room (drawing aggro
safely). `ShootHandler` searches the current room first, then adjacent rooms.

## Pets / companions

`Core/PetSystem.cs`. A ranger `tame`s a wild animal (an NPC with the `Animal`/`Beast`
archetype), setting its `OwnerId` and `Player.Pet`. Then:

- **Follow:** `PetSystem.FollowOwner` (wired into move/flee) keeps the pet in the owner's room.
- **Assist:** `PetSystem.UpdatePets` (each combat tick) points the pet at the owner's target, so it fights through the normal combat loop.
- **Link:** with `companion_link`, the pet gains 25% of the owner's armor and inherits the owner's damage resistances.
- **Recall:** `call_companion` teleports the pet to the owner.

Pets don't trip their owner's traps and aren't aggressive. A dead pet is dropped from `Player.Pet` automatically.

## Traps

`Core/TrapSystem.cs` plus `Entities/Trap.cs` and `Room.Traps`. A character places a
one-shot trap with `TrapSystem.Place(room, owner, dice, type, rootRounds)` (the
ranger `set_trap` skill). A hostile NPC springs it for damage + a root, crediting
the owner for any kill. Two trigger points:

- `WorldState.MoveCharacter` calls `TrapSystem.OnEnter` when a character enters a room.
- The AI tick calls `TrapSystem.CheckRooms`, springing traps on a hostile NPC already standing in a trapped room.

## Adding a buff/debuff

Add a `StatusEffect` to the target with the right `EffectModifier`, `Magnitude`,
`DamageType`, and `TicksRemaining`. CC (`Stun`/`Root`/`Blind`) ages on the combat
pulse; everything else on the status pulse. No other wiring needed — the accessors
and resolver pick it up automatically.
