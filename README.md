# 🟦 Box Breathing Tray

A lightweight Windows 11 system tray application that animates the classic
**box-breathing** pattern — a dot travels around a square, spending exactly
4 seconds on each side (inhale → hold → exhale → hold), all visible as an
animated icon in your taskbar notification area.

---

## Features

| Feature | Details |
|---|---|
| 🎯 System tray only | No main window cluttering your desktop |
| 🔄 Smooth animation | ~60 fps dot-on-square in the tray icon |
| ⏱ Customisable timing | 1–60 seconds per side (default: 4 s) |
| 🎨 Customisable colours | Square outline, dot, and background |
| 💾 Persistent settings | Saved to `%APPDATA%\BoxBreathingTray\settings.json` |
| 🚀 Auto-start | Installer registers a run-at-login registry key |
| 📦 Single-file exe | No .NET runtime required — fully self-contained |

---

## Quick Start (pre-built)

1. Run `BoxBreathingTray_Setup_1.0.0.exe`
2. The app starts immediately and sits in the system tray (bottom-right corner — you may need to expand the tray overflow)
3. **Right-click** the animated icon → **Settings…** to customise colours and timing

---

## Building from Source

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (or later)
- Windows 10 / 11
- *(Optional)* [Inno Setup 6](https://jrsoftware.org/isinfo.php) — to compile the installer

### Steps

```powershell
# Clone / download the source, then:
cd BoxBreathingTray

# Build + publish (produces a self-contained single EXE)
.\build.ps1

# The exe will be at: .\publish\BoxBreathingTray.exe
# The installer (if Inno Setup is installed): .\installer\BoxBreathingTray_Setup_1.0.0.exe
```

Or manually:

```powershell
dotnet publish BoxBreathingTray.csproj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    /p:PublishSingleFile=true `
    --output publish
```

---

## Project Structure

```
BoxBreathingTray/
├── Program.cs                  # Entry point — starts TrayApplicationContext
├── TrayApplicationContext.cs   # ApplicationContext: owns the NotifyIcon
├── AnimationEngine.cs          # Background thread that renders icon frames
├── BreathingSettings.cs        # Settings model + JSON load/save
├── SettingsForm.cs             # Colour + timing settings window
├── BoxBreathingTray.csproj     # SDK-style project (net8.0-windows)
├── app.manifest                # Per-monitor DPI v2 + Win11 compat
├── installer.iss               # Inno Setup script → setup .exe
└── build.ps1                   # One-command build + package script
```

---

## How the Animation Works

```
Side 0 (top):    dot travels LEFT → RIGHT  (Inhale)
Side 1 (right):  dot travels TOP  → BOTTOM (Hold)
Side 2 (bottom): dot travels RIGHT → LEFT  (Exhale)
Side 3 (left):   dot travels BOTTOM → TOP  (Hold)
```

At the default 4 s/side the full cycle is 16 seconds, matching the
[box-breathing (4-4-4-4) technique](https://www.healthline.com/health/box-breathing).

---

## Settings File

`%APPDATA%\BoxBreathingTray\settings.json`

```json
{
  "SecondsPerSide": 4,
  "SquareColorArgb": -11206656,
  "DotColorArgb": -1,
  "BackgroundColorArgb": -16777186
}
```

Delete this file to reset all settings to defaults.

---

## Uninstalling

**Control Panel → Programs → Box Breathing Tray → Uninstall**  
This removes the application files and the auto-start registry entry.
