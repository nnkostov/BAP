# CLAUDE.md

Guidance for working in this repository.

## What this is

**The Brick Automation Project (BAP)**, formerly "The Lego Train Project". It's a Windows desktop app that connects to LEGO and third-party Bluetooth hubs to control model trains, switches, lights and sensors. Users automate a layout with event programs, C# scripts, and a "self-driving" section/path module with anti-collision. The original author (Cosmik42 / Vincent Vergonjeanne) open-sourced v1.5 as-is in June 2020. This repo (`nnkostov/BAP`) is a fork of `Cosmik42/BAP`.

Supported hardware: LEGO Powered Up hub and remote, Boost Move Hub, Control+ (L/XL motors), WeDo 2.0, EV3 (classic Bluetooth over a COM port), SBrick / SBrick Plus, PFx Brick, and BuWizz.

## Build and run

- **Windows only.** C# on .NET Framework 4.6.1, WinForms, and the WinRT Bluetooth LE APIs (`Windows.Devices.Bluetooth.*`). The solution is `LegoTrainProject.sln` with a single old-style (non-SDK) project, `LegoTrainProject.csproj`, from the VS2015 era (ToolsVersion 14).
- You can't build or run it in a Linux container: there's no Mono/.NET toolchain, and WinRT isn't available there anyway. On Linux, check changes by careful reading only. Never say a change "builds" or "works" unless it was actually compiled on Windows.
- Dependencies:
  - `FastColoredTextBox` 2.16.24 (NuGet `FCTB`), used by the code editor. The csproj HintPath is `packages\FCTB.2.16.24\lib\FastColoredTextBox.dll`, but the repo only commits the `.nupkg` (under `Packages/`, capital P), not the extracted DLL. Run `nuget restore` first, or extract `lib/FastColoredTextBox.dll` from the nupkg.
  - `Packages/Windows.winmd` is committed and referenced directly for WinRT.
  - `System.Runtime.WindowsRuntime` is referenced through a brittle relative HintPath into `Program Files (x86)\Reference Assemblies\...\.NETCore\v4.5`. It only resolves if that SDK is installed and the checkout depth matches. Fix it locally if the build can't find it.
- Output assembly: `LegoTrainProject.exe`. **Don't rename it.** User scripts are compiled at runtime against `LegoTrainProject.exe` (see "Runtime C# scripting" below).
- There are no tests, no CI, and no linters.

## Layout

```
Program.cs                 Entry point: Application.Run(new MainBoard())
EV3.Core/                  Vendored Lego EV3 library (BrianPeek/legoev3); namespaces Lego.Ev3.Core / Lego.Ev3.Desktop
LTP.Core/                  Domain + device logic (namespace LegoTrainProject)
  TrainProject.cs          Root saved object: hubs, programs, global code, sections
  TrainProgram.cs          TrainProgram + TrainProgramEvent: event→action rules and runtime C# compilation
  Sections.cs              Section + Sections: self-driving / anti-collision engine
  Path.cs                  A named list of section indices a train follows
  Sections - BeforeFix.cs  Dead copy of an older Sections.cs. NOT in the csproj, so it isn't compiled
  Devices/
    Hub.cs                 Base class = LEGO Wireless Protocol (LWP3) hubs: Powered Up, Boost; motor/switch/ramp/LED API
    Port.cs                Port model + Devices / Functions / Colors / RemoteButtons enums
    RemoteHub.cs           Powered Up remote (2 button ports)
    WedoHub.cs             WeDo 2.0 (separate GATT services/characteristics)
    SbrickHub.cs           SBrick (own protocol; ping timer; sensor calibration)
    PFxHub.cs              PFx Brick (light FX, audio files)
    BuWizzHub.cs           BuWizz
    EV3Hub.cs              EV3 over a Bluetooth serial COM port via EV3.Core
    ConnectionLimitationSettings.cs  MAC allow-list ("convention mode"); namespace LegoTrainProject.LTP.Core.Devices
LTP.Desktop/               WinForms UI (Designer.cs + .resx per form/control)
  Main UI/MainBoard.cs     Main window (~1.7k lines): BLE scanning/connection loop, program tabs/event editor UI, load/save, console
  Main UI/FormCodeEditor   FastColoredTextBox-based C# editor with autocomplete
  Hubs UI/                 HubControl (per-hub card), HubEditor
  Sections UI/             Self-driving editor: sections, paths, switches, train options
  Program/                 ToolStripProgram
Properties/                AssemblyInfo, Resources.resx (icons in Resources/), Settings
```

## Architecture and key flows

- **Discovery and connection** (`MainBoard.SearchForTrains` / `TryToConnect`): a `BluetoothLEAdvertisementWatcher` queues advertisements, and a forever loop pops them off one at a time. Hub type is decided by name (`SBrick`, `PFx`, `Wizz`, `HUB NO.4`) or by service UUID plus manufacturer-data byte (`GetTypeOfHub` → `Hub.HubManufacturerID`). `CreateNewTrain` then builds the right `Hub` subclass. If the device already exists in the project (matched by `DeviceId`), the existing object is reused and `StartListening` is called on it again. `ConnectionLimitationSettings.IsMacAddressAllowed` gates connections.
- **Hub lifecycle:** constructor → `InitPorts()` → `StartListening()` → `RenewCharacteristic()` (finds the GATT service/characteristic) → subscribes `Characteristic_ValueChanged` → `InitializeNotifications()`. Subclasses override these, plus `WriteMessage`, `SetMotorSpeed`, `SetLightBrightness`, `SetLEDColor`, `Disconnect` and `ActivatePortDevice`. Any write failure calls `Dispose()` and drops the address from `MainBoard.registeredBluetoothDevices` so the hub can be rediscovered.
- **Incoming data** (base `Hub`): it parses LWP3 messages by type byte (`0x01` device info, `0x04` port attach/detach, `0x45` sensor values, `0x82` port feedback). Sensor data raises the `ColorTriggered`, `DistanceTriggered` and `RemoteTriggered` events, rate-limited per port by `DistanceColorCooldownMs`. `RegisterDeviceAttachement` maps a `Port.Devices` value to a default `Port.Functions` value and activates sensor reporting.
- **Programs** (`TrainProgram`): a list of `TrainProgramEvent` rules (trigger on hub/port → action on target hub/port). `Start()` subscribes to every hub's sensor events. `ActivateAction` runs the action.
- **Self-driving** (`Sections`): each `Section` has a detector and an optional switch. Both are stored as the string `"<PortId>*<DeviceId>"`. Sections also hold `LeftSection`/`RightSection` links, `CurrentHub`/`ReservedBy` occupancy, `MaxSpeed`, and an `ActionOnRelease`. `Sections.Start()` builds an internal "Self-Driving" `TrainProgram` with one `Distance_is_below 3` event per detector. When a train trips a detector, `Reserve()` waits for the next section (and optionally the one after), throws switches, sets detector LEDs red or green, chooses a resume speed (`SpeedWhenAboutToStop` near switches or the end of the path), and releases the section after `ClearingTimeInMs`. A train's route is a `Path` (`int[]` of section **indices**) together with `Hub.CurrentPathPositionIndex` and `LoopCurrentPath`.

## Runtime C# scripting (user-facing API surface)

The `Execute_Code` actions, the global code tab, and the section "Execute Code" release action all splice user text into the `TrainProgramEvent.Code` template. They replace `Wait(` with `await Task.Delay(` and compile with `CSharpCodeProvider` against `System.dll`, `System.Windows.Forms.dll`, `System.Drawing.dll` and **`LegoTrainProject.exe`**. The result runs `DynamicCode.ExecuteCode(TrainProgramEvent e, List<Hub> Hub, Sections Sections)` on a new thread.

So the **public members of `Hub`, `Port`, `Sections`, `TrainProgram` (including `TrainProgram.Global`) and `MainBoard.WriteLine` are a scripting API**, and users' saved projects contain code that calls them. Don't rename or remove public members or change their signatures. Add new members instead. The code editor's hard-coded autocomplete list in `FormCodeEditor.cs` should be kept in sync with that API. It's currently out of sync: it suggests `ActivateSwitchOnLeft/OnRight`, but the real methods are `ActivateSwitchToLeft/ToRight`.

## Persistence and compatibility gotchas

- Projects are saved as `*.ltp` files with **`BinaryFormatter`** (`TrainProject.Save`/`Load`). The connection allow-list is saved to `connectionSettings.bap` in the app directory, also with `BinaryFormatter`. Serialization depends on type names, namespaces and **field names**. Renaming fields, classes or namespaces, or moving types, breaks every existing user project. That includes misspellings like `RegistredPorts`, `HubManufacturerID.SBRIK`, `Sections.sections` and `EventType.User_Triggerd`. Runtime-only state must stay `[NonSerialized]` (`[field: NonSerialized]` for events). New fields must tolerate being `null` or `0` after loading an old file. `MainBoard.loadToolStripMenuItem_Click` patches nulls after load: follow that pattern.
- `TrainProgramEvent` persists enum **numeric values**, and the WinForms combo boxes fill from `Enum.GetValues`, using the combo index as the enum value. Keep enum values contiguous from 0 and only **append** new members.
- `TrainProgramEvent.ParamTypes` is a hard-coded `new TrainParamType[14][]` indexed by `(int)ActionType`. A new `ActionType` needs a matching row there (and an array size bump), plus handling in `GetParamText`, `HasParam`, `ActivateAction` and the event UI in `MainBoard.GenerateEventUI` / `ActionBox_SelectedValueChanged`.
- `Path.Sections` stores section **indices**. Deleting or reordering sections silently changes what existing paths point to.

## Threading

Bluetooth callbacks, `System.Timers.Timer` callbacks (ramps, switch pulses, stop-fast) and compiled user scripts all run off the UI thread. UI updates must go through the existing `InvokeRequired`/`Invoke` helpers: `MainBoard.WriteLine`, `SetControlPropertyThreadSafe`, `AddControlToFlowPanel`, and `HubControl.UpdateHubLabel`. Hubs notify the UI through the `DataUpdated` / `PortTypeUpdated` events. `MainBoard.WriteLine(text[, Color])` is the logging channel to the in-app console, so use it rather than `Console`/`Debug`.

## Adding a new hub type (checklist)

1. Add values to `Hub.Types` and `Hub.HubManufacturerID` (append only).
2. Create `LTP.Core/Devices/<Name>Hub.cs`: `[Serializable]`, subclass `Hub`, override `RenewCharacteristic`, `InitPorts`, `InitializeNotifications`, `Characteristic_ValueChanged`, `WriteMessage`, `SetMotorSpeed`, `SetLightBrightness` and `SetLEDColor` as needed. Mark GATT fields `[NonSerialized]`.
3. Add a `<Compile Include=...>` entry to `LegoTrainProject.csproj`. Old-style projects don't glob, so a file left out of the csproj isn't compiled.
4. Teach `MainBoard.TryToConnect` / `GetTypeOfHub` to recognise it, and `CreateNewTrain` to construct it.

## Conventions

- Match the surrounding file. Indentation is mixed: newer code uses tabs and older code uses 4 spaces, sometimes in the same file. Braces go on their own line (Allman), with XML `/// <summary>` comments on members. Files are UTF-8 with a BOM, and line endings are normalized by `.gitattributes` (`* text=auto`).
- WinForms: edit the layout in `*.Designer.cs`/`.resx` carefully, the way the VS designer would. New forms and controls need `Compile` (with `SubType`/`DependentUpon`) and `EmbeddedResource` entries in the csproj.
- Version strings: the user-visible version is the hard-coded `MainBoard.Version` (`"V1.5 - 08/21/19"`). `AssemblyInfo` still says 1.0.0.0. The changelog lives in `README.md`.
- License: MIT (`LICENSE`). Third-party credits are listed in `README.md`.
