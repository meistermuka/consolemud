using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Core;

public class StatusAndRegenSystem
{
    private readonly WorldState _world;

    public StatusAndRegenSystem(WorldState world) => _world = world;

    public void Tick()
    {
        bool stateChanged = false;

        foreach (var character in _world.Characters.Values)
        {
            // Resting and sitting speed recovery; combat suppresses the bonus.
            int regenFactor = character.CombatTarget != null
                ? 1
                : character.Position switch
                {
                    Position.Resting => Core.Services.TuningRegistry.GetInt("regen.restingMultiplier", 3),
                    Position.Sitting => Core.Services.TuningRegistry.GetInt("regen.sittingMultiplier", 2),
                    _ => 1
                };

            // Wilderness lore: faster recovery resting/sitting outdoors.
            if (regenFactor > 1 && character.KnownSkills.ContainsKey("wilderness_lore")
                && _world.Rooms.TryGetValue(character.CurrentRoomId, out var rm) && rm.IsOutside)
                regenFactor += Core.Services.TuningRegistry.GetInt("regen.wildernessLoreBonus", 1);

            if (character.Mana < character.MaxMana)
            {
                int manaRegen = Core.Services.TuningRegistry.GetInt("regen.manaBase", 5) * regenFactor;
                // Arcane meditation speeds mana recovery while sitting or resting.
                if (character.Position != Position.Standing && character.KnownSkills.ContainsKey("arcane_meditation"))
                    manaRegen *= Core.Services.TuningRegistry.GetInt("regen.arcaneMeditationMultiplier", 2);
                character.Mana = Math.Min(character.MaxMana, character.Mana + manaRegen);
                stateChanged = true;
            }

            if (character.Health < character.MaxHealth)
            {
                character.Health = Math.Min(character.MaxHealth,
                    character.Health + Core.Services.TuningRegistry.GetInt("regen.healthBase", 2) * regenFactor);
                stateChanged = true;
            }

            // Process active effects: damage/heal over time, then age and expire them.
            // Crowd-control effects (stun/root/blind) are aged by the combat tick instead.
            for (int i = character.StatusEffects.Count - 1; i >= 0; i--)
            {
                var effect = character.StatusEffects[i];

                if (effect.Modifier is EffectModifier.Stun or EffectModifier.Root or EffectModifier.Blind)
                    continue;

                if (effect.Modifier == EffectModifier.DamageOverTime)
                {
                    int dealt = DamageResolver.Apply(character, effect.DamageType, (int)effect.Magnitude);
                    character.Health -= dealt;
                    Console.WriteLine($"\n[STATUS] {character.Name} takes {dealt} {effect.DamageType} damage from {effect.Name}!");
                    stateChanged = true;
                }
                else if (effect.Modifier == EffectModifier.HealOverTime)
                {
                    int healed = Math.Min((int)effect.Magnitude, character.MaxHealth - character.Health);
                    if (healed > 0)
                    {
                        character.Health += healed;
                        Console.WriteLine($"\n[STATUS] {character.Name} recovers {healed} health from {effect.Name}.");
                        stateChanged = true;
                    }
                }

                if (effect.IsPermanent)
                    continue;

                effect.TicksRemaining--;
                if (effect.IsExpired)
                {
                    character.StatusEffects.RemoveAt(i);
                    Console.WriteLine($"The effects of {effect.Name} fade from {character.Name}.");
                    stateChanged = true;
                }
            }
        }

        // If something displayed while the player is fighting, clean up the command prompt view
        if (stateChanged && _world.Characters.Values.Any(c => c.CombatTarget != null))
        {
            var p = _world.Characters.Values.OfType<Player>().FirstOrDefault();
            if (p != null)
            {
                Console.Write($"[HP: {p.Health}/{p.MaxHealth} | Mana: {p.Mana}/{p.MaxMana}]\n> ");
            }
        }
    }
    
}