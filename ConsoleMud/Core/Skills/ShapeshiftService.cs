using ConsoleMud.Core.Services;
using ConsoleMud.Entities;
using ConsoleMud.Entities.Definitions;
using ConsoleMud.Enums;
using ConsoleMud.Helpers;

namespace ConsoleMud.Core.Skills;

/// <summary>
/// Druid shapeshifting: enter/leave a form, applying its stat buffs as a tagged
/// effect plus a tracked temporary max-HP bump, and restoring on revert. Combat
/// and the skill executor read the active form for attack swaps and gates.
/// </summary>
public static class ShapeshiftService
{
    private const string Tag = "form";
    private static DefinitionRegistry _definitions;

    public static void Initialize(DefinitionRegistry definitions) => _definitions = definitions;

    /// <summary>The active form's definition, or null when human.</summary>
    public static FormDefinition GetForm(Character c)
    {
        if (c.Form == Form.Human || _definitions == null)
            return null;
        return _definitions.Forms.TryGetValue(c.Form.ToString(), out var fd) ? fd : null;
    }

    public static void Enter(Character c, Form form, WorldState world)
    {
        if (form == Form.Human) { Revert(c); return; }
        if (c.Form == form) { ColorConsole.WriteLine("You are already in that form."); return; }
        if (_definitions == null || !_definitions.Forms.TryGetValue(form.ToString(), out var fd))
        {
            ColorConsole.WriteLine("You don't know that form.");
            return;
        }

        Revert(c); // shed any current form first

        c.Form = form;
        c.FormHpBonus = fd.HpBonus;
        c.MaxHealth += fd.HpBonus;
        c.Health += fd.HpBonus;

        if (fd.ArmorBonus != 0)
            c.StatusEffects.Add(new StatusEffect
            {
                Name = fd.Name, SourceSkillId = Tag, Modifier = EffectModifier.ArmorMod,
                Magnitude = fd.ArmorBonus, Polarity = EffectPolarity.Positive, TicksRemaining = -1
            });

        ColorConsole.WriteLine("\n" + (fd.TransformMessage ?? $"You shift into a {fd.Name}."), ConsoleColor.Green);
    }

    public static void Revert(Character c)
    {
        if (c.Form == Form.Human)
            return;

        c.StatusEffects.RemoveAll(e => e.SourceSkillId == Tag);
        c.MaxHealth -= c.FormHpBonus;
        c.FormHpBonus = 0;
        if (c.Health > c.MaxHealth) c.Health = c.MaxHealth;
        if (c.Health < 1) c.Health = 1;
        c.Form = Form.Human;

        ColorConsole.WriteLine("You return to your normal form.", ConsoleColor.Gray);
    }
}
