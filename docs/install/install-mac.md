# Install on macOS

Tested on Mac with **Intel** processor and macOS Sequoia.

The official [documentation](https://docs.monogame.net/articles/getting_started/1_setting_up_your_os_for_development_macos.html).

Outside of Windows you need [Wine's](https://www.winehq.org) help for Effects(HLSL), at least for [now](https://github.com/MonoGame/MonoGame/issues/2167).

## Install Homebrew

- [Homebrew](https://brew.sh).

## Set up MonoGame

```bash
brew install dotnet@8
echo 'export PATH="/opt/homebrew/opt/dotnet@6/bin:$PATH"' >> ~/.zshrc
source ~/.zshrc
# To check the version installed.
dotnet --info
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

For now it does not work on ARM chips (M1 and M2).

```bash
brew install curl p7zip wget
brew install --cask wine-stable

wine --version
```

You will need to open Wine manually first (Wine Stable). Otherwise, you will get an error that Apple couldn't verify it.

Next open System Settings. Click Privacy & Security, scroll down, and click the Open Anyway button to confirm your intent to open or install the app (Wine Stable).

```bash
wget -qO- https://monogame.net/downloads/net8_mgfxc_wine_setup.sh | bash
```

## Set up the IDE

You can use Visual Studio Code or Rider.

### Visual Studio Code

```bash
brew install --cask visual-studio-code

# Visual Studio Code extensions
# Copilot (optional)
code --install-extension Github.copilot
code --install-extension Github.copilot-chat
# C# dev tools
code --install-extension ms-dotnettools.csdevkit
# HLSL tools
code --install-extension timgjones.hlsltools
```

### JetBrains Rider

```bash
brew install --cask rider
```

## Set up tgc-monogame-samples

```bash
brew install git git-lfs
git lfs install
git clone https://github.com/tgc-utn/tgc-monogame-samples.git
cd tgc-monogame-samples
# MonoGame Effects Compiler (MGFXC)
dotnet tool install -g dotnet-mgfxc
dotnet restore
dotnet build
dotnet run --project TGC.MonoGame.Samples
```

### Known issues

- An error occurred trying to start process '/Users/administrador/.winemonogame/wine64' - `ln -s /usr/local/bin/wine /usr/local/bin/wine64`
- Assimp.AssimpException: Error loading unmanaged library from path: libassimp.dylib - WIP
- System.DllNotFoundException: Unable to load shared library 'freetype6' or one of its dependencies. - WIP
