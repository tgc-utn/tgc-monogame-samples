# Install on Windows

Test on Windows 10/11.

The offical [documentation](https://docs.monogame.net/articles/getting_started/1_setting_up_your_development_environment_windows.html).

## Install PowerShell, Terminal and WinGet CLI on Windows 10 (on Windows 11 is already install)

* [PowerShell](https://aka.ms/powershell).
* [Windows Terminal](https://aka.ms/terminal).
* [WinGet CLI](https://aka.ms/winget-cli).

## Set up MonoGame

```bash
winget install Microsoft.VCRedist.2013.x64
winget install Microsoft.VCRedist.2015+.x64
winget install Microsoft.DotNet.SDK.6
```

Open other terminal ([Issue 222](https://github.com/microsoft/winget-cli/issues/222)) so you can use `dotnet` and type:

```bash
dotnet new --install MonoGame.Templates.CSharp

# Create a basic project to test if MonoGame is working.
dotnet new mgdesktopgl -o MyGame
cd MyGame
dotnet restore
dotnet build
dotnet run
```

## Set up the IDE

You can use Visual Studio Code or Rider. The official documentation only explains it for Visual Studio but it is up to you which one you are more comfortable with.

### Visual Studio Code

```bash
winget install Microsoft.VisualStudioCode
```

Open other terminal ([Issue 222](https://github.com/microsoft/winget-cli/issues/222)) so you can use `code` and type:

```bash
# Visual Studio Code extensions
code --install-extension ms-dotnettools.csharp
code --install-extension timgjones.hlsltools
```

### JetBrains Rider

```bash
winget install JetBrains.Rider
```

### Visual Studio

```bash
winget install Microsoft.VisualStudio.2022.Community
```

#### Extensions

* [HLSL Tools](https://marketplace.visualstudio.com/items?itemName=TimGJones.HLSLToolsforVisualStudio)
* [MonoGame](https://marketplace.visualstudio.com/items?itemName=MonoGame.MonoGame-Templates-VSExtension)

## Set up tgc-monogame-samples

```bash
winget install Git.Git
```

Open other terminal ([Issue 222](https://github.com/microsoft/winget-cli/issues/222)) so you can use `git` and type:

```bash
git clone https://github.com/tgc-utn/tgc-monogame-samples.git
cd tgc-monogame-samples
# MonoGame Effects Compiler (MGFXC)
dotnet tool install -g dotnet-mgfxc
dotnet restore
dotnet build
dotnet run --project TGC.MonoGame.Samples
```

### Known issues

* Unable to load DLL 'freetype6.dll' - Install [Microsoft Visual C++ Redistributable 2012](https://www.microsoft.com/en-us/download/details.aspx?id=30679).
* Unable to load DLL 'libmojoshader_64.dll' - Install [Microsoft Visual C++ Redistributable 2013](https://aka.ms/highdpimfc2013x64enu).
* Unable to load DLL 'FreeImage' - Install [Microsoft Visual C++ Redistributable for Visual Studio 2015, 2017 and 2019](https://aka.ms/vs/16/release/vc_redist.x64.exe).
