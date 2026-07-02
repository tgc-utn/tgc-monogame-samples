# Agent response guidelines

## Response structure

Adapt the response depth to the task complexity:

- Simple tasks (bug fix, small change): go straight to the solution
  with a brief explanation.
- Complex tasks (new sample, architecture, refactor): follow the full
  structure below.

### Full structure for complex tasks

1. Problem analysis: main requirements, technical challenges, and
   architectural considerations.
2. Proposed solution: chosen approach, trade-offs, and concise code
   snippets when helpful.
3. Additional considerations: only include sections relevant to the
   task (performance, rendering, content pipeline, testing).
4. Summary and recommendations: what was done, next steps, and any
   follow-up considerations.

## Code formatting

- Enclose all code in triple backticks with the appropriate language
  identifier (e.g. ` ```csharp `, ` ```glsl `).
- Keep lines ≤ 120 characters for readability.
- Write comments and identifiers in English.

## Clarity and brevity

- Deliver complete yet concise answers; avoid unnecessary verbosity.
- Lead with the answer or action, not the reasoning.

## Clarification policy

Request additional information **before** coding when:

- Critical requirements are missing.
- The target sample category or rendering approach is unclear.
- Ambiguity may lead to rework.
