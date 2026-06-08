using ConsoleMud.Core;
using ConsoleMud.Entities;
using ConsoleMud.Enums;

namespace ConsoleMud.Helpers;

public static class WorldBuilder
{
    public static WorldState CreateSampleWorld()
    {
        var state = new WorldState();
        // 1. Instantiate Rooms
        var foyer = new Room { Name = "The Dusty Foyer", Description = "A dim entrance hall smelling of old paper. Cobwebs cling to a heavy oak door leading east." };
        var hallway = new Room { Name = "The Grand Hallway", Description = "A long corridor lined with faded velvet tapestries. You hear a faint scratching noise to the north." };
        var armory = new Room { Name = "The Abandoned Armory", Description = "Racks of rusted breastplates line the walls. A pristine weapon rests on an iron anvil." };

        // 2. Link Rooms via Guids (Safe from circular serialization issues)
        foyer.Exits[Direction.East] = hallway.Id;
        hallway.Exits[Direction.West] = foyer.Id;
        hallway.Exits[Direction.North] = armory.Id;
        armory.Exits[Direction.South] = hallway.Id;

        // Add rooms to the master state dictionary
        state.Rooms[foyer.Id] = foyer;
        state.Rooms[hallway.Id] = hallway;
        state.Rooms[armory.Id] = armory;

        // 3. Populate Items
        var sword = new Item
        {
            Name = "sword", 
            Description = "A sharp, lightweight steel shortsword.", 
            IsGetable = true,
            IsWeapon = true,
            DiceNotation = "3d8",
            AttackVerbs = new[] { "slash", "stab", "slice" }
        };
        armory.Items.Add(sword);

        var anvil = new Item { Name = "anvil", Description = "A heavy black anvil. It's not budging.", IsGetable = false };
        armory.Items.Add(anvil);

        // 4. Populate NPCs
        var ratClaws = new Item
        {
            Name = "sharp claws",
            IsWeapon = true,
            DiceNotation = "1d4",
            AttackVerbs = new[] { "scratch", "claw" }
        };
        var rat = new NonPlayerCharacter { Name = "rat", Description = "A giant, red-eyed mangy rat gnawing on a bone.", Health = 12, MaxHealth = 12, CurrentRoomId = hallway.Id };
        rat.Inventory.Add(new Item { Name = "key", Description = "A small, shiny key.", IsGetable = true});
        rat.EquippedWeapon = ratClaws;
        hallway.Characters.Add(rat);
        state.Characters[rat.Id] = rat;

        return state;
    }
}