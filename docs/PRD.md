# WinIsland PRD (v1.0)

Status: MVP v1.0 approved by you on 30 Oct 2026, after reviewing the prototype. Open item: WhatsApp desktop notification behavior (U7), tested in the spike.

Labels used below: **[Confirmed]** you decided it. **[Verified]** I checked it against Microsoft docs or another source this session. **[Proposed]** my recommendation, needs your yes. **[Unverified]** I believe it but it must be tested before we rely on it.

## 1. Overview and problem

WinIsland is a small Windows 11 app that puts a pill-shaped overlay at the top center of the screen, modeled on the iPhone Dynamic Island. It shows a clock, current music, and incoming WhatsApp and Discord notifications, so you can see them in a game without alt-tabbing.

The problem: in a game, music and message alerts either do not show at all (Windows suppresses banners in fullscreen) or pull you out of the game.

## 2. Goals and success criteria

- G1. See what is playing and control it without leaving the game. Success: play/pause/next/previous work from the island in windowed and borderless games.
- G2. Know a WhatsApp or Discord message arrived without leaving the game. Success: a preview shows in the island within about 2 seconds of the Windows notification.
- G3. Never get in the way. Success: the island does not take keyboard focus, and you can hide it instantly.
- G4. Stay light. Proposed target: under 100 MB RAM and near-zero CPU when idle. [Proposed] RAM is measured as working set. [Confirmed 31 Oct 2026] The spike's bare test window already used about 101 MB working set, so the real number is measured in Phase 2 and the target is decided then. [Confirmed 31 Oct 2026]

## 3. Users

One user: you, on one Windows 11 PC with one 27-inch monitor (the design should also suit 24 to 32 inch), playing games in windowed or borderless mode. [Confirmed] Personal use only, private GitHub repo. [Confirmed] No public release, so no installer polish, code signing, or Spotify extended-quota worries for now.

## 4. Scope

### In scope for MVP

- Always-visible black pill, top center, fixed position and fixed default size. [Confirmed]
- Idle state shows a clock. [Confirmed]
- Music: title, artist, artwork, progress, play/pause, next, previous, via Windows media controls (SMTC), from the Spotify desktop app logged in with your own Premium account. Seek is a stretch goal, see risks. [Confirmed approach]
- Notifications from WhatsApp desktop (to be installed, see U7), Discord desktop, and a general Windows toast path. [Confirmed]
- Global hotkey to expand the island and make it clickable without taking keyboard focus. [Confirmed]
- Second hotkey to hide and show the island without quitting. [Confirmed]
- Tray icon with Pause for 1 hour, Settings, Quit. Launch with Windows, minimized to tray. [Confirmed]
- Settings: hotkeys, notification duration, per-app notification on/off, hide message content, launch at startup. [Confirmed list]

### Out of scope for MVP

- Exclusive fullscreen and any game injection or hooking. [Confirmed]
- Replying to messages from the island. [Confirmed]
- Spotify Web API, playlists, library browsing. Decided: use the Spotify desktop app only, no Web API.
- Light theme, movable position, size setting, multi-monitor, animation tuning. Also next release (see the backlog doc): shuffle and repeat buttons, glass or blur look, grouping notifications by category, opening the source app from a notification, and showing recent notifications when idle or together with the music controls.
- Notification sources beyond WhatsApp, Discord, and generic Windows toasts.
- Opening WhatsApp or Discord when you tap a notification. Planned for the next release, see the backlog doc.

## 5. User stories

1. As a player, I see a clock in the pill when nothing else is happening, so I can check the time in a game.
2. As a player, I see the song title and a small animation when music plays, so I know what is on.
3. As a player, I press the hotkey or click the island, it expands, and I can click play/pause/next/previous without my game losing keyboard focus.
4. As a player, a WhatsApp or Discord message shows sender and a short preview, then goes away on its own or when I read it in the app.
5. As a player, when messages arrive I see the newest with a +N badge, and I can tap it to read the last 3 (or the full text of a single message).
6. As a player, I press the second hotkey to hide the island when it distracts me, and press it again to bring it back.
7. As a user, I choose which apps may show previews, and I can hide message text entirely.
8. As a user, the app starts with Windows and stays out of my way in the tray.

## 6. Functional requirements

**Overlay**

- FR-1. Topmost, borderless, transparent window, top center of the primary monitor, fixed default size. The size follows the display automatically so it suits 24 to 32 inch monitors: per-monitor DPI aware, with a scale of display width in DIPs divided by 2560, limited to 0.85 to 1.25. No manual size setting. [Confirmed]
- FR-2. Three visual states: compact (clock or music), expanded (music controls), notification (preview). Expanding: click the compact island with the mouse, or press hotkey 1. A click only expands. Play/pause, next and previous work only once it is expanded. With nothing playing, neither the click nor the hotkey does anything. [Confirmed] Clicking the open island outside its buttons closes it immediately, and it also closes about 2 seconds after the pointer leaves. [Confirmed] There is no Escape key to close it: the island never has keyboard focus (FR-3), so keys go to the game. [Confirmed 31 Oct 2026] If a notification arrives while the island is expanded, the banner replaces the expanded view. [Confirmed 31 Oct 2026, follows the prototype]
- FR-3. Window must not take keyboard focus when shown or clicked. [Proposed technique: non-activating window style. Unverified in games.]
- FR-4. Smooth morph animation between states, spring-like in the style of the iPhone Dynamic Island. [Confirmed]
- FR-5. Hotkey 1 (Ctrl+Alt+I) toggles expanded state. Hotkey 2 (Ctrl+Alt+H) hides or shows the island. [Confirmed as defaults] Hotkey 3 (proposed Alt+X, a simple two-key combo) hides the notification banner. It does not delete notifications. [Proposed] Windows hands a global hotkey to this app only, so a common combo would stop working in other programs. The app reports a conflict and the key stays configurable. Both configurable. App reports a clear error if a hotkey is already taken: a conflict banner shows for 10 seconds at start, and the same error shows in Settings (see `docs/design/DESIGN_SPEC.md`). [Confirmed 31 Oct 2026]
- FR-6. Tray menu: Pause for 1 hour (hides the island and stops showing notifications, then returns automatically), Hide island (Ctrl+Alt+H, the same as hotkey 2; reads Show island while hidden), Settings, Quit. [Hide island item confirmed 31 Oct 2026, from the design boards] While paused, the tray icon switches to a dimmed variant so the paused state is visible. [Confirmed 31 Oct 2026] While paused, the Pause item becomes **Resume**, which ends the pause early. [Confirmed 31 Oct 2026] Notifications that arrive while paused are dropped: no banner, and they are not added to the last 3 list. [Confirmed 31 Oct 2026, follows the prototype] The tray icon is built with the WinForms `NotifyIcon` that ships with .NET, so no extra package is needed. [Confirmed 31 Oct 2026]
- FR-6a. Hiding the island with hotkey 2 only hides it. Notifications are still processed in the background, so if you show the island again before the banner timer ends, the banner is there. [Confirmed 31 Oct 2026, follows the prototype]
- FR-6b. Only one WinIsland runs at a time. Starting it again while it runs does nothing: the second copy closes right away, so there is never a second island or a second set of hotkeys. [Confirmed 31 Oct 2026]

**Music**

- FR-7. Read the current media session: title, artist, artwork, play state, position, duration.
- FR-8. Controls: play/pause, next, previous.
- FR-9. Progress bar updates smoothly between system updates.
- FR-10. Handle no session, paused session, and session changes (for example Spotify closes) without errors.
- FR-11. Seek by tapping the progress bar, only if the source app supports it. Hidden or disabled otherwise. Mouse only: click or drag. No arrow keys, because the island never has keyboard focus (FR-3). [Confirmed 31 Oct 2026]

**Notifications**

- FR-12. Ask for notification access once, with clear in-app explanation, and handle denial gracefully.
- FR-13. Read toast notifications and extract app name, title, and body text.
- FR-14. Filter by app so only enabled apps show. WhatsApp and Discord on by default.
- FR-15. Show newest, +N badge for the rest, tap to open the list. Tapping the banner opens a list of the last 3 notifications from all apps, newest first. With only one notification, it shows the full message, up to 3 lines. Items leave the list when Windows removes the notification, for example after you read it in the app. The list is kept in memory only. Swiping the banner sideways (mouse drag) hides the current banner and shows the next waiting one, if any. Hotkey 3 hides all waiting banners and also closes the list if it is open. Neither deletes anything from Windows or from the list. [Confirmed 31 Oct 2026, follows the prototype]
- FR-16. Auto-dismiss after a configurable time (default 6 seconds, you asked for 5 to 6 seconds while music plays). [Confirmed] There is one timer for all waiting banners: each new notification restarts it, and when it ends, all waiting banners hide at once. Opening the list stops the timer. [Confirmed 31 Oct 2026, follows the prototype] Also dismiss when Windows removes the notification, which should happen if the source app clears it after you read the message. [Unverified per app]
- FR-17. Hide-content setting: one "Hide message text" switch. When on, the banner and the list show only the sender and the app, never the message text. [Confirmed 31 Oct 2026, replaces the earlier "app only" option]
- FR-18. Message text is held in memory only. Never written to disk or logs.

**Settings**

- FR-19. Settings window for the list in section 4, saved locally.
- FR-20. Launch at startup toggle.

## 7. Non-functional requirements

- Performance: no visible frame drops in the animation, near-zero CPU when idle, no measurable FPS impact on games (we will measure).
- Reliability: no crash if Spotify, WhatsApp, or Discord are closed. Recover if the notification listener or media session drops.
- Privacy: no network calls in MVP. No message content stored. No screen capture, process inspection, or memory reading.
- Safety with games: no injection, no hooks into game processes, so anti-cheat should have nothing to flag. We still cannot promise this for every game.
- Accessibility: keyboard-operable settings, readable contrast. The island itself is glanceable, not a full accessibility surface. Animations follow the Windows "Animation effects" setting: when it is off, states change instantly. [Confirmed 31 Oct 2026, from the design spec]

## 8. UI and UX direction

[Confirmed] Close to the iPhone Dynamic Island: pure black pill, rounded, smooth morphing, dark only.

Design sources, in `docs/design/` [Confirmed 31 Oct 2026]: `prototype.html` is the source of truth for look and motion, `DESIGN_SPEC.md` holds the exact values, and `winisland-design-boards.html` is a static snapshot of every state. This PRD is the source of truth for behavior. Differences between them are reported to you, not decided silently.

States to prototype first:

1. Compact, idle: clock.
2. Compact, music playing: small artwork, scrolling or truncated title, tiny equalizer animation.
3. Expanded: artwork, title, artist, progress bar, previous/play/next.
4. Notification: app icon, sender, one or two lines of preview, +N badge. Tap opens the last 3 list, or the full message when there is only one.
5. Hidden and paused (nothing visible, tray only).
6. Empty or error: no permission for notifications, hotkey conflict.

The clock uses 24-hour format, hours and minutes only (no seconds, no AM/PM). [Confirmed] Exact sizes, fonts, and easing were settled in the prototype and are listed in `docs/design/DESIGN_SPEC.md`. [Confirmed 31 Oct 2026] Prototype lives in Claude as an interactive HTML mock. It can show look, animation, and states with fake data. It cannot prove overlay-over-game behavior, Spotify control, or notification reading. Those get proven in Claude Code.

## 9. Technology stack (proposal)

[Proposed] C# on .NET 10 (LTS, supported to November 2028) with WPF. .NET 8 support ends on 10 November 2026, so it is not used.

- Why: WPF handles transparent, borderless, topmost windows and animations well, and the non-activating window style is easy to apply. Windows media and notification APIs are callable from .NET.
- Package identity: the notification listener appears to need it (see section 10). We can add identity with a small sparse package ("packaging with external location") while keeping a normal WPF app. [Verified that this mechanism exists]
- Alternative WinUI 3: more modern look and tooling, but custom transparent overlay windows are fiddlier and the app is heavier.
- Alternative Tauri or Electron: easier web-style animation, but extra runtime weight and more bridging work for Windows APIs. Electron is the heaviest option for something that should be tiny.

This is a recommendation from reading docs, not from building. A small spike will confirm it.

## 10. Research findings and what is still unknown

**Verified this session**

- The Windows media session API (Windows.Media.Control) exposes session info, media properties, playback info, timeline (position, seek range), and a method to change playback position. Available since Windows 10 1809. Spotify desktop is expected to publish here, but I have not confirmed that or that it honors seeking.
- The notification listener API exists, requires explicit user permission, can return toast notifications, and raises an event when a notification is added or removed. Only toast notifications are supported.
- Windows can turn Do Not Disturb on automatically when you play a game or use a fullscreen app. Notifications that arrive during DND go to the notification center.
- Package identity can be granted to an unpackaged desktop app with a package with external location, on Windows 10 2004 and later. A community notification-overlay project reports that the listener needs this identity, and Microsoft documents that Windows App SDK notification APIs need it. I did not find Microsoft stating it for the listener specifically, so the spike confirms it.

**Unverified, must be tested on your PC**

- U1. Whether the listener still receives WhatsApp and Discord notifications while DND is active during a game.
- U2. Whether turning off banners (but keeping the notification center on) still lets the listener see them.
- U3. Whether WhatsApp and Discord remove their notification from Windows when you read the message. This decides the "disappears when I open it" behavior.
- U4. Whether the island stays visible and non-focus-stealing over your actual games in borderless mode.
- U5. Whether global hotkeys fire while your games have focus.
- U6. Whether Spotify accepts seek requests through the media session.
- U7. You will use the WhatsApp desktop app. To test: whether its notifications reach the listener under a WhatsApp app name, and whether it clears them when you read the message. WhatsApp Web in a browser would arrive under the browser's name instead, so it is not the supported path.

## 11. Data and integrations

- Windows media session API, local only. The music source is the Spotify desktop app, logged in with your own Premium account. WinIsland itself does not log in to Spotify.
- Windows notification listener API, local only.
- Local settings file. No cloud, no accounts, no network in MVP.

## 12. Business and validation rules

- Only enabled apps show previews. Everything else is ignored, not stored.
- Notification duration has a sane minimum and maximum.
- Two hotkeys cannot be the same.
- Paused state wins over everything: nothing shows until the hour ends or you resume.

## 13. States and edge cases

- No music session: show clock.
- Spotify paused: show the paused music state for 5 seconds, then the clock. [Confirmed 31 Oct 2026]
- Notification permission denied: show a one-time hint and stay functional for music and clock.
- Very long sender or message: truncate cleanly.
- Notification with no text or image-only: show app and sender only.
- Multiple notifications arrive together: newest first, +N badge.
- Fullscreen exclusive game: island will not show. Documented limitation, not a bug.
- Windows Do Not Disturb on: behavior depends on U1.

## 14. Security, privacy, performance

- Notification access needs your explicit permission and can be revoked in Windows settings.
- Message text never persisted or logged. Logs hold event types and errors only.
- No secrets in MVP. If Spotify Web API is added later, tokens go in Windows credential storage, never in the repo.
- Performance targets in section 2, measured during the spike phase.

## 15. Acceptance criteria (MVP)

- AC-1. With no media and no notifications, the pill shows the correct local time and does not take focus.
- AC-2. With Spotify desktop playing, title, artist, artwork, and progress show within 2 seconds, and play/pause/next/previous work.
- AC-3. Pressing hotkey 1 or clicking the compact island with the mouse expands it, buttons are clickable, and the game keeps keyboard focus. Tested in at least two of Forza Horizon 6, Skyrim, Honkai Star Rail, and Zenless Zone Zero, in borderless mode.
- AC-4. A WhatsApp desktop message and a Discord desktop message each show sender and preview within 2 seconds with banners off.
- AC-5. Previews auto-dismiss at the configured time (default 6 seconds), can be hidden by swiping or with hotkey 3, and also on read if U3 turns out true for that app. Tapping the banner opens the list of the last 3 notifications from all apps, newest first (the full text when there is only one), and items disappear when Windows removes them.
- AC-6. Hotkey 2 hides and shows the island. Pause for 1 hour hides it and restores it after an hour.
- AC-7. Disabling an app in settings stops its previews. Hide-content shows no message text.
- AC-8. The app starts with Windows into the tray and survives Spotify, WhatsApp, and Discord being closed and reopened.
- AC-9. Closing the app leaves nothing running and no message content on disk.

## 16. Phased plan

- Phase 0: Prototype in Claude chat (visual states and animation), then your design approval.
- Phase 1: Technical spike in Claude Code. Create the private GitHub repo, a bare overlay window, and small tests for U1 to U7. Report results honestly, including failures.
- Phase 2: Overlay shell, clock, states, animations, hotkeys, tray.
- Phase 3: Music through the media session.
- Phase 4: Notifications, filtering, badge, auto-dismiss, read-dismiss.
- Phase 5: Settings, startup, pause, hardening, tests in real games.

## 17. Top risks

1. Notifications may not reach the listener during DND or with banners off (U1, U2). Mitigation: test first. Fallback is a documented setup, for example turning off the automatic DND rule for games.
2. Read-dismiss may not work for WhatsApp or Discord (U3). Mitigation: timer fallback, already agreed.
3. Games may behave badly with a topmost overlay (U4). Mitigation: only support windowed and borderless, test in your real games.
4. Package identity adds setup friction. Mitigation: sparse package script in the repo.
5. Spotify may not support seeking via the media session (U6). Mitigation: hide seek in MVP, consider Web API later.

## 18. Decisions and open items

1. Clock format: 24-hour, hours and minutes only. Confirmed.
2. Pause for 1 hour also silences notifications, not just hides the pill. Confirmed.
3. Hotkeys: Ctrl+Alt+I and Ctrl+Alt+H confirmed as defaults, as long as they do not clash with other hotkeys. The app checks for conflicts.
4. Games: Forza Horizon 6, Skyrim, Honkai Star Rail, Zenless Zone Zero. All can run borderless. These are the test games for AC-3.
5. WhatsApp: desktop app, to be installed by you. Notification behavior to be tested, see U7.
6. MVP v1.0 approved by you on 30 Oct 2026 after the prototype review. Not in v1.0, see the backlog doc: shuffle and repeat, opening the source app from a notification, recent notifications when idle or together with the music controls, glass or blur look, grouping by category.
