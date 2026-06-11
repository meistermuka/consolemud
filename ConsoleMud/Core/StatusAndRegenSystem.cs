using ConsoleMud.Entities;

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

            // Process ongoing negative modifiers
            for (int i = character.StatusEffects.Count - 1; i >= 0; i--)
            {
                var effect = character.StatusEffects[i];
                character.Health -= effect.DamagePerTick;
                effect.TicksRemaining--;

                Console.WriteLine($"\n🤢 [STATUS] {character.Name} takes {effect.DamagePerTick} damage from {effect.Name}!");
                stateChanged = true;

                if (effect.TicksRemaining <= 0)
                {
                    character.StatusEffects.RemoveAt(i);
                    Console.WriteLine($"✨ The effects of {effect.Name} fade from {character.Name}.");
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