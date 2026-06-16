using ConsoleMud.Entities;

namespace ConsoleMud.Core.Combat;

/// <summary>
/// Centralized death handling so both the combat tick and skill handlers
/// resolve a kill the same way. Phase 7 will replace the player branch with
/// safe-room recall at 1 HP.
/// </summary>
public static class DeathService
{
    public static void HandleDeath(Character deadCharacter, WorldState world, Character killer = null)
    {
        deadCharacter.CombatTarget = null;

        if (deadCharacter is Player player)
        {
            // Undying faith: a lethal blow leaves the cleric at 1 HP, briefly invulnerable,
            // once per encounter, instead of dying.
            if (player.KnownSkills.ContainsKey("undying_faith") && player.EncounterFlags.Add("undying_faith"))
            {
                player.Health = 1;
                foreach (var t in new[] { Enums.DamageType.Physical, Enums.DamageType.Magic, Enums.DamageType.Holy, Enums.DamageType.Fire })
                    player.StatusEffects.Add(new Entities.StatusEffect
                    {
                        Name = "undying faith",
                        Modifier = Enums.EffectModifier.ImmunityOverride,
                        DamageType = t,
                        Polarity = Enums.EffectPolarity.Positive,
                        TicksRemaining = 1
                    });
                Helpers.ColorConsole.WriteLine(
                    "\nYour faith refuses death! You cling to life at 1 HP, briefly untouchable.", ConsoleColor.Yellow);
                return;
            }

            // Gaean embrace: a lethal blow outdoors reincarnates the druid in place at half
            // health, once per encounter, instead of recalling.
            bool outside = world.Rooms.TryGetValue(player.CurrentRoomId, out var here) && here.IsOutside;
            if (outside && player.KnownSkills.ContainsKey("gaean_embrace") && player.EncounterFlags.Add("gaean_embrace"))
            {
                player.StatusEffects.Clear();
                player.Health = Math.Max(1, player.MaxHealth / 2);
                foreach (var c in world.Characters.Values.Where(c => c.CombatTarget == player))
                    c.CombatTarget = null;
                Helpers.ColorConsole.WriteLine(
                    "\nYour body dissolves into autumn leaves and reforms — the wild will not let you die here.",
                    ConsoleColor.Green);
                return;
            }

            // Break anything fighting the player, then recall to the safe room at 1 HP.
            foreach (var c in world.Characters.Values.Where(c => c.CombatTarget == player))
                c.CombatTarget = null;

            player.StatusEffects.Clear();
            player.Health = 1;
            player.Position = Enums.Position.Standing;
            player.IsHidden = false;

            if (world.SafeRoomId is { } safeId && world.Rooms.ContainsKey(safeId))
                world.MoveCharacter(player, safeId);

            Helpers.ColorConsole.WriteLine(
                "\nYou collapse... and awaken somewhere safe, clinging to life at 1 HP.", ConsoleColor.DarkRed);
            return;
        }

        if (deadCharacter is NonPlayerCharacter npc)
        {
            Helpers.ColorConsole.WriteLine($"\nThe {npc.Name} collapses to the ground, dead!", ConsoleColor.Yellow);

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

            // Award experience to the killer.
            if (killer is Player slayer)
                Services.LevelingService.AwardXp(slayer, npc.XpReward);
        }
    }
}
