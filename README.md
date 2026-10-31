# WinIsland

A small Windows 11 overlay inspired by the iPhone Dynamic Island. A black pill at the top center of the screen shows the time, what Spotify is playing, and WhatsApp and Discord messages, so you can see them in a game without alt-tabbing.

## Status

**Phase 2: overlay shell, in progress.** Phase 1 (technical spike) is done; results are in [docs/spike-results.md](docs/spike-results.md). The app currently shows the island with a clock at the top of the screen, a tray icon (Pause for 1 hour or Resume, Hide or Show island, Quit; dimmed while paused), and the hotkeys Ctrl+Alt+H (hide or show) and Ctrl+Alt+I (expand, which needs music, so it does nothing yet). Music, notifications and settings are not built yet. The plan and requirements are in [docs/PRD.md](docs/PRD.md).

Personal project, private repository.

## Requirements

- Windows 11 (64-bit).
- Games in **windowed or borderless** mode. Exclusive fullscreen is not supported.
- Spotify desktop app for music.
- WhatsApp desktop (from the Microsoft Store) and Discord desktop for notifications.

## Install on another PC

These steps apply once the first release exists.

1. Sign in to GitHub in your browser (the repository is private).
2. Open the repository's **Releases** page and download `WinIsland-vX.Y.Z-win-x64.zip`.
   Or, with GitHub CLI: `gh release download --repo ardhana2819/WinIsland --pattern "*.zip"`
3. Unzip it to a folder you keep, for example `%LOCALAPPDATA%\Programs\WinIsland`.
   No .NET install is needed. The build is self-contained.
4. **Package identity (optional).** Spike S1 showed WinIsland can read notifications without it (checking once per second). With it, notifications arrive instantly. The optional setup (a personal signing certificate, trusted once as administrator) will be described here when notifications are built in Phase 4; the scripts are in [packaging/README.md](packaging/README.md).
5. Run `WinIsland.App.exe`.

## First run

1. **Allow notification access.** WinIsland asks once. You can change it later in Windows Settings > Privacy & security > Notifications.
2. **Turn off Windows banners for WhatsApp and Discord**, so you do not see each message twice. Keep the notification center on, because that is where WinIsland reads them from.
   Windows Settings > System > Notifications > WhatsApp (and then Discord) > turn off **Show notification banners**, keep **Show notifications in notification center** on.
   Do this in **Windows Settings only**. Keep notifications turned on inside WhatsApp and Discord themselves (in WhatsApp: Settings > Notifications > Show notification banner = Always). If an app's own setting is off, it never sends the notification to Windows, so WinIsland cannot see it either.
   Alternative: Windows **Do not disturb** also hides the banners while WinIsland still receives the notifications.

## Build from source

You need git, the .NET 10 SDK, and (for the identity package) the Windows SDK.

```powershell
git clone https://github.com/ardhana2819/WinIsland.git
cd WinIsland
git config core.hooksPath .githooks   # one-time setup, see "Branch workflow"
dotnet restore WinIsland.slnx
dotnet format WinIsland.slnx --verify-no-changes
dotnet build WinIsland.slnx -c Release -warnaserror
dotnet test WinIsland.slnx -c Release
```

## Folder overview

| Folder | What it holds |
|---|---|
| `src/WinIsland.App` | The WPF app: windows, views, animations. Presentation only. |
| `src/WinIsland.Core` | Plain C# logic: island states, last 3 notifications, timers, settings. |
| `src/WinIsland.Platform` | Thin wrappers around Windows APIs: media, notifications, hotkeys, tray, startup. |
| `tests/WinIsland.Core.Tests` | Unit tests for Core. |
| `spikes/` | Throwaway Phase 1 test programs. Never used by the app. |
| `packaging/` | Package identity manifest and scripts. |
| `docs/` | PRD, backlog, engineering standards, architecture, spike results. |

More detail: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Branch workflow

Nothing is pushed straight to `main`. Every change goes through a branch and a pull request.

1. Start from an up-to-date `main` and create a branch: `feat/`, `fix/`, `chore/`, `docs/`, or `spike/` plus a short name.
2. Make small commits with clear messages, for example `feat: add clock state to island`.
3. Run format, build, and tests locally.
4. Push the branch and open a pull request into `main`. CI runs format, build, and tests.
5. After review and approval, squash merge and delete the branch.
6. Pushing a tag like `v1.0.0` builds a release automatically.

**One-time setup per clone:** run `git config core.hooksPath .githooks`. This turns on a local hook that refuses pushes to `main`. It is a safety net on your own PC only. GitHub does not enforce branch protection on free private repositories, and the hook can be skipped with `git push --no-verify`.

## Privacy

- WinIsland makes **no network calls**.
- Music information comes only from the Windows media session. WinIsland never signs in to Spotify.
- Notifications are read only through the Windows notification listener, and only after you allow it.
- Message text stays **in memory only**. It is never written to disk or to logs. Logs hold event types and errors only.
- No screen capture, no reading other programs' memory, no injection or hooks into games.
