using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core;

/// <summary>
/// Ranger companions: taming, following the owner, assisting in combat, the
/// companion link (shared armor + resistances), and recall.
/// </summary>
public static class PetSystem
{
    private const string LinkTag = "petlink";

    public static void Tame(Player owner, NonPlayerCharacter beast, WorldState world)
    {
        // A previous pet returns to the wild.
        if (owner.Pet != null)
            owner.Pet.OwnerId = null;

        beast.OwnerId = owner.Id;
        beast.IsAggressive = false;
        beast.CombatTarget = null;
        owner.Pet = beast;

        ColorConsole.WriteLine($"{beast.Name} bonds with you and becomes your companion.", ConsoleColor.Green);
    }

    /// <summary>Move the pet to the owner's room after the owner travels.</summary>
    public static void FollowOwner(Player owner, WorldState world)
    {
        var pet = owner.Pet;
        if (pet == null || pet.Health <= 0 || pet.CurrentRoomId == owner.CurrentRoomId)
            return;
        world.MoveCharacter(pet, owner.CurrentRoomId);
    }

    /// <summary>call_companion: whistle the pet to the owner's room.</summary>
    public static void Recall(Player owner, WorldState world)
    {
        if (owner.Pet == null || owner.Pet.Health <= 0)
        {
            Console.WriteLine("You have no companion to call.");
            return;
        }
        world.MoveCharacter(owner.Pet, owner.CurrentRoomId);
        ColorConsole.WriteLine($"{owner.Pet.Name} bounds to your side.", ConsoleColor.Gray);
    }

    /// <summary>Each combat tick: pets assist their owner and refresh the link.</summary>
    public static void UpdatePets(WorldState world)
    {
        foreach (var owner in world.Characters.Values.OfType<Player>())
        {
            var pet = owner.Pet;
            if (pet == null) continue;

            // Drop a dead pet.
            if (pet.Health <= 0 || !world.Characters.ContainsKey(pet.Id))
            {
                owner.Pet = null;
                continue;
            }

            ApplyLink(owner, pet);

            // Assist: attack whatever the owner is fighting, when together.
            if (pet.CurrentRoomId == owner.CurrentRoomId && owner.CombatTarget is { Health: > 0 } target)
                pet.CombatTarget = target;
            else if (owner.CombatTarget == null)
                pet.CombatTarget = null;
        }
    }

    private static void ApplyLink(Player owner, NonPlayerCharacter pet)
    {
        pet.StatusEffects.RemoveAll(e => e.SourceSkillId == LinkTag);
        if (!owner.KnownSkills.ContainsKey("companion_link"))
            return;

        // Inherit a quarter of the owner's armor...
        int shared = (int)(owner.TotalArmourRating * 0.25);
        if (shared > 0)
            pet.StatusEffects.Add(new StatusEffect
            {
                Name = "companion link", SourceSkillId = LinkTag, Modifier = EffectModifier.ArmorMod,
                Magnitude = shared, Polarity = EffectPolarity.Positive, TicksRemaining = -1
            });

        // ...and the owner's racial resistances.
        foreach (var kv in owner.DamageMultipliers)
            pet.DamageMultipliers[kv.Key] = kv.Value;
    }
}
