# Install on Ubuntu

Tested on Ubuntu 24.04 LTS.

The offical [documentation](https://docs.monogame.net/articles/getting_started/1_setting_up_your_development_environment_unix.html).

Outside of Windows you need [Wine's](https://www.winehq.org) help for Effects(HLSL), at least for [now](https://github.com/MonoGame/MonoGame/issues/2167).

## Set up MonoGame

```bash
sudo apt update && sudo apt full-upgrade
sudo add-apt-repository ppa:dotnet/backports
sudo apt install dotnet6

# To check the version installed.
dotnet --info
dotnet new --install MonoGame.Templates.CSharp
dotnet new -l

# Create a basic project to test if MonoGame is working.
dotnet new mgdesktopgl -o MyGame
cd MyGame
dotnet restore
dotnet build
dotnet run
```

## Set up Wine for effect compilation

```bash
sudo apt install curl p7zip-full wget wine64
wine --info
sudo ln -s /usr/lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so
sudo ln -s /usr/bin/wine /usr/bin/wine64
sudo apt install libminizip-dev
wget -qO- https://raw.githubusercontent.com/MonoGame/MonoGame/develop/Tools/MonoGame.Effect.Compiler/mgfxc_wine_setup.sh | bash
```

## Set up the IDE

You can use Visual Studio Code or Rider. The official documentation only explains it for Visual Studio but it is up to you which one you are more comfortable with.

### Visual Studio Code

```bash
flatpak install flathub com.visualstudio.code

# Visual Studio Code extensions
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.csdevkit
code --install-extension ms-dotnettools.dotnet-maui
code --install-extension timgjones.hlsltools
```

### JetBrains Rider

```bash
flatpak install flathub com.jetbrains.Rider
```

## Set up tgc-monogame-samples

```bash
sudo apt install git git-lfs
cd tgc-monogame-samples/TGC.MonoGame.Samples
LD_LIBRARY_PATH=~/.nuget/packages/dotnet-mgcb/3.8.1.303/tools/net6.0/any dotnet run
```

### Known issues

* Unable to load shared library 'nvtt' - Use ```LD_LIBRARY_PATH=~/.nuget/packages/dotnet-mgcb/3.8.1.303/tools/net6.0/any``` with dotnet CLI commands.
* ```dotnet run --project TGC.MonoGame.Samples``` make some samples crash, a simple workaround is to run in the cproj folder instead of the sln folder.
