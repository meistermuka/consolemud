namespace ConsoleMud.Helpers;

/// <summary>
/// Console output that understands colour markup. During the TUI session all
/// output is routed through GameOutput so it lands in the scrollable game
/// pane rather than the raw terminal. Use this anywhere a string might carry
/// a coloured name or description so the codes render instead of printing raw.
/// </summary>
public static class ColorConsole
{
    public static void Write(string text, ConsoleColor? baseColor = null)
        => GameOutput.Print(text, baseColor ?? ConsoleColor.Gray);

    public static void WriteLine(string text, ConsoleColor? baseColor = null)
        => GameOutput.Print(text + "\n", baseColor ?? ConsoleColor.Gray);

    public static void WriteLine()
        => GameOutput.Print("\n", ConsoleColor.Gray);
}
