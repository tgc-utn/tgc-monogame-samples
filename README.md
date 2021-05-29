# TGC - MonoGame - Samples
[#BuiltWithMonoGame](http://www.monogame.net) and [.NET Core](https://dotnet.microsoft.com)

## Install
* [.NET Core SDK](https://docs.microsoft.com/dotnet/core/install/sdk)
* [Git Large File Storage (LFS)](https://git-lfs.github.com/)
* IDE alternatives:
  * Cross platform
    * [Rider](https://www.jetbrains.com/rider)
    * [Visual Studio Code](https://code.visualstudio.com) and [HLSL extension](https://marketplace.visualstudio.com/items?itemName=TimGJones.hlsltools)
  * Windows
    * [Visual Studio](https://visualstudio.microsoft.com/es/vs) and [HLSL extension](https://marketplace.visualstudio.com/items?itemName=TimGJones.HLSLToolsforVisualStudio)
  * macOS
    * [Visual Studio for Mac](https://visualstudio.microsoft.com/es/vs/mac)
  
* [MGCB Editor](https://docs.monogame.net/articles/tools/mgcb_editor.html)
* [MGFXC](https://docs.monogame.net/articles/tools/mgfxc.html)

Read more about [.NET Core CLI Tools telemetry](https://aka.ms/dotnet-cli-telemetry) and [Visual Studio Code telemetry](https://code.visualstudio.com/docs/getstarted/telemetry) are enabled by default.

## Setting up your development environment
 * [Windows 10](https://docs.monogame.net/articles/getting_started/1_setting_up_your_development_environment_windows.html)
   * You can use Visual Studio Code or Rider. The official documentation only explains it for Visual Studio but it is up to you which one you are more comfortable with.
 * [Linux (test on Ubuntu 20.04)](https://docs.monogame.net/articles/getting_started/1_setting_up_your_development_environment_ubuntu.html)
 * [Mac (test on macOS Big Sur)](https://docs.monogame.net/articles/getting_started/1_setting_up_your_development_environment_macos.html)

Outside of Windows you need [Wine's](https://www.winehq.org) help for Effects(HLSL), at least for [now](https://github.com/MonoGame/MonoGame/issues/2167).

The assets are stored using [Git LFS](https://git-lfs.github.com). Before cloning the repository it is convenient to have this installed so the pull is made automatically. If you already have it you can do `git lfs pull`.

## To run in a terminal
```bash
dotnet restore
dotnet build
dotnet run --project TGC.MonoGame.Samples
```

### Known issues

#### [MGCB Editor](https://docs.monogame.net/articles/tools/mgcb_editor.html)
* Open Asset Import Library with Obj and Dae fails, you have to change it to Fbx importer.

#### Windows 10
* Unable to load DLL 'freetype6.dll' - Install [Microsoft Visual C++ Redistributable 2012](https://www.microsoft.com/en-us/download/details.aspx?id=30679)
* Unable to load DLL 'libmojoshader_64.dll' - Install [Microsoft Visual C++ Redistributable 2013](https://aka.ms/highdpimfc2013x64enu)
* Unable to load DLL 'FreeImage' - Install [Microsoft Visual C++ Redistributable for Visual Studio 2015, 2017 and 2019](https://aka.ms/vs/16/release/vc_redist.x64.exe)
