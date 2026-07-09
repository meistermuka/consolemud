using ConsoleMud.Core.Combat;
using ConsoleMud.Core.Services;
using ConsoleMud.Core.Skills;
using ConsoleMud.Entities;
using ConsoleMud.Helpers;
using MoonSharp.Interpreter;

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
    private readonly SkillExecutor _executor;
    private readonly DefinitionRegistry _definitions;

    public ScriptApi(WorldState world, SkillExecutor executor, DefinitionRegistry definitions)
    {
        _world = world;
        _executor = executor;
        _definitions = definitions;
    }

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
        // Resolve death for anyone who drops — DeathService handles both the
        // player branch (recall/undying_faith/gaean_embrace) and NPC corpses.
        if (ch.Health <= 0)
            DeathService.HandleDeath(ch, _world, killer: null);
    }

    public void use_skill(string casterId, string spellId, string? targetId = null)
    {
        if (!TryResolveCharacter(casterId, out var caster)) return;

        Character? target = null;
        if (targetId != null && TryResolveCharacter(targetId, out var t))
            target = t;

        _executor.TryUse(caster, spellId, [], _world, target);
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

    // --- Inventory ---

    /// <summary>True if the character carries an item matching the keyword.</summary>
    public bool has_item(string charId, string keyword)
    {
        if (!TryResolveCharacter(charId, out var ch)) return false;
        return ch.Inventory.Any(i => i.MatchesKeyword(keyword));
    }

    /// <summary>
    /// Mint an item from the global template registry (by VirtualId) and add it to
    /// the character's inventory. Returns false if the character or template is unknown.
    /// </summary>
    public string give_item(string charId, string templateId)
    {
        if (!TryResolveCharacter(charId, out var ch)) return string.Empty;
        if (templateId == null || !_world.ItemTemplates.TryGetValue(templateId, out var bp))
            return string.Empty;
        var givenItem = ItemFactory.CreateLiveItem(bp);
        ch.Inventory.Add(givenItem);
        return givenItem.Name;
    }

    /// <summary>
    /// Spawn <paramref name="count"/> NPCs from the global template registry (by VirtualId)
    /// into the room identified by VirtualId. Each NPC is fully initialised, placed in the
    /// room, and registered in world state so it can fight and be targeted. Safe no-op if the
    /// room or template is unknown.
    /// </summary>
    public void spawn_npc(string virtualRoomId, string templateId, int count)
    {
        if (!_world.TryGetRoomByVirtualId(virtualRoomId, out var room))
        {
            ColorConsole.WriteLine($"Error: Room not found: {virtualRoomId}");
            return;
        }
        if (templateId == null || !_world.NpcTemplates.TryGetValue(templateId, out var bp))
        {
            ColorConsole.WriteLine($"Error: NPC template not found: {templateId}");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var npc = NpcFactory.CreateLiveNpc(bp, room.Id, _world.ItemTemplates, _definitions);
            room.Characters.Add(npc);
            _world.Characters[npc.Id] = npc;
        }
    }

    /// <summary>Number of NPCs currently in the room identified by VirtualId.</summary>
    public int count_npcs(string virtualRoomId)
    {
        if (!_world.TryGetRoomByVirtualId(virtualRoomId, out var room)) return 0;
        return room.Characters.Count(c => c is NonPlayerCharacter);
    }

    /// <summary>
    /// Remove the first inventory item matching the keyword. When <paramref name="drop"/>
    /// is true the item is left in the character's current room; otherwise it is destroyed.
    /// Returns false if nothing matched.
    /// </summary>
    public bool take_item(string charId, string keyword, bool drop = false)
    {
        if (!TryResolveCharacter(charId, out var ch)) return false;

        var item = ch.Inventory.FirstOrDefault(i => i.MatchesKeyword(keyword));
        if (item == null) return false;

        ch.Inventory.Remove(item);
        if (drop && _world.Rooms.TryGetValue(ch.CurrentRoomId, out var room))
            room.Items.Add(item);
        return true;
    }

    /// <summary>
    /// Return the character's inventory as a Lua table of read-only item proxies,
    /// so scripts can iterate with ipairs. Empty table if the character is unknown.
    /// </summary>
    public DynValue inventory(ScriptExecutionContext ctx, string charId)
    {
        var table = new Table(ctx.GetScript());
        if (TryResolveCharacter(charId, out var ch))
            foreach (var item in ch.Inventory)
                table.Append(UserData.Create(new LuaItemProxy(item)));
        return DynValue.NewTable(table);
    }

    // --- Helpers ---

    private bool TryResolveCharacter(string charId, out Character character)
    {
        character = null;
        return Guid.TryParse(charId, out var guid)
               && _world.Characters.TryGetValue(guid, out character);
    }
}
