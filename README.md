# WinIsland

A small Windows 11 overlay inspired by the iPhone Dynamic Island. A black pill at the top center of the screen shows the time, what Spotify is playing, and WhatsApp and Discord messages, so you can see them in a game without alt-tabbing.

## Status

**Beta (v1.2.0-beta.1).** WinIsland is ready to use, but still in development: more features are planned. All PRD v1.0, v1.1 and v1.2 features are built; see [docs/PRD.md](docs/PRD.md). Phase 1 (technical spike) is done; results are in [docs/spike-results.md](docs/spike-results.md). The app currently shows the island with a clock at the top of the screen, a tray icon (Pause for 1 hour or Resume, Hide or Show island, Settings, Quit; dimmed while paused), the hotkeys Ctrl+Alt+H (hide or show) and Ctrl+Alt+I (expand), the current Spotify song with play, pause, next, previous and seek, colored from the song's artwork, also when paused or with an empty view when Spotify is closed, WhatsApp and Discord notifications (banner, +N, the last 3 on cards, Alt+X to hide), an expanded island with a music page and a notifications page (scroll, Alt+Up and Alt+Down, or the page dots), and a Settings window (tray > Settings) that also turns on start with Windows. The plan and requirements are in [docs/PRD.md](docs/PRD.md).

Personal project, private repository.

## Requirements

- Windows 11 (64-bit).
- Games in **windowed or borderless** mode. Exclusive fullscreen is not supported.
- Spotify desktop app for music.
- For notifications: WhatsApp Web installed as an app from Chrome (Chrome menu > Cast, save and share > Install page as app), or WhatsApp desktop from the Microsoft Store, and Discord desktop. With both WhatsApp apps open, WinIsland shows each message once, from WhatsApp Web.

## Install on another PC

1. Sign in to GitHub in your browser (the repository is private).
2. Open the repository's **Releases** page and download `WinIsland-<version>-win-x64.zip` (about 83 MB) from the newest release. Beta releases are marked **Pre-release**.
   Or, with GitHub CLI, naming the version: `gh release download v1.2.0-beta.1 --repo ardhana2819/WinIsland --pattern "*.zip"`
   (without a version, GitHub CLI only looks at releases that are not pre-releases).
3. Right-click the zip > **Properties** > tick **Unblock** > OK. Windows marks downloaded files, and without this the setup scripts in step 5 are refused.
4. Unzip it to a folder you keep, for example `%LOCALAPPDATA%\Programs\WinIsland`.
   No .NET install is needed. The build is self-contained.
5. **Add WinIsland to the Start menu:** in that folder, open the `packaging` folder, right-click `Add-StartMenuShortcut.ps1` > **Run with PowerShell**. To pin it: Start > All, right-click WinIsland > Pin to Start.
6. Start **WinIsland** from the Start menu. It starts with Windows from then on (switch it off in Settings).
7. **Optional: instant notifications.** Without this step WinIsland checks notifications once per second; with it they arrive in about a tenth of a second. It gives WinIsland "package identity" with a personal signing certificate, trusted once as administrator. Steps, including for another PC: [packaging/README.md](packaging/README.md). If you move the WinIsland folder later, run the setup again.

## First run

1. **Allow notification access.** WinIsland asks once. You can change it later in Windows Settings > Privacy & security > Notifications.
2. **Turn off Windows banners for WhatsApp and Discord**, so you do not see each message twice. Keep the notification center on, because that is where WinIsland reads them from.
   Windows Settings > System > Notifications > WhatsApp (and then Discord) > turn off **Show notification banners**, keep **Show notifications in notification center** on.
   Do this in **Windows Settings only**. Keep notifications turned on inside WhatsApp and Discord themselves (in WhatsApp: Settings > Notifications > Show notification banner = Always). If an app's own setting is off, it never sends the notification to Windows, so WinIsland cannot see it either.
   Alternative: Windows **Do not disturb** also hides the banners while WinIsland still receives the notifications.

## Uninstall

WinIsland has no installer, so there is no entry in Windows Settings > Apps. Deleting the folder removes the program, but a few small things outside the folder stay behind. Do the steps in this order:

1. **Turn off start with Windows:** tray icon > Settings > switch off start with Windows > Save. This removes WinIsland from the startup list. If you skip it, Windows keeps a startup entry that points at a missing file. It does no harm, but it stays in Settings > Apps > Startup.
2. **Quit WinIsland:** tray icon > Quit.
3. **Remove the Start menu shortcut:** open PowerShell in the WinIsland folder's `packaging` folder (in File Explorer: right-click the folder > Open in Terminal) and run:
   `.\Add-StartMenuShortcut.ps1 -Remove`
4. **Only if you set up instant notifications (step 7 of the install):** in the same folder, in a normal PowerShell, run:
   `.\Unregister-IdentityPackage.ps1 -PackageName WinIsland`
5. **Delete the WinIsland folder**, for example `%LOCALAPPDATA%\Programs\WinIsland`.
6. **Delete the settings and log folder** `%USERPROFILE%\.winisland` (it holds `settings.json` and `winisland.log`, never message text).
7. **Optional, only if you trusted the signing certificate:** in PowerShell opened as administrator, run:
   `Get-ChildItem Cert:\LocalMachine\TrustedPeople | Where-Object Subject -eq "CN=WinIsland Dev" | Remove-Item`
   If you created the certificate on that PC (packaging option B), also remove its private key, in a normal PowerShell:
   `Get-ChildItem Cert:\CurrentUser\My | Where-Object Subject -eq "CN=WinIsland Dev" | Remove-Item`
   Do not do this on the PC you build WinIsland on if you still want to sign new packages there.

If you already deleted the folder first, the startup entry can be removed in PowerShell:
`Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name WinIsland`
and the shortcut by deleting `WinIsland` from `%APPDATA%\Microsoft\Windows\Start Menu\Programs`. An identity package can still be removed with `Get-AppxPackage WinIsland | Remove-AppxPackage`.

Windows may still list WinIsland under Settings > Privacy & security > Notifications. That entry is only the permission you gave and does nothing without the app.

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
| `docs/` | PRD, backlog, engineering standards, architecture, spike results, performance results. |

More detail: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Branch workflow

Nothing is pushed straight to `main`. Every change goes through a branch and a pull request.

1. Start from an up-to-date `main` and create a branch: `feat/`, `fix/`, `chore/`, `docs/`, or `spike/` plus a short name.
2. Make small commits with clear messages, for example `feat: add clock state to island`.
3. Run format, build, and tests locally.
4. Push the branch and open a pull request into `main`. CI runs format, build, and tests.
5. After review and approval, squash merge and delete the branch.
6. Pushing a tag like `v1.0.0-beta.1` builds a release automatically. A tag with `-beta` becomes a pre-release.

**One-time setup per clone:** run `git config core.hooksPath .githooks`. This turns on a local hook that refuses pushes to `main`. It is a safety net on your own PC only. GitHub does not enforce branch protection on free private repositories, and the hook can be skipped with `git push --no-verify`.

## Privacy

- WinIsland makes **no network calls**.
- Music information comes only from the Windows media session. WinIsland never signs in to Spotify.
- Notifications are read only through the Windows notification listener, and only after you allow it.
- Message text stays **in memory only**. It is never written to disk or to logs. Logs hold event types and errors only.
- Settings are saved in `%USERPROFILE%\.winisland\settings.json`: hotkeys, which apps show, banner time, hide message text, start with Windows. Next to it, `winisland.log` (at most 256 KB) records event types and errors only, never message text. Nothing else is written to disk.
- No screen capture, no reading other programs' memory, no injection or hooks into games.
