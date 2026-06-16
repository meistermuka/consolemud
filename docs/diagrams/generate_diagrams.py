#!/usr/bin/env python3
"""Generates the developer-documentation diagrams as PNGs using Graphviz.

Run:  python3 docs/diagrams/generate_diagrams.py
Output: docs/diagrams/*.png
"""
import os
from graphviz import Digraph

OUT = os.path.dirname(os.path.abspath(__file__))

# Shared styling
BASE = dict(fontname="Helvetica", fontsize="11")
NODE = dict(shape="box", style="rounded,filled", fillcolor="#eef3fb",
            color="#3b6ea5", fontname="Helvetica", fontsize="11")
ACCENT = dict(shape="box", style="rounded,filled", fillcolor="#fbeede",
              color="#c08a3e", fontname="Helvetica", fontsize="11")
DECISION = dict(shape="diamond", style="filled", fillcolor="#fdf6e3",
                color="#b58900", fontname="Helvetica", fontsize="10")
STORE = dict(shape="cylinder", style="filled", fillcolor="#e8f6ec",
             color="#3a8a55", fontname="Helvetica", fontsize="11")


def new(name, rankdir="TB"):
    g = Digraph(name)
    g.attr(rankdir=rankdir, bgcolor="white", **BASE)
    g.attr("edge", color="#555555", fontname="Helvetica", fontsize="9")
    return g


def render(g, name):
    path = os.path.join(OUT, name)
    g.render(path, format="png", cleanup=True)
    print("wrote", path + ".png")


# 1. Architecture overview
g = new("architecture")
g.attr(label="ConsoleMud — Boot & Runtime Architecture", labelloc="t", fontsize="14")
g.node("prog", "Program.Main\n(bootstrap)", **ACCENT)
for n, l in [("defs", "DefinitionRegistry\nspecies/skills/classes JSON"),
             ("area", "AreaLoaderService\nAreas/*.json -> Rooms"),
             ("skillreg", "SkillHandlerRegistry\n+ SkillExecutor"),
             ("parser", "CommandParser\n(verbs -> ICommand)"),
             ("time", "TimeEngine\n(master 250ms pulse)")]:
    g.node(n, l, **NODE)
g.node("world", "WorldState\nRooms / Characters / lookup", **STORE)
g.edge("prog", "defs"); g.edge("prog", "area"); g.edge("prog", "skillreg")
g.edge("prog", "parser"); g.edge("prog", "time")
g.edge("area", "world"); g.edge("time", "world"); g.edge("parser", "world")
g.edge("defs", "skillreg", style="dashed", label="lookups")
g.edge("skillreg", "parser", style="dashed", label="skill fallback")
g.node("loop", "Main input loop\nConsole.ReadLine", **ACCENT)
g.edge("prog", "loop"); g.edge("loop", "parser", label="each command")
render(g, "architecture")

# 2. Tick system
g = new("tick-system", rankdir="LR")
g.attr(label="TimeEngine — Master Pulse & Derived Subsystems", labelloc="t", fontsize="14")
g.node("master", "Master Pulse\nevery 250 ms\n(_masterPulseCount++)", **ACCENT)
subs = [("combat", "Combat\nevery 4 pulses (1.0s)\nCombatSystem.Tick"),
        ("ai", "AI + Stealth\nevery 8 pulses (2.0s)\nUpdateStealth / UpdateNpcIntelligence"),
        ("status", "Status & Regen\nevery 12 pulses (3.0s)\nStatusAndRegenSystem.Tick"),
        ("save", "Autosave\nevery 480 pulses (120s)\nSaveService.Save")]
for n, l in subs:
    g.node(n, l, **NODE)
    g.edge("master", n, label="% interval == 0")
render(g, "tick-system")

# 3. Command flow
g = new("command-flow")
g.attr(label="Command Parsing & Dispatch", labelloc="t", fontsize="14")
g.node("in", "raw input line", **ACCENT)
g.node("tok", "tokenize\nverb + args\nset LastActionUtc", **NODE)
g.node("known", "verb in\n_commands?", **DECISION)
g.node("cmd", "ICommand.Execute\n(look, get, kill, cast, ...)", **NODE)
g.node("skill", "SkillExecutor.TryUse\n(verb as learned skill)", **NODE)
g.node("isskill", "real skill?", **DECISION)
g.node("unknown", "\"Unknown command\"", **ACCENT)
g.edge("in", "tok"); g.edge("tok", "known")
g.edge("known", "cmd", label="yes")
g.edge("known", "skill", label="no")
g.edge("skill", "isskill")
g.edge("isskill", "unknown", label="no")
g.edge("isskill", "cmd", label="yes (handled)", style="dashed")
render(g, "command-flow")

# 4. Skill execution
g = new("skill-execution")
g.attr(label="SkillExecutor.TryUse — Active Skill Gate Sequence", labelloc="t", fontsize="14")
steps = [("def", "skill in registry?", DECISION, "no -> return false"),
         ("active", "Kind == Active?", DECISION, "passive -> message"),
         ("known", "KnownSkills has it?", DECISION, "no -> \"not learned\""),
         ("cc", "stunned / blinded?", DECISION, "blocked -> message"),
         ("cd", "off cooldown?", DECISION, "no -> \"on cooldown\""),
         ("mana", "enough mana? (spells)", DECISION, "no -> \"not enough mana\""),
         ("handler", "handler registered?", DECISION, "no -> \"not fully learned\"")]
prev = None
for n, l, st, fail in steps:
    g.node(n, l, **st)
    g.node(n + "_f", fail, **ACCENT)
    g.edge(n, n + "_f", label="fail")
    if prev:
        g.edge(prev, n, label="ok")
    prev = n
g.node("commit", "COMMIT:\nspend mana, set cooldown,\nbreak stealth", **NODE)
g.node("roll", "ProficiencyMath.RollSuccess\n(gain on every attempt)", **NODE)
g.node("succ", "success?", **DECISION)
g.node("fire", "handler.Execute(SkillContext)", **ACCENT)
g.node("fizzle", "\"You fail to execute X\"", **ACCENT)
g.edge("handler", "commit", label="ok")
g.edge("commit", "roll"); g.edge("roll", "succ")
g.edge("succ", "fire", label="yes"); g.edge("succ", "fizzle", label="no")
render(g, "skill-execution")

# 5. Combat pipeline
g = new("combat-pipeline")
g.attr(label="Combat Round — CombatSystem.Tick & AttackResolver", labelloc="t", fontsize="14")
g.node("tick", "CombatSystem.Tick\n(age CC, loop combatants)", **ACCENT)
g.node("valid", "target valid & alive\n& same room?", **DECISION)
g.node("stun", "attacker stunned?", **DECISION)
g.node("exec", "ExecuteAttack\n(AttackRate swings + off-hand)", **NODE)
g.node("resolve", "AttackResolver.Resolve", **NODE)
g.node("hit", "to-hit\n85 + accuracy - avoidance", **NODE)
g.node("crit", "crit?\n(max roll / buff)", **NODE)
g.node("dmg", "roll + attr bonus,\nx crit, x DamageDealt,\n- armor", **NODE)
g.node("mult", "DamageResolver\nspecies x immunity x reduction", **NODE)
g.node("apply", "apply damage", **NODE)
g.node("death", "DeathService.HandleDeath\n(corpse / recall) + XP", **ACCENT)
g.node("clear", "clear target\n+ EncounterFlags", **NODE)
g.edge("tick", "valid")
g.edge("valid", "clear", label="no")
g.edge("valid", "stun", label="yes")
g.edge("stun", "exec", label="no")
g.edge("exec", "resolve"); g.edge("resolve", "hit"); g.edge("hit", "crit")
g.edge("crit", "dmg"); g.edge("dmg", "mult"); g.edge("mult", "apply")
g.edge("apply", "death", label="HP <= 0")
render(g, "combat-pipeline")

# 6. StatusEffect / DamageResolver
g = new("statuseffect", rankdir="LR")
g.attr(label="StatusEffect Modifiers & Damage Resolution", labelloc="t", fontsize="14")
g.node("se", "StatusEffect\n(Modifier, Magnitude, DamageType,\nTicksRemaining, Polarity)", **ACCENT)
mods = "DamageOverTime / HealOverTime\nArmorMod / AccuracyMod\nAttackRateMod / DamageDealtMod\nImmunityOverride / FlatDamageReduction\nStun / Root / Blind\nAvoidanceMod / CritChanceMod"
g.node("mods", mods, **NODE)
g.node("char", "Character accessors\nTotalArmourRating, AttackRate,\nAvoidanceChance, IsStunned...", **NODE)
g.node("dr", "DamageResolver.GetDamageMultiplier\n1) species matrix\n2) immunity -> 0\n3) flat reductions", **NODE)
g.edge("se", "mods")
g.edge("mods", "char", label="static reads")
g.edge("mods", "dr", label="combat reads")
render(g, "statuseffect")

# 7. Character creation
g = new("creation-flow")
g.attr(label="Character Creation (Milestone A)", labelloc="t", fontsize="14")
chain = [("start", "SelectCharacter\n(new vs load)", ACCENT),
         ("name", "name (reject duplicates)", NODE),
         ("species", "species\n(show modifiers + resists)", NODE),
         ("cls", "class\n(show L1 skill, bias)", NODE),
         ("roll", "roll 6x 3d6\n(up to 3 rerolls)", NODE),
         ("assign", "assign values to STR..CHA", NODE),
         ("mods", "apply species modifiers", NODE),
         ("build", "BuildPlayer:\nvitals, KnownSkills,\nDamageMultipliers, passives", NODE),
         ("confirm", "confirm?", DECISION),
         ("done", "enter world", ACCENT)]
for n, l, st in chain:
    g.node(n, l, **st)
for a, b in zip([c[0] for c in chain], [c[0] for c in chain][1:]):
    g.edge(a, b)
g.edge("confirm", "name", label="no (restart)", style="dashed")
render(g, "creation-flow")

# 8. Persistence
g = new("persistence")
g.attr(label="Persistence — Save / Load by VirtualId", labelloc="t", fontsize="14")
g.node("save", "SaveService.Save", **ACCENT)
g.node("dto", "PlayerSave DTO\n(no CombatTarget / transient)", **NODE)
g.node("vid", "room Guid -> VirtualId", **NODE)
g.node("file", "Saves/<name>.json", **STORE)
g.edge("save", "dto"); g.edge("dto", "vid"); g.edge("vid", "file")
g.node("load", "SaveService.TryLoad", **ACCENT)
g.node("resolve", "VirtualId -> live Guid\n(fallback: safe room)", **NODE)
g.node("rebuild", "rebuild Player\n+ PassiveService.Refresh", **NODE)
g.edge("file", "load"); g.edge("load", "resolve"); g.edge("resolve", "rebuild")
render(g, "persistence")

# 9. Add-a-skill workflow
g = new("add-skill")
g.attr(label="How to Add a Skill", labelloc="t", fontsize="14")
g.node("d", "1. Define in Definitions/skills.json\n(Id, Kind, IsSpell, dice, Parameters...)", **NODE)
g.node("c", "2. Reference in classes.json\n(SkillId + unlock Level)", **NODE)
g.node("k", "3a. Active: implement ISkillHandler\n+ register in SkillHandlerRegistry", **ACCENT)
g.node("p", "3b. Static passive: add case in PassiveService", **ACCENT)
g.node("t", "3c. Trigger passive: wire in combat / TriggerBus", **ACCENT)
g.node("done", "Learned at level-up via LevelingService\nshown by 'skills' command", **NODE)
g.edge("d", "c")
g.edge("c", "k"); g.edge("c", "p"); g.edge("c", "t")
g.edge("k", "done"); g.edge("p", "done"); g.edge("t", "done")
render(g, "add-skill")

print("done")
