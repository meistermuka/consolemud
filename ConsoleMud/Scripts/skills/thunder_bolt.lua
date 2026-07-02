-- thunder_bolt: a Lua-backed offensive spell that deals lightning damage.
-- Metadata (mana cost, cooldown, dice) lives in skills.json.
-- This script provides only the runtime effect.

skill_id = "thunder_bolt"

function execute(ctx)
    if ctx.target_id == nil then
        game.print("{YStrike what with your lightning?{x")
        return
    end

    local dmg = game.roll_dice("2d6") + ctx.spell_power
    game.damage(ctx.target_id, dmg)
    game.print("{YA crackling bolt of lightning arcs into " .. ctx.target_name
               .. " for " .. dmg .. " damage!{x")
end
