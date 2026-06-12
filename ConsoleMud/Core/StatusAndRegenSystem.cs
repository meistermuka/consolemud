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
            // Passive mana regeneration up to maximum limits
            if (character.Mana < character.MaxMana)
            {
                character.Mana = Math.Min(character.MaxMana, character.Mana + 5);
                stateChanged = true;
            }

            // Process active effects: damage/heal over time, then age and expire them.
            for (int i = character.StatusEffects.Count - 1; i >= 0; i--)
            {
                var effect = character.StatusEffects[i];

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