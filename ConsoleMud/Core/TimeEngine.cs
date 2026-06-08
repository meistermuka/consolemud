namespace ConsoleMud.Core;

public class TimeEngine
{
    private readonly WorldState _world;
    private long _masterPulseCount = 0;
    
    // Multipliers relative to base 250ms tick rate
    private const int CombatInterval = 4;
    private const int StatusInterval = 12;
    private const int WeatherInterval = 240;

    public TimeEngine(WorldState world) => _world = world;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _masterPulseCount++;
            
            // 1. Resolve combat loop
            if (_masterPulseCount % CombatInterval == 0)
                UpdateCombatRounds();
            
            // 2. Resolve status effects and resource regeneration
            if (_masterPulseCount % StatusInterval == 0)
                UpdateStatusAndRegen();
            
            // 3. Resolve Env (weather, day/night changes)
            if (_masterPulseCount % WeatherInterval == 0)
                UpdateWorldEnvironment();
        }
    }

    private void UpdateCombatRounds()
    {
        
    }

    private void UpdateStatusAndRegen()
    {
        foreach (var character in _world.Characters.Values)
        {
            // Passive recovery
            character.Mana = Math.Min(character.MaxMana, character.Mana + 5);

            // Process active detrimental damage tracking sequences (like poison)
            for (int i = character.StatusEffects.Count - 1; i >= 0; i--)
            {
                var effect = character.StatusEffects[i];
                character.Health -= effect.DamagePerTick;
                effect.TicksRemaining--;

                Console.WriteLine($"\n🤢 [STATUS] {character.Name} suffers {effect.DamagePerTick} damage from {effect.Name}!");

                if (effect.TicksRemaining <= 0)
                {
                    character.StatusEffects.RemoveAt(i);
                    Console.WriteLine($"✨ The effects of {effect.Name} wear off of {character.Name}.");
                }
            }
        }
    }
    
    private void UpdateWorldEnvironment()
    {
        // Simple mock global example: Day/Night or Weather state toggle
        bool isRaining = Random.Shared.Next(0, 2) == 0; 

        if (isRaining)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\n🌧️ A cold rain begins to fall from the sky.");
            Console.ResetColor();

            // Apply direct dynamic consequences natively to any player standing outside
            foreach (var room in _world.Rooms.Values)
            {
                if (room.IsOutside)
                {
                    foreach (var character in room.Characters)
                    {
                        Console.WriteLine($"The freezing rain dampens {character.Name}'s gear.");
                    }
                }
            }
        }
    }
}