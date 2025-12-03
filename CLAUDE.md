# CLAUDE.md - AI Assistant Guide for The Brick Automation Project (BAP)

## Project Overview

**The Brick Automation Project (BAP)** is a Windows desktop application for controlling LEGO brick devices and implementing automated train systems. It enables users to:

- Connect and control multiple LEGO hubs (EV3, Powered UP, WeDo, SBrick, BuWizz, PFx)
- Program custom automation logic using C# code
- Implement self-driving train systems with automatic collision detection and routing
- Configure track sections with sensors and switches for autonomous train management

**Version**: 1.5 (August 2019)
**License**: MIT (Copyright 2020 - Vincent Vergonjeanne)
**Target Framework**: .NET Framework 4.6.1
**UI Framework**: Windows Forms

---

## Directory Structure

```
/BAP
├── EV3.Core/                    # LEGO EV3 protocol implementation
│   ├── Brick.cs                 # EV3 device abstraction
│   ├── Command.cs               # EV3 protocol commands
│   ├── DirectCommand.cs         # Low-level EV3 commands
│   ├── BluetoothCommunication.cs # Serial/BLE communication
│   ├── NetworkCommunication.cs  # WiFi socket communication
│   └── Enums.cs                 # EV3 constants and enumerations
│
├── LTP.Core/                    # Core business logic
│   ├── Devices/                 # Hub implementations
│   │   ├── Hub.cs               # Base class for all device hubs
│   │   ├── WedoHub.cs           # WeDo 2.0 Smart Hub
│   │   ├── EV3Hub.cs            # EV3 Intelligent Brick
│   │   ├── SbrickHub.cs         # SBrick hub
│   │   ├── PFxHub.cs            # PFx Brick
│   │   ├── BuWizzHub.cs         # BuWizz power controller
│   │   ├── RemoteHub.cs         # LEGO Remote control
│   │   └── Port.cs              # Port abstraction (motors, sensors, LEDs)
│   ├── TrainProject.cs          # Project serialization (root data model)
│   ├── TrainProgram.cs          # Event programming & C# code execution
│   ├── Path.cs                  # Train path definitions
│   └── Sections.cs              # Track sections, detectors, switches
│
├── LTP.Desktop/                 # Windows Forms UI
│   ├── Main UI/
│   │   ├── MainBoard.cs         # Main application window
│   │   ├── FormCodeEditor.cs    # C# code editor
│   │   ├── FormWelcome.cs       # Welcome dialog
│   │   ├── SplashForm.cs        # Splash screen
│   │   └── ConnectionLimit.cs   # Device filtering dialog
│   ├── Hubs UI/
│   │   ├── HubControl.cs        # Hub display/control widget
│   │   └── HubEditor.cs         # Hub configuration dialog
│   ├── Program/
│   │   └── ToolStripProgram.cs  # Programming UI toolbar
│   └── Sections UI/
│       ├── SectionsEditor.cs    # Sections/paths editor
│       ├── SectionControl.cs    # Individual section display
│       ├── PathEditor.cs        # Path route configuration
│       └── TrainSectionControl.cs # Train section behavior UI
│
├── Properties/                  # Assembly metadata and resources
├── Resources/                   # Embedded images and assets (50+ icons)
├── Packages/                    # NuGet packages (FastColoredTextBox)
│
├── LegoTrainProject.csproj      # MSBuild project file
├── LegoTrainProject.sln         # Visual Studio solution
├── Program.cs                   # Application entry point
├── App.config                   # .NET runtime configuration
└── packages.config              # NuGet dependencies
```

---

## Key Architecture Components

### Hub Hierarchy

All device types inherit from the base `Hub` class (`LTP.Core/Devices/Hub.cs`):

```
Hub (base class - 1,368 LOC)
├── WedoHub      - WeDo 2.0 Smart Hub (Bluetooth LE)
├── EV3Hub       - Mindstorms EV3 (Bluetooth serial or WiFi)
├── SbrickHub    - SBrick controller (Bluetooth LE)
├── PFxHub       - PFx Brick (Bluetooth LE)
├── BuWizzHub    - BuWizz power controller (Bluetooth LE)
└── RemoteHub    - LEGO Powered UP Remote (Bluetooth LE)
```

### Core Data Models

| Class | File | Purpose |
|-------|------|---------|
| `TrainProject` | `LTP.Core/TrainProject.cs` | Root project model; serializes all data |
| `Hub` | `LTP.Core/Devices/Hub.cs` | Device connection, ports, events |
| `Port` | `LTP.Core/Devices/Port.cs` | Motors, LEDs, sensors abstraction |
| `TrainProgram` | `LTP.Core/TrainProgram.cs` | Event handlers and C# code execution |
| `Sections` | `LTP.Core/Sections.cs` | Track sections for collision detection |
| `Path` | `LTP.Core/Path.cs` | Sequence of sections for routing |

### Communication Layer

- **Bluetooth LE**: Uses Windows Runtime (WinRT) via `Windows.Devices.Bluetooth` for most hubs
- **Bluetooth Serial**: Used for EV3 via `System.IO.Ports.SerialPort`
- **Network Socket**: WiFi communication for EV3 via `System.Net.Sockets`

---

## Build and Development

### Prerequisites

- Visual Studio 2015 or later
- .NET Framework 4.6.1 SDK
- Windows 10 (required for Bluetooth LE WinRT APIs)

### Building

```bash
# Open solution in Visual Studio
LegoTrainProject.sln

# Or build via command line
msbuild LegoTrainProject.sln /p:Configuration=Release
```

### Output

- Debug: `bin/Debug/LegoTrainProject.exe`
- Release: `bin/Release/LegoTrainProject.exe`

### Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| FastColoredTextBox | 2.16.24 | C# code editor with syntax highlighting |

---

## Coding Conventions

### Naming

- **Classes**: PascalCase (`Hub`, `TrainProject`, `WedoHub`)
- **Methods/Properties**: PascalCase (`GetBatteryLevel`, `IsConnected`)
- **Private fields**: lowercase (`device`, `characteristic`)
- **Constants**: UPPERCASE (`WEDO2_BATTERY`, `WEDO2_MOTOR_VALUE_WRITE`)
- **Enums**: PascalCase values (`Change_Color_To_Yellow`, `Button_Plus_is_pressed`)

### Namespace

Single root namespace: `LegoTrainProject`

### Serialization

- Classes use `[Serializable]` attribute with `BinaryFormatter`
- Transient data marked with `[NonSerialized]` attribute
- Project files saved as binary format (not JSON/XML)

### Thread Safety

- Use `SetControlPropertyThreadSafe` delegate pattern for cross-thread UI updates
- Bluetooth operations use `async/await` extensively
- Timer-based events need proper thread marshaling

### Code Organization

- Each Windows Form has three files: `.cs`, `.Designer.cs`, `.resx`
- Never manually edit `.Designer.cs` files - use Visual Studio designer
- Hub implementations follow a consistent pattern (see existing hub classes)

---

## Important Patterns

### Adding a New Hub Type

1. Create new class in `LTP.Core/Devices/` inheriting from `Hub`
2. Implement required abstract methods:
   - `Connect()` / `Disconnect()`
   - `SetMotorSpeed()`, `SetLightColor()`
   - Device-specific GATT characteristics for Bluetooth LE
3. Update `MainBoard.cs` device discovery logic
4. Add corresponding UI in `HubControl.cs` if needed

### Event Programming System

`TrainProgram.cs` contains the event system with:
- `EventType` enum: Defines trigger types (sensor, button, timer)
- `TriggerType` enum: Specific trigger conditions
- Runtime C# code compilation using `CSharpCodeProvider`

### Self-Driving Module

`Sections.cs` and related UI implement:
- Track section reservation system
- Collision detection between trains
- Automatic path following with section clearing

---

## Critical Files to Understand

| Priority | File | Why Important |
|----------|------|---------------|
| 1 | `LTP.Core/Devices/Hub.cs` | Base class for all connectivity |
| 2 | `LTP.Core/TrainProject.cs` | Data model and serialization |
| 3 | `LTP.Core/TrainProgram.cs` | Event system and code execution |
| 4 | `LTP.Desktop/Main UI/MainBoard.cs` | Main application logic |
| 5 | `LTP.Core/Sections.cs` | Self-driving train logic |

---

## Cautions and Gotchas

### Serialization

- Uses `BinaryFormatter` which has security considerations
- Adding/removing fields in serializable classes may break existing project files
- Mark new transient fields with `[NonSerialized]`

### Bluetooth LE

- WinRT Bluetooth APIs are Windows 10+ only
- Device discovery can be slow; existing code has hardcoded timeouts
- GATT characteristic UUIDs are device-specific constants

### Windows Forms Designer

- `.Designer.cs` files are auto-generated
- Manual edits will be overwritten
- Use Visual Studio's Form Designer for UI changes

### Thread Safety

- All UI updates from background threads must use `Invoke`/`BeginInvoke`
- Bluetooth callbacks run on background threads
- Timer events need thread marshaling

### EV3 Protocol

- `EV3.Core/` contains a separate protocol implementation
- Based on the legoev3 library (credited in README)
- Direct command format is complex; refer to `DirectCommand.cs`

---

## No Automated Tests

This project does not have automated tests. When making changes:

1. Build and run the application manually
2. Test device connectivity with actual LEGO hardware
3. Verify project save/load functionality
4. Test self-driving module if modifying sections logic

---

## Third-Party Credits

- **FastColoredTextBox**: https://github.com/PavelTorgashov/FastColoredTextBox
- **Lego EV3 Library**: https://github.com/BrianPeek/legoev3

---

## Common Tasks for AI Assistants

### Understanding Device Communication

Start with `LTP.Core/Devices/Hub.cs` then examine specific hub implementations like `WedoHub.cs` for Bluetooth LE patterns.

### Modifying the UI

1. Locate the form in `LTP.Desktop/`
2. Read both `.cs` and `.Designer.cs` files
3. Make logic changes only in `.cs` file
4. For UI layout changes, describe what's needed (requires VS designer)

### Adding Features

1. Identify the relevant layer (Core vs Desktop)
2. Follow existing patterns in similar classes
3. Consider serialization implications for persistent data
4. Test with actual hardware if possible

### Debugging Connectivity Issues

- Check `MainBoard.cs` for device discovery
- Hub-specific connection in individual hub classes
- Console output via `MainBoard.WriteLine()`
