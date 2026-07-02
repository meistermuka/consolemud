# ConsoleMud Script Authoring Guide

Scripts are Lua 5.2 files loaded by `ScriptEngine` at startup. Each file is sandboxed —
it can only access the `game` global; file I/O, OS, and debug modules are removed.

## Directory layout

```
Scripts/
  npcs/    — NPC behaviour scripts (Layer 3)
  rooms/   — Room event scripts (Layer 4)
  skills/  — Lua-backed active skills (Layer 2)
```

## The `game` API

```lua
game.print("{Rred text{x")            -- print colour-markup to game output
game.roll_dice("2d6+3")               -- returns integer result
game.damage(char_id, amount)          -- deal raw damage; triggers death if <= 0
game.heal(char_id, amount)            -- restore health, capped at max
game.engage(attacker_id, target_id)  -- start mutual combat between two characters
game.teleport(char_id, virtual_room) -- move character to a room by its VirtualId
```

`char_id` is always the runtime GUID string (e.g. the `id` field on proxies
passed by the tick hooks in Layers 2–4).

## Colour markup

The same codes used in area files work here:
`{r` dark red · `{R` red · `{g` dark green · `{G` green · `{y` dark yellow ·
`{Y` yellow · `{b` dark blue · `{B` blue · `{m` dark magenta · `{M` magenta ·
`{c` dark cyan · `{C` cyan · `{w` gray · `{W` white · `{d` dark gray ·
`{k` black · `{x` reset

## NPC behaviour hook (`npcs/*.lua`)

```lua
function on_tick(npc, room, player)
    -- npc.id, npc.name, npc.health, npc.max_health, npc.health_pct, npc.level
    -- room.id, room.virtual_id, room.name
    -- player may be nil if no player is in the room
    if player == nil then return end
    game.engage(npc.id, player.id)
end
```

## Room entry hook (`rooms/*.lua`)

```lua
function on_enter(character, room)
    -- character.id, character.name, character.is_player
    if character.is_player then
        game.print("{YA cold wind sweeps through the room...{x")
    end
end
```

## Skill handler (`skills/*.lua`)

```lua
skill_id = "my_skill"   -- must match the id in skills.json

function execute(ctx)
    -- ctx.caster_id, ctx.target_id, ctx.target_name, ctx.spell_power, ctx.param(key)
    local dmg = game.roll_dice("1d8") + ctx.spell_power
    game.damage(ctx.target_id, dmg)
    game.print("{BA bolt of energy strikes " .. ctx.target_name .. "!{x")
end
```
