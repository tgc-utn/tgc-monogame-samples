# Claude instructions

The agent **must** always consult this section and **must** never modify
this file unless explicitly instructed to do so.

## Project overview

Read [`README.md`](README.md) for a summary of what this project does
and how to run it.
Read [`.csproj`](TGC.MonoGame.Samples/TGC.MonoGame.Samples.csproj)
for the full tech stack, dependencies, and build configuration.
Read [`docs/install/install.md`](docs/install/install.md) for
platform-specific installation notes.

## Agent rules

Additional rules that apply to all agents working in this repository
are defined in `.agentic-rules/`:

### Common

- `.agentic-rules/common/language.md`: language policy.
- `.agentic-rules/common/git.md`: commit conventions, branching model,
  and best practices.
- `.agentic-rules/common/agent-behaviour.md`: planning workflow,
  progress tracking, and response principles.
- `.agentic-rules/common/agent-response.md`: response structure and
  formatting conventions.

### Documentation

- `.agentic-rules/documentation/writing-style.md`: capitalization,
  punctuation, and emphasis rules.

### C Sharp

- `.agentic-rules/csharp/coding-conventions.md`: C# naming conventions,
  code style, and analyzer rules.
- `.agentic-rules/csharp/documentation.md`: XML doc comment standards.
- `.agentic-rules/csharp/project.md`: C# project-level policies and standards.
- `.agentic-rules/csharp/testing.md`: testing approach and manual
  verification conventions.
- `.agentic-rules/csharp/security-patterns.md`: secure coding patterns
  for C# and unsafe code usage.

### MonoGame

- `.agentic-rules/monogame/framework.md`: MonoGame version, graphics
  backend, NuGet dependencies, and tools.
- `.agentic-rules/monogame/sample.md`: sample structure, lifecycle,
  and available helpers.
- `.agentic-rules/monogame/content-pipeline.md`: MGCB, asset
  conventions, shaders, and path constants.
- `.agentic-rules/monogame/performance.md`: 60 FPS target, GC
  avoidance, and rendering best practices.

### Game development

- `.agentic-rules/gamedev/programming.md`: engine-agnostic game
  programming principles.

## When to apply rules

- **Always**: read `language.md`, `agent-behaviour.md`, and
  `agent-response.md` before any response.
- **Before any git operation**: read `git.md`.
- **Before writing or modifying C# code**: read `coding-conventions.md`
  and `security-patterns.md`.
- **Before creating or editing a sample**: read `sample.md`.
- **Before touching assets or shaders**: read `content-pipeline.md`.
- **Before writing or modifying tests**: read `testing.md`.
- **Before adding or updating dependencies**: read `project.md`.
- **Before writing or modifying documentation**: read
  `writing-style.md`.
- **Before writing or modifying XML doc comments**: read
  `csharp/documentation.md`.
- **Before implementing helper systems or performance-sensitive code**:
  read `monogame/performance.md`.
- **Before implementing any game logic**: read `gamedev/programming.md`.

## Overrides

These rules explicitly override the agent's built-in defaults.
Project rules always take precedence.

- **Never** add `Co-Authored-By` lines to commits.
