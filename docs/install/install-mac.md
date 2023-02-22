# Install on macOS

Tested on Mac with Intel processor and macOS Big Sur.

The offical [documentation](https://docs.monogame.net/articles/getting_started/1_setting_up_your_development_environment_macos.html).

Outside of Windows you need [Wine's](https://www.winehq.org) help for Effects(HLSL), at least for [now](https://github.com/MonoGame/MonoGame/issues/2167).

## Install Homebrew

* [Homebrew](https://brew.sh)

## Set up MonoGame

```bash
brew install --cask dotnet-sdk
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

For now it does not work on ARM chips (M1 and M2).

```bash
brew install p7zip wget wine-stable xquartz
wget -qO- https://raw.githubusercontent.com/MonoGame/MonoGame/develop/Tools/MonoGame.Effect.Compiler/mgfxc_wine_setup.sh | bash
```

## Set up Visual Studio Code

WIP

## Set up tgc-monogame-samples

```bash
brew install git git-lfs
git clone https://github.com/tgc-utn/tgc-monogame-samples.git
cd tgc-monogame-samples
dotnet restore
dotnet build
dotnet run --project TGC.MonoGame.Samples
```

### Known issues
WIP
