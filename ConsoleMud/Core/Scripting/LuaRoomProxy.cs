using ConsoleMud.Entities;
using ConsoleMud.Helpers;
using MoonSharp.Interpreter;

namespace ConsoleMud.Core.Scripting;

/// <summary>
/// Read-only Lua proxy for a <see cref="Room"/>.
/// Passed to NPC behaviour scripts (Layer 3) and room event scripts (Layer 4).
///
/// Lua usage:
///   room.id         -- runtime GUID string
///   room.virtual_id -- stable area-file id, e.g. "forest_entrance"
///   room.name       -- plain name (colour codes stripped)
///   room.is_outside
///   room.is_dark
/// </summary>
[MoonSharpUserData]
public class LuaRoomProxy
{
    public string id         { get; }
    public string virtual_id { get; }
    public string name       { get; }
    public bool   is_outside { get; }
    public bool   is_dark    { get; }

    public LuaRoomProxy(Room room)
    {
        id         = room.Id.ToString();
        virtual_id = room.VirtualId ?? "";
        name       = ColorMarkup.Strip(room.Name);
        is_outside = room.IsOutside;
        is_dark    = room.IsDark;
    }
}
