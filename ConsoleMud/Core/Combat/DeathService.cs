using ConsoleMud.Entities;

namespace ConsoleMud.Core.Combat;

/// <summary>
/// Centralized death handling so both the combat tick and skill handlers
/// resolve a kill the same way. Phase 7 will replace the player branch with
/// safe-room recall at 1 HP.
/// </summary>
public static class DeathService
{
    public static void HandleDeath(Character deadCharacter, WorldState world)
    {
        deadCharacter.CombatTarget = null;

        if (deadCharacter is Player)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("\n*** YOU HAVE BEEN SLAIN! ***\nGame Over.");
            Console.ResetColor();
            Environment.Exit(0);
            return;
        }

        if (deadCharacter is NonPlayerCharacter npc)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nThe {npc.Name} collapses to the ground, dead!");
            Console.ResetColor();

            if (world.Rooms.TryGetValue(npc.CurrentRoomId, out var room))
            {
                room.Characters.Remove(npc);

                // Spawn a corpse container holding the NPC's gear and inventory.
                var corpse = new Item
                {
                    Name = $"corpse of a {npc.Name}",
                    IsContainer = true,
                    IsGetable = false,
                    Description = $"The cold remains of a {npc.Name}."
                };
                foreach (var gear in npc.Equipment.Values)
                    corpse.Contents.Add(gear);
                corpse.Contents.AddRange(npc.Inventory);
                room.Items.Add(corpse);
            }

            world.Characters.Remove(npc.Id);

            // Break any other combatants still locked onto this corpse.
            foreach (var ch in world.Characters.Values.Where(c => c.CombatTarget == npc))
                ch.CombatTarget = null;
        }
    }
}
