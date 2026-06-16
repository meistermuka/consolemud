using ConsoleMud.Enums;

namespace ConsoleMud.Entities;

/// <summary>
/// A concealed one-shot hazard on a room floor. The next hostile NPC to trip it
/// takes damage and is rooted. The owner is credited for any resulting kill.
/// </summary>
public class Trap
{
    public Guid OwnerId { get; set; }
    public string SetterName { get; set; } = "someone";
    public string DiceNotation { get; set; } = "2d6";
    public DamageType DamageType { get; set; } = DamageType.Physical;
    public int RootRounds { get; set; } = 3;
}
