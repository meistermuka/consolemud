using ConsoleMud.Entities;

namespace ConsoleMud.Core;

public class TimeEngine
{
    private readonly WorldState _world;
    private readonly CombatSystem _combatSystem;
    private long _masterPulseCount = 0;
    
    // Multipliers relative to base 250ms tick rate
    private const int UniversalTickTimeBase = 250;
    private const int CombatInterval = 4;
    private const int StatusInterval = 12;
    //private const int WeatherInterval = 240;

    public TimeEngine(WorldState world)
    {
        _world = world;
        _combatSystem = new CombatSystem(world); // initializing combat engine context
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _masterPulseCount++;
            
            // 1. Resolve combat loop
            if (_masterPulseCount % CombatInterval == 0)
                _combatSystem.Tick();
            
            // 2. Resolve status effects and resource regeneration
            if (_masterPulseCount % StatusInterval == 0)
                UpdateStatusAndRegen();
            
            // 3. Resolve Env (weather, day/night changes)
            /*if (_masterPulseCount % WeatherInterval == 0)
                UpdateWorldEnvironment();*/
            
            await Task.Delay(UniversalTickTimeBase, cancellationToken);
        }
    }

    private void UpdateCombatRounds()
    {
        
    }

    private void UpdateStatusAndRegen()
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