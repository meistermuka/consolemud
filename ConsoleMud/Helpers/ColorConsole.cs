namespace ConsoleMud.Helpers;

/// <summary>
/// Console output that understands colour markup. Use this anywhere a string
/// might carry a coloured name or description so the codes render instead of
/// printing raw. Pass a base colour for headers; "{x" resets to it.
/// </summary>
public static class ColorConsole
{
    public static void Write(string text, ConsoleColor? baseColor = null)
        => ColorMarkup.Render(text, baseColor ?? Console.ForegroundColor);

    public static void WriteLine(string text, ConsoleColor? baseColor = null)
    {
        ColorMarkup.Render(text, baseColor ?? Console.ForegroundColor);
        Console.WriteLine();
    }

    public static void WriteLine() => Console.WriteLine();
}
