# Colour Markup

Names and descriptions can carry inline colour codes so any substring renders in
colour. The rule that keeps it safe: **logic strips codes, display renders them.**

## Codes

A code is a brace plus one letter. Lowercase = dark, uppercase = bright.
`{x` resets to the base colour; `{{` prints a literal brace.

```
{r {R red      {g {G green    {y {Y yellow   {b {B blue
{m {M magenta  {c {C cyan     {w {W white    {d gray  {k black
{x reset
```

Example (in `Areas/emerald_forest.json`):

```json
"Name": "ferocious {Rdire wolf{x"
```

## Two operations (`Helpers/ColorMarkup.cs`)

- `Strip(text)` — removes codes; used by anything that compares or stores names (keyword matching in `Item`/`NonPlayerCharacter`, saves).
- `Render(text, baseColor)` — writes to the console, applying codes over a base colour and restoring afterward.

`Helpers/ColorConsole.cs` wraps `Render` as `Write`/`WriteLine(text, baseColor)`.

## Using it

Print anything that may contain a coloured name through `ColorConsole`:

```csharp
Helpers.ColorConsole.WriteLine($"You see a {item.Name} here.", ConsoleColor.Gray);
```

Already routed: look, inventory, equipment, combat, death, skill handlers, the
get/drop/put/wield/wear/second/kill messages, and NPC aggro. A few rare
error/validation lines still use plain `Console.WriteLine` (migrate as needed).

## Why matching still works

`Item.Keywords` / `MatchesKeyword` call `ColorMarkup.Strip` first, so `get sword`
matches `{Wgleaming{x sword`. Never compare against the raw, coded `Name`.

## Notes

- Real ANSI colour shows in a terminal; .NET suppresses colour when output is redirected (piped), so captured logs show plain text — that is expected, not a bug.
- To colour an item/NPC from data, just embed codes in its `Name`/`Description` in the area JSON. No schema change.
