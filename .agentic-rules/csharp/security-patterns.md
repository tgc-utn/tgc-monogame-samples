# Security patterns

`semgrep --config p/csharp` and `detect-private-key` run on every
commit and cover generic patterns automatically.

## Rules

- **Avoid** using reflection to load or execute code at runtime.
  `TGCViewerModel.LoadTreeSamples` is the only sanctioned use in this
  project; do not extend it without explicit review.
- **Never** bypass `ContentManager` to load arbitrary files at runtime.
- **Never** hardcode secrets, API keys, or credentials in source code
  or configuration files committed to the repository.
- **Always** validate file paths and configuration values at system
  boundaries before passing them to file system APIs.
- **Always** document why an unsafe block is necessary with a comment;
  prefer `Span<T>` / `Memory<T>` before reaching for `unsafe`.
- **Avoid** expanding unsafe blocks beyond what is strictly required for
  the rendering or interop path.
