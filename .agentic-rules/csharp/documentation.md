# C# documentation guidelines

## XML doc comments

- Document all public classes, methods, properties, and fields.
- Focus on *what* and *why*, not *how*.
- Keep `<summary>` concise; one or two sentences maximum.
- Use `<param>`, `<returns>`, `<exception>`, `<see>`, and `<inheritdoc>`
  tags where applicable.
- List possible exceptions thrown by a method using `<exception>`.

## Sample class header

- Every sample class **must** open with a `<summary>` containing:
  - Sample name.
  - Short description of what it demonstrates.
  - Author name.

## Documentation hygiene

- Keep docs in sync with code changes.
- Remove outdated or misleading comments immediately.
- Do not describe what the code does if well-named identifiers already convey it.
- Do not reference the current task, PR, or issue number in code comments.
