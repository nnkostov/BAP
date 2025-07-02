@echo off
REM ============================================
REM Build Script for The Brick Automation Project
REM ============================================

setlocal enabledelayedexpansion

echo.
echo ===== The Brick Automation Project Build Script =====
echo.

REM Check if running from project root
if not exist "LegoTrainProject.sln" (
    echo ERROR: LegoTrainProject.sln not found!
    echo Please run this script from the project root directory.
    echo.
    pause
    exit /b 1
)

REM Configuration
set "CONFIG=Release"
set "PLATFORM=Any CPU"
set "VERBOSITY=minimal"

REM Parse command line arguments
if "%1"=="debug" set CONFIG=Debug
if "%1"=="Debug" set CONFIG=Debug
if "%2"=="verbose" set VERBOSITY=detailed
if "%2"=="diagnostic" set VERBOSITY=diagnostic

echo Configuration: %CONFIG%
echo Platform: %PLATFORM%
echo Verbosity: %VERBOSITY%
echo.

REM Step 1: Check for .NET Framework 4.6.1
echo [1/6] Checking .NET Framework version...
reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release > nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET Framework 4.0 or higher not found!
    echo Please install .NET Framework 4.6.1 from:
    echo https://www.microsoft.com/en-us/download/details.aspx?id=49982
    echo.
    pause
    exit /b 1
)

for /f "tokens=3" %%a in ('reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release ^| findstr /i "Release"') do set NET_VERSION=%%a
if %NET_VERSION% LSS 394254 (
    echo WARNING: .NET Framework 4.6.1 or higher recommended!
    echo Current version might be older. Build may fail.
    echo.
)
echo .NET Framework check: OK
echo.

REM Step 2: Find MSBuild
echo [2/6] Locating MSBuild...
set MSBUILD_PATH=

REM Try VS 2022 first
if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    echo Found: VS 2022 Community
    goto :msbuild_found
)
if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
    echo Found: VS 2022 Professional
    goto :msbuild_found
)
if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    echo Found: VS 2022 Enterprise
    goto :msbuild_found
)

REM Try VS 2019
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    echo Found: VS 2019 Community
    goto :msbuild_found
)
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
    echo Found: VS 2019 Professional
    goto :msbuild_found
)
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    echo Found: VS 2019 Enterprise
    goto :msbuild_found
)

REM Try VS 2017
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" (
    set "MSBUILD_PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
    echo Found: VS 2017 Community
    goto :msbuild_found
)

REM Try system PATH
where msbuild >nul 2>&1
if %errorlevel% equ 0 (
    for /f "tokens=*" %%i in ('where msbuild') do set "MSBUILD_PATH=%%i"
    echo Found: MSBuild in PATH
    goto :msbuild_found
)

echo ERROR: MSBuild not found!
echo Please install Visual Studio 2017 or later with .NET desktop development workload.
echo.
pause
exit /b 1

:msbuild_found
echo MSBuild: %MSBUILD_PATH%
echo.

REM Step 3: Find or download NuGet
echo [3/6] Checking NuGet...
set NUGET_PATH=

REM Check if NuGet exists in current directory
if exist "nuget.exe" (
    set "NUGET_PATH=nuget.exe"
    goto :nuget_found
)

REM Check if NuGet is in PATH
where nuget >nul 2>&1
if %errorlevel% equ 0 (
    for /f "tokens=*" %%i in ('where nuget') do set "NUGET_PATH=%%i"
    goto :nuget_found
)

REM Download NuGet if not found
echo NuGet not found. Downloading...
powershell -Command "& {[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile 'nuget.exe'}"
if exist "nuget.exe" (
    set "NUGET_PATH=nuget.exe"
    echo NuGet downloaded successfully.
    goto :nuget_found
) else (
    echo ERROR: Failed to download NuGet!
    echo Please download manually from https://www.nuget.org/downloads
    echo.
    pause
    exit /b 1
)

:nuget_found
echo NuGet: %NUGET_PATH%
echo.

REM Step 4: Clean previous builds (optional)
if "%3"=="clean" (
    echo [4/6] Cleaning previous build...
    if exist "bin" rmdir /s /q "bin"
    if exist "obj" rmdir /s /q "obj"
    if exist "packages" rmdir /s /q "packages"
    echo Clean complete.
    echo.
) else (
    echo [4/6] Skipping clean (use 'build.bat %CONFIG% %VERBOSITY% clean' to clean first)
    echo.
)

REM Step 5: Restore NuGet packages
echo [5/6] Restoring NuGet packages...
"%NUGET_PATH%" restore LegoTrainProject.sln
if %errorlevel% neq 0 (
    echo ERROR: NuGet restore failed!
    echo.
    pause
    exit /b 1
)
echo Package restore complete.
echo.

REM Step 6: Build the project
echo [6/6] Building project...
echo.
"%MSBUILD_PATH%" LegoTrainProject.sln /p:Configuration=%CONFIG% /p:Platform="%PLATFORM%" /v:%VERBOSITY% /m
if %errorlevel% neq 0 (
    echo.
    echo ERROR: Build failed!
    echo Check the error messages above for details.
    echo.
    echo Common issues:
    echo - Missing Windows SDK (install via Visual Studio Installer)
    echo - Missing Windows.winmd reference
    echo - .NET Framework version mismatch
    echo.
    echo See COMPILATION_TROUBLESHOOTING.md for help.
    echo.
    pause
    exit /b 1
)

echo.
echo ===== Build completed successfully! =====
echo.
echo Output location: bin\%CONFIG%\LegoTrainProject.exe
echo.
echo To run the application:
echo   cd bin\%CONFIG%
echo   LegoTrainProject.exe
echo.
echo Or double-click: bin\%CONFIG%\LegoTrainProject.exe
echo.

REM Optional: Ask if user wants to run the application
set /p RUN_APP="Do you want to run the application now? (Y/N): "
if /i "%RUN_APP%"=="Y" (
    echo.
    echo Starting The Brick Automation Project...
    start "" "bin\%CONFIG%\LegoTrainProject.exe"
)

pause