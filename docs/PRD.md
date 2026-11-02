# WinIsland PRD (v1.4 draft)

Status: MVP v1.0 approved by you on 30 Oct 2026, after reviewing the prototype, and released as v1.0.0-beta.1 on 1 Nov 2026. **v1.1 requirements approved by you on 1 Nov 2026** (written the same day from the backlog), designed in the prototype (Phase 6), built in Phases 7 to 9 and tested by you, and released as v1.1.0-beta.1 on 1 Nov 2026. **v1.2 (WhatsApp Web) requirements agreed by you in chat on 1 Nov 2026**, after the WhatsApp Web spike (`docs/spike-results.md`), built and tested by you (AC-17 and AC-18; AC-19 not yet triggered), and released as v1.2.0-beta.1 on 1 Nov 2026. The 10 minute cover window in FR-29 is still marked [Proposed]. **v1.3 (first batch of the 2 Nov 2026 requests) written and approved by you on 2 Nov 2026, with every [Proposed] item in FR-30 to FR-34, after reviewing the prototype; you picked app icon concept B, "Mono pill".** Built in Phases 10 to 12, tested by you, and released as v1.3.0-beta.1 on 2 Nov 2026 together with the drawn WhatsApp logo and the README uninstall steps. Exact values are in `docs/design/DESIGN_SPEC.md` section 10.

### What v1.3 adds, in short

- A WinIsland app icon, picked by you from 3 concepts in the prototype (FR-34).
- An island size setting: Small, Medium, Large, Extra large (FR-32).
- The page dots hide when the island is still and show while pages change (FR-31).
- A small white dot on the idle island when there are notifications you have not seen (FR-33).
- Discord notifications show the white Discord logo on a blurple circle, like WhatsApp (FR-30).

### What v1.4 adds, in short (draft, 2 Nov 2026, not approved yet)

- Messages from the same chat stack into one card on the notifications page (FR-35).
- An optional setting that also clears a notification from Windows when you hide it on the island (FR-36).
- A Glass look next to the solid black one, if spike G1 to G5 shows Windows can do it well (FR-37).
- An incoming call banner for WhatsApp calls, without buttons, if spike C1 to C5 shows WhatsApp tells Windows about calls (FR-38).

### What v1.2 adds, in short

- WhatsApp Web, installed as an app from Chrome, as a notification source next to WhatsApp desktop (FR-28).
- WhatsApp Web comes first: the same message from both shows once, from Web, and desktop's "You may have new messages" line is ignored (FR-29).

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

### In scope for v1.2 [Confirmed 1 Nov 2026]

- WhatsApp Web notifications from Chrome, because WhatsApp desktop has become heavy and you plan to use Web. No new look: Web messages show like desktop ones.

### In scope for v1.3 [Confirmed 2 Nov 2026]

- WinIsland app icon.
- Island size setting: Small, Medium, Large, Extra large.
- Page dots that hide when the island is still.
- Unread dot on the idle island.
- Discord logo on a blurple circle.

### In scope for v1.4 [Confirmed 2 Nov 2026, details in draft]

- Stacking messages per chat on the notifications page (the "grouping by category" idea, as you chose it).
- Optional clearing of hidden notifications from Windows.
- Glass look, if the spike passes.
- Incoming call banner, if the spike passes.

### Out of scope for MVP

- Exclusive fullscreen and any game injection or hooking. [Confirmed]
- Replying to messages from the island. [Confirmed] [v1.3] Asked for again on 2 Nov 2026 and kept out: the notification listener gives WinIsland only the notification's text and images, not its buttons or reply box [Verified: the `Notification` class has only `Visual` and `ExpirationTime`], and typing would take keyboard focus from the game (FR-3). [Confirmed 2 Nov 2026]
- [v1.3] Answering or declining a WhatsApp call from the island. Same reason: WinIsland cannot press another app's notification buttons without controlling that app's window, which section 7 rules out. An incoming call banner without buttons is planned for v1.4 instead. [Confirmed 2 Nov 2026]
- Spotify Web API, playlists, library browsing. Decided: use the Spotify desktop app only, no Web API.
- Light theme, movable position, size setting ([v1.3] now in scope, FR-32), multi-monitor, animation tuning. Also later (see the backlog doc): glass or blur look, grouping notifications by category.
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
13. [v1.3] As a player, I pick how big the island is, so it suits my monitor and my eyes.
14. [v1.3] As a player, I see at a glance on the idle island that a message came in, even after its banner is gone.
15. [v1.3] As a player, I recognise WinIsland by its own icon in the Start menu and Settings.

## 6. Functional requirements

**Overlay**

- FR-1. Topmost, borderless, transparent window, top center of the primary monitor, fixed default size. The size follows the display automatically so it suits 24 to 32 inch monitors: per-monitor DPI aware, with a scale of display width in DIPs divided by 2560, limited to 0.85 to 1.25. No manual size setting. [Confirmed]
  - [v1.3] Changed: there is now a size setting on top of this automatic scale (FR-32). "No manual size setting" no longer applies. [Confirmed 2 Nov 2026]
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

**WhatsApp Web [v1.2]**

- FR-28. WhatsApp Web is a notification source (spike W1, W2). [Confirmed 1 Nov 2026]
  - A notification counts as WhatsApp Web when its app name contains "WhatsApp" and it does not come from the WhatsApp desktop app (desktop's app id contains "WhatsApp"; the Chrome app's id does not). Other Chrome sites have other app names and stay ignored.
  - It counts as WhatsApp: the WhatsApp switch in Settings covers both, "Hide message text" applies, and the replacement and read rules of FR-15 apply (spike W3, W6).
  - Tested with WhatsApp Web installed as an app from Chrome. A plain Chrome tab and Brave are not tested (U11).
- FR-29. WhatsApp Web comes first, so one message never shows twice (spike W4, W5, W7). [Confirmed 1 Nov 2026: "prioritaskan notif WA Web"]
  - WinIsland does not check whether WhatsApp desktop is running: that would mean inspecting other processes, which section 7 rules out. It decides from the notifications themselves.
  - The same message from desktop and Web (same chat, same text, within a minute) shows once, as the Web one.
  - A desktop notification for a chat that WhatsApp Web sent a notification for in the last 10 minutes is ignored. [Proposed: 10 minutes]
  - Desktop's one-line "You may have new messages", which it sends while it is closed, is always ignored.
  - With WhatsApp Web not in use, desktop notifications show as before.

**v1.3 [Confirmed 2 Nov 2026]**

- FR-30. Discord notifications show the white Discord logo on a round blurple circle (#5865F2), the same way WhatsApp shows its logo: one look, whatever Windows sends. It replaces both the letter "D" and the logo Discord sends. [Confirmed 2 Nov 2026]
  - The logo shape is drawn by WinIsland, like the WhatsApp logo. [Confirmed 2 Nov 2026: the Discord icon from Simple Icons, CC0, as for WhatsApp]
- FR-31. Page dots show only while they help. [Confirmed 2 Nov 2026]
  - When the island expands, the dots show, so you see there is a second page. They fade out after 1.5 seconds without a page change.
  - Changing page (scroll, Alt+Up or Alt+Down, a dot click) shows them again, and they fade out 1.5 seconds after the last change.
  - Changed 2 Nov 2026 after your test: pointing at the island does not show the dots; they show only on expanding and on a page change. Hidden dots can still be clicked. [Confirmed 2 Nov 2026]
  - [Confirmed 2 Nov 2026] Fading in and out takes about 0.2 seconds. With Windows animation effects off, they appear and disappear at once.
- FR-32. Island size setting: Small, Medium, Large, Extra large, in the Settings window. [Confirmed 2 Nov 2026]
  - The sizes are 85, 100, 115 and 130 percent of today's island. Medium is today's size and the default. [Confirmed 2 Nov 2026]
  - Everything scales together: the pill, text, icons, artwork, cards and spacing, in every state (compact, expanded, banner). The setting multiplies the automatic scale of FR-1, so the island still suits the monitor. Example at automatic scale 1: Extra large expanded is 546 x 247.
  - A change shows at once, without a restart, and is saved with the other settings.
  - A first install starts at Medium. After that the last size chosen is kept: WinIsland starts with it after a restart, a Quit, or switching the PC off and on, until it is changed again (`settings.json`, `IslandSize`). [Confirmed 2 Nov 2026]
  - Changing the size grows or shrinks the island from the top center of the screen with the shape spring (0.6 s); the island stays centered the whole time. [Confirmed 2 Nov 2026, after your test]
  - [Confirmed 2 Nov 2026] Settings shows it as one choice with four options, not a slider.
- FR-33. Unread dot on the idle island. [Confirmed 2 Nov 2026]
  - A small white dot at the right of the clock shows when at least one notification on the notifications page has not been seen there yet.
  - A notification counts as seen when the notifications page has been open while it was on it. Only seeing its banner does not count.
  - The dot goes away when you open the notifications page, or when Windows removes every unseen notification (for example you read them in the app).
  - [Confirmed 2 Nov 2026] It also shows on the compact music pill, at its right end. The exact place is set in the prototype.
  - Nothing new comes in while paused (FR-6), so the dot does not appear then. It is hidden while the island is hidden.
- FR-34. WinIsland app icon. [Confirmed 2 Nov 2026]
  - I make 3 concepts in the prototype; you pick one. [Confirmed 2 Nov 2026] You picked concept B, "Mono pill": a black tile with the white pill, artwork square and equalizer bars. [Confirmed 2 Nov 2026]
  - It is used for `WinIsland.App.exe` (File Explorer, the Start menu shortcut) and the Settings window.
  - [Confirmed 2 Nov 2026] The tray icon stays a simple white pill, because a detailed icon is hard to read at 16 pixels on the taskbar; the chosen concept may change it if it reads well that small.
  - [Confirmed 2 Nov 2026] The icon is kept as a small `.ico` file in the repo (`src/WinIsland.App/Assets/WinIsland.ico`), drawn from the design spec by `Assets/New-AppIcon.ps1`, made from the chosen design. Today the tray icon is drawn in code to avoid binary files, but Windows needs a real `.ico` for the exe icon.

**v1.4 [draft, 2 Nov 2026]**

- FR-35. Messages from the same chat stack into one card on the notifications page, like the iPhone. [Confirmed 2 Nov 2026: "tumpuk per chat"]
  - "Same chat" is the rule WinIsland already uses for WhatsApp's replacements (FR-15): same app and same chat name. A group is one chat.
  - The card shows the chat, the newest message, the time of the newest message, and how many messages are in the stack. [Confirmed 2 Nov 2026: "3 messages" in grey before the time, only when there are 2 or more; the cards behind it peek out underneath like the iPhone, see DESIGN_SPEC section 11]
  - The page still shows at most 3 cards, newest chat first, so it can now stand for more than 3 messages. [Proposed: a stack counts up to 99, shown as "99+" above that]
  - Read in the app (Windows removes that chat's notification): the whole stack leaves, as today. [Confirmed rule, FR-15]
  - With only one card on the page, it shows the newest message's full text, up to 3 lines, as today. [Proposed]
  - Clicking a stack opens that chat on the notifications page: the chat name, the message count, and its last 3 messages, newest first, each on one line with its time. Clicking it again goes back to the list. Closing the island or switching to the music page also goes back to the list. [Proposed, Diva's idea 2 Nov 2026]
    - A chat with more than 3 messages still shows only the last 3; the count says how many there are.
    - Only messages that arrived while WinIsland was running can show. WhatsApp replaces its older notification, so earlier text cannot be read back from Windows.
    - If the chat leaves (read in the app, or cleared from Windows), the page goes back to the list.
    - "Hide message text" is respected: each line shows the same placeholder as the card.
    - Mouse only, no new hotkey, so nothing new can clash with a game's keys.
    - Clicking a single message card still does nothing (FR-22).
  - The banner is unchanged: one banner per new message, with +N for the others waiting.
  - Message text stays in memory only (FR-18). [Proposed: a stack keeps its last 3 messages' text and a count, nothing older; at most 9 texts in total]
- FR-36. Optional: hiding a notification on the island also clears it from Windows. [Confirmed 2 Nov 2026: a setting, off by default]
  - A new Settings switch, "Also clear from Windows notification center", off by default. Off: everything works as today.
  - On: swiping a banner away clears that chat's notifications from Windows, and Alt+X on a banner clears the notifications of every waiting banner's chat. [Proposed: the whole chat, because Discord leaves one Windows notification per message, and clearing only one would leave the older ones behind while the island's card is already gone] WinIsland uses the listener's own remove call (`UserNotificationListener.RemoveNotification`). [Verified 2 Nov 2026: the method exists in the Microsoft docs]
  - Once cleared from Windows, the notification also leaves the notifications page, because Windows reports it as removed. [Proposed: say so under the switch]
  - [Proposed] Alt+X on the open notifications page only closes the island, as today; it does not clear the page.
  - Clearing from Windows does not mark the message as read in WhatsApp or Discord.
- FR-37. Glass look. [Confirmed 2 Nov 2026: a Solid or Glass choice; depends on spike G1 to G5]
  - Settings gets "Look": Solid (today's black, the default) or Glass.
  - Glass: Windows' own blur behind the island, a dark tint over it, and a thin light edge (backlog F6). Windows draws the blur; WinIsland never captures the screen (section 7).
  - The music page keeps its solid artwork colour in both looks, so text stays readable (FR-25).
  - White text must stay readable over bright and dark games; the tint is set from spike G5. [Proposed: 45 to 55 percent black]
  - Built only if the spike passes G1 to G4. If it does not, this requirement moves back to the backlog with the spike's reasons.
- FR-38. Incoming call banner for WhatsApp, without buttons. [Confirmed 2 Nov 2026; depends on spike C1 to C5]
  - When WhatsApp tells Windows about an incoming call, the island shows a call banner: WhatsApp icon, caller name, "Incoming call" (or "Incoming video call"), and a green phone symbol. No answer or decline buttons (section 4).
  - [Proposed] It stays while the call rings, instead of the normal banner time, and goes away when Windows removes the call notification (answered, declined or stopped ringing). Exact behaviour follows spike C2.
  - [Proposed] A missed call shows as a normal notification card ("Missed call"), so it is on the notifications page later. Exact text follows spike C4.
  - Built only for what the spike shows: if WhatsApp Web sends no call notification, only WhatsApp desktop gets the banner; if neither does, this requirement moves back to the backlog.

**Fixes [v1.1]**

- FR-27. The artwork shows for the first song after WinIsland starts, not only from the second song on. Seen in your v1.0 tests. This is what AC-2 already asks for. [Confirmed]

## 7. Non-functional requirements

- Performance: no visible frame drops in the animation, near-zero CPU when idle, no measurable FPS impact on games (we will measure).
- Reliability: no crash if Spotify, WhatsApp, or Discord are closed. Recover if the notification listener or media session drops.
- Privacy: no network calls in MVP. No message content stored. No screen capture, process inspection, or memory reading.
- Safety with games: no injection, no hooks into game processes, so anti-cheat should have nothing to flag. We still cannot promise this for every game.
- Accessibility: keyboard-operable settings, readable contrast. The island itself is glanceable, not a full accessibility surface. Animations follow the Windows "Animation effects" setting: when it is off, states change instantly. [Confirmed 31 Oct 2026, from the design spec]
- [v1.1] Size: any change to the island's size is confirmed with you first and must not make it much bigger. [Confirmed 1 Nov 2026] The only v1.1 size change is the expanded island at 420 x 190 (FR-21).
- [v1.3] Size: the size setting (FR-32) is the one approved size change in v1.3. The largest size, Extra large, is 130 percent of today's island. [Confirmed 2 Nov 2026]
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

[v1.3] States to add in the prototype, before any v1.3 code:

12. The 3 app icon concepts, at large size and at 16, 32 and 48 pixels (FR-34).
13. The island at Small, Medium, Large and Extra large, compact and expanded (FR-32).
14. Page dots fading in on expand and on a page change, and out after 1.5 seconds (FR-31).
15. The unread dot on the clock and on the compact music pill (FR-33).
16. The Discord logo on a blurple circle, on a card and on the banner (FR-30).

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
- U11. [v1.2] WhatsApp Web in a plain Chrome tab, and in Brave. Not tested; you use the Chrome app.
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
- [v1.4 draft] A chat with more messages than the stack can count: it shows "99+". [Proposed]
- [v1.4 draft] Clearing from Windows fails (for example access was turned off): the island still hides it, and the error goes to the log. [Proposed]
- [v1.3] Discord with its in-game overlay: in an exclusive fullscreen game, Discord shows its message in its own overlay and sends nothing to Windows, so the island cannot show it. In borderless or windowed games, and without a game, Discord messages reach the island as before (AC-4). Found in your test on 2 Nov 2026; accepted, nothing to build. [Confirmed 2 Nov 2026]
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

### Acceptance criteria (v1.2)

- AC-17. With WhatsApp desktop fully closed and WhatsApp Web open, a message shows sender and preview within 2 seconds, and "You may have new messages" never shows.
- AC-18. With both open, each message shows once (one banner, one card), from WhatsApp Web; reading it in Web or in desktop removes it.
- AC-19. Notifications from other Chrome sites do not show.

### Acceptance criteria (v1.3)

- AC-20. Every Discord notification, on the banner and on a card, shows the white Discord logo on a blurple circle, also right after WinIsland starts.
- AC-21. On expanding, the page dots show and fade out after 1.5 seconds; a page change shows them again and they fade out 1.5 seconds later. Moving the pointer onto the island does not show them.
- AC-22. Choosing each of the four sizes in Settings resizes the island at once, in compact, expanded and banner states, and the choice is still there after a restart. Medium looks the same as v1.2.
- AC-23. A notification that arrives while the island is closed puts a white dot next to the clock after its banner ends. Opening the notifications page removes the dot. Reading the message in WhatsApp or Discord also removes it when Windows removes that notification.
- AC-24. WinIsland.App.exe, the Start menu shortcut and the Settings window show the chosen icon.
- AC-25. Idle memory stays under 150 MB working set with the v1.3 features (G4).

### Acceptance criteria (v1.4, draft)

- AC-26. Three messages from one chat and one from another give two cards on the notifications page: the first with the newest message and "3 messages", the second with its single message. Reading the first chat in WhatsApp removes its whole card.
- AC-26b. Five messages from one chat, then clicking its card, shows "5 messages" and the last 3 messages with their times. Clicking again shows the list. The game keeps keyboard focus throughout.
- AC-27. With "Also clear from Windows notification center" off, swiping a banner leaves the notification in the Windows notification center. With it on, the swiped notification is gone from the notification center and from the notifications page.
- AC-28. (If FR-37 is built) Choosing Glass shows a blurred, tinted island over the desktop and over a borderless game, the game keeps keyboard focus, and the clock stays readable over a bright and a dark scene. Solid looks the same as v1.3.
- AC-29. (If FR-38 is built) An incoming WhatsApp call shows the call banner within 2 seconds, with the caller's name, and the banner goes away when the call stops ringing.
- AC-30. Idle memory stays under 150 MB working set with the v1.4 features (G4).

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

v1.3 [Confirmed 2 Nov 2026]:

- Phase 10: Prototype design pass for the v1.3 states (section 8), then your approval and your pick of the icon.
- Phase 11: Discord logo (FR-30), page dots (FR-31), unread dot (FR-33).
- Phase 12: Size setting (FR-32) and app icon (FR-34).
- Then release `v1.3.0-beta.1`, which also carries the WhatsApp logo fix and the README uninstall steps already on main.

v1.4 [Proposed]:

- Phase 13: spikes G1 to G5 (glass) and C1 to C5 (calls), run by you; at the same time this PRD and a prototype pass for stacks, the new switch, and (after the spikes) the glass look and the call banner.
- Phase 14: stacks per chat (FR-35) and clearing from Windows (FR-36).
- Phase 15: glass look (FR-37) and call banner (FR-38), only what the spikes allow.
- Then release `v1.4.0-beta.1`.

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
13. v1.2, 1 Nov 2026 [Confirmed]: add WhatsApp Web; Web comes first; decide from the notifications themselves (matching chat and text) instead of checking whether WhatsApp desktop runs, which would be process inspection. Spike W1 to W7 in `docs/spike-results.md`.
14. v1.2, 1 Nov 2026: WhatsApp Web notifications show the WhatsApp logo, borrowed from WhatsApp desktop, because the Chrome app sends none (your request). Released as v1.2.0-beta.1 the same day.
15. 2 Nov 2026 [Confirmed]: your new requests are split in two batches of about five. v1.3: app icon, size setting, page dots that hide, unread dot, Discord logo. v1.4: glass or blur look (needs a spike), grouping notifications by category, removing a notification from Windows when hidden, and an incoming call banner without buttons (needs a spike: whether WhatsApp sends call notifications to Windows at all). Answering calls and replying to messages are not possible with the notification listener and stay out (section 4). The installer (backlog F7) waits for a later release.
16. v1.3 answers, 2 Nov 2026 [Confirmed]: sizes 85/100/115/130 percent, in the Settings window only; page dots show on expand and on page change and fade after 1.5 seconds; the unread dot is small, white, right of the clock; Discord blurple #5865F2; 3 icon concepts to pick from.
17. v1.3 approved by you on 2 Nov 2026: the PRD with all its proposals, and app icon concept B, "Mono pill".
18. v1.3 released as v1.3.0-beta.1 on 2 Nov 2026 (your request). Next: the v1.4 batch (decision 15).
19. v1.4 answers, 2 Nov 2026 [Confirmed]: grouping means stacking messages per chat; clearing from Windows is a Settings switch, off by default; glass is a Solid or Glass choice; spikes for glass and calls run alongside the PRD and the two sure features.
