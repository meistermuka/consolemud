using ConsoleMud.Entities;

namespace ConsoleMud.Core.Commands;

/// <summary>
/// Finds a container the player can reach (room floor first, then inventory).
/// Phase 3 will add a parallel direction path for doors to open/close/lock.
/// </summary>
internal static class ContainerFinder
{
    public static Item Find(Player player, WorldState world, string keyword)
    {
        var room = world.Rooms[player.CurrentRoomId];
        return room.Items.FirstOrDefault(i => i.MatchesKeyword(keyword))
               ?? player.Inventory.FirstOrDefault(i => i.MatchesKeyword(keyword));
    }
}

public class OpenCommand : ICommand
{
    public string Description => "Open a container (later: a door).";
    public string Usage => "open <container>";
    public string Example => "open chest";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0) { Console.WriteLine("Open what?"); return; }
        string name = string.Join(" ", args);
        var c = ContainerFinder.Find(player, world, name);
        if (c == null) { Console.WriteLine($"You don't see a '{name}' here."); return; }
        if (!c.IsContainer) { Console.WriteLine($"You can't open the {c.Name}."); return; }
        if (c.IsLocked) { Console.WriteLine($"The {c.Name} is locked."); return; }
        if (c.IsOpen) { Console.WriteLine($"The {c.Name} is already open."); return; }
        c.IsOpen = true;
        Helpers.ColorConsole.WriteLine($"You open the {c.Name}.", ConsoleColor.Gray);
    }
}

public class CloseCommand : ICommand
{
    public string Description => "Close a container (later: a door).";
    public string Usage => "close <container>";
    public string Example => "close chest";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0) { Console.WriteLine("Close what?"); return; }
        string name = string.Join(" ", args);
        var c = ContainerFinder.Find(player, world, name);
        if (c == null) { Console.WriteLine($"You don't see a '{name}' here."); return; }
        if (!c.IsContainer || !c.IsCloseable) { Console.WriteLine($"The {c.Name} can't be closed."); return; }
        if (!c.IsOpen) { Console.WriteLine($"The {c.Name} is already closed."); return; }
        c.IsOpen = false;
        Helpers.ColorConsole.WriteLine($"You close the {c.Name}.", ConsoleColor.Gray);
    }
}

public class LockCommand : ICommand
{
    public string Description => "Lock a closed container you hold the key for.";
    public string Usage => "lock <container>";
    public string Example => "lock chest";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0) { Console.WriteLine("Lock what?"); return; }
        string name = string.Join(" ", args);
        var c = ContainerFinder.Find(player, world, name);
        if (c == null) { Console.WriteLine($"You don't see a '{name}' here."); return; }
        if (!c.IsContainer || string.IsNullOrEmpty(c.LockKeyId)) { Console.WriteLine($"The {c.Name} has no lock."); return; }
        if (c.IsLocked) { Console.WriteLine($"The {c.Name} is already locked."); return; }
        if (c.IsOpen) { Console.WriteLine($"Close the {c.Name} first."); return; }
        if (!player.HasKey(c.LockKeyId)) { Console.WriteLine($"You don't have the key to lock the {c.Name}."); return; }
        c.IsLocked = true;
        Helpers.ColorConsole.WriteLine($"You lock the {c.Name}.", ConsoleColor.Gray);
    }
}

public class UnlockCommand : ICommand
{
    public string Description => "Unlock a container you hold the key for.";
    public string Usage => "unlock <container>";
    public string Example => "unlock chest";

    public void Execute(Player player, string[] args, WorldState world)
    {
        if (args.Length == 0) { Console.WriteLine("Unlock what?"); return; }
        string name = string.Join(" ", args);
        var c = ContainerFinder.Find(player, world, name);
        if (c == null) { Console.WriteLine($"You don't see a '{name}' here."); return; }
        if (!c.IsContainer || string.IsNullOrEmpty(c.LockKeyId)) { Console.WriteLine($"The {c.Name} has no lock."); return; }
        if (!c.IsLocked) { Console.WriteLine($"The {c.Name} isn't locked."); return; }
        if (!player.HasKey(c.LockKeyId)) { Console.WriteLine($"You don't have the key to unlock the {c.Name}."); return; }
        c.IsLocked = false;
        Helpers.ColorConsole.WriteLine($"You unlock the {c.Name}.", ConsoleColor.Gray);
    }
}
