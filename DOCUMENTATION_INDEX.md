# The Brick Automation Project - Documentation Index

Welcome to The Brick Automation Project documentation! This index provides a guide to all available documentation for building and running the application on Windows.

## 📚 Available Documentation

### 1. **[WINDOWS_BUILD_AND_RUN_GUIDE.md](WINDOWS_BUILD_AND_RUN_GUIDE.md)** - Complete Guide
The comprehensive documentation covering everything you need to know:
- Detailed system requirements
- Step-by-step setup instructions
- Building from source
- Running the application
- Project structure overview
- Features and supported hardware
- Advanced configuration options

**Start here if:** You want the complete picture and detailed explanations.

### 2. **[QUICK_START_WINDOWS.md](QUICK_START_WINDOWS.md)** - Quick Start Guide
A condensed version focusing on getting you up and running quickly:
- Prerequisites checklist
- Minimal setup steps
- Quick build instructions
- Basic troubleshooting

**Start here if:** You're experienced with C# development and just need the basics.

### 3. **[COMPILATION_TROUBLESHOOTING.md](COMPILATION_TROUBLESHOOTING.md)** - Troubleshooting Guide
Detailed solutions for compilation issues:
- Common build errors and fixes
- Visual Studio configuration problems
- Environment-specific issues
- Advanced debugging techniques

**Start here if:** You're encountering build errors or compilation problems.

## 🛠️ Build Scripts

### **[build.bat](build.bat)** - Windows Batch Script
Automated build script for Windows Command Prompt:
```cmd
build.bat                    # Build Release
build.bat debug              # Build Debug
build.bat release verbose    # Build with detailed output
build.bat release normal clean  # Clean and build
```

### **[build.ps1](build.ps1)** - PowerShell Script
Modern PowerShell build script with more options:
```powershell
.\build.ps1                      # Build Release
.\build.ps1 -Configuration Debug # Build Debug
.\build.ps1 -Clean -Run          # Clean, build, and run
.\build.ps1 -Verbosity detailed  # Detailed output
```

## 🚀 Quick Start Steps

1. **Install Prerequisites:**
   - [.NET Framework 4.6.1](https://www.microsoft.com/en-us/download/details.aspx?id=49982)
   - [Visual Studio 2019/2022](https://visualstudio.microsoft.com/downloads/) with:
     - .NET desktop development workload
     - Universal Windows Platform development workload

2. **Get the Source Code:**
   ```cmd
   git clone [repository-url] C:\Projects\LegoTrainProject
   cd C:\Projects\LegoTrainProject
   ```

3. **Build the Project:**
   - **Easy way:** Double-click `build.bat`
   - **PowerShell way:** Run `.\build.ps1 -Run`
   - **Visual Studio way:** Open `LegoTrainProject.sln` and press F5

4. **Run the Application:**
   ```cmd
   cd bin\Release
   LegoTrainProject.exe
   ```

## 📋 Documentation by Use Case

### "I want to understand what this project does"
- Read the overview section in [WINDOWS_BUILD_AND_RUN_GUIDE.md](WINDOWS_BUILD_AND_RUN_GUIDE.md#overview)
- Check the [Features](WINDOWS_BUILD_AND_RUN_GUIDE.md#features) section
- Review [Supported LEGO Hardware](WINDOWS_BUILD_AND_RUN_GUIDE.md#supported-lego-hardware)

### "I just want to compile and run it"
- Follow [QUICK_START_WINDOWS.md](QUICK_START_WINDOWS.md)
- Use the automated `build.bat` script

### "I'm getting build errors"
- Start with [Common Issues](QUICK_START_WINDOWS.md#common-issues-and-quick-fixes) in the Quick Start
- For detailed solutions, see [COMPILATION_TROUBLESHOOTING.md](COMPILATION_TROUBLESHOOTING.md)

### "I want to modify the code"
- Set up Visual Studio following [Development Environment Setup](WINDOWS_BUILD_AND_RUN_GUIDE.md#development-environment-setup)
- Review the [Project Structure](WINDOWS_BUILD_AND_RUN_GUIDE.md#project-structure)

### "I need to deploy this to other computers"
- See [Minimal File Distribution](QUICK_START_WINDOWS.md#minimal-file-distribution)
- Check [Creating a Deployment Package](WINDOWS_BUILD_AND_RUN_GUIDE.md#2-creating-a-deployment-package)

## 💡 Pro Tips

1. **First Time Building?**
   - Use `build.bat` - it handles everything automatically
   - Run as Administrator if you encounter permission issues

2. **Bluetooth Not Working?**
   - Ensure Windows Bluetooth is enabled
   - Update Bluetooth drivers
   - Check that your adapter supports Bluetooth LE (4.0+)

3. **Visual Studio Issues?**
   - Make sure to install the Universal Windows Platform development workload
   - The Windows 10 SDK is required for Bluetooth functionality

4. **Need Help?**
   - Enable verbose logging: `build.bat release verbose`
   - Check the console output in the application
   - Review Windows Event Viewer for detailed errors

## 📞 Support Resources

- **Project README:** Original project documentation
- **LEGO Resources:**
  - [Powered Up Protocol](https://github.com/LEGO/lego-ble-wireless-protocol-docs)
  - [LEGO Education](https://education.lego.com/)
- **Community:**
  - [Eurobricks Forum](https://www.eurobricks.com/forum/)
  - [LEGO Trains Community](https://www.eurobricks.com/forum/index.php?/forums/forum/111-lego-train-tech/)

## 🎯 Next Steps

1. Build the project using your preferred method
2. Connect your LEGO devices via Bluetooth
3. Start automating your LEGO creations!
4. Share your projects with the AFOL community

---

*Happy building! If you encounter any issues not covered in the documentation, check the troubleshooting guide or community forums for help.*