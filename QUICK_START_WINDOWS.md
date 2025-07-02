# Quick Start Guide - The Brick Automation Project on Windows

This guide provides the fastest path to compile and run The Brick Automation Project on Windows.

## Prerequisites Checklist

- [ ] Windows 7 SP1 or later (Windows 10/11 recommended)
- [ ] Bluetooth 4.0 LE capable adapter
- [ ] 2 GB RAM minimum
- [ ] 500 MB free disk space

## Step 1: Install Required Software

### 1.1 Install .NET Framework 4.6.1
Download and install from: https://www.microsoft.com/en-us/download/details.aspx?id=49982

### 1.2 Install Visual Studio Community 2019 or later
1. Download from: https://visualstudio.microsoft.com/downloads/
2. During installation, select:
   - **.NET desktop development** workload
   - **Universal Windows Platform development** workload

## Step 2: Get the Source Code

### Option A: Download ZIP
1. Download the project ZIP file
2. Extract to `C:\Projects\LegoTrainProject` (avoid paths with spaces)

### Option B: Clone with Git
```cmd
git clone [repository-url] C:\Projects\LegoTrainProject
cd C:\Projects\LegoTrainProject
```

## Step 3: Build the Project

### Using Visual Studio

1. **Open the solution:**
   - Launch Visual Studio
   - File → Open → Project/Solution
   - Navigate to `C:\Projects\LegoTrainProject`
   - Select `LegoTrainProject.sln`

2. **Restore NuGet packages:**
   - Solution Explorer → Right-click solution → Restore NuGet Packages
   - Wait for "Restore completed" message

3. **Set build configuration:**
   - Toolbar: Set to `Release` and `Any CPU`

4. **Build:**
   - Press `Ctrl+Shift+B` or Build → Build Solution
   - Check Output window for "Build succeeded"

### Using Command Line

```cmd
cd C:\Projects\LegoTrainProject

REM Restore NuGet packages
nuget restore LegoTrainProject.sln

REM Build the project
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" LegoTrainProject.sln /p:Configuration=Release /p:Platform="Any CPU"
```

## Step 4: Run the Application

### From Visual Studio
- Press `F5` (Debug mode) or `Ctrl+F5` (without debugging)

### From Windows Explorer
1. Navigate to `C:\Projects\LegoTrainProject\bin\Release`
2. Double-click `LegoTrainProject.exe`

### From Command Line
```cmd
cd C:\Projects\LegoTrainProject\bin\Release
LegoTrainProject.exe
```

## Step 5: First Run Configuration

1. **Enable Bluetooth:**
   - Windows Settings → Devices → Bluetooth ON

2. **Launch the application:**
   - Accept any Windows security prompts
   - Allow Bluetooth access when prompted

3. **Connect LEGO devices:**
   - Turn on your LEGO hub (press the power button)
   - The app will automatically scan and find devices
   - Click on discovered devices to connect

## Common Issues and Quick Fixes

### Build Error: "Windows.winmd not found"
**Fix:** In Visual Studio, add reference to:
`C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.19041.0\Windows.winmd`

### Build Error: "FastColoredTextBox not found"
**Fix:** Right-click solution → Restore NuGet Packages

### Runtime Error: "Bluetooth not available"
**Fix:** 
1. Enable Bluetooth in Windows Settings
2. Update Bluetooth drivers via Device Manager
3. Run app as Administrator

### No devices found
**Fix:**
1. Ensure LEGO device is powered on
2. Check battery level (low battery = poor connection)
3. Move device closer (within 5 meters)
4. Try pairing through Windows Bluetooth settings first

## Minimal File Distribution

To run on another Windows PC, copy these files:
```
LegoTrainProject.exe
LegoTrainProject.exe.config
FastColoredTextBox.dll
```

Target PC must have .NET Framework 4.6.1 installed.

## Quick Command Reference

```cmd
REM Check .NET Framework version
reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release

REM Build from command line
msbuild LegoTrainProject.sln /p:Configuration=Release

REM Run as administrator
runas /user:Administrator LegoTrainProject.exe
```

## Support

- Check `WINDOWS_BUILD_AND_RUN_GUIDE.md` for detailed documentation
- Enable debug logging: View → Show Bluetooth Connection Debug
- Check Windows Event Viewer for crash details

---

*For complete documentation, see WINDOWS_BUILD_AND_RUN_GUIDE.md*