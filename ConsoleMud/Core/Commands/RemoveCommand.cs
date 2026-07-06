using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Commands;

public class RemoveCommand : ICommand
{
    public string Description => "Remove a piece of armor or equipment, or 'remove all'.";
    public string Usage => "remove <item|all>";
    public string Example => "remove all";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0) { ColorConsole.WriteLine("Remove what?"); return; }

        if (args[0].Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            RemoveAll(player);
            return;
        }
        
        string targetName = string.Join(" ", args);
        var match = player.Equipment.FirstOrDefault(kvp => kvp.Value.MatchesKeyword(targetName));
        
        if (match.Key == default)
        {
            ColorConsole.WriteLine($"You aren't carrying a '{targetName}'.");
            return;
        }

        player.Equipment.Remove(match.Key);
        player.Inventory.Add(match.Value);
        ColorConsole.WriteLine($"You stop using your {match.Value.Name}.");
    }
    
    private void RemoveAll(Player player)
    {
        if (player.Equipment.Count == 0)
        {
            ColorConsole.WriteLine("You are not wearing anything."); 
            return;
        }

        foreach (var item in player.Equipment)
        {
            player.Equipment.Remove(item.Key);
            player.Inventory.Add(item.Value);
            ColorConsole.WriteLine($"You stop using your {item.Value.Name}.");
        }
        ColorConsole.WriteLine("You remove all your equipment.");
    }
}