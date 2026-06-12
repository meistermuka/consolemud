namespace ConsoleMud.Entities.Definitions;

public class ClassDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Vital scaling notes for creation (tunable; consumed by CharacterGenerator).
    public int HpBonus { get; set; }
    public int ManaBonus { get; set; }

    // Which skills this class learns, and at what level.
    public List<ClassSkillEntry> Skills { get; set; } = new();
}

public class ClassSkillEntry
{
    public string SkillId { get; set; }
    public int Level { get; set; }
}
