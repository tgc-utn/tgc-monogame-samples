# Install on macOS

Tested on Mac with Intel processor and macOS Sonoma.

The offical [documentation](https://docs.monogame.net/articles/getting_started/1_setting_up_your_development_environment_macos.html).

Outside of Windows you need [Wine's](https://www.winehq.org) help for Effects(HLSL), at least for [now](https://github.com/MonoGame/MonoGame/issues/2167).

## Install Homebrew

* [Homebrew](https://brew.sh).

## Set up MonoGame

```bash
brew install dotnet@6
echo 'export PATH="/opt/homebrew/opt/dotnet@6/bin:$PATH"' >> ~/.zshrc
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
brew install p7zip xquartz wget wine-stable
wine64 --version

brew install freeimage
# To know where the library is installed.
brew list freeimage
sudo ln -s /opt/homebrew/Cellar/freeimage/3.18.0/lib/libfreeimage.dylib /usr/local/lib/libfreeimage

brew install freetype
# To know where the library is installed.
brew list freetype
sudo ln -s /opt/homebrew/Cellar/freetype/2.13.2/lib/libfreetype.6.dylib /usr/local/lib/libfreetype6.dylib
```

You will need to open Wine manually first. Otherwise, you will get an error that ```Apple couldn't verify it.```

```bash
wget -qO- https://raw.githubusercontent.com/MonoGame/MonoGame/master/Tools/MonoGame.Effect.Compiler/mgfxc_wine_setup.sh | bash
```

## Set up the IDE

You can use Visual Studio Code or Rider. The official documentation only explains it for Visual Studio but it is up to
you which one you are more comfortable with.

### Visual Studio Code

```bash
brew install --cask visual-studio-code

# Visual Studio Code extensions
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.csdevkit
code --install-extension ms-dotnettools.dotnet-maui
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

* Assimp.AssimpException: Error loading unmanaged library from path: libassimp.dylib - WIP
