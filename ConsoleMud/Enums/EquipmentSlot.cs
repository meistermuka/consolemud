namespace ConsoleMud.Enums;

public enum EquipmentSlot
{
    Head,
    Mask,
    Necklace,
    Torso,
    Gloves,
    Belt,
    Pants,
    Shins,
    Boots,
    MainHand,  // Weapon 1
    OffHand,   // Shield OR Weapon 2

    // Generic (declared) family slots that items use. The wear logic places them
    // into the first free physical slot of the matching family below.
    Ring,
    Earring,
    Arm,
    Forearm,

    // Physical storage slots for the paired families (the Equipment dictionary keys).
    Ring1,
    Ring2,
    Earring1,
    Earring2,
    LeftArm,
    RightArm,
    LeftForearm,
    RightForearm
}
