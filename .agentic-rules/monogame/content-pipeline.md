# Content pipeline guidelines

## Asset location

```text
Content/
├── Content.mgcb          — MGCB manifest (register all assets here)
├── Effects/              — HLSL shaders (.fx)
├── Textures/             — 2D textures
├── 3D/                   — 3D models
├── SpriteFonts/          — Sprite fonts (.spritefont)
├── Music/                — Background music
└── Sounds/               — Sound effects
```

## Path constants

- **Never** hardcode content path strings; use the constants defined
  on `TGCSample`.

## Registering new assets

- Every new asset **must** be registered in `Content/Content.mgcb`
  before it can be loaded; unregistered assets cause a
  `ContentLoadException` at runtime.
- Use power-of-two dimensions for textures (256, 512, 1024…) to ensure
  cross-platform compatibility.

## Model formats

- Verify that imported models load cleanly with `dotnet build` before
  committing.

## HLSL shaders

- Shader files (`.fx`) go in `Content/Effects/`.
- Write shaders in HLSL; MGCB compiles them and MojoShader transpiles
  to GLSL at build time. **Never** write GLSL directly.
- Target shader model: `vs_3_0` / `ps_3_0` (DesktopGL / OpenGL).
- Access shader parameters with `?` (null-conditional) if the parameter
  is not guaranteed to exist:
  `Effect.Parameters["Variable"]?.SetValue(value)`.
- `SV_POSITION` is not supported on OpenGL and **must** be aliased to
  `POSITION` in every shader.
