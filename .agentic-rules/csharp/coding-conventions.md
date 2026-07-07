# C# coding conventions

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
  - Namespace must match the folder path: `TGC.MonoGame.Samples.<Folder>`.
  - Use file-scoped namespace declarations (`namespace Foo.Bar;`).
  - **Never** place multiple namespaces in a single file.
- Use `T?` for nullable reference types and `T` for non-nullable.
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
