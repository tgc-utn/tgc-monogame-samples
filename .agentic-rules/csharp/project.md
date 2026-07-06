# C# project

## Technology stack

- Language: C# 12.
- SDK: .NET 8 (LTS); target LTS releases only.
- Build tool: dotnet CLI.

## Dependencies

- All dependencies are managed via NuGet. **Never** vendor binaries directly.
- Pin versions explicitly in `.csproj`; avoid floating version ranges.
- Do not add new dependencies without justification; prefer minimal
  dependencies.
