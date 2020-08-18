# TGC - MonoGame - Samples
[#BuiltWithMonoGame](http://www.monogame.net) and [.NET Core](https://dotnet.microsoft.com)

## Install
* [.NET Core SDK](https://docs.microsoft.com/dotnet/core/install/sdk)
* The IDE you prefer:
  * [Visual Studio Code](https://code.visualstudio.com)
  * [Visual Studio](https://visualstudio.microsoft.com/es/vs) or [Visual Studio for Mac](https://visualstudio.microsoft.com/es/vs/mac)
  * [Rider](https://www.jetbrains.com/rider)
* [MGCB Editor](https://docs.monogame.net/articles/tools/mgcb_editor.html)

## Setting up your development environment
 * [Windows 10](https://docs.monogame.net/articles/getting_started/1_setting_up_your_development_environment_windows.html)
   * You can use Visual Studio Code or Rider. The official documentation only explains it for Visual Studio but it is up to each one of you that you feel more comfortable with.
 * [Linux (test on Ubuntu 20.04)](https://docs.monogame.net/articles/getting_started/1_setting_up_your_development_environment_ubuntu.html)
 * [Mac (test on macOS Mojave)](https://docs.monogame.net/articles/getting_started/1_setting_up_your_development_environment_macos.html)

Outside of Windows you need [Wine's](https://www.winehq.org) help for Effects, at least for [now](https://github.com/MonoGame/MonoGame/issues/2167).

## To run in a terminal
```bash
dotnet restore
dotnet build
dotnet run --project TGC.MonoGame.Samples
```

### General problems
* Open Asset Import Library with Obj and Dae fails, you have to change it to Fbx importer.
