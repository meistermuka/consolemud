using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core.Services;

public static class NpcFactory
{
    // Proficiency granted for skills listed on an NPC blueprint. NPCs are
    // "masters" of their listed skills so scripted casts don't randomly fizzle.
    private const double NpcSkillProficiency = 100.0;

    public static NonPlayerCharacter CreateLiveNpc(NpcBlueprint bp, Guid roomId, IReadOnlyDictionary<string, ItemBlueprint> itemTemplates)
    {
        var npc = new NonPlayerCharacter
        {
            Name = bp.Name,
            Description = bp.Description,
            Health = bp.Health,
            MaxHealth = bp.MaxHealth,
            Mana = bp.Mana,
            MaxMana = bp.MaxMana,
            Level = bp.Level < 1 ? 1 : bp.Level,
            CurrentRoomId = roomId,
            IsAggressive = bp.IsAggressive,
            InnateDarkvision = bp.HasDarkvision,
            Archetypes = ParseArchetypes(bp.Archetypes),
            // Fallback reward scales with the NPC's level and toughness.
            XpReward = bp.XpReward > 0 ? bp.XpReward : (bp.Level < 1 ? 1 : bp.Level) * 10 + bp.MaxHealth,
            ScriptId = bp.ScriptId,
        };

        // Grant any listed skills at mastery proficiency.
        if (bp.Skills != null)
            foreach (var skillId in bp.Skills)
                if (!string.IsNullOrWhiteSpace(skillId))
                    npc.KnownSkills[skillId] = NpcSkillProficiency;

        // If the NPC template requests an equipped starter item weapon, generate it automatically
        if (!string.IsNullOrEmpty(bp.EquippedWeaponTemplateId) && itemTemplates.TryGetValue(bp.EquippedWeaponTemplateId, out var weaponBp))
            npc.Equipment[EquipmentSlot.MainHand] = ItemFactory.CreateLiveItem(weaponBp);

        return npc;
    }

    private static List<Archetype> ParseArchetypes(string[] names)
    {
        var list = new List<Archetype>();
        if (names == null) return list;
        foreach (var n in names)
            if (Enum.TryParse<Archetype>(n, true, out var a))
                list.Add(a);
        return list;
    }
}
