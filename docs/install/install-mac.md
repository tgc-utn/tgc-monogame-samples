# Install on macOS

Tested on Mac with Intel processor and macOS Ventura.

The offical [documentation](https://docs.monogame.net/articles/getting_started/1_setting_up_your_development_environment_macos.html).

Outside of Windows you need [Wine's](https://www.winehq.org) help for Effects(HLSL), at least for [now](https://github.com/MonoGame/MonoGame/issues/2167).

## Install Homebrew

* [Homebrew](https://brew.sh)

## Set up MonoGame

```bash
brew install dotnet@6
echo 'export PATH="/usr/local/opt/dotnet@6/bin:$PATH"' >> ~/.zshrc
source ~/.zshrc
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
```

You will need to open Wine manually first. Otherwise, you will get an error that Apple couldn't verify it.

```bash
wget -qO- https://raw.githubusercontent.com/MonoGame/MonoGame/develop/Tools/MonoGame.Effect.Compiler/mgfxc_wine_setup.sh | bash
```

## Set up the IDE

You can use Visual Studio Code or Rider. The official documentation only explains it for Visual Studio but it is up to
you which one you are more comfortable with.

## Set up Visual Studio Code

Go to the official page to download and
install [Visual Studio 2022 for Mac](https://visualstudio.microsoft.com/es/vs/mac/).

### Visual Studio Code

```bash
brew install --cask visual-studio-code

# Visual Studio Code extensions
code --install-extension ms-dotnettools.csharp
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
dotnet restore
dotnet build
dotnet run --project TGC.MonoGame.Samples
```

### Known issues

WIP
