# Development Guide

This guide will help you set up the development environment for the Shader Glass Playnite Plugin.

## Prerequisites

- **Visual Studio 2019 or later** (or Visual Studio Code with C# extension)
- **.NET Framework 4.6.2** or later
- **Playnite SDK 6.2.0** (will be restored via NuGet)
- **Git** (for cloning the repository)

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd shaderglass-playnite-plugin/ShaderGlass
```

### 2. Open the Solution

Open `ShaderGlass.sln` in Visual Studio:
- Double-click `ShaderGlass.sln`, or
- File → Open → Project/Solution → Select `ShaderGlass.sln`

### 3. Restore NuGet Packages

Visual Studio should automatically restore NuGet packages when you open the solution. If not:

**Option A: Automatic Restore**
- Right-click on the solution in Solution Explorer
- Select **Restore NuGet Packages**

**Option B: Manual Restore via Package Manager Console**
- Tools → NuGet Package Manager → Package Manager Console
- Run: `Update-Package -reinstall`

**Option C: Command Line**
```bash
nuget restore ShaderGlass.sln
```

### 4. Verify Dependencies

Ensure the following NuGet package is restored:
- **PlayniteSDK** version 6.2.0 (should be in `packages/PlayniteSDK.6.2.0/`)

### 5. Build the Project

- **Build** → **Build Solution** (or press `Ctrl+Shift+B`)
- The output will be in `bin/Debug/` or `bin/Release/` depending on the configuration

## Project Structure

```
ShaderGlass/
├── src/                          # Source code directory
│   ├── ShaderGlass.cs           # Main plugin class
│   ├── ShaderGlassSettings.cs   # Settings class
│   ├── ShaderGlassSettingsView.xaml      # Settings UI (XAML)
│   ├── ShaderGlassSettingsView.xaml.cs   # Settings UI code-behind
│   ├── App.xaml                 # Application resources
│   └── Properties/
│       └── AssemblyInfo.cs      # Assembly metadata
├── Localization/                 # Localization files
│   └── en_US.xaml
├── extension.yaml                # Plugin metadata
├── icon.png                      # Plugin icon
├── ShaderGlass.csproj           # Project file
├── ShaderGlass.sln              # Solution file
├── packages.config               # NuGet packages configuration
└── README.md                    # User documentation
```

## Building for Release

1. Change the build configuration to **Release**:
   - Build → Configuration Manager → Set "Active solution configuration" to **Release**

2. Build the solution:
   - Build → Build Solution

3. The release files will be in `bin/Release/`:
   - `ShaderGlass.dll` - The compiled plugin
   - `extension.yaml` - Plugin metadata
   - `icon.png` - Plugin icon
   - `Localization/en_US.xaml` - Localization file

## Testing the Plugin

### Local Testing

1. Build the project in **Debug** configuration
2. Copy the contents of `bin/Debug/` to your Playnite extensions folder:
   - Portable installation: `%AppData%\Playnite\Extensions\`
   - Or `Extensions\` folder in your Playnite installation directory
3. Restart Playnite
4. The plugin should appear in Settings → Extensions → Shader Glass

### Debugging

1. Set breakpoints in your code
2. Attach Visual Studio debugger to Playnite process:
   - Debug → Attach to Process → Select `Playnite.DesktopApp.exe`
3. Or configure Playnite as the startup project (if you have Playnite source)

## Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML comments for public methods and classes
- Keep methods focused and single-purpose

## Dependencies

- **Playnite.SDK** (6.2.0) - Required for Playnite plugin development
- **.NET Framework 4.6.2** - Target framework

## Troubleshooting

### NuGet packages not restoring
- Check your internet connection
- Verify NuGet package source is accessible
- Try clearing NuGet cache: `nuget locals all -clear`

### Build errors
- Ensure .NET Framework 4.6.2 Developer Pack is installed
- Clean and rebuild: Build → Clean Solution, then Build → Rebuild Solution
- Delete `bin/` and `obj/` folders and rebuild

### Plugin not loading in Playnite
- Check Playnite log files for errors
- Verify all required files are in the extensions folder
- Ensure `extension.yaml` has correct format
- Check that `ShaderGlass.dll` is compiled for .NET Framework 4.6.2

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Test thoroughly
5. Commit your changes (`git commit -m 'Add some amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

## Resources

- [ShaderGlass](https://github.com/mausimus/ShaderGlass)
- [Playnite Plugin Development Documentation](https://playnite.link/docs/)
- [Playnite SDK API Reference](https://playnite.link/docs/api/)
- [C# Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/)

