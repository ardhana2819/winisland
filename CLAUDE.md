# CLAUDE.md

Instructions Claude Code loads at the start of every session in this repo.

## Project

WinIsland is a small Windows 11 desktop app for one user (Diva), inspired by the iPhone Dynamic Island. A black, pill-shaped, topmost, non-activating overlay at the top center of the screen shows a clock, the current Spotify track with controls, and WhatsApp and Discord notification previews, so they are visible in borderless games without alt-tabbing. It is a personal project in a private GitHub repo.

**Before any feature work, read `docs/PRD.md`.** It is the source of truth. Build nothing that is not in it. `docs/BACKLOG.md` is next-release ideas only and is out of scope. If a requirement is missing, ambiguous, or conflicting, stop and ask.

## Engineering standards

@docs/ENGINEERING_STANDARDS.md

## Locked technical decisions

- C# on .NET 10 (LTS) with WPF. Windows 11 only.
- Music comes only from the Windows media session API (`Windows.Media.Control`), using the Spotify desktop app. No Spotify Web API.
- Notifications come only from the Windows notification listener API (`UserNotificationListener`), with Diva's permission.
- No game injection, no hooks into other processes, no reading other apps' memory, no screen capture.
- No network calls in the app.
- Message text stays in memory only. Never write it to disk or logs.
- Releases contain only the app zip. No certificates or keys in CI or in the repo. Package identity is set up on the target PC.

## Readability rules

The code must be readable even for people who are not programmers.

- One responsibility per file. Prefer several small files over one big file.
- Every file starts with a short plain-English comment saying what it does and why it exists.
- Clear names, no clever tricks. Comments explain why, not what.
- `docs/ARCHITECTURE.md` explains the project in plain English. Update it when structure changes.

## Project layout

- `src/WinIsland.App`: WPF presentation only.
- `src/WinIsland.Core`: plain C# logic, no Windows UI or APIs. Must have unit tests.
- `src/WinIsland.Platform`: thin wrappers around Windows APIs.
- `tests/WinIsland.Core.Tests`: unit tests for Core.
- `spikes/`: throwaway Phase 1 programs. Never referenced by product code, not in the solution.
- `packaging/`: package identity manifest and scripts.

## Git workflow

1. Start from an up-to-date `main`. Create a branch: `feat/<short-name>`, `fix/<short-name>`, `chore/<short-name>`, `docs/<short-name>`, or `spike/<short-name>`.
2. Small commits with Conventional Commit messages, for example `feat: add clock state to island`.
3. Run format, build, and tests locally before pushing.
4. Push the branch and open a pull request into `main` with `gh pr create`, filling in `.github/pull_request_template.md`, including the PRD IDs (FR-x, AC-x).
5. Wait for CI and report the actual result, including failures. Fix and push again if needed.
6. Diva reviews. Merge only after she explicitly says so, with squash merge, then delete the branch.

Never commit or push directly to `main`. Never weaken or skip tests to make CI pass. The local `.githooks/pre-push` hook blocks pushes to `main`; enable it per clone with `git config core.hooksPath .githooks`.

## Commands

The repo path contains a space. Always quote it: `"D:\Claude Project\WinIsland"`.

```powershell
dotnet restore WinIsland.slnx
dotnet format WinIsland.slnx --verify-no-changes   # format check (dotnet format WinIsland.slnx to fix)
dotnet build WinIsland.slnx -c Release -warnaserror
dotnet test WinIsland.slnx -c Release
```
