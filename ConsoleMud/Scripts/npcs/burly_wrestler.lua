function on_tick(npc, room, player)
    if player == nil then return end

    if npc.is_in_combat then
        if npc.health_pct < 0.25 then
            game.print("{YThe wrestler is down but not out!{x")
            game.heal(npc.id, npc.max_health)
            return
        end

        -- 1 out of 5 chance to retaliate with a suplex at its attacker.
        if math.random(1, 5) == 1 then
            game.use_skill(npc.id, "suplex", player.id)
        end
    end
end