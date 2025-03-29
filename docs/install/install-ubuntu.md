# Install on Ubuntu

Tested on Ubuntu 24.04 LTS.

The offical [documentation](https://docs.monogame.net/articles/getting_started/1_setting_up_your_os_for_development_ubuntu.html).

Outside of Windows you need [Wine's](https://www.winehq.org) help for Effects(HLSL), at least for [now](https://github.com/MonoGame/MonoGame/issues/2167).

## Set up MonoGame

```bash
sudo apt update && sudo apt full-upgrade
sudo apt install dotnet-sdk-8.0
```

```bash
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

## Set up Wine for effect compilation

```bash
sudo apt install curl p7zip-full wget wine64
ln -s /usr/lib/wine/wine64 /usr/bin/wine64
```

WIP

## Set up tgc-monogame-samples

```bash
sudo apt install git git-lfs
```

WIP

### Known issues

WIP
