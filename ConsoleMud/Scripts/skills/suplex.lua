skill_id = "suplex"

function execute(ctx)
    if ctx.target_id == nil then
        game.print("{YSuplex who?{x")
        return
    end

    local dmg = game.roll_dice("3d6")
    game.damage(ctx.target_id, dmg)
    game.print("{YA " .. ctx.caster_name .. " performs a {RSUPLEX{x on " .. ctx.target_name
               .. " for " .. dmg .. " damage!{x")
end