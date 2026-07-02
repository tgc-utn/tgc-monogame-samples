# MonoGame framework

## Version

- MonoGame 3.8.4 (DesktopGL), targeting Windows 8.1+, Linux, and macOS 10.15+.
- Graphics backend: OpenGL via SDL 2.32.2.1.
- Shaders: written in HLSL Shader Model 3.0 (`vs_3_0` / `ps_3_0`) and
  transpiled to GLSL by MojoShader at content build time.

## NuGet dependencies

- `MonoGame.Framework.DesktopGL` 3.8.4: main framework.
- `MonoGame.Framework.Content.Pipeline` 3.8.4: content pipeline processors.
- `MonoGame.Content.Builder.Task` 3.8.4: MSBuild integration for MGCB.
- `BepuPhysics` 2.4.0: physics engine.
- `ImGui.NET` 1.91.6.1: immediate mode UI used by the sample browser.
- `Microsoft.Extensions.Configuration.Json` 9.0.8: runtime configuration via `app-settings.json`.

## Tools

All tools are local dotnet tools managed via `dotnet tool restore`.

- `mgcb`: MGCB command line builder.
- `mgcb-editor`: cross-platform visual content editor.
- `mgcb-editor-linux`, `mgcb-editor-windows`, `mgcb-editor-mac`:
  platform-specific editor variants.
