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
    //private const int WeatherInterval = 240;

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
                UpdateNpcIntelligence();
            
            // 3. Resolve Env (weather, day/night changes)
            /*if (_masterPulseCount % WeatherInterval == 0)
                UpdateWorldEnvironment();*/
            
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
            
            // check if any NPCs in this room want to attack the player
            foreach (var npc in room.Characters.OfType<NonPlayerCharacter>())
            {
                // if the NPC is aggressive, alive and not fighting anyone
                if (npc.IsAggressive && npc.Health > 0 && npc.CombatTarget == null)
                {
                    npc.CombatTarget = player;
                    
                    // if the player isn't fighting anyone yet, have them retaliate against the NPC
                    if (player.CombatTarget == null)
                        player.CombatTarget = npc;
                    
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n The {npc.Name} attacks {player.Name}!");
                    Console.ResetColor();
                    Console.Write("> ");
                }
            }
        }
    }
}