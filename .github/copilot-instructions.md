# Copilot Instructions for B2S Designer

This document provides essential information for coding agents working with the B2S (Backglass 2nd Screen) Designer repository. Follow these instructions to minimize exploration time and avoid common build failures.

## Repository Overview

**Project Purpose**: B2S Designer is a WYSIWYG editor for creating and editing directB2S backglasses for Visual Pinball tables. It generates XML configuration files that control backglass displays, LED panels, score reels, and animations for pinball machines.

**Repository Information**:
- **Size**: Medium-sized Visual Basic .NET Windows Forms application (~790 source files)
- **Language**: Visual Basic .NET  
- **Framework**: .NET Framework 4.8 (both projects now use 4.8)
- **Platform**: Windows only, targets Any CPU architecture
- **IDE**: Visual Studio 2019+ (any version from 2019 should work)
- **Build System**: MSBuild via Visual Studio solution files

## Build Instructions

### Prerequisites
- **Visual Studio 2019 or later** with Visual Basic .NET support
- **.NET Framework 4.8** (required for both projects)
- **MSBuild** (included with Visual Studio)
- **HTML Help Workshop** (optional, for compiling .chm help files - requires elevated privileges)

### Build Commands

**ALWAYS build in this exact order and configuration:**

1. **Main Designer Application** (Primary project):
   ```cmd
   msbuild "b2sbackglassdesigner\B2SBackglassDesigner.sln" -p:Configuration=Release "/p:Platform=Any CPU"
   ```
   - ✅ **This builds successfully** 
   - Output: `b2sbackglassdesigner\b2sbackglassdesigner\bin\x64\Release\B2SBackglassDesigner.exe`

2. **VPinMAME Starter** (Secondary tool - now builds successfully):
   ```cmd  
   msbuild "B2SVPinMAMEStarter\B2SVPinMAMEStarter.sln" -p:Configuration=Release "/p:Platform=Any CPU"
   ```
   - ✅ **This builds successfully** after updating to .NET Framework 4.8
   - Output: `B2SVPinMAMEStarter\bin\x64\Release\B2SVPinMAMEStarter.exe`

3. **Clean command** (when needed):
   ```cmd
   msbuild "b2sbackglassdesigner\B2SBackglassDesigner.sln" -t:Clean -p:Configuration=Release "/p:Platform=Any CPU"
   ```

4. **Help File Compilation** (optional):
   ```cmd
   "C:\Program Files (x86)\HTML Help Workshop\hhc.exe" "b2sbackglassdesigner\b2sbackglassdesigner\htmlhelp\B2SBackglassDesigner.hhp"
   ```
   - Requires HTML Help Workshop installation with admin privileges
   - May fail but doesn't affect main application functionality

### Build Warnings (Expected)
- `MSB3884: Regelsatzdatei "MinimumRecommendedRules.ruleset" konnte nicht gefunden werden` - **IGNORE**: Missing code analysis ruleset, doesn't affect build

### Build Time
- Debug build: ~7 seconds
- Release build: ~3 seconds  
- Clean operation: <1 second

## Project Architecture & Key Locations

### Main Project Structure
```
b2sbackglassdesigner/
  ├── B2SBackglassDesigner.sln          # Main solution file
  └── b2sbackglassdesigner/              # Main project folder  
      ├── B2SBackglassDesigner.vbproj    # Project file (.NET 4.8)
      ├── formDesigner.vb                # Main application form (1710 lines)
      ├── ApplicationStartup.vb          # Application entry point
      ├── app.config                     # Runtime configuration
      ├── classes/                       # Core business logic
      │   ├── B2S/                       # Backglass data structures
      │   ├── Animations/                # Animation system  
      │   ├── Illumination/              # LED/light management
      │   ├── Reels/                     # Score reel handling
      │   └── Images/                    # Image management
      ├── forms/                         # UI dialog forms
      ├── htmlhelp/                      # Help documentation
      └── bin/x64/Release/               # Build output location
```

### Secondary Projects
```
B2SVPinMAMEStarter/
  ├── B2SVPinMAMEStarter.sln            # Test mode launcher (.NET 4.8)
  └── B2SVPinMAMEStarter.vbproj         # ✅ Builds successfully with .NET 4.8

B2STools/
  ├── directb2sReelSoundsONOFF.cmd      # Sound configuration script
  └── directb2sReelSoundsONOFF.xsl      # XSLT transformation
```

### Configuration Files
- `app.config` - Application runtime settings and logging configuration
- `*.resx` - Resource files for UI forms and images
- `My Project/` - VB.NET project settings and assembly info

## CI/CD & Validation Pipeline

### GitHub Actions Workflow (`.github/workflows/b2s-designer.yml`)
**Build Matrix**: Debug/Release × Any CPU platform on Windows Server 2022
**Build Steps**:
1. Update assembly version with Git SHA + build number
2. Build both B2SVPinMAMEStarter and B2SBackglassDesigner projects
3. Compile HTML help (with failure tolerance)
4. Bundle executables, PDB files, help file, license, and changelog
5. Upload artifacts as `B2S.Designer-{version}-{config}-win-AnyCPU`

**Build Command Used in CI**:
```cmd
msbuild B2SVPinMAMEStarter/B2SVPinMAMEStarter.sln /t:Rebuild /p:Configuration=Release "/p:Platform=Any CPU"
msbuild b2sbackglassdesigner/B2SBackglassDesigner.sln /t:Rebuild /p:Configuration=Release "/p:Platform=Any CPU"
```

### Pre-commit Validation
- ✅ Main designer builds without errors
- ✅ VPinMAMEStarter builds successfully with .NET Framework 4.8
- ⚠️ HTML help compilation failure is **expected and tolerated**

## Development Guidelines

### Making Code Changes
1. **Always test build after changes**:
   ```cmd
   msbuild "b2sbackglassdesigner\B2SBackglassDesigner.sln" -t:Rebuild -p:Configuration=Release "/p:Platform=Any CPU"
   ```

2. **Key files to understand**:
   - `formDesigner.vb` - Main UI form, entry point for most features
   - `classes/B2S/BackglassData.vb` - Core data model
   - `classes/Save.vb` - File I/O operations
   - `classes/Recent.vb` - Recent files management

3. **No automated tests exist** - manual testing required

4. **Runtime Dependencies**: 
   - .NET Framework 4.8 on target system
   - Windows Forms applications

### Code Patterns
- **VB.NET Windows Forms** application with event-driven architecture
- **WYSIWYG editor** with canvas-based design surface
- **XML file generation** for directB2S format
- **Resource management** for images and assets

## Common Issues & Workarounds

### Build Failures
1. **"MinimumRecommendedRules.ruleset not found"**
   - **Cause**: Missing code analysis configuration
   - **Workaround**: Ignore warning, doesn't affect build
   - **Impact**: None

2. **HTML Help Workshop errors**
   - **Cause**: Missing installation or insufficient privileges  
   - **Workaround**: Install with admin rights or skip help compilation
   - **Impact**: No .chm help file generated

### File Dependencies
- **NOT obvious**: VB.NET project uses extensive Resource.resx files
- **Critical**: app.config must be deployed with executable
- **Important**: .ico files embedded for application icon

## Important Notes

- **Trust these instructions** - they are based on actual build testing and CI pipeline analysis
- **Main designer always builds successfully** on systems with .NET 4.8 and Visual Studio
- **Both projects now build successfully** after updating to .NET Framework 4.8
- **Release configuration recommended** for final builds  
- **Any CPU platform is the primary target** for cross-platform compatibility

Only perform additional exploration if information in these instructions proves incomplete or incorrect for your specific scenario.
