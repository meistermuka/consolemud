using ConsoleMud.Core.Combat;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills.Handlers.Druid;

public class LightningStrikeHandler : ISkillHandler
{
    public string SkillId => "lightning_strike";

    public void Execute(SkillContext ctx)
    {
        var target = ctx.ResolveNpcTarget();
        if (target == null) { ColorConsole.WriteLine("Call lightning down on what?"); return; }

        ctx.Engage(target);
        var outcome = AttackResolver.Resolve(ctx.Caster, target, ctx.Definition.DiceNotation ?? "3d6", DamageType.Nature);
        if (!outcome.Hit) { Helpers.ColorConsole.WriteLine($"The bolt misses {target.Name}.", ConsoleColor.Gray); return; }

        int dmg = outcome.Damage;
        bool storm = ctx.World.IsStormy;
        if (storm) dmg = (int)(dmg * ctx.Param("weatherBonus", 2));

        target.Health -= dmg;
        Helpers.ColorConsole.WriteLine(
            $"Lightning blasts {target.Name} for {dmg}{(storm ? " (storm-empowered!)" : "")}! -> [{target.Name} HP: {Math.Max(0, target.Health)}]",
            ConsoleColor.Gray);

        if (target.Health <= 0)
            DeathService.HandleDeath(target, ctx.World, ctx.Caster);
    }
}
