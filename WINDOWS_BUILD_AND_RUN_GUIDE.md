# The Brick Automation Project - Windows Build and Run Guide

## Table of Contents
1. [Overview](#overview)
2. [System Requirements](#system-requirements)
3. [Prerequisites](#prerequisites)
4. [Development Environment Setup](#development-environment-setup)
5. [Building the Project](#building-the-project)
6. [Running the Application](#running-the-application)
7. [Troubleshooting](#troubleshooting)
8. [Project Structure](#project-structure)
9. [Features](#features)
10. [Supported LEGO Hardware](#supported-lego-hardware)

## Overview

The Brick Automation Project (formerly known as Lego Train Project) is a Windows desktop application that allows you to control various LEGO hardware devices via Bluetooth. It provides a comprehensive automation system for LEGO trains and other motorized creations, including self-driving capabilities, sensor-based events, and custom programming features.

**Version:** 1.5 (Released: August 21, 2019)  
**Framework:** .NET Framework 4.6.1  
**Language:** C#  
**UI Framework:** Windows Forms  
**IDE:** Visual Studio (2015 or later recommended)

## System Requirements

### Minimum Requirements
- **Operating System:** Windows 7 SP1 or later (Windows 10/11 recommended)
- **Processor:** 1 GHz or faster processor
- **RAM:** 2 GB (4 GB recommended)
- **Disk Space:** 100 MB for application + additional space for .NET Framework
- **Bluetooth:** Bluetooth 4.0 (Bluetooth Low Energy) support required
- **Display:** 1280x720 minimum resolution

### Software Requirements
- .NET Framework 4.6.1 or later
- Visual Studio 2015 or later (for development)
- Windows SDK (included with Visual Studio)

## Prerequisites

### 1. Install .NET Framework 4.6.1

1. Check if .NET Framework 4.6.1 is already installed:
   - Open Command Prompt as Administrator
   - Run: `reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release`
   - If the value is 394254 or higher, you have .NET 4.6.1 or later installed

2. If not installed, download from:
   - https://www.microsoft.com/en-us/download/details.aspx?id=49982
   - Run the installer and follow the prompts
   - Restart your computer after installation

### 2. Install Visual Studio (for building from source)

1. Download Visual Studio Community (free):
   - https://visualstudio.microsoft.com/downloads/
   - Choose "Visual Studio Community"

2. During installation, select these workloads:
   - **.NET desktop development**
   - **Universal Windows Platform development** (for Windows Runtime support)

3. Ensure these individual components are selected:
   - .NET Framework 4.6.1 targeting pack
   - Windows 10 SDK (any version)
   - NuGet package manager

### 3. Enable Bluetooth

1. Ensure Bluetooth is enabled on your Windows PC:
   - Open Settings → Devices → Bluetooth & other devices
   - Turn on Bluetooth
   - Ensure your Bluetooth adapter supports Bluetooth 4.0 LE

2. Install latest Bluetooth drivers:
   - Open Device Manager
   - Expand "Bluetooth"
   - Right-click your Bluetooth adapter → Update driver

## Development Environment Setup

### 1. Clone or Download the Project

**Option A: Using Git**
```bash
git clone https://github.com/[repository-url]/LegoTrainProject.git
cd LegoTrainProject
```

**Option B: Download ZIP**
1. Download the project as ZIP
2. Extract to a folder (e.g., `C:\Projects\LegoTrainProject`)
3. Ensure the path has no special characters or spaces

### 2. Open the Project in Visual Studio

1. Launch Visual Studio
2. Click "Open a project or solution"
3. Navigate to the project folder
4. Select `LegoTrainProject.sln`
5. Click "Open"

### 3. Restore NuGet Packages

Visual Studio should automatically restore packages. If not:

1. Right-click the solution in Solution Explorer
2. Select "Restore NuGet Packages"
3. Wait for the restoration to complete

The project uses one NuGet package:
- **FastColoredTextBox** (v2.16.24) - for code editing features

### 4. Verify Project References

1. In Solution Explorer, expand "References"
2. Ensure no references show warning icons
3. Key references to verify:
   - System.Runtime.WindowsRuntime
   - Windows.winmd (for Bluetooth LE support)
   - FastColoredTextBox

If references are missing:
1. Right-click "References" → "Add Reference"
2. For Windows Runtime:
   - Browse to: `C:\Program Files (x86)\Windows Kits\10\UnionMetadata\[version]\Windows.winmd`
   - Select and add

## Building the Project

### 1. Configure Build Settings

1. In Visual Studio toolbar, set:
   - Configuration: `Debug` (for testing) or `Release` (for distribution)
   - Platform: `Any CPU`

### 2. Build the Solution

**Method 1: Using Visual Studio GUI**
1. Menu: Build → Build Solution
2. Or press `Ctrl+Shift+B`
3. Watch the Output window for build results

**Method 2: Using MSBuild Command Line**
```cmd
cd "C:\Path\To\LegoTrainProject"
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" LegoTrainProject.sln /p:Configuration=Release /p:Platform="Any CPU"
```

### 3. Build Output

The compiled application will be in:
- Debug build: `bin\Debug\LegoTrainProject.exe`
- Release build: `bin\Release\LegoTrainProject.exe`

## Running the Application

### 1. From Visual Studio (Debug Mode)

1. Press `F5` or click "Start" button
2. The application will launch with debugging enabled
3. Set breakpoints as needed for troubleshooting

### 2. Running the Compiled Executable

1. Navigate to the output folder:
   - `LegoTrainProject\bin\Debug\` or
   - `LegoTrainProject\bin\Release\`

2. Required files in the same directory:
   - `LegoTrainProject.exe` (main executable)
   - `LegoTrainProject.exe.config` (configuration file)
   - `FastColoredTextBox.dll` (dependency)
   - Any `.pdb` files (for debugging)

3. Double-click `LegoTrainProject.exe` to run

### 3. First Run Setup

1. **Welcome Screen:**
   - The application will show a splash screen
   - Accept any terms of service if prompted

2. **Bluetooth Permissions:**
   - Windows may ask for Bluetooth permissions
   - Click "Allow" when prompted

3. **Main Interface:**
   - The main window will open with tabs for:
     - Connected devices
     - Programs
     - Self-driving module
     - Console output

## Troubleshooting

### Common Build Errors

#### 1. "Windows.winmd could not be found"
**Solution:**
```xml
<!-- In .csproj file, ensure this reference exists -->
<Reference Include="Windows">
  <HintPath>C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.19041.0\Windows.winmd</HintPath>
</Reference>
```

#### 2. "Could not load file or assembly 'System.Runtime.WindowsRuntime'"
**Solution:**
- Install Windows 10 SDK through Visual Studio Installer
- Add reference to: `C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETCore\v4.5\System.Runtime.WindowsRuntime.dll`

#### 3. NuGet Package Restore Failed
**Solution:**
```cmd
# Clear NuGet cache
nuget locals all -clear

# Restore packages manually
nuget restore LegoTrainProject.sln
```

### Runtime Issues

#### 1. "Bluetooth adapter not found"
**Solution:**
- Ensure Bluetooth is enabled in Windows
- Update Bluetooth drivers
- Check if adapter supports Bluetooth LE

#### 2. "Access denied" when connecting to devices
**Solution:**
- Run the application as Administrator
- Pair LEGO devices through Windows Bluetooth settings first

#### 3. Application crashes on startup
**Solution:**
- Check Windows Event Viewer for detailed error
- Ensure .NET Framework 4.6.1 is installed
- Delete any corrupted configuration files in the app directory

### Debugging Bluetooth Issues

1. Enable Bluetooth debug mode:
   - In the application: View → Show Bluetooth Connection Debug
   - Check console output for detailed connection logs

2. Use Windows Bluetooth troubleshooter:
   - Settings → Update & Security → Troubleshoot → Bluetooth

## Project Structure

```
LegoTrainProject/
│
├── LegoTrainProject.sln          # Visual Studio solution file
├── LegoTrainProject.csproj       # Project file with build configuration
├── Program.cs                    # Application entry point
├── App.config                    # Runtime configuration
├── packages.config               # NuGet packages configuration
│
├── LTP.Core/                     # Core business logic
│   ├── Devices/                  # Device implementations
│   │   ├── Hub.cs               # Base hub class
│   │   ├── EV3Hub.cs            # LEGO EV3 support
│   │   ├── SbrickHub.cs         # SBrick support
│   │   ├── PFxHub.cs            # PFx Brick support
│   │   ├── BuWizzHub.cs         # BuWizz support
│   │   ├── WedoHub.cs           # WeDo 2.0 support
│   │   └── RemoteHub.cs         # Remote control support
│   ├── TrainProject.cs          # Project management
│   ├── TrainProgram.cs          # Programming logic
│   └── Sections.cs              # Self-driving sections
│
├── LTP.Desktop/                  # Windows Forms UI
│   ├── Main UI/                 # Main application windows
│   │   ├── MainBoard.cs         # Main window
│   │   ├── FormCodeEditor.cs    # Code editor window
│   │   └── SplashForm.cs        # Splash screen
│   ├── Hubs UI/                 # Device control UI
│   ├── Program/                 # Program editing UI
│   └── Sections UI/             # Self-driving UI
│
├── EV3.Core/                    # LEGO EV3 communication library
│
├── Properties/                   # Application properties
│   ├── AssemblyInfo.cs          # Assembly metadata
│   └── Resources.resx           # Embedded resources
│
└── Resources/                    # Icons and images
```

## Features

### 1. Device Control
- Connect multiple LEGO devices simultaneously
- Control motors with precise speed settings
- Read sensor values in real-time
- Configure LED colors
- Monitor battery levels

### 2. Programming System
- Create custom programs with sensor triggers
- Visual programming interface
- C# code editor for advanced users
- Event-based programming:
  - Sensor triggered events
  - User triggered sequences
  - Remote control events
  - Custom triggers

### 3. Self-Driving Module
- Define track sections
- Automatic train routing
- Collision prevention
- Signal light control
- Speed management based on track conditions

### 4. Connection Management
- Device whitelist/blacklist
- Project-specific device limitations
- Automatic device discovery
- Connection status monitoring

## Supported LEGO Hardware

### Powered Up System
- **Hubs:**
  - LEGO City Hub (60197)
  - LEGO Technic Hub
  - LEGO BOOST Move Hub
  - LEGO Control+ Hub
- **Motors:**
  - Powered Up Motor
  - Powered Up Train Motor
  - BOOST Interactive Motor
  - Control+ L Motor
  - Control+ XL Motor
- **Sensors:**
  - Powered Up Color & Distance Sensor
  - BOOST Color & Distance Sensor
  - WeDo 2.0 Motion Sensor
  - WeDo 2.0 Tilt Sensor

### Third-Party Devices
- **SBrick & SBrick Plus** - Bluetooth control for Power Functions
- **BuWizz & BuWizz 2.0** - High-power motor controller
- **PFx Brick** - Advanced lighting and sound controller

### LEGO MINDSTORMS
- **EV3 Brick** - Via Bluetooth connection
- All EV3 motors and sensors supported

### Remote Controls
- Powered Up Remote
- Control+ Remote

## Advanced Configuration

### 1. Modifying App.config

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <startup> 
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6.1"/>
    </startup>
    <!-- Add custom settings here -->
</configuration>
```

### 2. Creating a Deployment Package

1. Build in Release mode
2. Create a new folder for distribution
3. Copy these files:
   ```
   LegoTrainProject.exe
   LegoTrainProject.exe.config
   FastColoredTextBox.dll
   ```
4. Optional: Use ILMerge to create single executable
5. Create installer using:
   - Visual Studio Installer Projects
   - WiX Toolset
   - Inno Setup

### 3. Code Signing (Optional)

To avoid Windows SmartScreen warnings:
```cmd
signtool sign /a /t http://timestamp.digicert.com /fd SHA256 LegoTrainProject.exe
```

## Tips for Successful Usage

1. **Bluetooth Range:** Keep devices within 10 meters for stable connection
2. **Battery Levels:** Monitor battery levels; low battery affects performance
3. **Multiple Devices:** Limit to 5-7 simultaneous connections for best performance
4. **Save Projects:** Regularly save your automation projects (.bap files)
5. **Updates:** Check for firmware updates for your LEGO devices

## Support and Resources

- **Project Documentation:** Check the README.md file
- **LEGO Documentation:** 
  - [Powered Up Documentation](https://github.com/LEGO/lego-ble-wireless-protocol-docs)
  - [EV3 Developer Kit](https://education.lego.com/en-us/downloads/mindstorms-ev3)
- **Community Forums:**
  - [Eurobricks](https://www.eurobricks.com/forum/)
  - [LEGO Trains Forum](https://www.eurobricks.com/forum/index.php?/forums/forum/111-lego-train-tech/)

## License

This software is released under the terms specified in the LICENSE file. It's free software developed for the AFOL (Adult Fan of LEGO) community.

---

*Last updated: Documentation created for The Brick Automation Project v1.5*