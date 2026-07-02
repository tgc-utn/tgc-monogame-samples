# Sample guidelines

## Spirit

- Each sample **must** demonstrate one concept clearly. Do not mix unrelated techniques.
- Prioritize readability; this is educational code. Clarity beats brevity.
- Use comments to explain *why* something is done, not *what* the code does.
- Prefer project helpers over custom implementations; they exist to
  keep samples focused.

## Sample lifecycle

Each lifecycle method has a single responsibility. Do not bleed logic between them.

- Constructor: set `Category`, `Name`, and `Description`.
- `Initialize()`: camera, `GraphicsDevice` states, `Game.Background`.
- `LoadContent()`: assets, primitives, effects.
- `Update(GameTime)`: camera, game logic, time-varying shader parameters.
- `Draw(GameTime)`: draw calls only.
- `UnloadContent()`: dispose all `IDisposable` resources; the sample
  may be shown again.

## What TGCSample provides

- `Game`: the `TGCViewer` instance.
- `GraphicsDevice`: available from `Initialize()` onwards.
- `Game.Content`: `ContentManager`; use to load assets.
- `Game.Background`: the clear color.
- `Game.Gizmos`: debug draw helper; enabled automatically by
  `Prepare()`. Call `UpdateViewProjection(view, projection)` each frame
  in `Update()`.
- `ModifierController`: ImGui runtime controls; cleared automatically on `ReloadContent()`.
- `ContentFolder*`: path prefixes for assets (`ContentFolder3D`,
  `ContentFolderEffects`, `ContentFolderTextures`, etc.).

## Available helpers

Use these before writing your own. Read the source in the corresponding
folder for the full list of available classes.

- Cameras: `TGC.MonoGame.Samples.Cameras`.
- Geometries (procedural primitives): `TGC.MonoGame.Samples.Geometries`.
- Collisions: `TGC.MonoGame.Samples.Collisions`.
- Mathematics: `TGC.MonoGame.Samples.Mathematics`.
- Models: `TGC.MonoGame.Samples.Models`.
