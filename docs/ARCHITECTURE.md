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

## What is in Core so far

| Folder | File | What it decides |
|---|---|---|
| `Island/` | `IslandStateMachine.cs` | What the island is doing: clock, music, expanded, hidden, paused for an hour. Expanding only works while music is present; paused music shows for 5 seconds (longer while expanded). It also says when the next time-based change is due. |
| `Island/` | `IslandView.cs` | The shapes the island can show (compact, expanded; more arrive with music and notifications). |
| `Island/` | `IslandOverlay.cs` | What is open on top of the clock or music: the expanded island, a notification banner, or the list. The latest request wins. |
| `Island/` | `IslandScale.cs` | How big to draw the island for the current display (width / 2560, limited to 0.85 to 1.25). |
| `Island/` | `ClockText.cs` | The clock text: 24-hour, hours and minutes only. |
| `Hotkeys/` | `Hotkey.cs`, `HotkeyModifiers.cs` | One key combination and how it is shown, for example "Ctrl+Alt+I". |
| `Hotkeys/` | `HotkeySet.cs` | The three default hotkeys and the rules: each needs a modifier, and no two may be the same. |
| `Hotkeys/` | `HotkeyAction.cs` | What each hotkey does: expand, hide or show, hide the banner. |
| `Hotkeys/` | `HotkeyConflictMessage.cs` | The short text for the conflict banner, for example "Alt+X is used by another app". |
| `Hotkeys/` | `MusicShortcut.cs`, `MusicKey.cs` | The music keys that work while the island is expanded: Space, Ctrl+Right, Ctrl+Left. |
| `Animation/` | `CubicBezier.cs` | The design's timing curves as math, including the spring curve that overshoots and settles. |
| `Tray/` | `TrayMenuLabels.cs` | The tray menu wording, which changes while paused or hidden. |
| `Music/` | `NowPlaying.cs`, `PlaybackStatus.cs` | What the music player reports: title, artist, playing or paused, position, length, whether seeking works. |
| `Music/` | `PlaybackPosition.cs` | Estimates where the song is between reports, so the progress bar moves smoothly. |
| `Music/` | `TimeLabel.cs`, `SeekPosition.cs` | Song times like "1:24", and turning a click on the progress bar into a position. |
| `Music/` | `MusicSource.cs` | Picks the Spotify media session and ignores browsers and other players. |
| `Notifications/` | `RawNotification.cs`, `IslandNotification.cs`, `NotificationSource.cs` | A Windows notification as read, and as the island shows it: app, sender or chat, preview. Memory only. |
| `Notifications/` | `NotificationParser.cs` | Turns the Windows text lines into sender and preview, and tells WhatsApp, Discord and other apps apart. |
| `Notifications/` | `NotificationSettings.cs` | Which apps show, "Hide message text", and how long banners stay (defaults until the Settings window exists). |
| `Notifications/` | `NotificationFeed.cs` | The last 3 list, the waiting banners with their shared timer, and the read or replaced rule for removals. |

Core never reads the clock itself: the app passes in the current time. That keeps the rules testable without waiting an hour for a pause to end.

## What is in Platform and App so far

| Part | File | What it does |
|---|---|---|
| Platform | `Windows/NonActivatingWindow.cs` | Makes the island window stay on top and take clicks without stealing focus from the game. |
| Platform | `Windows/PrimaryDisplay.cs` | Reads the primary monitor's width. |
| Platform | `Windows/NativeMethods.cs` | The Win32 functions above, in one place. They only touch WinIsland's own windows. |
| Platform | `Tray/TrayIcon.cs`, `Tray/TrayIconImage.cs` | The tray icon (a white pill drawn in code, dimmed while paused) and its menu: Pause for 1 hour or Resume, Hide or Show island, Quit. |
| Platform | `Tray/DarkMenuColors.cs`, `Tray/DarkMenuRenderer.cs` | Draw the tray menu in the dark colors of the design board. |
| Platform | `Hotkeys/GlobalHotkeys.cs`, `Hotkeys/HotkeyRegistration.cs` | Registers the global hotkeys with Windows, reports which ones another app already owns, and says when one is pressed. |
| Platform | `Media/SpotifyMediaSession.cs`, `Media/MediaSessionReader.cs` | Connects to Spotify through the Windows media session: reads the song, artwork and progress, sends play, pause, next, previous and seek, and follows Spotify opening and closing. |
| Platform | `Diagnostics/PlatformLog.cs` | Records Windows API problems (kind and error code only, never song or message text). |
| Platform | `Windows/PointerState.cs` | Reads the mouse position and whether a button is down (no hook), used only while the island is expanded. |
| App | `App.xaml.cs` | Starts the island window and the tray icon; Quit closes both. |
| App | `Island/IslandWindow.xaml(.cs)` | The island: a clear, fixed-size, always-on-top window with the black pill and the clock. |
| App | `Island/IslandLayout.cs` | Where the window goes and how big it is on the current display. |
| App | `Clock/MinuteTimer.cs` | Wakes the clock once per minute, and right away after a time change or sleep. |
| App | `Clock/IslandTimeTimer.cs` | Wakes the island exactly when Core says a time-based change is due: the end of a pause, or paused music going back to the clock. |
| App | `Music/MusicController.cs` | Moves Spotify reports to the UI thread, hands them to Core, decodes album art small, and forwards button presses. |
| App | `Music/MusicPillView.xaml`, `Music/EqualizerView.xaml` | The compact music pill: artwork, title, moving equalizer. |
| App | `Music/MusicControlsView.xaml` | The expanded island: artwork, title, artist, progress with seek, previous, play or pause, next. |
| App | `Music/ArtworkImage.cs`, `Music/ArtworkBrush.cs`, `Music/SongText.cs` | Album art (or the design gradient) and the title shown when a song has none. |
| App | `Island/IslandMusicPresenter.cs`, `Island/PointerLeaveCollapser.cs` | Keep the music views up to date, tick the progress once per second while expanded, and close the island 2 s after the pointer leaves. |
| App | `Island/OutsideClickWatcher.cs` | While expanded, closes the island when a mouse button is pressed anywhere outside it. |
| App | `Island/ExpandedMusicKeys.cs` | While expanded, turns on Space, Ctrl+Right and Ctrl+Left for play or pause, next and previous; off again when the island closes. |
| App | `Music/MarqueeText.xaml` | A line of text that scrolls sideways when the song title does not fit. |
| App | `SingleInstance.cs` | Makes sure only one WinIsland runs; a second copy closes at once. |
| App | `Island/PillShape.cs` | The island's size and corner radius for each view, from the design spec. |
| App | `Island/PillAnimator.cs`, `Island/PillGeometry.cs` | The motion from the design: shape change, content swap with blur, hide and show. |
| App | `Animation/CubicBezierEase.cs`, `Animation/Motion.cs` | Use the design's curves in WPF, and skip all motion when Windows animation effects are off. |

The island window never changes size. It is big enough for the largest island shape, and the clear parts let clicks through to the game. Resizing a transparent window during an animation would be slow.

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
