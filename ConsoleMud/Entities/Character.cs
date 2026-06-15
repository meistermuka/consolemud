using ConsoleMud.Enums;

namespace ConsoleMud.Entities;

public abstract class Character
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Progression
    public int Level { get; set; } = 1;
    public long Experience { get; set; }

    // Stats
    public int Health { get; set; }
    public int MaxHealth { get; set; }

    public int Mana { get; set; }
    public int MaxMana { get; set; }
    
    // Location tracking
    public Guid CurrentRoomId { get; set; }
    
    // Inventory
    public List<Item> Inventory { get; set; } = new();
    
    public Character CombatTarget { get; set; }

    // Stealth: hidden characters dodge NPC aggro and aren't shown to others.
    public bool IsHidden { get; set; }
    public DateTime LastActionUtc { get; set; } = DateTime.UtcNow;

    public Dictionary<string, DateTime> Cooldowns { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<StatusEffect> StatusEffects { get; set; } = new();

    // Crowd-control queries, read by combat, movement, and skill gates.
    public bool IsStunned => StatusEffects.Any(e => e.Modifier == EffectModifier.Stun && !e.IsExpired);
    public bool IsRooted => StatusEffects.Any(e => e.Modifier == EffectModifier.Root && !e.IsExpired);
    public bool IsBlinded => StatusEffects.Any(e => e.Modifier == EffectModifier.Blind && !e.IsExpired);

    public void BreakHidden()
    {
        if (IsHidden)
            IsHidden = false;
    }

    // Species (and later NPC) damage matrix: DamageType -> multiplier. Empty = all 1.0.
    public Dictionary<DamageType, double> DamageMultipliers { get; set; } = new();

    public Dictionary<EquipmentSlot, Item> Equipment { get; set; } = new();
    // Equipped armour plus any active armour-modifying effects.
    public int TotalArmourRating =>
        Equipment.Values.Sum(i => i.ArmourRating)
        + (int)StatusEffects.Where(e => e.Modifier == EffectModifier.ArmorMod).Sum(e => e.Magnitude);

    // Sum of accuracy-modifying effects (percentage points added to to-hit).
    public double AccuracyBonus =>
        StatusEffects.Where(e => e.Modifier == EffectModifier.AccuracyMod).Sum(e => e.Magnitude);

    // Base one attack per round, modified by haste/slow effects. Floored at 1.
    public int AttackRate =>
        Math.Max(1, 1 + (int)StatusEffects.Where(e => e.Modifier == EffectModifier.AttackRateMod).Sum(e => e.Magnitude));

    // Multiplier on outgoing damage from buffs like berserk (+50% -> 1.5).
    public double DamageDealtMultiplier =>
        1.0 + StatusEffects.Where(e => e.Modifier == EffectModifier.DamageDealtMod).Sum(e => e.Magnitude) / 100.0;

    // % chance to fully avoid an incoming attack (dodge/parry effects feed this).
    public double AvoidanceChance =>
        StatusEffects.Where(e => e.Modifier == EffectModifier.AvoidanceMod).Sum(e => e.Magnitude);

    // % chance to land a critical hit from buffs (max-roll crits are handled separately).
    public double CritChanceBonus =>
        StatusEffects.Where(e => e.Modifier == EffectModifier.CritChanceMod).Sum(e => e.Magnitude);

    public Item MainHandWeapon => Equipment.TryGetValue(EquipmentSlot.MainHand, out var item) && item.IsWeapon ? item : null;
    public Item OffHandWeapon => Equipment.TryGetValue(EquipmentSlot.OffHand, out var item) && item.IsWeapon ? item : null;
    
    public CharacterClass Class { get; set; }
    public Species Species { get; set; }
    public Form Form { get; set; } = Form.Human;
    public Position Position { get; set; } = Position.Standing;

    // Skill id -> proficiency (1.0 .. 100.0)
    public Dictionary<string, double> KnownSkills { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // Creature classification (Undead, Fiend, Animal, ...). Mostly used by NPCs.
    public List<Archetype> Archetypes { get; set; } = new();

    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Intelligence { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }
}