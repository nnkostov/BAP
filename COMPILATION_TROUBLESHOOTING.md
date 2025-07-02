# Compilation Troubleshooting Guide - The Brick Automation Project

This guide helps resolve common compilation issues when building The Brick Automation Project on Windows.

## Table of Contents
1. [Pre-Build Checklist](#pre-build-checklist)
2. [Common Compilation Errors](#common-compilation-errors)
3. [Visual Studio Configuration Issues](#visual-studio-configuration-issues)
4. [Command Line Build Issues](#command-line-build-issues)
5. [Environment-Specific Problems](#environment-specific-problems)
6. [Advanced Troubleshooting](#advanced-troubleshooting)

## Pre-Build Checklist

Before attempting to build, verify:

```cmd
REM 1. Check .NET Framework version (should return 394254 or higher for 4.6.1+)
reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release

REM 2. Check if MSBuild is in PATH
where msbuild

REM 3. Check if NuGet is available
where nuget

REM 4. Verify Windows SDK installation
dir "C:\Program Files (x86)\Windows Kits\10\UnionMetadata"
```

## Common Compilation Errors

### Error 1: "The type or namespace name 'Windows' could not be found"

**Symptoms:**
```
error CS0234: The type or namespace name 'Devices' does not exist in the namespace 'Windows'
error CS0246: The type or namespace name 'BluetoothLEAdvertisementWatcher' could not be found
```

**Solutions:**

1. **Add Windows Runtime reference manually:**
   ```xml
   <!-- Edit LegoTrainProject.csproj and add/update this reference -->
   <Reference Include="Windows">
     <HintPath>C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.19041.0\Windows.winmd</HintPath>
   </Reference>
   ```

2. **Install Windows 10 SDK:**
   - Open Visual Studio Installer
   - Modify your installation
   - Under "Individual components"
   - Select "Windows 10 SDK (10.0.19041.0)" or latest version
   - Click "Modify"

3. **Alternative Windows.winmd locations:**
   ```
   C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.19041.0\Windows.winmd
   C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.18362.0\Windows.winmd
   C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.17763.0\Windows.winmd
   C:\Program Files (x86)\Windows Kits\10\UnionMetadata\Facade\Windows.winmd
   ```

### Error 2: "Could not load file or assembly 'System.Runtime.WindowsRuntime'"

**Symptoms:**
```
error CS0006: Metadata file 'System.Runtime.WindowsRuntime.dll' could not be found
```

**Solutions:**

1. **Add reference manually in .csproj:**
   ```xml
   <Reference Include="System.Runtime.WindowsRuntime">
     <HintPath>C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETCore\v4.5\System.Runtime.WindowsRuntime.dll</HintPath>
   </Reference>
   ```

2. **Copy the DLL to project:**
   ```cmd
   copy "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETCore\v4.5\System.Runtime.WindowsRuntime.dll" "C:\Projects\LegoTrainProject\packages"
   ```

3. **Install via NuGet Package Manager Console:**
   ```powershell
   Install-Package System.Runtime.WindowsRuntime -Version 4.3.0
   ```

### Error 3: "The referenced component 'FastColoredTextBox' could not be found"

**Symptoms:**
```
error CS0246: The type or namespace name 'FastColoredTextBoxNS' could not be found
```

**Solutions:**

1. **Restore NuGet packages via command line:**
   ```cmd
   cd C:\Projects\LegoTrainProject
   nuget restore LegoTrainProject.sln
   ```

2. **Clear NuGet cache and restore:**
   ```cmd
   nuget locals all -clear
   nuget restore LegoTrainProject.sln -Force
   ```

3. **Manually install package:**
   ```cmd
   nuget install FCTB -Version 2.16.24 -OutputDirectory packages
   ```

4. **Update packages.config:**
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <packages>
     <package id="FCTB" version="2.16.24" targetFramework="net461" />
   </packages>
   ```

### Error 4: "The target 'Build' does not exist in the project"

**Solutions:**

1. **Ensure proper MSBuild import:**
   ```xml
   <!-- At the end of .csproj file -->
   <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
   ```

2. **Repair Visual Studio installation:**
   ```cmd
   "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vs_installer.exe" repair
   ```

## Visual Studio Configuration Issues

### Issue: "Project Load Failed"

1. **Check project compatibility:**
   - Right-click on project → Properties
   - Application tab → Target framework: .NET Framework 4.6.1
   - If missing, install via Visual Studio Installer

2. **Reset Visual Studio settings:**
   ```cmd
   devenv /resetuserdata
   devenv /resetsettings
   ```

### Issue: IntelliSense Not Working

1. **Delete .vs folder:**
   ```cmd
   cd C:\Projects\LegoTrainProject
   rmdir /s /q .vs
   ```

2. **Clear IntelliSense cache:**
   - Close Visual Studio
   - Delete: `%LOCALAPPDATA%\Microsoft\VisualStudio\[version]\ComponentModelCache`

### Issue: "Packages folder is missing"

1. **Enable automatic package restore:**
   - Tools → Options → NuGet Package Manager
   - Check "Allow NuGet to download missing packages"
   - Check "Automatically check for missing packages"

2. **Create .nuget folder with NuGet.config:**
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <packageRestore>
       <add key="enabled" value="True" />
       <add key="automatic" value="True" />
     </packageRestore>
   </configuration>
   ```

## Command Line Build Issues

### MSBuild Not Found

1. **Add MSBuild to PATH:**
   ```cmd
   REM For VS 2019
   set PATH=%PATH%;C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin

   REM For VS 2022
   set PATH=%PATH%;C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin
   ```

2. **Use Developer Command Prompt:**
   - Start Menu → Visual Studio 2019 → Developer Command Prompt

### NuGet Not Found

1. **Download NuGet.exe:**
   ```powershell
   Invoke-WebRequest -Uri https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile nuget.exe
   ```

2. **Add to PATH or use full path:**
   ```cmd
   C:\Tools\nuget.exe restore LegoTrainProject.sln
   ```

## Environment-Specific Problems

### Windows 7 Specific Issues

1. **Install required updates:**
   - KB2533623 (required for .NET 4.6.1)
   - KB3063858 (.NET Framework 4.6.1)

2. **Windows Management Framework 5.1:**
   - Download from Microsoft Download Center
   - Required for PowerShell package management

### Path Length Issues

1. **Enable long path support (Windows 10):**
   ```cmd
   reg add HKLM\SYSTEM\CurrentControlSet\Control\FileSystem /v LongPathsEnabled /t REG_DWORD /d 1
   ```

2. **Move project to shorter path:**
   ```cmd
   move "C:\Users\VeryLongUserName\Documents\Visual Studio Projects\LegoTrainProject" "C:\LTP"
   ```

### Permission Issues

1. **Run as Administrator:**
   ```cmd
   runas /user:Administrator "msbuild LegoTrainProject.sln"
   ```

2. **Fix folder permissions:**
   ```cmd
   icacls "C:\Projects\LegoTrainProject" /grant %USERNAME%:F /T
   ```

## Advanced Troubleshooting

### Enable Detailed MSBuild Logging

```cmd
msbuild LegoTrainProject.sln /v:diagnostic /fl /flp:logfile=build.log;verbosity=diagnostic
```

### Check Assembly Binding

1. **Enable Fusion logging:**
   ```cmd
   reg add HKLM\SOFTWARE\Microsoft\Fusion /v EnableLog /t REG_DWORD /d 1
   reg add HKLM\SOFTWARE\Microsoft\Fusion /v LogPath /t REG_SZ /d "C:\FusionLogs"
   ```

2. **Use Assembly Binding Log Viewer:**
   ```cmd
   "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\FUSLOGVW.exe"
   ```

### Manual Project File Fixes

1. **Update TargetFrameworkVersion:**
   ```xml
   <TargetFrameworkVersion>v4.6.1</TargetFrameworkVersion>
   ```

2. **Add missing imports:**
   ```xml
   <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
   ```

3. **Fix reference hints:**
   ```xml
   <Reference Include="System.Runtime.WindowsRuntime">
     <Private>True</Private>
     <HintPath>$(MSBuildProgramFiles32)\Reference Assemblies\Microsoft\Framework\.NETCore\v4.5\System.Runtime.WindowsRuntime.dll</HintPath>
   </Reference>
   ```

### Reset Everything

If all else fails, complete reset:

```cmd
REM 1. Clean solution
msbuild LegoTrainProject.sln /t:Clean

REM 2. Delete all generated files
rmdir /s /q bin
rmdir /s /q obj
rmdir /s /q packages
rmdir /s /q .vs
del *.user
del *.suo

REM 3. Restore and rebuild
nuget restore LegoTrainProject.sln
msbuild LegoTrainProject.sln /t:Rebuild
```

## Getting Help

1. **Generate diagnostic report:**
   ```cmd
   msbuild LegoTrainProject.sln /v:diagnostic > build_diagnostic.txt 2>&1
   ```

2. **Collect system information:**
   ```cmd
   systeminfo > system_info.txt
   dotnet --info > dotnet_info.txt 2>&1
   ```

3. **Check Event Viewer:**
   - Windows Logs → Application
   - Look for .NET Runtime or MSBuild errors

4. **Visual Studio logs:**
   - `%APPDATA%\Microsoft\VisualStudio\[version]\ActivityLog.xml`

---

*Remember: Most compilation issues are related to missing Windows SDK or incorrect reference paths. Always verify your Visual Studio installation includes the Windows 10 SDK and Universal Windows Platform development workload.*