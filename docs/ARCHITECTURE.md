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
 | (WhatsApp, Discord)   |    |  events into      last 3,           animations,  |
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
3. **Core** decides what that means for the island: show a banner, add it to the last 3, start the 6 second timer, ignore it because the app is turned off in settings.
4. **App** draws the new state and animates the change.
5. When you click a button, App asks Core what to do, and Platform sends the command to Windows (for example "next track").

## What is in Core so far

| Folder | File | What it decides |
|---|---|---|
| `Island/` | `IslandStateMachine.cs` | What the island is doing: clock, music, expanded, hidden, paused for an hour. Clicking or hotkey 1 always opens it, also from the clock (paused song or empty view); paused music shows for 5 seconds (longer while expanded). The expanded island has two pages, music and notifications. It knows whether a notification has not been seen on the notifications page yet (the unread dot). It also says when the next time-based change is due. |
| `Island/` | `IslandView.cs` | The shapes the island can show: clock, music pill, expanded, banner, permission prompt, hotkey conflict. |
| `Island/` | `ExpandedPage.cs` | The two pages of the expanded island: music and notifications. |
| `Island/` | `PageScroll.cs` | Turns mouse wheel and touchpad scrolling into page changes, one gesture per page. |
| `Island/` | `IslandOverlay.cs` | What is open on top of the clock or music: the expanded island or a notification banner. The latest request wins. |
| `Island/` | `IslandScale.cs`, `IslandSize.cs` | How big to draw the island for the current display (width / 2560, limited to 0.85 to 1.25), times the size chosen in Settings (Small 0.85, Medium 1, Large 1.15, Extra large 1.3). |
| `Island/` | `IslandLook.cs` | The look chosen in Settings: Solid (black, default) or Glass (v1.4). |
| `Privacy/` | `PrivacyIndicator.cs` | Which dot shows while an app uses the camera or the microphone (camera wins), and when Windows' record means "in use" (v1.5). |
| `Island/` | `ClockText.cs` | The clock text: 24-hour, hours and minutes only. |
| `Hotkeys/` | `Hotkey.cs`, `HotkeyModifiers.cs` | One key combination and how it is shown, for example "Ctrl+Alt+I". |
| `Hotkeys/` | `HotkeySet.cs` | The three default hotkeys and the rules: each needs a modifier, and no two may be the same. |
| `Hotkeys/` | `HotkeyAction.cs` | What each hotkey does: expand, hide or show, hide the banner. |
| `Hotkeys/` | `HotkeyConflictMessage.cs` | The short text for the conflict banner, for example "Alt+X is used by another app". |
| `Hotkeys/` | `ExpandedShortcut.cs`, `ShortcutKey.cs` | The keys that work while the island is expanded: Space, Ctrl+Right, Ctrl+Left for music, Alt+Down and Alt+Up for the pages. |
| `Animation/` | `CubicBezier.cs` | The design's timing curves as math, including the spring curve that overshoots and settles. |
| `Tray/` | `TrayMenuLabels.cs` | The tray menu wording, which changes while paused or hidden. |
| `Music/` | `NowPlaying.cs`, `PlaybackStatus.cs` | What the music player reports: title, artist, playing or paused, position, length, whether seeking works. |
| `Music/` | `PlaybackPosition.cs` | Estimates where the song is between reports, so the progress bar moves smoothly. |
| `Music/` | `TimeLabel.cs`, `SeekPosition.cs` | Song times like "1:24", and turning a click on the progress bar into a position. |
| `Music/` | `ArtworkPalette.cs`, `ColorMath.cs`, `RgbColor.cs` | Works out the music page's colors from the artwork's pixels: a dark background, an accent for the controls, and grey text, all checked for readable contrast. |
| `Music/` | `MusicSource.cs` | Picks which app's music shows: the one playing, music players before videos, else the one Windows marks as current (v1.5: every app, not only Spotify). |
| `Music/` | `PauseHold.cs` | Keeps a paused app on the island for 3 seconds before another app may take its place (v1.5). |
| `Music/` | `AudioAppMatch.cs` | Tells whether a sound in the Windows Volume Mixer belongs to the music app, so the volume slider changes only that app (v1.5). |
| `Notifications/` | `RawNotification.cs`, `IslandNotification.cs`, `NotificationSource.cs` | A Windows notification as read, and as the island shows it: app, sender or chat, preview. Memory only. |
| `Notifications/` | `NotificationParser.cs` | Turns the Windows text lines into sender and preview, and tells WhatsApp, Discord and other apps apart. |
| `Notifications/` | `NotificationSettings.cs` | Which apps show, "Hide message text", and how long banners stay (defaults until the Settings window exists). |
| `Notifications/` | `WhatsAppWebPriority.cs` | WhatsApp Web comes first: drops desktop's "You may have new messages" and desktop notifications for chats Web covered in the last 10 minutes. |
| `Notifications/` | `NotificationFeed.cs` | The last 3 chats with their last 3 messages and a count, the waiting banners with their shared timer, the read or replaced rule for removals, and which messages are still unseen. |
| `Notifications/` | `NotificationStack.cs` | One chat on the notifications page: its last 3 messages, how many it sent, and the "3 messages" or "99+ messages" text (v1.4). |
| `Notifications/` | `IncomingCall.cs` | Recognises a ringing WhatsApp Web call from its "Incoming voice call" or "Incoming video call" text (v1.4). |
| `Notifications/` | `AppLogoStyle.cs` | How another app's logo is drawn: white on the app's own colour, worked out from the logo's pixels, or the full tile (v1.5). |
| `Settings/` | `AppSettings.cs`, `AppSettingsJson.cs` | Everything Settings can change, with defaults and rules (hotkeys, 3 to 15 s banners), and the readable file format; a damaged file falls back to defaults. |

Core never reads the clock itself: the app passes in the current time. That keeps the rules testable without waiting an hour for a pause to end.

## What is in Platform and App so far

| Part | File | What it does |
|---|---|---|
| Platform | `Windows/NonActivatingWindow.cs` | Makes the island window stay on top and take clicks without stealing focus from the game. |
| Platform | `Windows/GlassBackdrop.cs` | Puts Windows' own blur (the host backdrop brush) into a window, cut to the island's rounded shape with smooth edges (v1.4 Glass). |
| Platform | `Windows/GlassWindow.cs` | The plain Windows window that carries the blur, see-through outside it, so the island's glass edge is smooth (v1.5 polish). |
| Platform | `Windows/PrimaryDisplay.cs` | Reads the primary monitor's width. |
| Platform | `Windows/NativeMethods.cs` | The Win32 functions above, in one place. They only touch WinIsland's own windows. |
| Platform | `Tray/TrayIcon.cs`, `Tray/TrayIconImage.cs` | The tray icon (the app icon, WinIsland.ico, since v1.5; dimmed while paused) and its menu: Pause for 1 hour or Resume, Hide or Show island, Quit. |
| Platform | `Tray/DarkMenuColors.cs`, `Tray/DarkMenuRenderer.cs` | Draw the tray menu in the dark colors of the design board. |
| Platform | `Hotkeys/GlobalHotkeys.cs`, `Hotkeys/HotkeyRegistration.cs` | Registers the global hotkeys with Windows, reports which ones another app already owns, and says when one is pressed. |
| Platform | `Media/MusicMediaSession.cs`, `Media/MediaSessionReader.cs` | Connects to the music app (any app, v1.5) through the Windows media session: reads the song, artwork and progress, sends play, pause, next, previous and seek, and follows Spotify opening and closing. |
| Platform | `Media/AppVolume.cs` | Reads and sets one app's volume through the Windows audio session API, like the Volume Mixer (v1.5). |
| Platform | `Privacy/CameraMicrophoneWatcher.cs` | Tells whether an app uses the camera (Windows' own record) or the microphone (MicrophoneUse), never touching the devices (v1.5). |
| Platform | `Media/CoreAudio.cs`, `Media/MicrophoneUse.cs` | The Windows audio session API parts WinIsland uses, shared by the volume slider and the microphone dot (v1.5). |
| Platform | `Diagnostics/PlatformLog.cs` | Records Windows API problems (kind and error code only, never song or message text). |
| Platform | `Windows/PointerState.cs` | Reads the mouse position and whether a button is down (no hook), used only while the island is expanded. |
| Platform | `Notifications/WindowsNotificationListener.cs`, `Notifications/ToastReader.cs`, `Notifications/NotificationArrival.cs` | Reads Windows notifications with permission: instant with package identity, otherwise checked once per second; reports new and removed ones and the app logo. |
| Platform | `Settings/SettingsFile.cs` | Reads and safely writes `%USERPROFILE%.winislandsettings.json` (outside AppData, so package identity cannot hide it). |
| Platform | `Startup/StartupRegistration.cs` | Turns "start with Windows" on or off in the per-user Run list. |
| Platform | `Windows/DarkTitleBar.cs` | Dark title bar for the Settings window. |
| App | `App.xaml.cs` | Starts the island window and the tray icon; Quit closes both. |
| App | `Island/IslandWindow.xaml(.cs)` | The island: a clear, fixed-size, always-on-top window with the black pill and the clock. |
| App | `Island/IslandLayout.cs` | Where the window goes and how big it is on the current display, at the chosen island size. |
| App | `Island/IslandResizeAnimator.cs` | Grows or shrinks the island with the shape spring when the size changes in Settings. |
| App | `Island/GlassLook.cs`, `Island/BlurWindow.cs` | The Glass look: an empty, click-through window right behind the island carries the blur, and follows the island's shape and opacity while it moves. Falls back to Solid if Windows refuses (v1.4). |
| App | `Island/IslandLookResources.cs` | The colors inside the island that change with the look (cards, second lines, badge, peeking cards) and the soft text shadow on Glass (v1.4). |
| App | `Assets/WinIsland.ico`, `Assets/New-AppIcon.ps1` | The app icon (black tile with the white pill) for the exe, the Start menu and the Settings window, and the script that draws it from the design spec. |
| App | `Clock/MinuteTimer.cs` | Wakes the clock once per minute, and right away after a time change or sleep. |
| App | `Clock/IslandTimeTimer.cs` | Wakes the island exactly when Core says a time-based change is due: the end of a pause, or paused music going back to the clock. |
| App | `Music/MusicController.cs` | Moves Spotify reports to the UI thread, hands them to Core, decodes album art small, and forwards button presses. |
| App | `Music/MusicPillView.xaml`, `Music/EqualizerView.xaml` | The compact music pill: artwork, title, moving equalizer (green here, in the accent color on the music page), and the unread dot at the end. |
| App | `Island/ExpandedIslandView.xaml` | The expanded island, 420 x 190: slides between the music and notifications pages and draws the page dots. |
| App | `Island/PageDotsFader.cs` | Shows the page dots when the island opens or changes page, and fades them out 1.5 s later. |
| App | `Notifications/IslandDot.cs` | The small dots on the clock and the compact music pill: white for a notification not seen yet, green or orange while an app uses the camera or microphone (v1.5). |
| App | `Music/MusicControlsView.xaml` | The music page: artwork, title, artist, progress with seek, previous, play or pause, next; or the empty view when Spotify is closed. |
| App | `Music/VolumeRowView.xaml`, `Music/RowSwap.cs` | The music app's volume, in the progress row's exact place, and the soft swap between the two rows (v1.5). |
| App | `Music/ArtworkImage.cs`, `Music/ArtworkBrush.cs`, `Music/SongText.cs` | Album art (or the grey placeholder with a music note) and the title shown when a song has none. |
| App | `Music/ArtworkColors.cs` | Reads the album art at 64 x 64 and asks Core for its colors, once per song. |
| App | `Island/PillColors.cs` | The artwork color behind the open music page, with the shade that fades it to black at the bottom; black everywhere else. |
| App | `Island/IslandMusicPresenter.cs`, `Island/PointerLeaveCollapser.cs` | Keep the music views up to date, tick the progress once per second while expanded, and close the island 2 s after the pointer leaves. |
| App | `Island/TopmostGuard.cs` | Every 2 s, checks the island is still on top (style and place among on-top windows) and non-activating, and repairs it if Windows moved it (seen after WhatsApp starts); each repair is logged. |
| App | `Island/OutsideClickWatcher.cs` | While expanded, closes the island when a mouse button is pressed anywhere outside it. |
| App | `Island/ExpandedKeys.cs` | While expanded, turns on Space, Ctrl+Right and Ctrl+Left for the music and Alt+Down and Alt+Up for the pages; off again when the island closes. |
| App | `Music/MarqueeText.xaml` | A line of text that scrolls sideways when the song title does not fit. |
| App | `Notifications/NotificationController.cs` | Asks for notification access, moves new and removed notifications to the UI thread, hands them to Core, keeps app logos. |
| App | `Notifications/NotificationBannerView.xaml`, `Notifications/AppIconView.xaml` | The banner on a grey card: app logo or letter circle (centered by its shape), sender, preview, +N badge; tap opens the notifications page, a sideways drag swipes it away. |
| App | `Notifications/AppLogoGlyph.cs` | The WhatsApp and Discord logo shapes, drawn white on their green or blurple circle. |
| App | `Notifications/CallSymbol.cs` | The green call circle with a phone or a video camera, pulsing on the call banner while a call rings (v1.4). |
| App | `Notifications/AppLogoStyles.cs` | Reads each app logo's pixels once and keeps how to draw it (v1.5). |
| App | `Notifications/NotificationsPageView.xaml`, `Notifications/NotificationCard.cs` | The notifications page: the last 3 chats on grey cards with the time and message count, the full text of a single one (up to 3 lines), or "No notifications". |
| App | `Notifications/StackCard.cs` | Puts the cards of older messages behind a chat's card, peeking out underneath like the iPhone (v1.4). |
| App | `Notifications/ChatView.cs` | An opened chat: its name, count and last 3 messages with their times (v1.4). |
| App | `Notifications/ChatSlide.cs` | The sideways move between the list and an opened chat, with the page-switch spring (v1.4). |
| App | `Notifications/PermissionPromptView.xaml`, `Notifications/PermissionPromptBehavior.cs` | "Allow notification access": opens the Windows settings page, hides itself after 10 seconds. |
| App | `Island/IslandNotificationPresenter.cs` | Keeps the banner and the notifications page up to date, with "Hide message text" applied. |
| App | `Island/IslandHotkeys.cs` | The three island hotkeys: applied at start and from Settings, freed while Settings records a new combination. |
| App | `Settings/SettingsController.cs` | Loads, checks, applies and saves settings; opens the Settings window. |
| App | `Settings/SettingsWindow.xaml`, `Settings/HotkeyCapture.cs` | The Settings window from the design board, and turning a key press into a hotkey. |
| App | `Diagnostics/ErrorLogFile.cs`, `Diagnostics/CrashGuard.cs` | The error log (`%USERPROFILE%.winislandwinisland.log`, 256 KB, errors and event types only) and catching unexpected errors so the island keeps running. |
| App | `SingleInstance.cs` | Makes sure only one WinIsland runs; a second copy closes at once. |
| App | `Island/PillShape.cs` | The island's size and corner radius for each view, from the design spec. |
| App | `Island/PillAnimator.cs`, `Island/PillGeometry.cs` | The motion from the design: shape change, content swap with blur, hide and show. |
| App | `Animation/CubicBezierEase.cs`, `Animation/Motion.cs`, `Animation/ColorFade.cs` | Use the design's curves in WPF, fade colors, and skip all motion when Windows animation effects are off. |

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
| `packaging/` | Start menu shortcut script and the optional package identity scripts. Shipped inside the release zip. See `packaging/README.md`. |
| `docs/` | PRD, backlog, engineering standards, this file, spike results, performance results. |
| `.github/` | CI and release workflows, pull request template. |
| `.githooks/` | Local Git hook that blocks pushes to `main`. |

## Shared build settings

`Directory.Build.props` applies the same rules to every project: nullable checks on, warnings treated as errors, and code style enforced on build. `global.json` pins the .NET 10 SDK. `.editorconfig` holds the formatting and style rules that `dotnet format` checks.
