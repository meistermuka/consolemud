// See https://aka.ms/new-console-template for more information

using ConsoleMud.Core;
using ConsoleMud.Core.Commands;
using ConsoleMud.Entities;
using ConsoleMud.Helpers;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Booting local world...standby...");
        var world = WorldBuilder.CreateSampleWorld();
        var parser = new CommandParser();
        
        var foyerId = world.Rooms.First(r => r.Value.Name.Contains("Foyer")).Key;

        var player = new Player
        {
            Name = "Kronos",
            Description = "An elf",
            Health = 100,
            MaxHealth = 100,
            CurrentRoomId = foyerId
        };
        
        // Tracking player in the master state
        world.Rooms[foyerId].Characters.Add(player);
        world.Characters[player.Id] = player;
        
        Console.Clear();
        Console.WriteLine("=== Welcome to the Sandbox MUD ===");
        
        // Renter initial starting room
        new LookCommand().Execute(player, Array.Empty<string>(), world);
        
        // combat engine thread
        Task.Run(async () =>
        {
            while (true)
            {
                // Combat round tick is 2 seconds
                await Task.Delay(2000);
                
                // check if player is actively fighting
                if (player.CombatTarget is NonPlayerCharacter npc)
                {
                    // make sure target has not moved or been killed
                    if (npc.Health <= 0)
                    {
                        player.CombatTarget = null;
                        continue;
                    }
                    
                    // player auto attack
                    ExecuteAutoAttack(player, npc, world);
                    
                    // Enemy counter attack if survived
                    if (npc.Health > 0)
                        ExecuteAutoAttack(npc, player, world);
                    else
                        player.CombatTarget = null;

                    Console.Write("\n> ");
                }
            }
        });
        // Main loop
        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine();

            if (input?.ToLower() == "exit" || input?.ToLower() == "quit")
            {
                Console.WriteLine("Goodbye!");
                break;
            }
            
            parser.ParseAndExecute(input, player, world);
        }
    }

    static void ExecuteAutoAttack(Character attacker, Character defender, WorldState world)
    {
        string verb = attacker.EquippedWeapon?.AttackVerbs[0] ?? "punch";
        string dice = attacker.EquippedWeapon?.DiceNotation ?? "1d3";

        int rawDamage = DiceRoller.Roll(dice);
        int armourMitigation = defender.EquippedArmour?.ArmourRating ?? 0;
        
        // core mitigation logic: damage minus armour rating, min of 1
        int finalDamage = Math.Max(1, rawDamage - armourMitigation);
        defender.Health -= finalDamage;
        
        Console.WriteLine($"{attacker.Name} {verb}s {defender.Name} for {finalDamage} damage! ({dice}) -> [{defender.Name} HP: {Math.Max(0, defender.Health)}/{defender.MaxHealth}]");
        
        if (defender.Health <= 0)
            HandleDeath(defender, world);
    }

    static void HandleDeath(Character deadCharacter, WorldState world)
    {
        if (deadCharacter is Player player)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("\n*** YOU HAVE DIED! ***");
            Console.ResetColor();
            Environment.Exit(0);
        }
        else if (deadCharacter is NonPlayerCharacter npc)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nThe {npc.Name} drops dead!");
            Console.ResetColor();
        
            var room = world.Rooms[npc.CurrentRoomId];
            room.Characters.Remove(npc);
            world.Characters.Remove(npc.Id);
            room.Items.Add(new Item { Name = $"corpse of a {npc.Name}", IsContainer = true });
        }
    }
}