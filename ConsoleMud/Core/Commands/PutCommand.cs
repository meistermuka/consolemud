using ConsoleMud.Entities;
using ConsoleMud.Helpers;

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
            ColorConsole.WriteLine("Syntax: put <item> in <container>");
            return;
        }

        string itemName = string.Join(" ", args.Take(inIndex));
        string containerName = string.Join(" ", args.Skip(inIndex + 1));

        var item = player.Inventory.FirstOrDefault(i => i.MatchesKeyword(itemName));
        if (item == null) { ColorConsole.WriteLine($"You aren't carrying a '{itemName}'."); return; }

        var container = ContainerFinder.Find(player, world, containerName);

        if (container == null) { ColorConsole.WriteLine($"You don't see a '{containerName}' here."); return; }
        if (!container.IsContainer) { ColorConsole.WriteLine($"You can't put things inside the {container.Name}."); return; }
        if (!container.IsOpen) { ColorConsole.WriteLine($"The {container.Name} is closed."); return; }
        if (ReferenceEquals(item, container)) { ColorConsole.WriteLine("You can't put something inside itself."); return; }

        player.Inventory.Remove(item);
        container.Contents.Add(item);
        ColorConsole.WriteLine($"You slide the {item.Name} safely into the {container.Name}.", ConsoleColor.Gray);
    }
}
