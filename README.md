# The Brick Automation Project (BAP)

A cross-platform desktop application for controlling LEGO trains and motorized devices over Bluetooth Low Energy (BLE).

Built with [Avalonia UI](https://avaloniaui.net/) and .NET 9 — runs on **Windows**, **macOS**, and **Linux**.

## Features

- **8 Hub Types** — Powered Up Hub, Boost Move Hub, Powered Up Remote, WeDo 2.0, SBrick, BuWizz, PFx Brick, EV3
- **Motor & Light Control** — per-port speed sliders, switch left/right, LED color, light brightness
- **Sensor Monitoring** — color sensors, distance sensors, battery level, remote buttons
- **Event Programming** — trigger → action rules (e.g., "when color sensor sees red → stop motor A")
- **C# Scripting** — write custom automation scripts with Roslyn (access hubs, motors, sensors from code)
- **Self-Driving System** — block-based anti-collision with section reservation, path following, automatic switch control
- **JSON Project Files** — human-readable `.bap` project files (replaces legacy binary format)

## Architecture

```
BAP.sln
├── src/BAP.Core        Domain models, interfaces, DI registration
├── src/BAP.Ble         BLE protocols, hub connections, scripting engine
├── src/BAP.Desktop     Avalonia UI with MVVM (CommunityToolkit.Mvvm)
└── tests/BAP.Tests     xUnit unit tests
```

Clean architecture with dependency injection. Protocol implementations are behind `IHubConnection` / `IBleAdapter` interfaces for testability and cross-platform support.

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Build & Run

```bash
# Clone
git clone https://github.com/nnkostov/BAP.git
cd BAP

# Build
dotnet build BAP.sln

# Run
dotnet run --project src/BAP.Desktop

# Test
dotnet test BAP.sln
```

### Publish (Self-Contained)

Build a single-file executable for your platform:

```bash
# Windows
dotnet publish src/BAP.Desktop -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish/win-x64

# macOS (Apple Silicon)
dotnet publish src/BAP.Desktop -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true -o publish/osx-arm64

# macOS (Intel)
dotnet publish src/BAP.Desktop -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true -o publish/osx-x64

# Linux
dotnet publish src/BAP.Desktop -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o publish/linux-x64
```

## Supported Hubs

| Hub | Protocol | Status |
|-----|----------|--------|
| LEGO Powered Up Hub | LPF2 | Supported |
| LEGO Boost Move Hub | LPF2 | Supported |
| LEGO Powered Up Remote | LPF2 | Supported |
| SBrick | SBrick BLE | Supported |
| WeDo 2.0 | WeDo BLE | Supported |
| BuWizz | BuWizz BLE | Supported |
| PFx Brick | PFx BLE | Supported |
| LEGO EV3 | Serial/SPP | Not yet ported (serial, not BLE) |

## Scripting Example

```csharp
// Move train forward for 3 seconds, then stop
await SetMotorSpeed(0, "A", 75);
WriteLine("Train moving...");
await Wait(3000);
await Stop(0, "A");
WriteLine("Done!");
```

Scripts have access to `Hub[]`, `Sections`, `Global[]`, `Wait()`, `WriteLine()`, `SetMotorSpeed()`, and `Stop()`.

## CI

GitHub Actions runs build + test on every push and PR across Windows, macOS, and Linux. Artifacts are published on merges to main.

## Legacy Version

The original WinForms application (V1.5, .NET Framework 4.6.1) source code is preserved in the `LTP.Core/` and `LTP.Desktop/` directories for reference.

---

## Version History

### V2.0.0 — Modernization
- Migrated from .NET Framework 4.6.1 to .NET 9
- Replaced WinForms with Avalonia UI (cross-platform)
- Clean architecture: BAP.Core / BAP.Ble / BAP.Desktop / BAP.Tests
- Replaced BinaryFormatter with System.Text.Json
- Replaced CSharpCodeProvider with Roslyn Scripting API
- Added dependency injection throughout
- Added 69 unit tests
- Added GitHub Actions CI (Windows + macOS + Linux)

<details>
<summary>V1.x Changelog</summary>

#### V1.5 — 08/21/19
- Control+ Hub support, new L + XL Technic engines
- Fix EV3 multi-sensor bug, Section UI redraw fix

#### V1.4 — 08/18/19
- Device connectivity limiting for conventions
- Fix Boost Move Hub ports on latest firmware

#### V1.3 — 05/04/19
- Updated motor control for latest PUP hardware

#### V1.2 — 03/16/19
- BuWizz support, Play Sound in events

#### V1.1 — 02/19/19
- PFx Brick support, improved section reservation
- Battery, motor slider, and EV3 motor bug fixes

#### V1.0 — 02/10/19
- Renamed to "The Brick Automation Project"
- EV3 support, SBrick port fix, PUP Remote events

#### V0.9 — 02/03/19
- WeDo 2.0 support, SBrick sensor calibration fix

#### V0.8 — 02/02/19
- Remote control, LED color config, custom trigger events
- Self-driving: green/red section lights, looped path fix

#### V0.7 — 01/28/19
- Programming: new properties, named sequences, trigger cooldown
- Self-driving: clearing time, 2-section-ahead, speed coefficient

#### V0.6 — 01/20/19
- Self-driving trains, new UI, port selection for sensors

#### V0.5 — 01/14/19
- SBrick Plus + PF Sensors, anti-collision system

#### V0.4 — 01/14/19
- SBrick support, global code editor

#### V0.3 — 12/26/18
- New UI, improved hub detection, color code editor, light management

</details>

## Credits

- [Avalonia UI](https://avaloniaui.net/) — Cross-platform .NET UI framework
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) — MVVM source generators
- [Roslyn](https://github.com/dotnet/roslyn) — C# scripting engine

## License

This software is free and developed for the community of AFOL (Adult Fans of LEGO).
