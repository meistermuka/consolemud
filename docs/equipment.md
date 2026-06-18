# Equipment & Slots

A character wears items in slots, tracked in `Character.Equipment`
(`Dictionary<EquipmentSlot, Item>`). An item declares the slot it targets via
`ItemBlueprint.TargetSlot` / `Item.TargetSlot`.

## Declared vs physical slots

Most slots are one-to-one (`Head`, `Torso`, `Belt`, `MainHand`, ...). Four families
have **two physical slots** but items declare a single **generic** slot:

| Declared (item uses) | Physical storage slots |
|---|---|
| `Ring` | `Ring1`, `Ring2` |
| `Earring` | `Earring1`, `Earring2` |
| `Arm` | `LeftArm`, `RightArm` |
| `Forearm` | `LeftForearm`, `RightForearm` |

So a ring is authored as `"TargetSlot": "Ring"` — you don't pick left/right.
`SlotResolver` (`Helpers/SlotResolver.cs`) maps a declared slot to its ordered
physical pool and chooses where the item goes:

- fill the **first free** physical slot in the family;
- if all are full, `wear` (single) **replaces the first/oldest**; bulk wear skips instead.

Old numbered values (`Ring1`, `LeftArm`, ...) are still accepted and resolve to the
same family, so existing data and saves keep working. The `equipment`/`eq` display
still shows the physical slots (Ring (L)/(R), etc.).

## Commands

- `wear <item>` — armor/accessories to their slot (replace-oldest on a full family).
- `wield <weapon>` — main hand. `second <weapon|shield>` — off hand.
- `equipment` / `eq` — show every slot.

## Authoring

Set `TargetSlot` to a declared slot: `Head, Mask, Necklace, Torso, Gloves, Belt,
Pants, Shins, Boots, Ring, Earring, Arm, Forearm, MainHand, OffHand`. The
[area builder](area-builder.md) offers exactly this list.
