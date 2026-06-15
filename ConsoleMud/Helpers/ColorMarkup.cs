namespace ConsoleMud.Helpers;

/// <summary>
/// Inline colour markup for names and descriptions. A code is a brace followed
/// by one letter: lowercase = dark, uppercase = bright. "{x" resets to the base
/// colour, "{{" prints a literal brace.
///
///   {r {R red      {g {G green    {y {Y yellow   {b {B blue
///   {m {M magenta  {c {C cyan     {w {W white    {d gray  {k black
///   {x reset
///
/// Logic (keyword matching, saves) strips codes; display renders them.
/// </summary>
public static class ColorMarkup
{
    public const char Marker = '{';
    public const char ResetCode = 'x';

    private static readonly Dictionary<char, ConsoleColor> Codes = new()
    {
        ['r'] = ConsoleColor.DarkRed,     ['R'] = ConsoleColor.Red,
        ['g'] = ConsoleColor.DarkGreen,   ['G'] = ConsoleColor.Green,
        ['y'] = ConsoleColor.DarkYellow,  ['Y'] = ConsoleColor.Yellow,
        ['b'] = ConsoleColor.DarkBlue,    ['B'] = ConsoleColor.Blue,
        ['m'] = ConsoleColor.DarkMagenta, ['M'] = ConsoleColor.Magenta,
        ['c'] = ConsoleColor.DarkCyan,    ['C'] = ConsoleColor.Cyan,
        ['w'] = ConsoleColor.Gray,        ['W'] = ConsoleColor.White,
        ['d'] = ConsoleColor.DarkGray,    ['k'] = ConsoleColor.Black
    };

    public static bool IsCode(char c) => c == ResetCode || Codes.ContainsKey(c);

    /// <summary>Removes all colour codes, returning plain text for logic and matching.</summary>
    public static string Strip(string text)
    {
        if (string.IsNullOrEmpty(text) || text.IndexOf(Marker) < 0)
            return text ?? string.Empty;

        var sb = new System.Text.StringBuilder(text.Length);
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == Marker && i + 1 < text.Length)
            {
                char next = text[i + 1];
                if (next == Marker) { sb.Append(Marker); i++; continue; }
                if (IsCode(next)) { i++; continue; } // drop the code
            }
            sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>Writes coloured text to the console, restoring colour afterward.</summary>
    public static void Render(string text, ConsoleColor baseColor)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var previous = Console.ForegroundColor;
        Console.ForegroundColor = baseColor;
        var sb = new System.Text.StringBuilder(text.Length);

        void Flush()
        {
            if (sb.Length > 0) { Console.Write(sb.ToString()); sb.Clear(); }
        }

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == Marker && i + 1 < text.Length)
            {
                char next = text[i + 1];
                if (next == Marker) { sb.Append(Marker); i++; continue; }
                if (next == ResetCode) { Flush(); Console.ForegroundColor = baseColor; i++; continue; }
                if (Codes.TryGetValue(next, out var color)) { Flush(); Console.ForegroundColor = color; i++; continue; }
            }
            sb.Append(c);
        }

        Flush();
        Console.ForegroundColor = previous;
    }
}
