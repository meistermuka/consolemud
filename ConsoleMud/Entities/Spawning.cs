namespace ConsoleMud.Entities;

public class SpawnRulesBlueprint
{
    public List<SpawnReference> Items { get; set; } = new();
    public List<SpawnReference> Npcs { get; set; } = new();
}

public class SpawnReference
{
    public string TemplateId { get; set; }
    public int Count { get; set; }
}