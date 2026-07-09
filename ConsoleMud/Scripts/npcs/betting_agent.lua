function on_receive(npc, player, item)
    game.print("{Y=== " .. item.name .. " ==={x")
    if item.name == "A platinum coin" then
        game.print("{YThe betting agent pockets the coin and nods approvingly.{x")
        local given_item_name = game.give_item(player.id, "glass_amulet")
        game.print("The betting agent gives you " .. given_item_name .. " for your bet.{x")
    else
        game.print("{YThe betting agent shakes his head and says 'I don't take that kind of coin.'{x")
        game.take_item(npc.id, item.name, true)
    end
end