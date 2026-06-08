namespace ConsoleMud.Entities;

public class ActiveEffect
{
    public string Name { get; set; }
    public int DamagePerTick { get; set; }
    public int TicksRemaining { get; set; }
}