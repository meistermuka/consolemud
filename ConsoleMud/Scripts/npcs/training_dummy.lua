function on_tick(npc, room, player)
    if player == nil then return end

    if npc.is_in_combat then
        if npc.health_pct < 0.25 then
            game.print("{YThe gods intervene and heal the training dummy!{x")
            game.heal(npc.id, npc.max_health)
            return
        end

        -- 1 out of 5 chance to retaliate with a thunder bolt at its attacker.
        if math.random(1, 5) == 1 then
            game.use_skill(npc.id, "thunder_bolt", player.id)
        end
    end
end