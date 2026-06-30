using ConsoleMud.Core.Combat;
using ConsoleMud.Entities;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Mage;

public class FireballHandler : ISkillHandler
{
    public string SkillId => "fireball";

    public void Execute(SkillContext ctx)
    {
        if (!ctx.World.Rooms.TryGetValue(ctx.Caster.CurrentRoomId, out var room)) return;
        var foes = room.Characters.OfType<NonPlayerCharacter>().Where(n => n.Health > 0).ToList();
        if (foes.Count == 0) { Helpers.ColorConsole.WriteLine("Your fireball roars through an empty room.", ConsoleColor.Gray); return; }

        ctx.Caster.BreakHidden();
        Helpers.ColorConsole.WriteLine("A roaring ball of flame detonates across the room!", ConsoleColor.Red);

        foreach (var foe in foes)
        {
            var outcome = AttackResolver.Resolve(ctx.Caster, foe, ctx.Definition.DiceNotation ?? "4d6", DamageType.Fire);
            if (outcome.Hit)
            {
                int dmg = outcome.Damage + ctx.SpellPowerBonus();
                foe.Health -= dmg;
                Helpers.ColorConsole.WriteLine($"  {foe.Name} burns for {dmg}! -> [HP: {Math.Max(0, foe.Health)}]", ConsoleColor.Gray);
            }

            if (foe.Health <= 0) { DeathService.HandleDeath(foe, ctx.World, ctx.Caster); continue; }

            // Survivors are dragged into combat.
            if (foe.CombatTarget == null) foe.CombatTarget = ctx.Caster;
            if (ctx.Caster.CombatTarget == null) ctx.Caster.CombatTarget = foe;
        }
    }
}
