# Install

The samples where tested in:

- macOS Ventura.
- Windows 10/11.

We need the following tools:

- [.NET Core SDK 8 (LTS)](https://learn.microsoft.com/dotnet/core/install/windows)
- [Git Large File Storage (LFS)](https://git-lfs.com/)
- IDE alternatives:
  - Cross platform
    - [Rider](https://www.jetbrains.com/rider)
    - [Visual Studio Code](https://code.visualstudio.com)
      - [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
      - [HLSL](https://marketplace.visualstudio.com/items?itemName=TimGJones.hlsltools)
  - Windows
    - [Visual Studio](https://visualstudio.microsoft.com/es/vs)
      - [HLSL](https://marketplace.visualstudio.com/items?itemName=TimGJones.HLSLToolsforVisualStudio)
- [MGCB](https://docs.monogame.net/articles/getting_started/tools/mgcb.html)
- [MGCB Editor](https://docs.monogame.net/articles/getting_started/tools/mgcb_editor.html)
- [MGFXC](https://docs.monogame.net/articles/getting_started/tools/mgfxc.html)

Read more about [.NET Core CLI Tools telemetry](https://aka.ms/dotnet-cli-telemetry) and [Visual Studio Code telemetry](https://code.visualstudio.com/docs/editor/telemetry) are enabled by default.

## Setting up your development environment

- [~~Install on Ubuntu~~](install-ubuntu.md).
- [~~Install on macOS~~](install-mac.md).
- [Install on Windows](install-windows.md).

The assets are stored using [Git LFS](https://git-lfs.com/). Before cloning the repository it is convenient to have this installed so the pull is made automatically. If you already have it you can do `git lfs pull` or `git lfs install`.
