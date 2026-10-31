# WinIsland Architecture

This page explains how WinIsland is put together, in plain English. Update it whenever the folder structure or the flow of data changes.

## The big picture

WinIsland is one small Windows program. It listens to two things Windows already provides, music and notifications, decides what the island should show, and draws the island on screen.

The code is split into three parts so that each part has one job:

| Part | Folder | Its one job | Knows about Windows? |
|---|---|---|---|
| App | `src/WinIsland.App` | Draws the island and the settings window, plays animations, passes your clicks on. | Yes, it is the WPF user interface. |
| Core | `src/WinIsland.Core` | Decides what the island should show: the island state machine, the last 3 notifications, timers, and the settings model. | No. Plain C# only, so it is easy to test. |
| Platform | `src/WinIsland.Platform` | Talks to Windows: media session, notification listener, global hotkeys, tray icon, launch at startup. | Yes, but only thin wrappers. |

## How data flows

```
   Windows                      WinIsland
 +-----------------------+    +-------------------------------------------------+
 | Media session         |    |                                                 |
 | (Spotify desktop)     |--->|  Platform         Core              App         |
 |                       |    |  (wrappers)  -->  (decides)   -->   (draws)     |
 | Notification listener |--->|  turns Windows    island state,     pill window, |
 | (WhatsApp, Discord)   |    |  events into      last 3 list,      animations,  |
 |                       |    |  plain C# data    timers            tray, settings
 | Hotkeys, tray         |--->|                                                 |
 +-----------------------+    |        ^                               |        |
                              |        |   your clicks and hotkeys     |        |
                              |        +-------------------------------+        |
                              |   (play/pause, expand, hide go back to Platform)|
                              +-------------------------------------------------+
```

In words:

1. Something happens in Windows: a song changes, a WhatsApp message arrives, you press a hotkey.
2. **Platform** picks it up and turns it into simple C# data, for example "new notification from Discord, sender X, text Y".
3. **Core** decides what that means for the island: show a banner, add it to the list, start the 6 second timer, ignore it because the app is turned off in settings.
4. **App** draws the new state and animates the change.
5. When you click a button, App asks Core what to do, and Platform sends the command to Windows (for example "next track").

## Rules that keep it this way

- **Core depends on nothing.** It does not reference App, Platform, WPF, or Windows APIs. That keeps the logic testable with plain unit tests in `tests/WinIsland.Core.Tests`.
- **Platform depends only on Core.** It translates Windows into Core's language and back.
- **App depends on Core and Platform.** It contains presentation only, no decisions.
- **Spikes stay separate.** `spikes/` holds throwaway Phase 1 test programs. Product code never references them, and they are not in the solution.
- **Message text stays in memory.** No part of the app writes message content to disk or logs.
- **No network.** No part of the app makes network calls.

## Other folders

| Folder | What it holds |
|---|---|
| `tests/WinIsland.Core.Tests` | Unit tests for Core. |
| `spikes/` | Throwaway Phase 1 test programs. See `spikes/README.md`. |
| `packaging/` | Package identity manifest and scripts. See `packaging/README.md`. |
| `docs/` | PRD, backlog, engineering standards, this file, spike results. |
| `.github/` | CI and release workflows, pull request template. |
| `.githooks/` | Local Git hook that blocks pushes to `main`. |

## Shared build settings

`Directory.Build.props` applies the same rules to every project: nullable checks on, warnings treated as errors, and code style enforced on build. `global.json` pins the .NET 10 SDK. `.editorconfig` holds the formatting and style rules that `dotnet format` checks.
