
using Terminal.Gui.ViewBase;
using Terminal.Gui.Drawing;
using TGuiAttr  = Terminal.Gui.Drawing.Attribute;



namespace ConsoleMud.Views;

/// <summary>
/// Scrollable, read-only view that renders colored game output using our
/// ColorMarkup segment model. All public methods must be called on the
/// Terminal.Gui main thread (use Application.Invoke from background threads).
/// </summary>
public class GameOutputView : View
{
    // Each entry is one screen-line made up of colored segments.
    private readonly List<List<(string Text, ConsoleColor Color)>> _lines = new();
    private int _scrollOffset;

    public GameOutputView()
    {
        CanFocus = false;
    }

    /// <summary>
    /// Appends colored segments (which may contain embedded newlines) to the
    /// output, then auto-scrolls to the bottom.
    /// </summary>
    public void Append(IReadOnlyList<(string Text, ConsoleColor Color)> segments)
    {
        if (_lines.Count == 0)
            _lines.Add(new List<(string, ConsoleColor)>());

        foreach (var (text, color) in segments)
        {
            // Split each segment on newlines so we maintain a line-per-entry model.
            var parts = text.Split('\n');
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                    _lines.Add(new List<(string, ConsoleColor)>());

                if (parts[i].Length > 0)
                    _lines[^1].Add((parts[i], color));
            }
        }

        ScrollToBottom();
        SetNeedsDraw();
    }

    private void ScrollToBottom()
    {
        int visibleRows = Viewport.Height;
        if (visibleRows <= 0)
            return;
        _scrollOffset = Math.Max(0, _lines.Count - visibleRows);
    }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        int width  = Viewport.Width;
        int height = Viewport.Height;

        for (int row = 0; row < height; row++)
        {
            int lineIdx = _scrollOffset + row;

            if (lineIdx >= _lines.Count)
            {
                // Clear the empty row so no stale data shows through.
                SetAttribute(new TGuiAttr(ColorName16.Gray, ColorName16.Black));
                Move(0, row);
                AddStr(new string(' ', width));
                continue;
            }

            var line = _lines[lineIdx];
            int col  = 0;

            foreach (var (text, consoleColor) in line)
            {
                if (col >= width) break;
                SetAttribute(new TGuiAttr(ToTguiColor(consoleColor), ColorName16.Black));
                Move(col, row);
                int available = width - col;
                var visible = text.Length <= available ? text : text[..available];
                AddStr(visible);
                col += text.Length;
            }

            // Pad remaining columns so no stale characters bleed through.
            if (col < width)
            {
                SetAttribute(new TGuiAttr(ColorName16.Gray, ColorName16.Black));
                Move(col, row);
                AddStr(new string(' ', width - col));
            }
        }

        return true;
    }

    private static ColorName16 ToTguiColor(ConsoleColor c) => c switch
    {
        ConsoleColor.Black       => ColorName16.Black,
        ConsoleColor.DarkBlue    => ColorName16.Blue,
        ConsoleColor.DarkGreen   => ColorName16.Green,
        ConsoleColor.DarkCyan    => ColorName16.Cyan,
        ConsoleColor.DarkRed     => ColorName16.Red,
        ConsoleColor.DarkMagenta => ColorName16.Magenta,
        ConsoleColor.DarkYellow  => ColorName16.Yellow,
        ConsoleColor.Gray        => ColorName16.Gray,
        ConsoleColor.DarkGray    => ColorName16.DarkGray,
        ConsoleColor.Blue        => ColorName16.BrightBlue,
        ConsoleColor.Green       => ColorName16.BrightGreen,
        ConsoleColor.Cyan        => ColorName16.BrightCyan,
        ConsoleColor.Red         => ColorName16.BrightRed,
        ConsoleColor.Magenta     => ColorName16.BrightMagenta,
        ConsoleColor.Yellow      => ColorName16.BrightYellow,
        ConsoleColor.White       => ColorName16.White,
        _                        => ColorName16.White
    };
}
