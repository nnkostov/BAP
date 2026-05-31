---
name: testing-bap-desktop
description: Test the BAP (Brick Automation Project) Avalonia desktop app end-to-end on headless Linux. Use when verifying UI changes, track layout features, or interactivity.
---

# Testing BAP Desktop App

## Environment Setup

1. Start Xvfb display:
   ```bash
   export DISPLAY=:99
   Xvfb :99 -screen 0 1280x720x24 &>/dev/null &
   ```

2. Launch the app:
   ```bash
   cd /home/ubuntu/repos/BAP
   DISPLAY=:99 dotnet run --project src/BAP.Desktop &>/tmp/bap-app.log &
   sleep 4
   ```

3. Take screenshots with `scrot`:
   ```bash
   DISPLAY=:99 scrot /home/ubuntu/screenshot.png
   ```

## Input Methods on Headless X11

Avalonia on headless X11 has specific limitations with synthetic input events:

### What works
- **`xdotool` clicks** — work for Canvas pointer events (selecting nodes on track layout)
- **`xte` (xautomation)** — works for BOTH clicks AND drag operations on Canvas
- **Keyboard shortcuts** — Ctrl+D (load demo), Ctrl+N (new project), Ctrl+P (add program), Ctrl+L (clear console)
- **Tab + Space/Enter** — activates focused Avalonia buttons

### What does NOT work
- **`xdotool` drag** (mousedown + mousemove + mouseup) — does NOT trigger Avalonia drag on Canvas
- **Synthetic mouse clicks on Avalonia Buttons** — neither `xdotool` nor `xte` mouseclick activates Button controls

### Workarounds
- **For drag operations:** Use `xte` with mousedown/mousemove/mouseup sequence:
  ```bash
  DISPLAY=:99 xte 'mousemove 525 190' 'usleep 200000' 'mousedown 1' 'usleep 100000' \
    'mousemove 525 200' 'usleep 50000' 'mousemove 525 250' 'usleep 50000' \
    'mousemove 525 310' 'usleep 200000' 'mouseup 1'
  ```
- **For button activation:** Tab to the button, then press Space:
  ```bash
  DISPLAY=:99 xte 'key Tab' 'usleep 100000' 'key Tab' ... 'key space'
  ```

## Track Layout Testing

### Load demo track
Click the "Load Demo" button (top-right of track layout panel) or use Ctrl+D.
The demo creates 7 sections: Station, North Curve, Switch A, Main Line, South Curve, Siding, Return.

### Node positions
Nodes are laid out in a BFS pattern:
- Row 1: ~y=170, spaced ~175px apart starting at ~x=280
- Row 2: ~y=255 (branch nodes like Siding)
- Row 3: ~y=340 (Return)

### Test procedures
1. **Click to select:** Click on a node → blue border appears, property panel opens on right (220px wide)
2. **Drag to move:** Use `xte` mousedown+mousemove+mouseup → node repositions, Bezier connections follow
3. **Edit properties:** Type in Name field (Ctrl+A to select all, then type), Tab to Apply Changes, Space to activate
4. **Delete section:** Select node, Tab to Delete Section button, Space to activate
5. **Deselect:** Click empty canvas area OR click ✕ button in property panel

## Unit Tests
```bash
cd /home/ubuntu/repos/BAP
dotnet test BAP.sln
```
Expected: 69 tests pass.

## Build
```bash
dotnet build BAP.sln
```
Expected: 0 errors, 0 warnings.

## Devin Secrets Needed
None — this is a local desktop app with no external service dependencies.
