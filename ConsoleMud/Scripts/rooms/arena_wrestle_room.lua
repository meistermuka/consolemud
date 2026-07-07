function on_enter(character, room)
    if not character.is_player then return end

    if game.count_npcs(room.virtual_id) == 0 then
        game.spawn_npc(room.virtual_id, "wrestler_npc", 2)
        game.print("{YTwo wrestlers step into the ring, ready to fight!{x")
    end

    game.print("{YThe ground rumbles as the wrestlers prepare for their next match.{x")
end
