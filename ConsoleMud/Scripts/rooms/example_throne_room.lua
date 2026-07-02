-- example_throne_room: demonstrates the room on_enter event contract.
--
-- To use this script, add to the room blueprint in your area JSON:
--   "ScriptId": "rooms/example_throne_room"
--
-- The function receives two arguments:
--   character — LuaCharacterProxy for the character entering (player or NPC)
--   room      — LuaRoomProxy for the room being entered

function on_enter(character, room)
    -- Fires for every character that enters; filter to players only when needed.
    if not character.is_player then return end

    game.print("{YA cold wind sweeps through the throne room as you enter...{x")
end
