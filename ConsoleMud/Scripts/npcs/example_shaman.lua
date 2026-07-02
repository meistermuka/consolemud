-- example_shaman: demonstrates the NPC on_tick behaviour contract.
--
-- To use this script, add to the NPC template in your area JSON:
--   "ScriptId": "npcs/example_shaman"
--
-- The function receives three arguments:
--   npc    — LuaCharacterProxy for this NPC (never nil)
--   room   — LuaRoomProxy for the room the NPC is in
--   player — LuaCharacterProxy for a player in the room, or nil if none

function on_tick(npc, room, player)
    -- No player in the room; nothing to do.
    if player == nil then return end

    -- At critical health, announce desperation but don't engage.
    if npc.health_pct < 0.25 then
        game.print("{MThe shaman shrieks and calls upon dark powers!{x")
        return
    end

    -- Engage the player if not already in combat.
    if not npc.is_in_combat then
        game.engage(npc.id, player.id)
        game.print("{RThe shaman's eyes gleam with malice as it turns on you!{x")
    end
end
