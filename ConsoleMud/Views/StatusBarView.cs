using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using TGuiAttr  = Terminal.Gui.Drawing.Attribute;

namespace ConsoleMud.Views;

public class StatusBarView : View
{
    private IReadOnlyList<(string Text, ConsoleColor Color)> _segments = [];

    public void SetSegments(IReadOnlyList<(string Text, ConsoleColor Color)> segments)
    {
        _segments = segments;
        SetNeedsDraw();
    }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        int width = Viewport.Width;
        int col = 0;
        SetAttribute(new TGuiAttr(ColorName16.Gray, ColorName16.DarkGray));
        Move(0, 0);
        AddStr(new string(' ', width));   // fill background

        foreach (var (text, color) in _segments)
        {
            if (col >= width) break;
            SetAttribute(new TGuiAttr(ToTguiColor(color), ColorName16.DarkGray));
            Move(col, 0);
            int available = width - col;
            var visible = text.Length <= available ? text : text[..available];
            AddStr(visible);
            col += text.Length;
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