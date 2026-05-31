---
name: testing-bap-desktop
description: Test the BAP Avalonia desktop application end-to-end. Use when verifying GUI rendering, tab navigation, console output, or build integrity after code changes.
---

# Testing BAP Desktop (Avalonia UI)

## Build & Unit Tests

```bash
cd /home/ubuntu/repos/BAP
dotnet build BAP.sln --configuration Release
dotnet test BAP.sln --configuration Release --verbosity normal
```

Expected: 69 tests pass, 0 warnings.

## Launching the GUI on Linux

```bash
DISPLAY=:0 dotnet run --project /home/ubuntu/repos/BAP/src/BAP.Desktop &>/tmp/bap-app.log &
sleep 5
DISPLAY=:0 xdotool search --name "Brick Automation"
```

The app requires an X11 display. On headless VMs, Xvfb should already be running on :0.

## GUI Verification Checklist

1. **Window title**: `xprop -id <WID> WM_NAME` → "Brick Automation Project"
2. **Tabs**: Hubs (default), Programs, Console — all clickable via xdotool on tab headers
3. **Console tab**: Dark background, "Debug Console" header, "Clear" button, 2 init messages with HH:mm:ss timestamps
4. **Status bar**: Bottom of window shows "Hubs: 0" and "Programs: 0"
5. **Hubs tab**: "Connected Hubs" header, empty list (no BLE hardware available)

## Screenshots

```bash
DISPLAY=:0 import -window root /home/ubuntu/screenshot.png
```

## Known Limitation: Avalonia Buttons on Headless X11

Avalonia `Button` controls do NOT respond to synthetic mouse events (xdotool, xte, python-xlib XTest) on headless X11 window managers. **Tab switching works**, but clicking buttons (+ Add Program, Clear, File menu items) does nothing.

This is an environment limitation, not a code bug. Buttons work fine with real user input on Windows/macOS/Linux with a real window manager.

**Workarounds:**
- Test button logic via unit tests (ViewModel commands are directly testable)
- Verify buttons render correctly via screenshots (visual presence)
- If button interaction testing is critical, consider adding keyboard shortcuts or using Playwright with Avalonia's automation peer support

## Window Management

```bash
# Kill Chrome if it covers the app window
pkill -f chrome

# Focus and raise the BAP window
DISPLAY=:0 xdotool windowfocus <WID> && xdotool windowraise <WID>

# Maximize (install wmctrl first)
sudo apt-get install -y wmctrl
DISPLAY=:0 wmctrl -i -r <WID> -b add,maximized_vert,maximized_horz
```

## Tab Switching Coordinates

Approximate click targets for tab headers (relative to window):
- **Hubs**: x=40, y=54
- **Programs**: x=131, y=54
- **Console**: x=231, y=54

These may shift if the window is resized. Use `xdotool mousemove --window <WID> x y && xdotool click 1`.

## CI

GitHub Actions CI runs on ubuntu-latest, windows-latest, macos-latest. All 3 must pass.

## Devin Secrets Needed

None — this is a desktop app with no external service dependencies for testing.
