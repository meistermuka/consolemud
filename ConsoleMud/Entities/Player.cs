using ConsoleMud.Enums;

namespace ConsoleMud.Entities;

public class Player : Character
{
    public string Username { get; set; }

    // Mage elemental specialization (Fire/Cold/Lightning), chosen at its level.
    // The level-up prompt that sets this lands with progression; null until then.
    public DamageType? Specialization { get; set; }

    // The ranger's active tamed companion, if any.
    public NonPlayerCharacter Pet { get; set; }
}
