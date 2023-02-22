# Install on Ubuntu

Tested on Ubuntu 22.04 LTS.

The offical [documentation](https://docs.monogame.net/articles/getting_started/1_setting_up_your_development_environment_ubuntu.html)

Outside of Windows you need [Wine's](https://www.winehq.org) help for Effects(HLSL), at least for [now](https://github.com/MonoGame/MonoGame/issues/2167).

## Set up MonoGame

```bash
sudo apt update && sudo apt full-upgrade
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

WIP

## Set up tgc-monogame-samples

```bash
sudo apt install git git-lfs
```
WIP

### Known issues

WIP
