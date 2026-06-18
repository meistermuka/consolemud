using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Helpers;

/// <summary>
/// Maps a declared equipment slot to its physical storage slots and picks where
/// an item should actually go. Paired families (Ring/Earring/Arm/Forearm) fill
/// their first free physical slot; a declared physical slot resolves to its own
/// family for backward compatibility (so "Ring1" behaves like "Ring").
/// </summary>
public static class SlotResolver
{
    private static readonly EquipmentSlot[] RingPool = { EquipmentSlot.Ring1, EquipmentSlot.Ring2 };
    private static readonly EquipmentSlot[] EarringPool = { EquipmentSlot.Earring1, EquipmentSlot.Earring2 };
    private static readonly EquipmentSlot[] ArmPool = { EquipmentSlot.LeftArm, EquipmentSlot.RightArm };
    private static readonly EquipmentSlot[] ForearmPool = { EquipmentSlot.LeftForearm, EquipmentSlot.RightForearm };

    /// <summary>The ordered physical slots a declared slot can occupy.</summary>
    public static EquipmentSlot[] Family(EquipmentSlot declared) => declared switch
    {
        EquipmentSlot.Ring or EquipmentSlot.Ring1 or EquipmentSlot.Ring2 => RingPool,
        EquipmentSlot.Earring or EquipmentSlot.Earring1 or EquipmentSlot.Earring2 => EarringPool,
        EquipmentSlot.Arm or EquipmentSlot.LeftArm or EquipmentSlot.RightArm => ArmPool,
        EquipmentSlot.Forearm or EquipmentSlot.LeftForearm or EquipmentSlot.RightForearm => ForearmPool,
        _ => new[] { declared }
    };

    /// <summary>
    /// Resolves the physical slot an item should occupy. Returns the first free
    /// slot in the family; if all are full, returns the first slot when
    /// <paramref name="allowReplace"/> is true (replace the oldest), otherwise null.
    /// </summary>
    public static EquipmentSlot? Resolve(Character wearer, EquipmentSlot declared, bool allowReplace)
    {
        var pool = Family(declared);

        foreach (var slot in pool)
            if (!wearer.Equipment.ContainsKey(slot))
                return slot;

        return allowReplace ? pool[0] : (EquipmentSlot?)null;
    }
}
