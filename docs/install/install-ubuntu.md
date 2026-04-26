# Install on Ubuntu

Tested on Ubuntu 24.04 LTS.

The official [documentation](https://docs.monogame.net/articles/getting_started/1_setting_up_your_os_for_development_ubuntu.html).

Outside of Windows you need [Wine's](https://www.winehq.org) help for Effects(HLSL), at least for [now](https://github.com/MonoGame/MonoGame/issues/2167).

## Set up MonoGame

```bash
sudo apt update && sudo apt full-upgrade
sudo apt install dotnet-sdk-8.0

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
wine --version
wget -qO- https://monogame.net/downloads/net8_mgfxc_wine_setup.sh | bash
```

This will create new directory called .winemonogame in your home directory. If you ever wish to undo the setup this script performed, just simply delete that directory.

## Set up the IDE

You can use JetBrains Rider or Visual Studio Code.

### JetBrains Rider

- [Download tar.gz](https://www.jetbrains.com/rider/download/?section=linux)
  - `wget https://download.jetbrains.com/rider/JetBrains.Rider-2025.2.0.1.tar.gz`
- [Flatpak](https://flathub.org/apps/com.visualstudio.code)
  - `flatpak install flathub com.jetbrains.Rider`

### Visual Studio Code

- [Download deb](https://code.visualstudio.com/Download)
  - `wget https://code.visualstudio.com/sha/download?build=stable&os=linux-deb-x64`
- [Flatpak](https://flathub.org/apps/com.visualstudio.code)
  - `flatpak install flathub com.visualstudio.code`

#### Plugins

```bash
# Copilot (optional)
code --install-extension Github.copilot
code --install-extension Github.copilot-chat

# C# dev tools
code --install-extension ms-dotnettools.csdevkit

# HLSL tools
code --install-extension timgjones.hlsltools
```

## Set up tgc-monogame-samples

```bash
sudo apt install git git-lfs
git lfs install
git clone https://github.com/tgc-utn/tgc-monogame-samples.git
cd tgc-monogame-samples

# MonoGame Effects Compiler (MGFXC).
dotnet tool install -g dotnet-mgfxc
# After the tool installation, check the command output for any additional setup instructions.
dotnet restore
dotnet build
dotnet run --project TGC.MonoGame.Samples
```

### Known issues

- Problem after install MGFXC
  - Tools directory '/Users/user/.dotnet/tools' is not currently on the PATH environment variable.
  - Some systems may require you to restart your terminal or IDE to recognize the new tool.
