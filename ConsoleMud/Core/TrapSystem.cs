using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core;

/// <summary>
/// Floor traps: placed by a character, sprung by a hostile NPC that enters or is
/// caught standing in the room. Each trap is one-shot.
/// </summary>
public static class TrapSystem
{
    public static void Place(Room room, Character owner, string dice, DamageType type, int rootRounds)
    {
        room.Traps.Add(new Trap
        {
            OwnerId = owner.Id,
            SetterName = ColorMarkup.Strip(owner.Name),
            DiceNotation = dice,
            DamageType = type,
            RootRounds = rootRounds
        });
    }

    /// <summary>Called when a character enters a room (from WorldState.MoveCharacter).</summary>
    public static void OnEnter(Character entrant, Room room, WorldState world)
    {
        // Pets are friendly and don't spring their owner's traps.
        if (entrant is NonPlayerCharacter npc && !npc.IsPet && npc.Health > 0 && room.Traps.Count > 0)
            Trip(room, npc, world);
    }

    /// <summary>Called each AI tick: springs traps on a hostile NPC already in the room.</summary>
    public static void CheckRooms(WorldState world)
    {
        foreach (var room in world.Rooms.Values)
        {
            if (room.Traps.Count == 0)
                continue;
            var npc = room.Characters.OfType<NonPlayerCharacter>().FirstOrDefault(n => n.Health > 0 && !n.IsPet);
            if (npc != null)
                Trip(room, npc, world);
        }
    }

    private static void Trip(Room room, NonPlayerCharacter npc, WorldState world)
    {
        var trap = room.Traps[0];
        room.Traps.RemoveAt(0);

        int dmg = DamageResolver.Apply(npc, trap.DamageType, DiceRoller.Roll(trap.DiceNotation));
        npc.Health -= dmg;
        npc.StatusEffects.Add(new StatusEffect
        {
            Name = "snared", Modifier = EffectModifier.Root, Polarity = EffectPolarity.Negative,
            Type = EffectType.Physical, TicksRemaining = trap.RootRounds
        });

        ColorConsole.WriteLine($"\n{npc.Name} springs a hidden trap! {dmg} damage and snared in place!", ConsoleColor.DarkYellow);
        Console.Write("> ");

        if (npc.Health <= 0)
        {
            world.Characters.TryGetValue(trap.OwnerId, out var owner);
            DeathService.HandleDeath(npc, world, owner);
        }
    }
}
