namespace ConsoleMud.Entities;

public class Item
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Composition flags instead of deep inheritance
    public bool IsGetable { get; set; }
    public bool IsContainer { get; set; }
    public List<Item> Contents { get; set; } = new(); // Used if IsContainer is True
    
    // For weapons/armour add optional stats class
    //public ItemAttributes Attributes { get; set; }
}