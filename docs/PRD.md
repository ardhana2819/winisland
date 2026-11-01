# WinIsland PRD (v1.1)

Status: MVP v1.0 approved by you on 30 Oct 2026, after reviewing the prototype, and released as v1.0.0-beta.1 on 1 Nov 2026. **v1.1 requirements approved by you on 1 Nov 2026** (written the same day from the backlog), designed in the prototype (Phase 6), built in Phases 7 to 9 and tested by you, and released as v1.1.0-beta.1 on 1 Nov 2026.

Labels used below: **[Confirmed]** you decided it. **[Verified]** I checked it against Microsoft docs or another source this session. **[Proposed]** my recommendation, needs your yes. **[Unverified]** I believe it but it must be tested before we rely on it. **[v1.1]** new or changed in v1.1.

### What v1.1 adds, in short

- Paused music stays one click away: expanding the idle island shows the paused song (FR-23).
- An empty music view when there is nothing to play (FR-24).
- Two pages in the expanded island, music and notifications, switched by scrolling or Alt+Up and Alt+Down (FR-21, FR-22).
- Colours from the artwork on the music page (FR-25).
- Notifications on grey cards (FR-26).
- Fix: artwork missing for the first song after start (FR-27).

## 1. Overview and problem

WinIsland is a small Windows 11 app that puts a pill-shaped overlay at the top center of the screen, modeled on the iPhone Dynamic Island. It shows a clock, current music, and incoming WhatsApp and Discord notifications, so you can see them in a game without alt-tabbing.

The problem: in a game, music and message alerts either do not show at all (Windows suppresses banners in fullscreen) or pull you out of the game.

## 2. Goals and success criteria

- G1. See what is playing and control it without leaving the game. Success: play/pause/next/previous work from the island in windowed and borderless games.
- G2. Know a WhatsApp or Discord message arrived without leaving the game. Success: a preview shows in the island within about 2 seconds of the Windows notification.
- G3. Never get in the way. Success: the island does not take keyboard focus, and you can hide it instantly.
- G4. Stay light. Proposed target: under 100 MB RAM and near-zero CPU when idle. [Proposed] RAM is measured as working set. [Confirmed 31 Oct 2026] The spike's bare test window already used about 101 MB working set, so the real number is measured in Phase 2 and the target is decided then. [Confirmed 31 Oct 2026] Target set after Phase 2 (idle about 103 MB working set): **under 150 MB working set**, measured and reported in every pull request. [Confirmed 31 Oct 2026]
- G5. [v1.1] Music and recent notifications are always within reach. Success: from the idle island, one click or hotkey 1 reaches the play button of a paused song, and one more scroll or Alt+Down shows the last 3 notifications, without leaving the game. [Confirmed 1 Nov 2026]

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

### In scope for v1.1 [Confirmed 1 Nov 2026]

- Expanding the idle island always shows the music page: the playing song, the paused song, or an empty view.
- Two pages in the expanded island: music and the last 3 notifications, switched vertically.
- Colours from the artwork on the music page: solid dark background and accent colours for the controls.
- Notifications shown on grey cards, on the notifications page and in the banner.
- Fix for the artwork that sometimes does not show for the first song after start.

### Out of scope for MVP

- Exclusive fullscreen and any game injection or hooking. [Confirmed]
- Replying to messages from the island. [Confirmed]
- Spotify Web API, playlists, library browsing. Decided: use the Spotify desktop app only, no Web API.
- Light theme, movable position, size setting, multi-monitor, animation tuning. Also later (see the backlog doc): glass or blur look, grouping notifications by category.
- Notification sources beyond WhatsApp, Discord, and generic Windows toasts.
- [v1.1] Opening WhatsApp or Discord when you tap a notification. Removed from the plans on 1 Nov 2026: an accidental click during a game would pull you out of it. [Confirmed]
- [v1.1] Shuffle and repeat buttons. Removed from the plans on 1 Nov 2026, to keep the controls clean. [Confirmed]

## 5. User stories

1. As a player, I see a clock in the pill when nothing else is happening, so I can check the time in a game.
2. As a player, I see the song title and a small animation when music plays, so I know what is on.
3. As a player, I press the hotkey or click the island, it expands, and I can click play/pause/next/previous without my game losing keyboard focus.
4. As a player, a WhatsApp or Discord message shows sender and a short preview, then goes away on its own or when I read it in the app.
5. As a player, when messages arrive I see the newest with a +N badge, and I can tap it to read the last 3 (or the full text of a single message).
6. As a player, I press the second hotkey to hide the island when it distracts me, and press it again to bring it back.
7. As a user, I choose which apps may show previews, and I can hide message text entirely.
8. As a user, the app starts with Windows and stays out of my way in the tray.
9. [v1.1] As a player, after I pause Spotify and the island shows the clock again, I click the island and press play, without opening Spotify.
10. [v1.1] As a player, I scroll or press Alt+Down on the open island to see my last 3 notifications, even when no banner is showing.
11. [v1.1] As a player, the music view takes the colour of the song's artwork, so it feels like the iPhone.
12. [v1.1] As a player, each notification sits on its own grey card, so it is easy to read.

## 6. Functional requirements

**Overlay**

- FR-1. Topmost, borderless, transparent window, top center of the primary monitor, fixed default size. The size follows the display automatically so it suits 24 to 32 inch monitors: per-monitor DPI aware, with a scale of display width in DIPs divided by 2560, limited to 0.85 to 1.25. No manual size setting. [Confirmed]
- FR-2. Three visual states: compact (clock or music), expanded (music controls), notification (preview). Expanding: click the compact island with the mouse, or press hotkey 1. A click only expands. Play/pause, next and previous work only once it is expanded. With nothing playing, neither the click nor the hotkey does anything. [Confirmed] Clicking the open island outside its buttons closes it immediately, and it also closes about 2 seconds after the pointer leaves. [Confirmed] There is no Escape key to close it: the island never has keyboard focus (FR-3), so keys go to the game. [Confirmed 31 Oct 2026] If a notification arrives while the island is expanded, the banner replaces the expanded view. [Confirmed 31 Oct 2026, follows the prototype] A mouse click anywhere outside the open island (for example in the game) also closes it right away; the click still reaches the game. WinIsland checks the mouse buttons only while the island is open and uses no mouse hook. [Confirmed 31 Oct 2026, Diva's request]
  - [v1.1] Changed: the click and hotkey 1 always expand the island, also from the idle clock. The expanded island opens on the music page, which shows the playing song, the paused song (FR-23) or the empty view (FR-24). This replaces "with nothing playing, neither the click nor the hotkey does anything". [Confirmed 1 Nov 2026]
  - [v1.1] The expanded state now has two pages (FR-21). A notification that arrives while the island is open on the music page replaces it with the banner, as in v1.0. On the notifications page no banner shows: the new notification appears at the top of the page. [Confirmed 1 Nov 2026]
- FR-3. Window must not take keyboard focus when shown or clicked. [Proposed technique: non-activating window style. Unverified in games.]
- FR-4. Smooth morph animation between states, spring-like in the style of the iPhone Dynamic Island. [Confirmed]
- FR-5. Hotkey 1 (Ctrl+Alt+I) toggles expanded state. Hotkey 2 (Ctrl+Alt+H) hides or shows the island. [Confirmed as defaults] Hotkey 3 (proposed Alt+X, a simple two-key combo) hides the notification banner. It does not delete notifications. [Proposed] Windows hands a global hotkey to this app only, so a common combo would stop working in other programs. The app reports a conflict and the key stays configurable. Both configurable. App reports a clear error if a hotkey is already taken: a conflict banner shows for 10 seconds at start, and the same error shows in Settings (see `docs/design/DESIGN_SPEC.md`). [Confirmed 31 Oct 2026]
- FR-6. Tray menu: Pause for 1 hour (hides the island and stops showing notifications, then returns automatically), Hide island (Ctrl+Alt+H, the same as hotkey 2; reads Show island while hidden), Settings, Quit. [Hide island item confirmed 31 Oct 2026, from the design boards] While paused, the tray icon switches to a dimmed variant so the paused state is visible. [Confirmed 31 Oct 2026] While paused, the Pause item becomes **Resume**, which ends the pause early. [Confirmed 31 Oct 2026] Notifications that arrive while paused are dropped: no banner, and they are not added to the last 3 list. [Confirmed 31 Oct 2026, follows the prototype] The tray icon is built with the WinForms `NotifyIcon` that ships with .NET, so no extra package is needed. [Confirmed 31 Oct 2026]
- FR-6a. Hiding the island with hotkey 2 only hides it. Notifications are still processed in the background, so if you show the island again before the banner timer ends, the banner is there. [Confirmed 31 Oct 2026, follows the prototype]
- FR-6b. Only one WinIsland runs at a time. Starting it again while it runs does nothing: the second copy closes right away, so there is never a second island or a second set of hotkeys. [Confirmed 31 Oct 2026]

**Music**

- FR-7. Read the current media session: title, artist, artwork, play state, position, duration. Song titles that do not fit scroll sideways, in the compact pill and the expanded island, instead of being cut with an ellipsis; short titles stay still. [Confirmed 31 Oct 2026, Diva's request]
- FR-8. Controls: play/pause, next, previous. While the island is expanded, the keyboard works too, with Spotify's keys: Space plays or pauses, Ctrl+Right goes to the next song, Ctrl+Left to the previous one. They are active only while the island is open, and during that time these keys do not reach the game; once it closes, they belong to the game again. [Confirmed 31 Oct 2026, Diva's request]
- FR-9. Progress bar updates smoothly between system updates.
- FR-10. Handle no session, paused session, and session changes (for example Spotify closes) without errors.
- FR-11. Seek by tapping the progress bar, only if the source app supports it. Hidden or disabled otherwise. Mouse only: click or drag. No arrow keys, because the island never has keyboard focus (FR-3). [Confirmed 31 Oct 2026]

**Notifications**

- FR-12. Ask for notification access once, with clear in-app explanation, and handle denial gracefully.
- FR-13. Read toast notifications and extract app name, title, and body text.
- FR-14. Filter by app so only enabled apps show. WhatsApp and Discord on by default.
- FR-15. Show newest, +N badge for the rest, tap to open the list. Tapping the banner opens a list of the last 3 notifications from all apps, newest first. With only one notification, it shows the full message, up to 3 lines. Items leave the list when Windows removes the notification, for example after you read it in the app. The list is kept in memory only. Swiping the banner sideways (mouse drag) hides the current banner and shows the next waiting one, if any. Hotkey 3 hides all waiting banners and also closes the list if it is open. Neither deletes anything from Windows or from the list. [Confirmed 31 Oct 2026, follows the prototype] WhatsApp keeps one Windows notification per chat and swaps it for each new message (spike U7). So when Windows removes a notification and a new one from the same app and chat follows within 1 second, it is a replacement: the older message stays in the list. A removal with nothing after it means the chat was read, and every item from that chat leaves the list. [Confirmed 31 Oct 2026]
  - [v1.1] Changed: tapping the banner opens the expanded island on the notifications page (FR-22) instead of a separate list. The list rules above (last 3, newest first, full text for one, removal and replacement, memory only) carry over to that page. Swiping the banner sideways stays as it is; it does not clash with the vertical page switch. Hotkey 3 also closes the island when it is open on the notifications page. [Confirmed 1 Nov 2026]
- FR-16. Auto-dismiss after a configurable time (default 6 seconds, you asked for 5 to 6 seconds while music plays). [Confirmed] There is one timer for all waiting banners: each new notification restarts it, and when it ends, all waiting banners hide at once. Opening the list stops the timer. [Confirmed 31 Oct 2026, follows the prototype] Also dismiss when Windows removes the notification, which should happen if the source app clears it after you read the message. [Unverified per app]
- FR-17. Hide-content setting: one "Hide message text" switch. When on, the banner and the list show only the sender and the app, never the message text. [Confirmed 31 Oct 2026, replaces the earlier "app only" option]
- FR-18. Message text is held in memory only. Never written to disk or logs.

**Settings**

- FR-19. Settings window for the list in section 4, saved locally.
- FR-20. Launch at startup toggle.

**Expanded island pages [v1.1]**

- FR-21. The expanded island has two pages: music first, notifications second. [Confirmed 1 Nov 2026]
  - Both pages are 420 x 190 (times the auto scale), so switching pages slides the content and the island never changes size. The music view grows from 168 to 190 high. [Confirmed 1 Nov 2026]
  - Switching is vertical: the mouse scroll wheel or a two-finger swipe on a touchpad, down for notifications and up for music. [Confirmed 1 Nov 2026] One gesture moves one page; the many small scroll steps a touchpad sends count as one gesture. [Confirmed 1 Nov 2026]
  - Alt+Down shows the notifications page and Alt+Up the music page. Like the music keys (FR-8), they are registered only while the island is open, so the rest of the time they belong to the game. [Confirmed 1 Nov 2026] They are fixed, not set in Settings, like the music keys. If another app already owns one, it just does not work and the error goes to the log. [Confirmed 1 Nov 2026]
  - Small page dots at the right edge show which page is open. [Confirmed 1 Nov 2026] Clicking a dot opens that page. [Confirmed 1 Nov 2026]
  - The content slides vertically with the same spring as other changes; with Windows animation effects off it switches at once (section 7). The exact motion is set in the prototype.
  - Expanding by click or hotkey 1 opens the music page. Tapping a notification banner opens the notifications page. The island closes as in FR-2 (click outside, 2 seconds after the pointer leaves).
  - Scrolling must not take keyboard focus from the game (FR-3).
- FR-22. The notifications page shows the last 3 notifications from all enabled apps, newest first, with the rules from FR-15: memory only, items leave when Windows removes them, WhatsApp replacements kept, "Hide message text" respected, nothing kept while paused. [Confirmed 1 Nov 2026]
  - With one notification its card shows the full text, up to 3 lines. [Confirmed 1 Nov 2026]
  - With none, the page says "No notifications". [Confirmed 1 Nov 2026]
  - Clicking a card does nothing: it never opens WhatsApp or Discord. [Confirmed 1 Nov 2026]

**Music states [v1.1]**

- FR-23. Paused music is one click away. When Spotify is paused, the compact island shows the paused state and then the clock, as in section 13. Expanding the island then shows the paused song on the music page, with working controls, so you can press play there. [Confirmed 1 Nov 2026]
  - While Spotify is open but stopped, its media session usually still reports the last song: that song shows as paused. [Confirmed 1 Nov 2026] [Unverified: U8]
- FR-24. Empty music view. When there is no media session (Spotify closed), the music page shows an empty view: a plain artwork placeholder, "Not playing" as the title, no progress bar, and dimmed controls that do nothing. A remembered last song is not shown once Spotify is closed, because its controls could not work. [Confirmed 1 Nov 2026]

**Look [v1.1]**

- FR-25. Colours from the artwork on the music page. [Confirmed 1 Nov 2026]
  - The background is a solid dark colour taken from the artwork's main colour, not see-through. Similar to your reference picture, not a copy of it.
  - The play or pause button, previous and next, the progress bar and the equalizer use an accent colour: a brighter version of the same colour.
  - Text stays white and light grey. The background is darkened until text meets the normal contrast rule for small text (at least 4.5 to 1), and the accent is kept at least 3 to 1 against the background. [Confirmed 1 Nov 2026]
  - No artwork, or the empty view, means black, as in v1.0. Grey or black-and-white artwork gives a neutral dark grey. [Confirmed 1 Nov 2026]
  - When the song changes, the colours fade over (about 0.4 seconds). [Confirmed 1 Nov 2026]
  - Only the expanded music page is coloured. The compact pill, banner and notifications page stay black, like the iPhone. While sliding between pages the background fades between the two. [Confirmed 1 Nov 2026]
  - The colour is worked out once per song, in memory, from the artwork WinIsland already has. The picking logic lives in Core with unit tests.
- FR-26. Notification cards. Each notification sits on its own rounded grey card inside the black island: app icon, sender in bold, preview below, and the time it arrived on the right (24-hour, like the clock). [Confirmed 1 Nov 2026]
  - Used on the notifications page and in the banner, so both look the same. [Confirmed 1 Nov 2026]
  - Card colour, radius and spacing are set in the prototype design pass. The card colour must keep the sender and preview text readable (at least 4.5 to 1).

**Fixes [v1.1]**

- FR-27. The artwork shows for the first song after WinIsland starts, not only from the second song on. Seen in your v1.0 tests. This is what AC-2 already asks for. [Confirmed]

## 7. Non-functional requirements

- Performance: no visible frame drops in the animation, near-zero CPU when idle, no measurable FPS impact on games (we will measure).
- Reliability: no crash if Spotify, WhatsApp, or Discord are closed. Recover if the notification listener or media session drops.
- Privacy: no network calls in MVP. No message content stored. No screen capture, process inspection, or memory reading.
- Safety with games: no injection, no hooks into game processes, so anti-cheat should have nothing to flag. We still cannot promise this for every game.
- Accessibility: keyboard-operable settings, readable contrast. The island itself is glanceable, not a full accessibility surface. Animations follow the Windows "Animation effects" setting: when it is off, states change instantly. [Confirmed 31 Oct 2026, from the design spec]
- [v1.1] Size: any change to the island's size is confirmed with you first and must not make it much bigger. [Confirmed 1 Nov 2026] The only v1.1 size change is the expanded island at 420 x 190 (FR-21).
- [v1.1] Performance: the v1.1 features stay inside the G4 target (under 150 MB working set, near-zero CPU when idle). The artwork colour is worked out once per song, not every frame.

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

[v1.1] States to add in the prototype design pass, before any v1.1 code:

7. Expanded music page at 420 x 190: playing, paused, and empty (FR-23, FR-24), with page dots.
8. Expanded music page with artwork colours, using a few sample covers: red, blue, very light, grey (FR-25).
9. Notifications page at 420 x 190 with 3, 1 and 0 notifications, on grey cards (FR-22, FR-26).
10. Notification banner on a grey card (FR-26).
11. The vertical slide between the two pages (FR-21).

The new exact values (card colour and radius, page dots, spacing at 190 high, slide motion) go into `docs/design/DESIGN_SPEC.md` after you approve the updated prototype.

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

**Unverified for v1.1**

- U8. What Spotify's media session reports while Spotify is open but stopped, including right after it starts with nothing played yet. Decides whether FR-23 shows a last song or FR-24 shows the empty view in that case.
- U9. Whether the island receives the mouse wheel and touchpad scrolling without being the active window. Windows sends the wheel to the window under the pointer when "Scroll inactive windows when I hover over them" is on (the Windows default). To test over your games, and to check the game keeps keyboard focus. **Result (you, 1 Nov 2026):** scrolling and clicking the open island worked while playing a game.
- U10. Whether Alt+Up and Alt+Down can be registered while a game runs and whether any of your four games use them. They are only taken while the island is open. **Result (you, 1 Nov 2026):** Alt+Up and Alt+Down worked while playing a game, with no problem in the game.
- U8 was not tested on its own; paused and closed Spotify (FR-23, FR-24) worked in your Phase 7 test.

## 11. Data and integrations

- Windows media session API, local only. The music source is the Spotify desktop app, logged in with your own Premium account. WinIsland itself does not log in to Spotify.
- Windows notification listener API, local only.
- Local settings file. No cloud, no accounts, no network in MVP.

## 12. Business and validation rules

- Only enabled apps show previews. Everything else is ignored, not stored.
- Notification duration has a sane minimum and maximum.
- Two hotkeys cannot be the same.
- Paused state wins over everything: nothing shows until the hour ends or you resume.
- [v1.1] Both expanded pages always have the same size (FR-21).
- [v1.1] Text on the coloured music page and on notification cards keeps at least 4.5 to 1 contrast (FR-25, FR-26).

## 13. States and edge cases

- No music session: show clock. [v1.1] Expanding shows the empty music view (FR-24).
- Spotify paused: show the paused music state for 5 seconds, then the clock. [Confirmed 31 Oct 2026] While the island is expanded, the paused music and its controls stay, so you can press play again; the 5 seconds start when the island closes. [Confirmed 31 Oct 2026] [v1.1] After that, expanding the island shows the paused song again (FR-23).
- [v1.1] Spotify closes while the music page is open: the page switches to the empty view, without errors.
- [v1.1] Artwork that is missing, very dark, very light or grey: the colours fall back as in FR-25, and text stays readable.
- [v1.1] No notifications: the notifications page says "No notifications".
- [v1.1] A notification arrives while the island is open: see FR-2.
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

### Acceptance criteria (v1.1)

- AC-10. After pausing Spotify and waiting until the island shows the clock, clicking the island or pressing hotkey 1 shows the paused song, and play starts it again. The game keeps keyboard focus.
- AC-11. With Spotify closed, expanding the island shows the empty music view; its controls do nothing and no error appears in the log. Opening Spotify and playing a song replaces it within 2 seconds.
- AC-12. On the open island, scrolling down or pressing Alt+Down shows the notifications page, and scrolling up or Alt+Up shows the music page. The island stays 420 x 190 (times the auto scale) on both pages, and the game keeps keyboard focus. Once the island closes, Alt+Up and Alt+Down reach the game again. Tested in at least two of your four games.
- AC-13. The notifications page shows the last 3 notifications on grey cards, newest first, each with app icon, sender, preview and time. With none it says "No notifications". Tapping a banner opens this page. Clicking a card opens nothing.
- AC-14. The music page background and controls take their colour from the artwork within 1 second of a song change. Text contrast is at least 4.5 to 1 for every sample in the unit tests, including very light and grey artwork. Without artwork the page is black.
- AC-15. The artwork shows for the first song after WinIsland starts, within 2 seconds.
- AC-16. Idle memory stays under 150 MB working set with the v1.1 features (G4).

## 16. Phased plan

- Phase 0: Prototype in Claude chat (visual states and animation), then your design approval.
- Phase 1: Technical spike in Claude Code. Create the private GitHub repo, a bare overlay window, and small tests for U1 to U7. Report results honestly, including failures.
- Phase 2: Overlay shell, clock, states, animations, hotkeys, tray.
- Phase 3: Music through the media session.
- Phase 4: Notifications, filtering, badge, auto-dismiss, read-dismiss.
- Phase 5: Settings, startup, pause, hardening, tests in real games.

v1.1 [Confirmed 1 Nov 2026]:

- Phase 6: Prototype design pass for the new states (section 8), then your approval. The new values go into the design spec.
- Phase 7: Fix the first-song artwork (FR-27), paused music one click away (FR-23), empty music view (FR-24). Small and testable on its own.
- Phase 8: Two pages and notification cards (FR-21, FR-22, FR-26, changes to FR-2 and FR-15).
- Phase 9: Colours from the artwork (FR-25).
- Then a new beta release.

## 17. Top risks

1. Notifications may not reach the listener during DND or with banners off (U1, U2). Mitigation: test first. Fallback is a documented setup, for example turning off the automatic DND rule for games.
2. Read-dismiss may not work for WhatsApp or Discord (U3). Mitigation: timer fallback, already agreed.
3. Games may behave badly with a topmost overlay (U4). Mitigation: only support windowed and borderless, test in your real games.
4. Package identity adds setup friction. Mitigation: sparse package script in the repo.
5. Spotify may not support seeking via the media session (U6). Mitigation: hide seek in MVP, consider Web API later.
6. [v1.1] Scrolling may not reach the island while a game is active (U9). Mitigation: Alt+Up and Alt+Down and the page dots switch pages without the wheel.
7. [v1.1] Some artwork gives ugly or hard-to-read colours. Mitigation: contrast rules and fallbacks in FR-25, unit tests with extreme samples, and your review with real covers.
8. [v1.1] Alt+Up or Alt+Down may be taken by another app or used by a game (U10). Mitigation: taken only while the island is open; if registering fails, the other ways still work.

## 18. Decisions and open items

1. Clock format: 24-hour, hours and minutes only. Confirmed.
2. Pause for 1 hour also silences notifications, not just hides the pill. Confirmed.
3. Hotkeys: Ctrl+Alt+I and Ctrl+Alt+H confirmed as defaults, as long as they do not clash with other hotkeys. The app checks for conflicts.
4. Games: Forza Horizon 6, Skyrim, Honkai Star Rail, Zenless Zone Zero. All can run borderless. These are the test games for AC-3.
5. WhatsApp: desktop app, to be installed by you. Notification behavior to be tested, see U7.
6. MVP v1.0 approved by you on 30 Oct 2026 after the prototype review. Not in v1.0, see the backlog doc: shuffle and repeat, opening the source app from a notification, recent notifications when idle or together with the music controls, glass or blur look, grouping by category.
7. v1.0 released as v1.0.0-beta.1 on 1 Nov 2026, marked beta because more features are planned.
8. v1.1 decisions, 1 Nov 2026 [Confirmed]: paused music reachable from the idle island; empty view once Spotify is closed, even if a last song is remembered; two vertical pages switched by scroll wheel, touchpad, or Alt+Up and Alt+Down; both pages 420 x 190; the pages replace the old "recent notifications when idle" and "music plus notifications" ideas; solid (not see-through) artwork colours, with accent colours for the controls in the same step; grey cards for both the page and the banner; shuffle and repeat and opening the app from a notification removed.
9. Release name for v1.1: `v1.1.0-beta.1`, so the version number shows the new features. [Confirmed 1 Nov 2026]
10. The detailed v1.1 proposals in FR-2, FR-15, FR-21, FR-22, FR-25 and the phase plan were all approved by you on 1 Nov 2026.
11. Found in your v1.1 tests and fixed before release, 1 Nov 2026: WhatsApp posts the same notification again when its window is opened or minimized, which showed one message as two cards; the same text from the same chat within a minute now counts as one message. Line breaks inside a long group message now become spaces, so a card keeps one line. Notification events (app, Windows id, time, how many items the page holds; never sender or text) are now logged, to diagnose a "messages vanished" report that did not come back.
12. v1.1 released as v1.1.0-beta.1 on 1 Nov 2026.
