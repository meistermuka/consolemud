# Tuning Reference

Engine-wide balance constants. Edit values in [`ConsoleMud/Definitions/tuning.json`](../ConsoleMud/Definitions/tuning.json); this file is generated from it by `docs/generate_tuning_doc.py`.

Per-skill numbers (dice, charges, proc chances, durations) live with each skill in `Definitions/skills.json` instead. Timing constants (tick intervals, autosave) stay in `Core/TimeEngine.cs`.

## combat

| Key | Value | Description |
|---|---|---|
| `combat.baseHitChance` | 85 | Base percent chance an attack lands, before attacker accuracy and defender avoidance. |
| `combat.critMultiplier` | 2.0 | Damage multiplier applied on a critical hit. |
| `combat.darkAdaptationCap` | 30 | Maximum the dark to-hit penalty can ease over a single fight. |
| `combat.darkAdaptationPerRound` | 5 | Percentage points the dark to-hit penalty eases each combat round as eyes adjust. |
| `combat.darkMissPenalty` | 50 | Initial to-hit penalty (percentage points) when attacking a target you can't see in the dark. |
| `combat.markBonusMultiplier` | 1.2 | Damage multiplier against a hunter's-marked target (mark_of_the_hunter). |

## leveling

| Key | Value | Description |
|---|---|---|
| `leveling.maxLevel` | 101 | Highest attainable level (the HERO tier). |
| `leveling.xpPerLevel` | 100 | XP-to-advance coefficient: cost to reach the next level = value * current level. |

## passive

| Key | Value | Description |
|---|---|---|
| `passive.evasionDexDivisor` | 2 | Evasion avoidance% = Dexterity / this divisor. |
| `passive.opportunistCrit` | 10 | Flat critical-hit% granted by opportunist. |
| `passive.parryCap` | 40 | Maximum avoidance% granted by parry. |
| `passive.parryProficiencyFactor` | 0.4 | Parry avoidance% = parry proficiency * this factor. |
| `passive.reflexiveDexDivisor` | 4 | Reflexive-dodge avoidance% = Dexterity / this divisor. |

## proficiency

| Key | Value | Description |
|---|---|---|
| `proficiency.baseGain` | 2.0 | Proficiency gained per attempt at 0; tapers toward the ceiling as proficiency rises. |
| `proficiency.ceiling` | 99.999 | Maximum effective success chance; even 100 proficiency fails a tiny fraction of the time. |
| `proficiency.minGain` | 0.01 | Minimum proficiency gain per attempt, so the climb to 100 never fully stalls. |

## regen

| Key | Value | Description |
|---|---|---|
| `regen.arcaneMeditationMultiplier` | 2 | Mana-regen multiplier from arcane_meditation while sitting or resting. |
| `regen.healthBase` | 2 | Health restored per status tick while standing and out of combat. |
| `regen.manaBase` | 5 | Mana restored per status tick while standing and out of combat. |
| `regen.restingMultiplier` | 3 | Regen multiplier while resting (out of combat). |
| `regen.sittingMultiplier` | 2 | Regen multiplier while sitting (out of combat). |
| `regen.wildernessLoreBonus` | 1 | Extra regen factor from wilderness_lore while resting/sitting outdoors. |
