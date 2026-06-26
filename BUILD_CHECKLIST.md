# MUD Build Checklist — Character Creation + Persistence

Two milestones (A: creation, B: persistence) plus the framework they sit on.
Tick items as they land. `[~]` = stubbed on purpose (prerequisite deferred).

---

## Phase 0 — Model and enum foundation
- [x] `Enums/Species.cs` (Human, Elf, Dwarf, Halfling, Gnome, Orc)
- [x] `Enums/DamageType.cs` (Physical, Poison, Magic, Psychic, Charm, Sleep, Fear, Fire, Cold, Lightning, Force, Holy, Nature)
- [x] `Enums/Form.cs` (Human, Bear, Wolf, Owl, Dragon)
- [x] `Enums/WeaponType.cs` (Unarmed, Sword, Dagger, Mace, Club, Axe, Spear, Staff, Bow)
- [x] `Enums/Position.cs` (Standing, Sitting, Resting)
- [x] `Enums/Archetype.cs` (Humanoid, Animal, Beast, Undead, Fiend, Dragon, Elemental)
- [x] `Character`: add Level, Experience, Species, Form, Position, KnownSkills, Archetypes
- [x] `Item`: add WeaponType
- [x] NPC inherits Level/Archetypes (no change needed; verified)
- [x] Build green

## Phase 1 — Definition registries (JSON)
- [x] `Entities/Definitions/SpeciesDefinition.cs`
- [x] `Entities/Definitions/SkillDefinition.cs` (incl. `Parameters` bag)
- [x] `Entities/Definitions/ClassDefinition.cs`
- [x] `Core/Services/DefinitionRegistry.cs`
- [x] `Definitions/species.json` (all 6, locked modifiers + damage matrix)
- [x] `Definitions/skills.json` (starter: full Fighter set + each class L1)
- [x] `Definitions/classes.json` (full progression tables, all 6 classes)
- [x] `Program.cs`: load registry at startup
- [x] `.csproj`: copy `Definitions/**` to output
- [x] Build green; registry loads (6/23/6), validator flags undefined skills

## Phase 2 — StatusEffect framework
- [x] `Entities/StatusEffect.cs` (duration, polarity, type tag, charges, modifier kind)
- [x] `Enums/EffectType.cs`
- [x] `Enums/EffectModifier.cs`
- [x] `Enums/EffectPolarity.cs` (added; needed by StatusEffect)
- [x] `Core/Combat/DamageResolver.cs` (species matrix + overrides + flat reductions)
- [x] `Character`: effective-stat accessors (armor, accuracy, attack-rate, dmg-dealt) + `DamageMultipliers` map
- [x] `Core/StatusAndRegenSystem.cs`: tick uses new model (DoT/HoT, expiry)
- [x] Remove `Entities/ActiveEffect.cs`
- [x] Build green
- [x] (done in Phase 5) deleted legacy `EquippedWeapon`/`EquippedArmour` with the BashCommand retrofit

## Phase 3 — Combat refactor
- [x] `Core/Combat/AttackResolver.cs` (hit/crit/damage pipeline)
- [x] To-hit step (accuracy vs parry/dodge) — verified misses fire
- [x] Crit (max roll + buff-driven) — infrastructure in place, gated until passives wire it
- [x] Per-character attack-rate (haste/slow) — `AttackRate` drives main-hand swing count
- [x] Damage via `DamageResolver`; `AttributeBonus` added (tabletop modifier)
- [x] `Core/DiceRoller.cs`: expose max-roll detection
- [x] Fixed corpse loot to read `Equipment` (legacy `EquippedWeapon` was null)
- [x] Build green; live combat smoke test passed

## Phase 4 — Character creation (MILESTONE A)
- [x] Split `Helpers/CharacterGenerator.cs` into steps (orchestrator + BuildPlayer + sheet)
- [x] `Helpers/CreationSteps.cs` (prompts + roll/reroll + assignment)
- [x] Steps: name, species, class, roll (+3 rerolls), assign six, apply modifiers, confirm
- [x] Seed `KnownSkills` with L1 skills; set Level 1
- [x] Populate species `DamageMultipliers` map from registry
- [x] `Program.cs` wiring (passes DefinitionRegistry)
- [x] Build green; full flow smoke test passed (Orc Fighter: modifiers + vitals + skill verified)
- [x] **Milestone A complete**

## Phase 5 — Skill execution framework
- [x] `Core/Skills/SkillExecutor.cs` (knowledge/cooldown/mana/proficiency, gain-on-attempt)
- [x] `Core/Skills/ISkillHandler.cs` + `SkillContext.cs`
- [x] `Core/Skills/SkillHandlerRegistry.cs` (Kick + Bash vertical-slice handlers)
- [x] `Core/Skills/ProficiencyMath.cs` (99.999 ceiling + diminishing gain)
- [x] `Core/Skills/TriggerBus.cs` (enum + IPassiveHandler + bus scaffolding; fire points wired in Phase 6/8)
- [x] `Core/Combat/DeathService.cs` (extracted shared death so skills can kill)
- [x] `Core/Commands/CastCommand.cs`
- [x] SkillCommand: folded into CommandParser fallback (typed skill verb -> executor)
- [x] Mage `Specialization` field on Player (level-up prompt deferred to progression)
- [x] Retrofit bash: old BashCommand removed; bash now a gated learned skill via handler
- [x] `CommandParser` wiring (ctor takes executor, `cast`, skill fallback); `Program.cs` order
- [x] Deleted dead `WorldBuilder.cs` + legacy `EquippedWeapon`/`EquippedArmour` fields
- [x] Build green; pipeline smoke test passed (gate, proficiency fail, cooldown/mana paths)
- [ ] (Phase 6/8) wire TriggerBus fire points into combat/look

## Colour system (out-of-band feature, done)
- [x] `Helpers/ColorMarkup.cs` — inline codes ({R, {g, {x, {{), Strip + Render
- [x] `Helpers/ColorConsole.cs` — markup-aware Write/WriteLine with base colour
- [x] Logic strips codes: `Item`/`NonPlayerCharacter` Keywords + MatchesKeyword
- [x] Display routed through ColorConsole: Look, Inventory, Equipment, Combat, Death, skill handlers, Get/Drop/Put/Wield/Wear/Second, Kill, NPC aggro
- [x] Colours set inline in ItemTemplates/NpcTemplates strings (no schema change)
- [x] Verified: codes render as colour, never leak as text; matching works on coloured names
- [ ] (follow-up) migrate remaining error/validation name lines + StatusCommand to ColorConsole

## Phase 6 — Base commands and shared states
- [x] `Core/Commands/FleeCommand.cs` (random exit, breaks combat, root-gated)
- [x] `Core/Commands/SitCommand.cs`
- [x] `Core/Commands/RestCommand.cs`
- [x] `Core/Commands/StandCommand.cs` (added; movement auto-stands)
- [x] Position state transitions + position-scaled HP/mana regen (suppressed in combat)
- [x] Stealth/Hidden state on Character + break-on-action + idle auto-hide (gated on knowing "hide"); aggro & look respect hidden
- [x] Stun / root / blind via EffectModifier; aged on combat pulse; gates in CombatSystem (stun), Move/Flee (root), SkillExecutor (stun/blind)
- [x] bash now applies a stagger stun (demonstrates CC)
- [x] Fixed combat bugs found in working tree: unarmed null-ref + off-hand using main-hand name
- [x] Build green; commands + gates smoke tested
- [ ] (Phase 8) wire TriggerBus fire points; full hide/stealth skill handler

## Phase 7 — Persistence (MILESTONE B)
- [x] `Room` retains `VirtualId`; `AreaLoaderService` populates it + lookup
- [x] `WorldState`: `VirtualId -> Guid` lookup + `SafeRoomId`
- [x] `Entities/PlayerSave.cs` (flat serialization DTO)
- [x] `Core/Services/SaveService.cs` (Save / TryLoad / Exists, sanitized filenames)
- [x] `Core/Commands/SaveCommand.cs`
- [x] Save-on-quit + timed autosave (480 pulses) in TimeEngine
- [x] Startup create-or-load (Program.SelectCharacter); block duplicate-name creation
- [x] Death: recall to `SafeRoomId` at 1 HP, clears effects (replaces Environment.Exit)
- [x] Hardened creation prompts against end-of-input (no infinite loop)
- [x] Verified: save->quit->load round-trip restores location + inventory; duplicate-name blocked
- [x] **Milestone B complete**

## Phase 8 — Per-class skill handlers (tier by tier)
NOTE: blocked beyond L1 — there is no XP/leveling system yet, so a character only
ever knows its level-1 skill. A leveling phase is a prerequisite for tiers 2-5.
- [x] SkillContext helpers: ResolveNpcTarget / ResolveFriendlyTarget / Engage / AttributeModifier
- [x] Passive infra: `PassiveService` (static passives as permanent effects, refreshed on
      creation/level-up/load); EncounterFlags; critical-mastery + second-wind wired into combat
- [x] **Fighter — COMPLETE**: kick, rescue, bash, disarm, taunt, cleave, lunge, berserk,
      decapitate, onslaught (actives); armor_optimization, parry, indomitable_will (static
      passives); critical_mastery + second_wind (combat-wired). Verified: lvl-up learns all,
      passive armor applied, taunt fires.
      (dual_wield_specialization = no-op until an off-hand penalty exists; blind_fight needs
       Phase 9g environment; counterstrike needs an on-parry event hook.)
- [ ] Tier-1 leftovers: peek (Thief, needs OnLook wiring), shoot (Ranger, needs Phase 9c)
- [x] Mage L1: magic_missile  |  Druid L1: entangle (done earlier)
- [x] **TriggerBus wired** into combat: OnIncomingHit + OnOutgoingHit fire via PassiveService
- [x] **Cleric — COMPLETE**: minor_heal, bless, turn_undead, cure_poison, smite, sanctuary,
      major_heal, prayer, dispel_magic, divine_intervention, judgment (actives);
      divine_grace (heal scaling), divine_armor, soul_ward (static passives);
      retribution_aura + holy_fervor (trigger passives); undying_faith (cheat-death in DeathService).
      Verified: smite/bless/divine_armor + retribution_aura trigger fired live.
      (resurrection = informative stub until the corpse/death model exists.)
- [x] OnCast trigger wired (SkillExecutor) for channeling_flow mana refund
- [x] **Mage — COMPLETE**: magic_missile, shield, shocking_grasp, fireball, blink, haste,
      ice_storm, disintegrate, time_stop (foes frozen), meteor_swarm (single-room) (actives);
      sage_insight (spell power), arcane_meditation (rest regen), elemental_mastery (via
      `specialize` cmd), channeling_flow (OnCast refund) (passives).
      Verified: shield/fireball/specialize + sage_insight damage live.
      (detect_magic/teleport informative stubs; spell_mirror inert until NPC casters;
       arcane_singularity partial; meteor_swarm multi-room deferred.)
- [x] **Druid — COMPLETE**: entangle, shapeshift_bear/wolf/owl/dragon (+breath), rejuvenate (HoT),
      lightning_strike (storm-boosted), thorns (reflect via EffectModifier.Thorns + combat hook),
      insect_swarm, call_lightning, wrath_of_nature (actives); skin_of_oak, toxic_resilience (static
      passives); eco_location (look augment, Outside); gaean_embrace (outdoor cheat-death in DeathService).
      Verified: skin_of_oak armor, rejuvenate HoT, thorns reflection, eco_location sensing.
      (natures_speech/natural_herbology stubs; adaptive_synergy inert no-op.)
- [x] **Thief — COMPLETE**: steal (+catch risk), backstab (hidden, 3x/5x w/ anatomic_precision),
      poison (WeaponCoat proc in combat), trip, smoke_bomb, blindside, assassinate (instakill/cripple),
      death_dance (retains stealth) (actives); hide (Phase 6 idle-stealth), evasion/reflexive_dodge/
      opportunist/slippery_mind (static passives); peek (look augment); untouchable (combat-wired
      low-HP immunity); anatomic_precision (backstab upgrade). Verified: peek, steal, trip.
      (picklock/shadowstep stubs.)
- [x] **Ranger — COMPLETE**: shoot (9c), tame/call_companion (9b), set_trap (9e), track (mark + trail),
      forest_camouflage, scout, snare, spirit_of_the_pack, arrow_of_slaying, alpha_strike (actives);
      wilderness_lore (outdoor regen), arrow_volley (shoot mod), companion_link (PetSystem),
      mark_of_the_hunter (AttackResolver), weather_hardened (Cold immunity), natural_attunement
      (outdoor combat rider), point_blank_shot (no-op). Verified: track, scout, snare.
      (picklock-style stubs: none for ranger.)

### ALL SIX CLASSES COMPLETE — Phase 8 closed.

## Phase 8a — Leveling / XP
- [x] NPC `Level` + `XpReward` (blueprint + fallback formula in loader)
- [x] `ClassDefinition` HpPerLevel/ManaPerLevel (+ classes.json for all 6)
- [x] `Core/Services/LevelingService.cs` (XP curve, multi-level, vital growth, auto-learn skills, full heal)
- [x] XP awarded on kill via `DeathService.HandleDeath(..., killer)`; wired in combat + skill handlers
- [x] `StatusCommand` shows Level + XP/next; `Program` initializes LevelingService
- [x] Verified live: 350 XP -> level 3, +HP/+mana per level, auto-learned rescue, XP math correct
- [ ] (future tuning) XP curve values; DoT/environment kills don't yet attribute a killer

## Phase 9 — Dedicated subsystems
- [x] 9a Shapeshift forms (Druid): `FormDefinition` + `forms.json` (loaded by DefinitionRegistry),
      `ShapeshiftService` (enter/revert, buffs, attack swap), `shapeshift`/`breath` commands,
      casting/physical gates in SkillExecutor, beast-attack swap in CombatSystem.
      Verified: bear buffs + casting lock, dragon breath + rake attack, revert.
- [x] 9b Pet/companion (Ranger): `Core/PetSystem` (tame/follow/assist/link/recall), `Player.Pet`,
      `NonPlayerCharacter.OwnerId`, NPC `Archetypes` (blueprint+loader), `tame`/`call_companion` skills.
      Wired: follow on move/flee, assist each combat tick, pets skip own traps. Verified live.
- [x] 9c Ranged + cross-room attack: `ItemBlueprint.WeaponType` (+loader), bow in world,
      `ShootHandler` (in-room or pull from adjacent room). Verified live. (Also fixed `wield`
      to use keyword matching.)
- [x] **Phase 9 COMPLETE.**
- [x] 9d Cross-room perception: `PerceptionService` (DescribeAdjacent + ScanNearby) — powers scout + eco_location
- [x] 9e Traps: `Entities/Trap`, `Room.Traps`, `Core/TrapSystem` (OnEnter + CheckRooms),
      wired into MoveCharacter + AI tick; `set_trap` skill places them. Verified live.
- [x] 9f Movement history + target mark: `Character.LastExit` recorded on move; `MarkedTarget` + mark bonus in AttackResolver
- [x] 9g Environment tier: `Weather` enum, `Room.IsOutside` (+ blueprint/loader), weather pulse in TimeEngine, `weather` command, `WorldState.IsStormy`

## Tuning centralization (out-of-band, done)
- [x] `Definitions/tuning.json` (value+desc objects) + `Core/Services/TuningRegistry`
- [x] Rewired engine readers: AttackResolver, ProficiencyMath, LevelingService, StatusAndRegenSystem, PassiveService
- [x] Fixed JSON-param bypasses: channeling_flow, holy_fervor, retribution_aura (ctx/SkillParam);
      second_wind, untouchable, poison proc (CombatSystem via PassiveService.SkillParam);
      gaean_embrace, undying_faith (DeathService)
- [x] `PassiveService.SkillParam` helper for combat/death-wired skill params
- [x] `docs/tuning.md` + `docs/generate_tuning_doc.py` auto-generator; docs cross-linked
- [x] Timing constants left in TimeEngine (per decision); build green; verified load
- [ ] (follow-up) remaining handler duration fallbacks (10/15) are per-skill defaults, left as-is

## Area builder (out-of-band, done)
- [x] `Helpers/AreaBuilder.cs` interactive wizard (meta -> items -> npcs -> rooms -> exits -> validate -> write)
- [x] Reuses AreaBlueprint/*Blueprint + System.Text.Json (schema can't drift); load-back check
- [x] Launched via `dotnet run -- build-area` (Program arg)
- [x] Validation: unique ids, exit/spawn/weapon references, dice format; reciprocal exits
- [x] `docs/area-builder.md`; verified by building a 2-room area that loads cleanly
- [ ] (future) `validate-area <file>` command for hand-edited files; in-game OLC

## Equipment & GET/WEAR improvements
- [x] Phase A — slot generalization: generic `Ring`/`Earring`/`Arm`/`Forearm` declared slots +
      physical pairs kept; `Helpers/SlotResolver` (first-free, replace-oldest, back-compat with
      numbered/L-R); `WearCommand` rewired; area JSON migrated; AreaBuilder offers declared slots;
      `docs/equipment.md`. Verified: ring fills L then R then replaces oldest.
- [x] Phase B — GET all: `get all` / `get all <kw>` / `get all [<kw>] from <container>`; container matched by
      keyword (fixed); getable-only with a count summary. Verified: floor bulk+keyword+empty, container take.
- [x] Phase C — WEAR all: `wear all` dresses armor/accessories into free slots (skips occupied),
      excludes weapons/shields (hand slots), reports worn/skipped + total defense. Verified live.

## Containers / Doors / Light — design in docs/world-objects.md
- [x] Phase 1 — Keys + containers: Item open/close/lock state + KeyId; `Character.HasKey`;
      `open`/`close`/`lock`/`unlock` commands; get/put/look-in gated on open; ItemBlueprint
      + loader (IsCloseable/StartsLocked/LockKeyId/KeyId); AreaBuilder prompts. Verified live.
- [x] Phase 2 — Light/darkness/darkvision: `Room.IsDark` (blueprint + loader); `Item.IsLightSource`
      + `GrantsDarkvision` (blueprint + loader + AreaBuilder prompts); species darkvision
      (Elf/Dwarf/Orc in species.json, `SpeciesDefinition.HasDarkvision`, set on player creation +
      persisted in `PlayerSave`); NPC darkvision (`NpcBlueprint.HasDarkvision`, loaded as
      `InnateDarkvision`); `Character.HasDarkvision` computed (species | item | spell) +
      `CanSee(room)` helper; look gate (room + target), kill gate, skill-target gate, NPC aggro
      gate (`npc.CanSee(room)` in `TimeEngine`); dark-combat miss penalty with per-round
      adaptation ramp in `CombatSystem.DarknessHitModifier()` (tuning: darkMissPenalty=50,
      darkAdaptationPerRound=5, darkAdaptationCap=30); `darkvision` spell handler (Mage L18 /
      Cleric L22, `EffectModifier.Darkvision`); cave_entrance dark room + torch + goggles_of_night
      + cave_bat in emerald_forest.json for live verification. Verified live.
- [ ] Phase 3 — Doors: Exit/Door objects (Room.Exits refactor), shared two-sided doors, both-sides
      area schema, open/close/lock/unlock by direction, movement blocked by closed doors, picklock stub retired

## Teams (Multi-Body Spirit) — design locked, not started
Design: `docs/teams.md`. The human is a spirit controlling a roster of full-character bodies.
- [ ] T0 — Spirit & roster plumbing: `Spirit` (name=save key, bodies, active+main refs, slot count
      from main level); bodies are `Player`s owned by the spirit; refactor `Program`/`CommandParser`
      to act on the active body; `SpiritSave` roster persistence. Behavior-preserving (single body).
- [ ] T1 — Switching & inspection: `switch <body>` (look/prompt follow active); `roster`/`team`; `stats <body>`
- [ ] T2 — Slot unlock & body creation: main level 26/51/76/101 unlocks a slot -> full creation for new body
- [ ] T3 — Follow & group movement: followers move with active leader; `follow`/`stay` modes; party can split
- [ ] T4 — Combat auto-assist & friend/foe: followers assist active's target; bodies/pets mutually friendly;
      NPCs aggro any body present
- [ ] T5 — Command addressing: per-body prefix (`grog: kill rat`), group keyword (`all north`), standing orders
- [ ] T6 — Team XP: track per-engagement involvement (dealt/took damage); killer double share, others split rest
- [ ] T7 — Persistence completeness & polish: full SpiritSave round-trip; look/combat show body names + active marker

Open/deferred: all-bodies-down handling; re-designating main; body dismissal/replacement.

## Phase 10 — Deferred-prerequisite backlog
- [~] Locks + NPC exit-blocking (picklock, shadowstep)
- [~] Consumables (Natural Herbology)
- [~] Hidden content / caches / boss locations (detect_magic, Nature's Speech)
- [~] Grouping / party + discovered-location recall (teleport, prayer reach)
- [~] Fuller death/corpse model (resurrection)
- [~] Multi-room AoE, channeling, room-freeze (meteor_swarm, time_stop, wrath_of_nature)
