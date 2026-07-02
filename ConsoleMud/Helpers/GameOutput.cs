using Terminal.Gui;
using ConsoleMud.Core;
using ConsoleMud.Entities;
using ConsoleMud.Views;
using ConsoleMud.Core.Services;
using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace ConsoleMud.Helpers;

/// <summary>
/// Central output bridge between game code and the Terminal.Gui TUI.
/// Thread-safe: Print/UpdateStatus may be called from the TimeEngine
/// background thread — they marshal onto the TUI main thread via Invoke.
/// </summary>
public static class GameOutput
{
    private static GameOutputView?  _outputView;
    private static StatusBarView?   _statusBar;
    private static IApplication?    _app;

    /// <summary>
    /// Called once, after Terminal.Gui is initialised, to wire up the views.
    /// </summary>
    public static void Setup(GameOutputView outputView, StatusBarView statusBar, IApplication app)
    {
        _outputView = outputView;
        _statusBar  = statusBar;
        _app        = app;
    }

    /// <summary>
    /// Appends a markup-encoded string to the game output pane.
    /// Safe to call from any thread.
    /// </summary>
    public static void Print(string markup, ConsoleColor baseColor = ConsoleColor.Gray)
    {
        var segments = ColorMarkup.ParseSegments(markup, baseColor);
        _app?.Invoke(() => _outputView?.Append(segments));
    }

    /// <summary>
    /// Refreshes the status bar with the current player HP, mana, and location.
    /// Safe to call from any thread.
    /// </summary>
    public static void UpdateStatus(Player player, WorldState world)
    {
        string location = world.Rooms.TryGetValue(player.CurrentRoomId, out var room)
            ? room.Name
            : "Unknown";
        string xpLine = player.Level >= LevelingService.MaxLevel
            ? "MAX"
            : $"{{y{player.Experience}{{x/{{y{LevelingService.XpForNextLevel(player.Level)}{{x";
        
        string text = $" {{wHP: {{G{player.Health}{{x/{{G{player.MaxHealth}{{x  "
                    + $"{{wMana: {{B{player.Mana}{{x/{{B{player.MaxMana}{{x  "
                    + $"XP: {xpLine}  "
                    + $"[ {location} ]";
        
        var segmets = ColorMarkup.ParseSegments(text, ConsoleColor.Gray);

        _app?.Invoke(() => _statusBar?.SetSegments(segmets));
    }

    /// <summary>
    /// Sets the status bar's text to the given markup.'
    /// </summary>
    /// <param name="markup"></param>
    public static void SetStatusMessage(string markup)
    {
        var segments = ColorMarkup.ParseSegments(markup, ConsoleColor.White);
        _app?.Invoke(() => _statusBar?.SetSegments(segments));
    }
}
