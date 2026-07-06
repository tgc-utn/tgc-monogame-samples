# Git guidelines

## Rule

This project uses
[Conventional Commits v1.0.0](https://www.conventionalcommits.org/en/v1.0.0/)
for commit messages.

## Format

```text
<type>: <description>
```

## Types

- `feat`: new sample or feature.
- `fix`: bug fix.
- `docs`: documentation changes.
- `style`: code style changes (formatting, no logic changes).
- `refactor`: code refactoring (no feature or bug fix).
- `perf`: performance improvements.
- `test`: adding or updating tests.
- `chore`: maintenance tasks (dependencies, build, content pipeline, etc.).
- `ci`: CI/CD changes.

## Branching model

This project follows Gitflow:

- `main`: production-ready code only. **Never** commit directly.
- `develop`: integration branch. All feature branches merge here.
- Feature branches are always created from `develop` and merged back into `develop`.

## Branch naming

- `feature/<kebab-name>`: new samples or features.
- `fix/<kebab-name>`: bug fixes, hotfixes, and patches.
- `enhancement/<kebab-name>`: improvements to existing samples or systems.
- `chore/<kebab-name>`: maintenance tasks (dependencies, build, etc.).
- Name should be short, lowercase, and hyphen-separated.

## Best practices

- Keep subject line under 50 characters.
- Use imperative mood ("add" not "added" or "adds").
- Do not end subject line with a period.
- If a related issue exists, reference it in the footer (e.g., "Refs: #123").
- Do not add Co-Authored-By lines.

## Verification

- Verify the project builds and all tests pass before committing.
- **Never** bypass pre-commit hooks with `--no-verify`.
- Run pre-commit checks locally before pushing: `pre-commit run`.
