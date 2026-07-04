# Performance guidelines

## Target

- Prioritize clarity over optimization. This is an educational project;
  write the clearest code first.
- Achieve a minimum of 60 FPS. Apply the rules below strictly in helper
  systems (`Cameras/`, `Collisions/`, `Geometries/`), not in
  demonstration samples.

## Game loop

- **Never** block the main thread. Avoid synchronous I/O,
  `Thread.Sleep`, and heavy computation in `Update()` or `Draw()`.
- Move expensive one-time work to `Initialize()` or `LoadContent()`,
  not to the first frame of `Update()`.
- **Avoid** loading content at runtime after startup; use
  `ContentManager` during `LoadContent()` only.

## Memory and garbage collection

- Avoid allocations in the hot path (`Update()` and `Draw()`). GC
  pauses cause frame drops.
- Do not use LINQ inside `Update()` or `Draw()`; it allocates
  enumerators.
- **Avoid** `foreach` over `IEnumerable<T>`, `IList<T>`, or any
  interface type in hot paths; the enumerator is boxed and allocates
  on the heap. Arrays and `List<T>` are safe; the compiler resolves
  the struct enumerator directly.
- Do not use string concatenation or interpolation in the game loop.
- Prefer `struct` over `class` for small, frequently created data
  (vectors, colors, transforms).
- Reuse arrays and collections across frames; avoid `new List<T>()`
  per frame.
- Prefer `Span<T>` / `Memory<T>` over array copies to avoid heap
  allocations.

## Rendering

- Batch draw calls; minimize state changes between draw calls
  (`Effect`, `RasterizerState`, `BlendState`).
- Sort opaque geometry front-to-back; transparent geometry
  back-to-front.
- Allocate `RenderTarget2D` instances once in `LoadContent()`, not
  per frame.
- Use `GraphicsDevice.SetRenderTargets()` (plural) instead of
  `SetRenderTarget()`; the singular form generates garbage on every
  call.
- Avoid passing individual targets to `SetRenderTargets()`; the
  implicit array instantiation generates garbage. Prefer passing a
  pre-allocated and cached array.

## Profiling

- Measure first, then optimize. Do not optimize based on intuition.
- Use `GameTime.ElapsedGameTime` for per-frame timing measurements.
