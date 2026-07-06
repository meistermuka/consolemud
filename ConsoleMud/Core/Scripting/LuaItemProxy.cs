using ConsoleMud.Entities;
using ConsoleMud.Helpers;
using MoonSharp.Interpreter;

namespace ConsoleMud.Core.Scripting;

/// <summary>
/// Read-only Lua proxy for an <see cref="Item"/>. Handed to scripts when
/// iterating a character's inventory (game.inventory) and to the on_receive
/// trigger fired by the player 'give' command.
///
/// Lua usage:
///   item.id              -- runtime GUID string
///   item.name            -- plain name (colour codes stripped)
///   item.is_weapon
///   item.is_container
///   item.is_light_source
/// </summary>
[MoonSharpUserData]
public class LuaItemProxy
{
    public string id              { get; }
    public string name            { get; }
    public bool   is_weapon       { get; }
    public bool   is_container    { get; }
    public bool   is_light_source { get; }

    public LuaItemProxy(Item item)
    {
        id              = item.Id.ToString();
        name            = ColorMarkup.Strip(item.Name);
        is_weapon       = item.IsWeapon;
        is_container    = item.IsContainer;
        is_light_source = item.IsLightSource;
    }
}
