using ConsoleMud.Core.Scripting;
using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class GiveCommand : ICommand
{
    public string Description => "Give an item from your inventory to an NPC.";
    public string Usage => "give <item> to <npc>";
    public string Example => "give token to guard";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0)
        {
            ColorConsole.WriteLine("Give what, and to whom?");
            return;
        }

        int toIndex = Array.IndexOf(args, "to");
        if (toIndex == -1 || toIndex == 0 || toIndex == args.Length - 1)
        {
            ColorConsole.WriteLine("Usage: give <item> to <npc>");
            return;
        }

        string itemName = string.Join(" ", args.Take(toIndex));
        string npcName = string.Join(" ", args.Skip(toIndex + 1));

        var item = player.Inventory.FirstOrDefault(i => i.MatchesKeyword(itemName));
        if (item == null)
        {
            ColorConsole.WriteLine($"You aren't carrying a '{itemName}'.");
            return;
        }

        var room = world.Rooms[player.CurrentRoomId];
        var npc = room.Characters
            .OfType<NonPlayerCharacter>()
            .FirstOrDefault(n => n.MatchesKeyword(npcName));
        if (npc == null)
        {
            ColorConsole.WriteLine($"You don't see a '{npcName}' here.");
            return;
        }

        player.Inventory.Remove(item);
        npc.Inventory.Add(item);
        ColorConsole.WriteLine($"You give the {item.Name} to the {npc.Name}.", ConsoleColor.Gray);

        // Fire the NPC's script trigger after the transfer, so the handler can
        // inspect, consume, or reward on the item it now holds.
        if (npc.ScriptId != null && ScriptEngine.HasScript(npc.ScriptId))
            ScriptEngine.RunFunction(npc.ScriptId, "on_receive",
                new LuaCharacterProxy(npc), new LuaCharacterProxy(player), new LuaItemProxy(item));
    }
}
