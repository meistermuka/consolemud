---
name: Terminal.Gui API Fix
overview: Fix all 4 compile errors and 3 obsolescence warnings in Program.cs by migrating from the deprecated static Application API to the modern instance-based API (Application.Create()), and fix the renamed types/events (ColorScheme→Scheme, Toplevel→Window, Accept→Accepting) discovered from inspecting the Terminal.Gui 2.4.16 package XML documentation.
todos:
  - id: fix-program-cs
    content: "Rewrite Program.cs TUI section: Application.Create()/Init()/Run()/Dispose(), Window instead of Toplevel, SchemeManager for status bar color, Accepting event instead of Accept, app.RequestStop(), remove unused usings"
    status: completed
  - id: fix-gameoutputview-cs
    content: Remove 'using ColorHelper;' from GameOutputView.cs
    status: completed
isProject: false
---

# Terminal.Gui API Fix

## What the 2.4.16 XML docs revealed

| Old (removed/obsolete) | New (2.4.16) | Namespace |
|---|---|---|
| `Application.Init()` static | `Application.Create()` → `app.Init()` | `Terminal.Gui.App` |
| `Application.Instance` static | the `IApplication` returned by `Create()` | `Terminal.Gui.App` |
| `Application.Run(top)` static | `app.Run(window)` | `Terminal.Gui.App` |
| `Application.Shutdown()` static | `app.Dispose()` | `Terminal.Gui.App` |
| `Application.RequestStop()` static | `app.RequestStop()` | `Terminal.Gui.App` |
| `new Toplevel()` | `new Window()` — implements `IRunnable` | `Terminal.Gui.Views` |
| `ColorScheme` + `View.ColorScheme` | `Scheme` + `SchemeManager` + `View.SchemeName` | `Terminal.Gui.Drawing` / `Terminal.Gui.Configuration` |
| `inputField.Accept += ` | `inputField.Accepting += ` | event on `View` |

## File-by-file changes

### 1. [`ConsoleMud/Program.cs`](ConsoleMud/Program.cs) — full rewrite of the TUI section

**Usings — remove:**
- `using ColorHelper;` — internal TGui dependency, causes `Color` type conflict
- `using Terminal.Gui.Drivers;` — not used

**Usings — add:**
- `using Terminal.Gui.Configuration;` — for `SchemeManager`
- `using Terminal.Gui.Input;` — for `CommandEventArgs` in `Accepting` handler

**Boot sequence — replace static API with instance API:**

```csharp
// Old:
Application.Init();
var top = new Toplevel();
GameOutput.Setup(outputView, statusBar, Application.Instance);
Application.Run(top);
Application.Shutdown();

// New:
var app = Application.Create();
app.Init();
var window = new Window { Width = Dim.Fill(), Height = Dim.Fill() };
GameOutput.Setup(outputView, statusBar, app);
app.Run(window);
app.Dispose();
```

**Status bar color — replace `ColorScheme` with `Scheme`:**

```csharp
// Old:
statusBar.ColorScheme = new ColorScheme { Normal = new TGuiAttr(...), ... };

// New:
SchemeManager.AddScheme("StatusBar",
    new Scheme(new Attribute(ColorName16.Black, ColorName16.Cyan)));
statusBar.SchemeName = "StatusBar";
```

`Scheme` only requires setting `Normal`; all other roles (`Focus`, `HotNormal`, etc.) are automatically derived.

**Enter-key handler — rename event:**

```csharp
// Old:
inputField.Accept += (s, e) => { ... Application.RequestStop(); ... };

// New:
inputField.Accepting += (s, e) => { ... app.RequestStop(); ... };
```

The `CommandEventArgs` event arg has an `e.Handled` property to prevent further processing.

**Layout root — replace `Toplevel` with `Window`:**

```csharp
// Old:
var top = new Toplevel();
top.Add(outputView, statusBar, inputField);

// New:
var window = new Window { Width = Dim.Fill(), Height = Dim.Fill() };
window.Add(outputView, statusBar, inputField);
```

`Window` implements `IRunnable` so it works directly with `app.Run(window)`.

### 2. [`ConsoleMud/Views/GameOutputView.cs`](ConsoleMud/Views/GameOutputView.cs)

Remove `using ColorHelper;` (line 1). This import is Terminal.Gui's internal dependency and causes a `Color` type conflict with `Terminal.Gui.Drawing.Color`.

No other changes needed — `using TGuiAttr = Terminal.Gui.Drawing.Attribute;` and `ColorName16` usage are already correct.

### 3. [`ConsoleMud/Helpers/GameOutput.cs`](ConsoleMud/Helpers/GameOutput.cs)

No changes needed. It already stores `IApplication` (the modern type) and uses `_app?.Invoke()` correctly. The `using Terminal.Gui.App;` is already present.

## Expected result after fixes

- 0 errors
- Remaining warnings are all pre-existing CS8618/CS8625 nullable warnings in unrelated game files — not introduced by the TUI work
