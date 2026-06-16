namespace ConsoleMud.Entities.Definitions;

/// <summary>
/// A druid shapeshift form. Data drives the numbers and restrictions; behaviour
/// (attack swap, gates) lives in ShapeshiftService / combat.
/// </summary>
public class FormDefinition
{
    public string Id { get; set; }              // matches the Form enum, lowercased (bear/wolf/owl/dragon)
    public string Name { get; set; }
    public int HpBonus { get; set; }            // temporary max-HP while in form
    public int ArmorBonus { get; set; }

    // Natural attack profile that replaces weapon swings (empty = no melee).
    public string AttackDice { get; set; }
    public string AttackVerb { get; set; }
    public string AttackAttribute { get; set; } // e.g. "Strength", "Dexterity"

    public bool LocksCasting { get; set; }       // bear/wolf: no spells
    public bool LocksPhysical { get; set; }      // owl: no melee / physical skills

    public string BreathDice { get; set; }       // dragon: enables the 'breath' command
    public string TransformMessage { get; set; }
}
