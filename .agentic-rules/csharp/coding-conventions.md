# C# coding conventions

## Technology stack

- Language: C# 12.
- Runtime: .NET 8.
- Build tool: dotnet CLI.

## Project flags (from `.csproj`)

- `Nullable = enable`: use `T?` for nullable reference types and `T`
  for non-nullable.
- `AllowUnsafeBlocks = true`: unsafe code is permitted but must be
  deliberate (see `security-patterns.md`).
- `GenerateDocumentationFile = true`: XML doc comments are required on
  all public members (see `documentation.md`).
- `PublishReadyToRun = false`: ReadyToRun AOT pre-compilation is
  disabled; MonoGame manages its own startup performance.
- `TieredCompilation = false`: tiered JIT is disabled to avoid
  inconsistent performance during the game loop.
- `RollForward = Major`: the runtime may roll forward to a newer major
  version if the exact one is unavailable.

## Code style

All formatting and naming rules are enforced by `.editorconfig` and
auto-fixed by `dotnet format`. If anything here conflicts with
`.editorconfig`, `.editorconfig` wins.

- Naming guidelines:
  - PascalCase for types, methods, properties, constants, and enums.
  - camelCase for parameters and local variables.
  - `_camelCase` for private instance fields.
  - `s_camelCase` for private static fields.
  - `IName` for interfaces.
- Prefer pattern matching and switch expressions over long if-else
  chains.
- Use `record` types for simple immutable data containers.

## Exception handling

- **Never** catch the root `Exception` without specific handling.
- **Always** include the original exception as inner exception when
  wrapping.
- Use specific exception types; avoid generic exceptions for domain
  errors.

## Development tools and practices

- Do not suppress analyzer warnings with `#pragma warning disable` or
  `[SuppressMessage]`. Resolve the root cause.
- Do not modify `.editorconfig` without team discussion. Style rules
  are enforced in CI.
