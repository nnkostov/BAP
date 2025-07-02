# ============================================
# Build Script for The Brick Automation Project
# PowerShell Version
# ============================================

param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
    [string]$Verbosity = 'minimal',
    
    [switch]$Clean,
    [switch]$Run
)

$ErrorActionPreference = 'Stop'

Write-Host "`n===== The Brick Automation Project Build Script (PowerShell) =====`n" -ForegroundColor Cyan

# Check if running from project root
if (-not (Test-Path "LegoTrainProject.sln")) {
    Write-Host "ERROR: LegoTrainProject.sln not found!" -ForegroundColor Red
    Write-Host "Please run this script from the project root directory." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Verbosity: $Verbosity" -ForegroundColor Yellow
Write-Host "Clean: $Clean" -ForegroundColor Yellow
Write-Host "Run after build: $Run`n" -ForegroundColor Yellow

# Step 1: Check for .NET Framework 4.6.1
Write-Host "[1/6] Checking .NET Framework version..." -ForegroundColor Green
try {
    $netVersion = Get-ItemProperty "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" -Name Release -ErrorAction Stop
    $releaseKey = $netVersion.Release
    
    $netVersionString = switch ($releaseKey) {
        { $_ -ge 533320 } { "4.8.1 or later" }
        { $_ -ge 528040 } { "4.8" }
        { $_ -ge 461808 } { "4.7.2" }
        { $_ -ge 461308 } { "4.7.1" }
        { $_ -ge 460798 } { "4.7" }
        { $_ -ge 394802 } { "4.6.2" }
        { $_ -ge 394254 } { "4.6.1" }
        { $_ -ge 393295 } { "4.6" }
        { $_ -ge 379893 } { "4.5.2" }
        { $_ -ge 378675 } { "4.5.1" }
        { $_ -ge 378389 } { "4.5" }
        default { "Unknown" }
    }
    
    Write-Host ".NET Framework version: $netVersionString (Release key: $releaseKey)" -ForegroundColor Gray
    
    if ($releaseKey -lt 394254) {
        Write-Host "WARNING: .NET Framework 4.6.1 or higher required!" -ForegroundColor Yellow
        Write-Host "Current version may not be sufficient. Build may fail." -ForegroundColor Yellow
        Write-Host "Download from: https://www.microsoft.com/en-us/download/details.aspx?id=49982`n" -ForegroundColor Yellow
    } else {
        Write-Host ".NET Framework check: OK`n" -ForegroundColor Green
    }
} catch {
    Write-Host "ERROR: Could not check .NET Framework version!" -ForegroundColor Red
    Write-Host "Please ensure .NET Framework 4.6.1 or higher is installed.`n" -ForegroundColor Red
}

# Step 2: Find MSBuild
Write-Host "[2/6] Locating MSBuild..." -ForegroundColor Green
$msbuildPath = $null

# VS 2022 locations
$vs2022Paths = @(
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
)

# VS 2019 locations
$vs2019Paths = @(
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
)

# VS 2017 locations
$vs2017Paths = @(
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe",
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
)

# Try to find MSBuild
foreach ($path in ($vs2022Paths + $vs2019Paths + $vs2017Paths)) {
    if (Test-Path $path) {
        $msbuildPath = $path
        Write-Host "Found: $path" -ForegroundColor Gray
        break
    }
}

# Try vswhere if available
if (-not $msbuildPath) {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $vsPath = & $vswhere -latest -requires Microsoft.Component.MSBuild -property installationPath
        if ($vsPath) {
            $msbuildPath = Join-Path $vsPath "MSBuild\Current\Bin\MSBuild.exe"
            if (-not (Test-Path $msbuildPath)) {
                $msbuildPath = Join-Path $vsPath "MSBuild\15.0\Bin\MSBuild.exe"
            }
        }
    }
}

# Try system PATH
if (-not $msbuildPath) {
    $msbuildCmd = Get-Command msbuild -ErrorAction SilentlyContinue
    if ($msbuildCmd) {
        $msbuildPath = $msbuildCmd.Path
        Write-Host "Found in PATH: $msbuildPath" -ForegroundColor Gray
    }
}

if (-not $msbuildPath -or -not (Test-Path $msbuildPath)) {
    Write-Host "ERROR: MSBuild not found!" -ForegroundColor Red
    Write-Host "Please install Visual Studio 2017 or later with .NET desktop development workload." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "MSBuild: $msbuildPath`n" -ForegroundColor Green

# Step 3: Find or download NuGet
Write-Host "[3/6] Checking NuGet..." -ForegroundColor Green
$nugetPath = $null

# Check current directory
if (Test-Path ".\nuget.exe") {
    $nugetPath = ".\nuget.exe"
    Write-Host "Found in current directory" -ForegroundColor Gray
} else {
    # Check PATH
    $nugetCmd = Get-Command nuget -ErrorAction SilentlyContinue
    if ($nugetCmd) {
        $nugetPath = $nugetCmd.Path
        Write-Host "Found in PATH: $nugetPath" -ForegroundColor Gray
    }
}

# Download if not found
if (-not $nugetPath) {
    Write-Host "NuGet not found. Downloading..." -ForegroundColor Yellow
    try {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile ".\nuget.exe"
        $nugetPath = ".\nuget.exe"
        Write-Host "NuGet downloaded successfully." -ForegroundColor Green
    } catch {
        Write-Host "ERROR: Failed to download NuGet!" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
        Write-Host "Please download manually from https://www.nuget.org/downloads" -ForegroundColor Red
        Read-Host "Press Enter to exit"
        exit 1
    }
}

Write-Host "NuGet: $nugetPath`n" -ForegroundColor Green

# Step 4: Clean (if requested)
if ($Clean) {
    Write-Host "[4/6] Cleaning previous build..." -ForegroundColor Green
    @("bin", "obj", "packages") | ForEach-Object {
        if (Test-Path $_) {
            Remove-Item $_ -Recurse -Force
            Write-Host "Removed: $_" -ForegroundColor Gray
        }
    }
    
    # Clean .vs folder if exists
    if (Test-Path ".vs") {
        try {
            Remove-Item ".vs" -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "Removed: .vs" -ForegroundColor Gray
        } catch {
            Write-Host "Warning: Could not remove .vs folder (may be in use)" -ForegroundColor Yellow
        }
    }
    
    Write-Host "Clean complete.`n" -ForegroundColor Green
} else {
    Write-Host "[4/6] Skipping clean (use -Clean flag to clean first)`n" -ForegroundColor Gray
}

# Step 5: Restore NuGet packages
Write-Host "[5/6] Restoring NuGet packages..." -ForegroundColor Green
try {
    $nugetOutput = & $nugetPath restore LegoTrainProject.sln 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: NuGet restore failed!" -ForegroundColor Red
        Write-Host $nugetOutput -ForegroundColor Red
        Read-Host "Press Enter to exit"
        exit 1
    }
    Write-Host "Package restore complete.`n" -ForegroundColor Green
} catch {
    Write-Host "ERROR: NuGet restore failed!" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Step 6: Build the project
Write-Host "[6/6] Building project...`n" -ForegroundColor Green
try {
    & $msbuildPath LegoTrainProject.sln /p:Configuration=$Configuration /p:Platform="Any CPU" /v:$Verbosity /m
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`nERROR: Build failed!" -ForegroundColor Red
        Write-Host "Check the error messages above for details.`n" -ForegroundColor Red
        Write-Host "Common issues:" -ForegroundColor Yellow
        Write-Host "- Missing Windows SDK (install via Visual Studio Installer)" -ForegroundColor Yellow
        Write-Host "- Missing Windows.winmd reference" -ForegroundColor Yellow
        Write-Host "- .NET Framework version mismatch`n" -ForegroundColor Yellow
        Write-Host "See COMPILATION_TROUBLESHOOTING.md for help." -ForegroundColor Yellow
        Read-Host "Press Enter to exit"
        exit 1
    }
    
    Write-Host "`n===== Build completed successfully! =====`n" -ForegroundColor Green
    Write-Host "Output location: bin\$Configuration\LegoTrainProject.exe" -ForegroundColor Cyan
    
} catch {
    Write-Host "`nERROR: Build failed with exception!" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Run if requested
if ($Run) {
    Write-Host "`nStarting The Brick Automation Project..." -ForegroundColor Green
    Start-Process "bin\$Configuration\LegoTrainProject.exe"
} else {
    Write-Host "`nTo run the application:" -ForegroundColor Yellow
    Write-Host "  .\bin\$Configuration\LegoTrainProject.exe" -ForegroundColor Gray
    Write-Host "`nOr use: .\build.ps1 -Run" -ForegroundColor Gray
}

Write-Host "`nBuild script completed.`n" -ForegroundColor Green

# Usage examples
Write-Host "Usage examples:" -ForegroundColor Cyan
Write-Host "  .\build.ps1                    # Build Release configuration" -ForegroundColor Gray
Write-Host "  .\build.ps1 -Configuration Debug    # Build Debug configuration" -ForegroundColor Gray
Write-Host "  .\build.ps1 -Clean             # Clean and build" -ForegroundColor Gray
Write-Host "  .\build.ps1 -Run               # Build and run" -ForegroundColor Gray
Write-Host "  .\build.ps1 -Verbosity detailed     # Build with detailed output" -ForegroundColor Gray