# Install on Windows

Test on Windows 10/11.

The official
[documentation](https://docs.monogame.net/articles/getting_started/1_setting_up_your_os_for_development_windows.html).

## Install PowerShell, Terminal and WinGet CLI

- [PowerShell](https://aka.ms/powershell).
- [Windows Terminal](https://aka.ms/terminal).
- [WinGet CLI](https://aka.ms/winget-cli).

## Set up MonoGame

```bash
winget install Microsoft.VCRedist.2013.x64
winget install Microsoft.VCRedist.2015+.x64
winget install Microsoft.DotNet.SDK.8

# To check the version installed.
dotnet --info
# Install MonoGame templates.
dotnet new install MonoGame.Templates.CSharp
dotnet new -l

# Create a basic project to test if MonoGame is working.
dotnet new mgdesktopgl -o MyGame
cd MyGame
dotnet restore
dotnet build
dotnet run
```

## Set up the IDE

You can use JetBrains Rider or Visual Studio or Visual Studio Code. The
official documentation only explains it for Visual Studio but it is up to you
which one you are more comfortable with.

### JetBrains Rider

```bash
winget install JetBrains.Rider
```

### Visual Studio

```bash
winget install Microsoft.VisualStudio.2022.Community
```

#### Extensions

- [HLSL Tools](https://marketplace.visualstudio.com/items?itemName=TimGJones.HLSLToolsforVisualStudio)
- [MonoGame](https://marketplace.visualstudio.com/items?itemName=MonoGame.MonoGame-Templates-VSExtension)

### Visual Studio Code

```bash
winget install Microsoft.VisualStudioCode

# Visual Studio Code extensions
# C# dev tools
code --install-extension ms-dotnettools.csdevkit
# HLSL tools
code --install-extension timgjones.hlsltools
```

## Set up tgc-monogame-samples

```bash
winget install Git.Git
winget install Python.Python.3
pip install pre-commit

git clone https://github.com/tgc-utn/tgc-monogame-samples.git
cd tgc-monogame-samples
pre-commit install

# MonoGame Effects Compiler (MGFXC)
dotnet tool install -g dotnet-mgfxc
dotnet restore
dotnet build
dotnet run --project TGC.MonoGame.Samples
```

### Known issues

- Unable to load DLL 'freetype6.dll' -
  Install [Microsoft Visual C++ Redistributable 2012](https://www.microsoft.com/en-us/download/details.aspx?id=30679).
- Unable to load DLL 'libmojoshader_64.dll' -
  Install [Microsoft Visual C++ Redistributable 2013](https://aka.ms/highdpimfc2013x64enu).
- Unable to load DLL 'FreeImage' -
  Install
[Microsoft Visual C++ Redistributable for Visual Studio 2015, 2017 and 2019](https://aka.ms/vs/16/release/vc_redist.x64.exe).
