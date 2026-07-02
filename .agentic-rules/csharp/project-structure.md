# C# project structure and dependency management

## Standard layout

```text
TGC.MonoGame.Samples/
├── Viewer/           — viewer infrastructure
├── Samples/          — sample scenes, one folder per category
│   └── <Category>/
├── Cameras/          — camera implementations
├── Collisions/       — collision detection helpers
├── Geometries/       — procedural geometry primitives
├── Models/           — model extensions and geometry data
├── Physics/          — BepuPhysics integration
├── Animations/       — skinned animation pipeline
├── Mathematics/      — math utilities
└── Content/          — game assets processed by MGCB
    ├── Effects/      — HLSL shaders (.fx)
    ├── Textures/
    ├── 3D/           — 3D models
    ├── SpriteFonts/
    ├── Music/
    └── Sounds/
```

## Namespace convention

- Namespace **must** match the folder path exactly.
- Follow the pattern `TGC.MonoGame.Samples.<Folder>.<SubFolder>`
  (e.g., a class in `Samples/Tutorials/` uses
  `namespace TGC.MonoGame.Samples.Samples.Tutorials;`).
- Use file-scoped namespace declarations (`namespace Foo.Bar;`).
- **Never** place multiple namespaces in a single file.

## Dependency management

- All dependencies are managed via NuGet. **Never** vendor binaries directly.
- Pin versions explicitly in `.csproj`; avoid floating version ranges.
- Do not add new dependencies without justification; minimal dependencies are preferred.
- Check for vulnerabilities: `dotnet list package --vulnerable --include-transitive`.

## Continuous integration

- Restore before building: `dotnet restore TGC.MonoGame.Samples.sln`.
- Standard build: `dotnet build TGC.MonoGame.Samples.sln`.
- All quality gates are defined in `.pre-commit-config.yaml`. Do not
  bypass them with `--no-verify`.
