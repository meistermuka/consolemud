using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Scripting;

/// <summary>
/// The only object Lua scripts can reach. All game-state mutations go through
/// this surface so scripts cannot access arbitrary C# types or WorldState directly.
///
/// Method names are intentionally lowercase to match Lua naming conventions.
/// MoonSharp maps public instance methods automatically.
/// </summary>
public class ScriptApi
{
    private readonly WorldState _world;

    public ScriptApi(WorldState world) => _world = world;

    // --- Output ---

    /// <summary>Print a colour-markup string to the game output pane.</summary>
    public void print(string msg)
        => ColorConsole.WriteLine(msg ?? "");

    // --- Dice ---

    /// <summary>Roll a dice expression (e.g. "2d6+3") and return the result.</summary>
    public int roll_dice(string notation)
        => DiceRoller.Roll(notation ?? "1d1");

    // --- Character manipulation ---

    /// <summary>Deal raw damage to a character by their runtime Id string.</summary>
    public void damage(string charId, int amount)
    {
        if (!TryResolveCharacter(charId, out var ch) || ch.Health <= 0) return;
        ch.Health = Math.Max(0, ch.Health - Math.Max(0, amount));
        if (ch.Health <= 0 && ch is NonPlayerCharacter npc)
            DeathService.HandleDeath(npc, _world, killer: null);
    }

    /// <summary>Restore health to a character by their runtime Id string.</summary>
    public void heal(string charId, int amount)
    {
        if (!TryResolveCharacter(charId, out var ch) || ch.Health <= 0) return;
        ch.Health = Math.Min(ch.MaxHealth, ch.Health + Math.Max(0, amount));
    }

    /// <summary>
    /// Teleport a character to a room identified by VirtualId (the stable string id
    /// from the area file, e.g. "forest_entrance"). Safe no-op if either id is unknown.
    /// </summary>
    public void teleport(string charId, string virtualRoomId)
    {
        if (!TryResolveCharacter(charId, out var ch)) return;
        if (!_world.TryGetRoomByVirtualId(virtualRoomId, out _)) return;
        var targetId = _world.RoomsByVirtualId[virtualRoomId];
        _world.MoveCharacter(ch, targetId);
    }

    /// <summary>
    /// Set two characters into mutual combat. Uses runtime Id strings.
    /// The attacker's hidden state is broken. Safe no-op if either id is unknown.
    /// </summary>
    public void engage(string attackerId, string targetId)
    {
        if (!TryResolveCharacter(attackerId, out var attacker)) return;
        if (!TryResolveCharacter(targetId, out var target)) return;
        attacker.BreakHidden();
        attacker.CombatTarget = target;
        if (target.CombatTarget == null)
            target.CombatTarget = attacker;
    }

    // --- Helpers ---

    private bool TryResolveCharacter(string charId, out Character character)
    {
        character = null;
        return Guid.TryParse(charId, out var guid)
               && _world.Characters.TryGetValue(guid, out character);
    }
}
