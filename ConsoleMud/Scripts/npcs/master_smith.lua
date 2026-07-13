function on_receive(npc, player, item)
    local has_orb = game.has_item(npc.id, "glass_orb")
    local has_staff = game.has_item(npc.id, "wood_staff")
    local has_ingot = game.has_item(npc.id, "gold_ingot")

    if has_orb and has_staff and has_ingot then
        game.print("{YThe master smith nods approvingly and says 'I can make a weapon for you.'{x")
        game.take_item(npc.id, "glass_orb", true)
        game.take_item(npc.id, "wood_staff", true)
        game.take_item(npc.id, "gold_ingot", true)
        local result = game.give_item(player.id, "staff_magi")
        game.print("The master smith gives you " .. result .. " for your materials.{x")
    else
        game.print("{YThe master smith shakes his head and says 'I need a glass orb, a wood staff, and a gold ingot to make a weapon.'{x")
    end

end