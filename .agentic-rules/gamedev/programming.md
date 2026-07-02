# Game programming rules

## Update vs Draw

- **Avoid** putting render calls inside `Update()`; it owns game state, not rendering.
- **Avoid** modifying game state inside `Draw()`; it renders the current
  state, nothing more.
- `Update()`: input, physics, positions, collisions, animations, AI, timers, camera.
- `Draw()`: render commands only; clear, draw calls, shader parameters.
- With a fixed timestep, `Update()` can be called multiple times before
  a single `Draw()`, and `Draw()` may be skipped entirely during
  slowdowns. Game logic in `Draw()` will not execute on those frames.

## Frame-rate independence

- **Always** scale movement and physics by
  `gameTime.ElapsedGameTime.TotalSeconds` in `Update()`.
