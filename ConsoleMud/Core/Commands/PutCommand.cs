using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

public class PutCommand : ICommand
{
    public string Description => "Place an item from your inventory into a container.";
    public string Usage => "put <item> in <container>";
    public string Example => "put ring in sack";

    public void Execute(Player player, string[] args, WorldState world)
    {
        int inIndex = Array.IndexOf(args, "in");
        if (inIndex <= 0 || inIndex == args.Length - 1)
        {
            Console.WriteLine("Syntax: put <item> in <container>");
            return;
        }

        string itemName = string.Join(" ", args.Take(inIndex));
        string containerName = string.Join(" ", args.Skip(inIndex + 1));

        // 1. Find item in player's inventory
        var item = player.Inventory.FirstOrDefault(i => i.MatchesKeyword(itemName));
        if (item == null) { Console.WriteLine($"You aren't carrying a '{itemName}'."); return; }

        // 2. Find container nearby
        var container = ContainerFinder.Find(player, world, containerName);

        if (container == null) { Console.WriteLine($"You don't see a '{containerName}' here."); return; }
        if (!container.IsContainer) { Console.WriteLine($"You can't put things inside the {container.Name}."); return; }
        if (!container.IsOpen) { Console.WriteLine($"The {container.Name} is closed."); return; }
        if (ReferenceEquals(item, container)) { Console.WriteLine("You can't put something inside itself."); return; }

        // 3. Perform transfer
        player.Inventory.Remove(item);
        container.Contents.Add(item);
        Helpers.ColorConsole.WriteLine($"You slide the {item.Name} safely into the {container.Name}.", ConsoleColor.Gray);
    }
}