# WinIsland Engineering Standards

Save as `docs/ENGINEERING_STANDARDS.md` in `D:\Claude Project\WinIsland`. Claude Code reads it in every session through `CLAUDE.md`. Owner: Diva. Last updated 31 Oct 2026.

These rules apply to every change in this repo, by Claude Code or by a person.

## 1. How to work

- Understand before coding: read the request, the relevant code, and `docs/PRD.md`. Do not assume a file, function, setting, or API exists until you have checked.
- If an important requirement is unclear or conflicting, ask a focused question. For small gaps, state your assumption and continue.
- Before changing code, give a short plan: goal, files involved, approach, risks, how it will be tested. For routine low-risk changes, continue after the plan unless Diva asked to review first.
- Build the smallest complete solution that meets the requirement. Work in small steps and check each one before building on it.
- If you find a problem that changes the plan, stop, explain, and revise the plan before going further.

## 2. Code quality

- The code must be readable by people who are not programmers. One responsibility per file. Prefer several small files over one big file.
- Start every file with a short plain-English comment: what it does and why it exists.
- Use clear names for files, classes, functions, and variables. No clever tricks. Comments explain why, not what.
- Small, focused functions with clear inputs and outputs. No duplicated logic, no deep nesting, no needless abstraction.
- Use SOLID or design patterns only when they clearly help. Do not over-engineer.
- Validate input and handle null, empty, and unexpected values. Return useful error messages. Never swallow exceptions silently.
- No placeholder code, fake success responses, dead code, or unexplained magic values. Use explicit types.
- Settings and environment-specific values go in configuration, not in code. Never put secrets, tokens, passwords, or private keys in code, docs, or git.

## 3. Keep changes small

- Change only what the task needs. No unrelated refactors or renames.
- Keep existing behavior and public interfaces unless the task requires a change.
- No new dependency without a clear reason, which you explain first.
- Never silently remove a feature, validation, error handling, or test. Never overwrite Diva's uncommitted work.
- If a bigger refactor would help, explain why and keep it separate from the required change.

## 4. Testing

- Test the normal case, failure cases, and edge cases. Add or update automated tests when the change is testable. Logic in `WinIsland.Core` must have unit tests.
- Before saying a task is done, run format check, build, and tests, and read the results.
- Never weaken, skip, or edit a test just to make it pass. If a check cannot run, say so and say why.
- UI work also covers: loading, empty, error, and success states; keyboard use; readable contrast; and the auto-scale rule for 24 to 32 inch monitors.

## 5. Honest reporting

- Never say something works, passed, or was verified unless it was actually run. Separate what you ran from what you did not run. Report failures as they are.
- Games and real Windows behavior can only be tested by Diva on her PC. Give her exact steps and record her results. Do not guess them.
- End every task with: Summary, Files Changed, Verification, Risks or Limitations, Next Steps.

## 6. Git and repo workflow

- Local folder: `D:\Claude Project\WinIsland`. Always quote paths, because they contain a space.
- Never commit or push directly to `main`, except the first bootstrap commit. Work happens on a branch, named `feat/`, `fix/`, `chore/`, `docs/`, or `spike/` plus a short name.
- Small commits with Conventional Commit messages, for example `feat: add clock state to island`.
- Run format, build, and tests locally. Then push the branch and open a pull request into `main` using the template.
- Wait for CI and report the real result. Diva reviews. Merge only after she says so, using squash merge, then delete the branch.
- A tag like `v1.0.0` triggers the release build on GitHub.
- Keep `.gitignore` current. Never commit build output, local settings, certificates, or private keys.

## 7. Documentation

- `README.md` stays tidy and current: what the app is, how to install it on another PC, how to build it, folder overview, workflow, and privacy.
- `docs/ARCHITECTURE.md` explains the project in plain English. Update it when structure changes.
- Update docs and config examples in the same pull request as the change. Do not create documents that add no value.
- Record important decisions and trade-offs briefly.

## 8. Product discipline

- `docs/PRD.md` is the source of truth. Do not build anything that is not in it.
- `docs/BACKLOG.md` is next-release ideas only. Do not build from it without an approved PRD for that release.
- Name the PRD requirement IDs (FR-x, AC-x) in each pull request.
- If a requirement changes, say what changed and what it affects (scope, design, tests), and get Diva's approval before building.
- UI must match the approved design. Do not change the visual direction without explaining why. Check that the result matches the design, not only that it compiles.

## 9. WinIsland privacy and safety rules

- Music comes only from the Windows media session API. No Spotify Web API.
- Notifications come only from the Windows notification listener API, with Diva's permission.
- No game injection, no hooks into other processes, no reading other programs' memory, no screen capture.
- No network calls in the app.
- Message text stays in memory only. Never write it to disk or logs. Logs hold event types and errors only.

## 10. Language and style

- Code, comments, file names, docs, commit messages, and UI text are in English.
- Chat replies to Diva can be in Indonesian. Ask her if she wants to change that.
- Write like a person: clear, direct, professional but conversational. No em dashes and no buzzwords.
- Diva wants you to double-check, stay skeptical, and research. She is not always right and neither are you. If something she asks looks wrong, say so and explain why.
