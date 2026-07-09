using ConsoleMud.Entities;

namespace ConsoleMud.Helpers;

public static class ItemExtensions
{
    /// <summary>
    /// Groups items by name and returns each group's count alongside a representative item
    /// (used for display, e.g. "2 x Iron Sword").
    /// </summary>
    public static IEnumerable<(int Count, Item Representative)> GroupedByName(this IEnumerable<Item> items) =>
        items
            .GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => (g.Count(), g.First()));
}
