using ConsoleMud.Entities;
using ConsoleMud.Helpers;
using MoonSharp.Interpreter;

namespace ConsoleMud.Core.Scripting;

/// <summary>
/// Read-only Lua proxy for any <see cref="Character"/> (player or NPC).
/// Passed to NPC behaviour scripts as the <c>npc</c> and <c>player</c> arguments,
/// and to room event scripts as the <c>character</c> argument (Layer 4).
///
/// Lua usage:
///   npc.id           -- string GUID
///   npc.name         -- plain name (colour codes stripped)
///   npc.health       -- current health
///   npc.max_health   -- maximum health
///   npc.health_pct   -- 0.0–1.0 fraction
///   npc.level
///   npc.is_player    -- true when wrapping a Player
///   npc.is_in_combat -- true when CombatTarget is set
/// </summary>
[MoonSharpUserData]
public class LuaCharacterProxy
{
    public string id           { get; }
    public string name         { get; }
    public int    health       { get; }
    public int    max_health   { get; }
    public double health_pct   { get; }
    public int    level        { get; }
    public bool   is_player    { get; }
    public bool   is_in_combat { get; }

    public LuaCharacterProxy(Character character)
    {
        id           = character.Id.ToString();
        name         = ColorMarkup.Strip(character.Name);
        health       = character.Health;
        max_health   = character.MaxHealth;
        health_pct   = character.MaxHealth > 0
                           ? (double)character.Health / character.MaxHealth
                           : 0.0;
        level        = character.Level;
        is_player    = character is Player;
        is_in_combat = character.CombatTarget != null;
    }
}
