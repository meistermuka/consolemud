using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Services;

/// <summary>
/// Builds live <see cref="Item"/> instances from an <see cref="ItemBlueprint"/>.
/// Shared by the area loader (spawning) and the scripting layer (game.give_item)
/// so both mint items identically from the same template definition.
/// </summary>
public static class ItemFactory
{
    public static Item CreateLiveItem(ItemBlueprint bp)
    {
        Enum.TryParse<EquipmentSlot>(bp.TargetSlot, true, out var targetSlot);
        Enum.TryParse<WeaponType>(bp.WeaponType, true, out var weaponType);
        return new Item
        {
            Name = bp.Name,
            Description = bp.Description,
            IsGetable = bp.IsGetable,
            IsContainer = bp.IsContainer,
            IsWeapon = bp.IsWeapon,
            WeaponType = weaponType,
            DiceNotation = bp.DiceNotation,
            AttackVerbs = bp.AttackVerbs,
            IsArmour = bp.IsArmor,
            IsEquippable = bp.IsEquippable,
            IsShield = bp.IsShield,
            TargetSlot = targetSlot,
            ArmourRating = bp.ArmorRating,
            IsCloseable = bp.IsCloseable,
            IsLocked = bp.StartsLocked,
            IsOpen = !bp.StartsLocked, // locked containers start closed
            LockKeyId = bp.LockKeyId,
            KeyId = bp.KeyId,
            IsLightSource = bp.IsLightSource,
            GrantsDarkvision = bp.GrantsDarkvision
        };
    }
}
