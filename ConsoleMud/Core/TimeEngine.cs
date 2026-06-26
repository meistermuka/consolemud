using System.Xml;
using ConsoleMud.Entities;

namespace ConsoleMud.Core;

public class TimeEngine
{
    private readonly WorldState _world;
    private readonly CombatSystem _combatSystem;
    private readonly StatusAndRegenSystem _statusAndRegenSystem;
    private long _masterPulseCount = 0;
    
    // Multipliers relative to base 250ms tick rate
    private const int UniversalTickTimeBase = 250;
    private const int CombatInterval = 4; // 1 second auto-attacks
    private const int AiInterval = 8; // 2 second AI thinking tick
    private const int StatusInterval = 12; // 3 second status effects and regen
    private const int WeatherInterval = 240; // 60 second weather change

    private const int HideIdleSeconds = 10; // idle time before a hider slips away
    private const int AutosaveInterval = 480; // 120 seconds

    public TimeEngine(WorldState world)
    {
        _world = world;
        _combatSystem = new CombatSystem(world); // initializing combat engine context
        _statusAndRegenSystem = new StatusAndRegenSystem(world);
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
                _statusAndRegenSystem.Tick();
            
            if (_masterPulseCount % AiInterval == 0)
            {
                UpdateStealth();
                TrapSystem.CheckRooms(_world);
                UpdateNpcIntelligence();
            }

            // Periodic autosave of every active player.
            if (_masterPulseCount % AutosaveInterval == 0)
                foreach (var p in _world.Characters.Values.OfType<Player>())
                    Services.SaveService.Save(p, _world);
            
            // 3. Resolve environment (weather changes)
            if (_masterPulseCount % WeatherInterval == 0)
                UpdateWeather();

            await Task.Delay(UniversalTickTimeBase, cancellationToken);
        }
    }

    private void UpdateNpcIntelligence()
    {
        foreach (var room in _world.Rooms.Values)
        {
            // find if there's a player in the room
            var player = room.Characters.OfType<Player>().FirstOrDefault();
            if (player == null || player.Health <= 0)
                continue;

            // Hidden players go unnoticed by aggressive NPCs.
            if (player.IsHidden)
                continue;
            
            // check if any NPCs in this room want to attack the player
            foreach (var npc in room.Characters.OfType<NonPlayerCharacter>())
            {
                // if the NPC is aggressive, alive, not fighting anyone, and can see the player
                if (npc.IsAggressive && npc.Health > 0 && npc.CombatTarget == null && npc.CanSee(room))
                {
                    npc.CombatTarget = player;
                    
                    // if the player isn't fighting anyone yet, have them retaliate against the NPC
                    if (player.CombatTarget == null)
                        player.CombatTarget = npc;
                    
                    Helpers.ColorConsole.WriteLine($"\nThe {npc.Name} attacks {player.Name}!", ConsoleColor.Red);
                    Console.Write("> ");
                }
            }
        }
    }

    private static readonly Enums.Weather[] WeatherStates =
        { Enums.Weather.Clear, Enums.Weather.Cloudy, Enums.Weather.Raining, Enums.Weather.Storming, Enums.Weather.Snowing };

    /// <summary>Rolls a new weather state and announces it to players standing outside.</summary>
    private void UpdateWeather()
    {
        var next = WeatherStates[Random.Shared.Next(WeatherStates.Length)];
        if (next == _world.CurrentWeather)
            return;
        _world.CurrentWeather = next;

        string line = next switch
        {
            Enums.Weather.Clear => "The skies clear.",
            Enums.Weather.Cloudy => "Clouds roll in overhead.",
            Enums.Weather.Raining => "Rain begins to fall.",
            Enums.Weather.Storming => "A storm breaks, thunder rolling.",
            Enums.Weather.Snowing => "Snow starts to drift down.",
            _ => null
        };

        foreach (var room in _world.Rooms.Values.Where(r => r.IsOutside))
            if (room.Characters.OfType<Player>().Any())
            {
                Helpers.ColorConsole.WriteLine($"\n{line}", ConsoleColor.Blue);
                Console.Write("> ");
                break;
            }
    }

    /// <summary>
    /// A character who knows how to hide slips into stealth after staying idle
    /// and out of combat long enough. Proficiency gates and trains the attempt.
    /// </summary>
    private void UpdateStealth()
    {
        foreach (var player in _world.Characters.Values.OfType<Player>())
        {
            if (player.IsHidden || player.CombatTarget != null)
                continue;
            if (!player.KnownSkills.ContainsKey("hide"))
                continue;
            if ((DateTime.UtcNow - player.LastActionUtc).TotalSeconds < HideIdleSeconds)
                continue;

            double proficiency = player.KnownSkills["hide"];
            if (Skills.ProficiencyMath.RollSuccess(proficiency))
            {
                player.IsHidden = true;
                Helpers.ColorConsole.WriteLine("\nYou slip into the shadows, hidden from view.", ConsoleColor.DarkGray);
                Console.Write("> ");
            }
            player.KnownSkills["hide"] = Skills.ProficiencyMath.Gain(proficiency);
        }
    }
}