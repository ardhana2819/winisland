# Phase 1 Spike Results

Branch: `spike/platform-checks`. Spike programs are in `spikes/`, identity packaging in `packaging/`.

Labels: **Pass**, **Fail**, **Partly**, **Not tested**. "Claude ran" means Claude Code ran it on Diva's PC during the session. "Diva ran" means Diva ran it and reported the result. Nothing is marked Pass or Fail without a real run.

Privacy: this file records behavior only, never message content.

Test machine (Diva's PC): Intel Core i5-9400F (6 cores), NVIDIA GeForce GTX 1650 Super (4 GB), 16 GB DDR4 2666 MHz, Windows 11 Pro 24H2 (build 26100), 1920x1080 in games.

## Results

**Summary (31 Oct 2026): all Phase 1 questions are answered.** S1, U1 to U7 and B3 pass. Main design inputs: identity is optional (event with identity, polling without); hotkey 1 is the main way to expand the island in games that hide the cursor; WhatsApp replaces its notification per chat, so a removal is not always a read; users must keep the apps' own notification settings on and hide banners only in Windows. See the rows below for evidence.

| Test | Result | Evidence | What it means for the PRD |
|---|---|---|---|
| S1. Does the notification listener need package identity? | **Pass** (answered) | **Unpackaged** (Claude ran, 31 Oct 2026): identity NO, access `Allowed`, polling with `GetNotificationsAsync` returned 2 existing toasts with app name, AUMID and text. `NotificationChanged` subscribe FAILED, `COMException 0x80070490` (Element not found). **With identity** (Diva approved the admin certificate step via UAC, Claude ran the rest): identity YES (`WinIsland.Spike.NotificationProbe_1.0.0.0_x64__2k9dtv6xm75sy`), access `Allowed`, `NotificationChanged` subscribed OK. A local test toast sent from PowerShell arrived as `ADDED via=event` with 0.1 s delay, and removing it from Windows arrived as `REMOVED via=event` within milliseconds. | Identity is **not needed to read** notifications, but **is needed for the instant push event**. Without identity the app must poll (up to 1 s delay, still inside the 2 s goal G2). Recommendation for Phase 4: use the event when identity is present and fall back to polling when it is not, so the app works either way and setup friction (PRD risk 4) becomes optional. |
| U1. WhatsApp and Discord reach the listener during a game with DND on | **Pass** (Do Not Disturb turned on by hand) | Diva ran, 31 Oct 2026, NotificationProbe with identity: with Do Not Disturb on, WhatsApp notifications still arrived in the probe (`ADDED via=event`); Windows showed no banner. The automatic "when playing a game" rule was not checked separately; it only decides when DND turns on, not what the listener receives. Discord under DND: not reported separately. | The listener keeps working during DND, so section 13 "DND on" is covered. DND is also a working way to hide Windows banners while WinIsland still gets the notifications (fallback for U2). |
| U2. Listener still receives them with banners off | **Pass** | Diva ran, 31 Oct 2026. First run used **WhatsApp's own in-app setting** "Show notification banner: Never": WhatsApp then creates no Windows notification at all, so nothing reached the probe or the notification center. Retest with the **Windows** setting (Settings > System > Notifications > app > "Show notification banners" off, "notification center" on; in-app settings back on), apps minimized: WhatsApp messages arrived (`ADDED via=event`, delay 0.0 s, pasted console output); Diva reported Discord passing the same way. | AC-4 can be met. First-run setup (README) must say: keep WhatsApp's and Discord's own notification settings on, and turn banners off only in Windows Settings (or use DND, see U1). |
| U3. Apps remove the notification after reading, listener reports removal | **Pass** (read on the PC) | Diva ran, 31 Oct 2026, NotificationProbe with identity, screenshot: after opening and reading the chat in the app, the notification was removed from Windows and the probe printed `REMOVED ... via=event`. **WhatsApp desktop**: ids 468, 469, 473, 476 removed after reading. **Discord desktop**: two DMs (474, 475) removed together when the DM was opened. Reading on the phone instead: Not tested. | FR-16 and AC-5 "dismiss on read" can be built for both apps on PC reads. |
| U4. Topmost non-activating pill stays visible and game keeps focus | **Pass** (3 of the 4 PRD games, AC-3 needs 2) | Claude ran: launching OverlayProbe did not change the foreground window. Diva ran the rest on 31 Oct 2026, with screenshots. **Forza Horizon 6** (1920x1080, game counter 55 FPS, GPU 99 %): pill visible from launch through loading to racing; Forza shows a pointer while driving, so the pill could be clicked directly (18 clicks, "Focus kept: YES"); pressing W right after a click moved the car at once, even when clicking while driving. **This proves the game keeps keyboard focus.** Smooth in fast driving, a busy city at night, and with Spotify playing. **Zenless Zone Zero** (fullscreen, resolution not noted): pill visible from launch through combat; clicking the pill and pressing hotkeys during combat did not disturb combat. The game stuttered and paused for a while right after launch (no freeze), then ran smoothly; closing and reopening the pill mid-game stayed smooth. No anti-cheat warning. **Honkai Star Rail**: windowed, pill visible, "Focus kept: YES", stable. First fullscreen attempt at 2560x1440 lagged heavily and the game once showed "stopped working". Retest at 1920x1080 fullscreen: smooth from launch to combat (including auto battle at 2x speed), with Spotify playing, skipping tracks via the Start menu, and closing and reopening the pill mid-game. No anti-cheat warning. **GTA San Andreas Definitive Edition** (extra game, windowed fullscreen): visible and smooth through a long session; it hides the cursor, so Start was needed before clicking. **Skyrim**: Not tested (not installed; skipped by Diva). **Forza low memory warning**: appeared once with the pill on, game kept running; not seen again in later Forza, HSR or ZZZ sessions. Claude measured the pill's GPU memory with the Windows "GPU Process Memory" counters: 2.0 MB dedicated, 1.1 MB shared (about 0.05 % of the 4 GB card). | FR-3 and AC-3 are met with the non-activating window technique: visible over the games, game keeps focus. The HSR 1440p lag was not isolated (no run at 1440p without the pill), but it went away at 1080p with the pill on, which points to GPU load on this machine rather than the overlay. The pill is unlikely to cause low memory warnings, but Diva wants the real island as light as possible: Phase 2 measures the real app's RAM, GPU memory and CPU (G4) and compares lighter rendering options. **Clicking the island is impractical in games that hide the cursor** (GTA), so hotkey 1 (FR-5) is the main way to expand it in game. |
| U5. Hotkeys fire in games, conflicts | **Pass** | Claude ran: Ctrl+Alt+I, Ctrl+Alt+H, Alt+X all free on this PC; while the probe ran, a second registration got error 1409, proving the probe owns them. Diva ran in game, with screenshots: all three registered and counted presses in **Forza Horizon 6** (I 13x, H 12x, X 16x), **Honkai Star Rail** (I 2x, H 3x, X 5x), **GTA San Andreas DE** (I 10x, H 10x, X 14x), and **Zenless Zone Zero** (all worked during combat). GTA also reacts to the Alt key on its own (shows stats) when Alt+X is pressed; Diva judged this not a blocker. | FR-5 defaults work. Known limitation: modifier keys (Alt, Ctrl) still reach the game before the full combination is caught, so a game that binds Alt alone also reacts. |
| U6. Spotify accepts seek | **Pass** | Claude ran `MediaProbe --report`: Spotify reports `seek(playbackPosition)=True`. Diva ran the interactive probe, 31 Oct 2026, screenshot: every command returned ACCEPTED, and Diva reported that all of them, including seek forward and back, really changed Spotify. The screenshot shows the state read back after each command (seek lines had scrolled out of view). `Previous` restarted the track (position 00:00), which is Spotify's normal behavior past the first seconds. | FR-11 seek can be in the MVP, using the mouse-only design from DESIGN_SPEC section 5. |
| U7. WhatsApp desktop notification identity, grouping, truncation | **Pass** | Diva ran, 31 Oct 2026, `--show-text`, screenshots (content not recorded). App name `WhatsApp`, AUMID `5319275A.WhatsAppDesktop_cv1g1gvanyjgm!App`; Discord: `Discord`, AUMID `com.squirrel.Discord.Discord`. Both arrive `via=event` with 0.0 to 0.2 s delay. **Text layout**: `text[0]` = chat name (person or group), `text[1]` = message; in groups `text[1]` starts with the sender name and a colon. Image-only messages: one text, "WhatsApp Image <date> at <time>". **Long text is not cut**: a long message arrived complete at 3,214 characters. **One notification per chat**: three short messages in a row from one chat each removed the previous notification and added a new one with a new id (`REMOVED 511`, `ADDED 512`, `REMOVED 512`, `ADDED 513`, each within about 50 ms); a long message was also replaced by the next message in the same chat. Discord instead kept two DMs as two separate notifications (474 and 475). Emoji show as `??` in the console (display only). | FR-13 maps `text[0]` to sender or chat and `text[1]` to preview. FR-15 full text up to 3 lines is possible; WinIsland does the cutting. **Design input for Phase 4**: a WhatsApp `REMOVED` can mean "read" (U3) or "replaced by a newer message in the same chat". Proposed rule, to be approved before Phase 4: a removal followed within 1 s by a new notification from the same app and chat is a replacement (keep the older message in the list, add the new one); a removal with no follow-up is a read. Image-only messages use the "app and sender only" case from section 13. |
| B3 (backlog, observe only). Spotify shuffle and repeat | **Pass** (observe only) | Diva ran the interactive probe, screenshot: shuffle off ACCEPTED and read back `Shuffle on: False`; repeat cycled None, List, Track, None, each ACCEPTED and read back with the new value. | Backlog F3 (shuffle and repeat buttons) is technically possible with the Spotify desktop app. Not in v1.0. |
| Idle CPU and memory of the test window | **Pass for CPU, borderline for memory** | Claude ran `measure-idle.ps1 -ProcessName OverlayProbe -Seconds 60` (6 logical cores, Release build, desktop idle): CPU average 0.01 %, peak 0.50 %. Working set average 101.3 MB, peak 101.4 MB. Private memory average 52.7 MB. | CPU meets G4. Working set is just over the proposed 100 MB target. Most of it is shared WPF and .NET libraries; private memory is about half. Phase 2 should measure the real app and decide whether the target means working set or private memory. |

### v1.2 spike: WhatsApp Web in Chrome (1 Nov 2026)

Branch `feat/whatsapp-web`. NotificationProbe with identity, without `--show-text`: it printed app, AUMID, ids and text lengths only, plus any line that is a bare site address (none were). Diva ran the four steps; Claude ran the probe and read the output. Diva uses WhatsApp Web installed as an app from Chrome. Brave: not tested (Diva uses Chrome only).

| Test | Result | Evidence | What it means for the PRD |
|---|---|---|---|
| W1. WhatsApp Web reaches the listener and can be told apart | **Pass** | App name `WhatsApp Web`, AUMID `Chrome._crx_hnpfjngllnfapefoaidbinmjnm` (the installed Chrome app), `via=event`, delay 0.0 to 0.1 s. Notification center shows `web.whatsapp.com` under the message, but it is not one of the text lines. | Recognise WhatsApp Web by the app name containing "WhatsApp" when the AUMID is not the WhatsApp desktop one. Other Chrome sites have other names and stay ignored. |
| W2. Text layout | **Pass** | Two lines, same as the desktop app: chat name, then the message; a group message has the same lengths from Web and desktop (12 and 33). | The existing parser works unchanged. |
| W3. One notification per chat | **Pass** | Each new message in a chat removed the previous Web notification and added a new one about 70 ms later (`REMOVED 849`, `ADDED 851`; `REMOVED 851`, `ADDED 854`). | The replacement rule (FR-15) applies to Web as it is. |
| W4. Desktop closed | **Pass, with a catch** | With WhatsApp desktop fully closed, Web sent the real message, and Windows still showed a one-line notification from the desktop app: "You may have new messages" (Diva's screenshot, 25 characters in the probe), every time. | That desktop line has no message in it; it must be ignored, or it shows as an extra card. |
| W5. Both open | **Pass** | Desktop and Web each sent the same message, same lengths, 10 to 80 ms apart, in either order. | Matching by chat and text stops the double card. |
| W6. Read in WhatsApp Web | **Pass** | The Web notification was removed right after reading (`REMOVED 861 via=event`). | Dismiss on read (FR-16) works for Web. |
| W7. Read in WhatsApp desktop | **Observed** | Opening and reading in desktop removed the Web notification, then desktop added one notification with a much longer text (611 characters) for the same group. | A desktop notification for a chat that Web recently covered must be ignored, or this re-sent text shows as a new card. |

### v1.4 spikes: glass look and incoming calls (2 Nov 2026)

Branch `spike/v1.4-glass-and-call`. Waiting for Diva's run; steps are under "How to run the tests" below (G1 to G5, C1 to C5). Results go in the two tables when they are in.

| Test | Result | Evidence | What it means for the PRD |
|---|---|---|---|
| G1. Windows draws a blurred backdrop behind a non-activating, always-on-top pill | Fail with the system backdrop (acrylic), 2 Nov 2026. Second round, same day: `--accent blur` shows a real blur of the wallpaper; `--accent acrylic` also blurs; `--keep-active` stays flat grey | Diva's screenshots: flat grey over the wallpaper and over Spotify, no blur. Status line: "DWM: extend 0x00000000, backdrop 0x00000000", so Windows accepted both calls | Windows draws the system backdrop only while the window is active and shows a flat fallback colour otherwise. The island is never active (FR-3), so the documented way does not work as is |
| G1b. Round 6, the documented way: Windows.UI.Composition host backdrop brush with a smooth rounded clip (`--host`) | Blur yes and pill-shaped, but WPF content missing, 2 Nov 2026 | Diva's screenshots and video over GTA: real blur while the pill is not active, no box ("sudah"), smooth round corners. But the clock and status text never show. While growing, the spring's overshoot went past the 420 wide probe window and was cut square for a moment | Round 7 (tint 0.3, with and without `--gpu`): the pill is no darker and the clock still missing, so the host backdrop hides all WPF content of its window. The square moment is gone with the larger window ("udah aman"). Diva likes the look ("kayak 3d"). Round 8 moves the blur into a second click-through window behind the pill window (BlurWindow.cs): the clock shows again, but a black rim showed around the pill and it could not be clicked (fully transparent pixels pass clicks through). Round 9: tint alpha at least 1 of 255, and no region on the transparent pill window | Decides FR-37: if it blurs while the island is not active and stays pill-shaped, Glass is built on it; if not, FR-37 goes back to the backlog |
| G2. The pill's edges look smooth enough (a window region has no edge smoothing) | Fail in round 2; round 3 no stair steps but a thin grey line | Round 2: "masih terlihat bergerigi putih". Round 3: "udah enggak bergerigi tapi keliatan garis abu abu tipis" | The thin line is the light edge itself; Diva wants no line at all, so round 4 drops it |
| G3. The blur follows the pill when it changes shape, without flicker | Pass with a note, 2 Nov 2026 | Diva: "lumayan mulus" (fairly smooth). Rounds 2 and 3: the whole 420 x 190 window is blurred around the pill, also expanded (square corners) | Round 4: adding `DwmEnableBlurBehindWindow` on top of the accent turned the pill solid white, but the status line showed the window region was set (complex, 525 x 238 = 420 x 190 at 125 percent) and the white was cut to the rounded shape. Round 5 (accent blur, tint 0): the region is set (complex, 525 x 238) but the blur still fills the whole 420 x 190 window with square corners. Confirmed: the accent blur ignores the window region. Blocker for the island, whose window is always sized for Extra large |
| G4. Over a borderless game: blur visible, game keeps focus, no stutter | Pass, 2 Nov 2026 | GTA Vice City Definitive Edition, borderless: blur visible, Diva: "aman" | The accent blur works over a borderless game |
| G5. White text readable over bright and dark game scenes, at tint 0.45 and 0.55 | Pass at 0.45 and 0.15 | Diva: "kebaca kok". 0.15 is still darker than her TranslucentTB taskbar. She prefers `--accent blur` (cleaner) over acrylic | Round 5: tint 0 over a bright GTA street, Diva: "masih bisa terbaca semua". Light enough for her |

| Test | Result | Evidence | What it means for the PRD |
|---|---|---|---|
| C1. WhatsApp desktop: an incoming voice call sends a Windows notification | Not run yet | | |
| C2. WhatsApp desktop: when that notification goes away (answered, declined, missed) | Not run yet | | |
| C3. WhatsApp Web (Chrome app): an incoming call sends a notification | Not run yet | | |
| C4. A missed call leaves a notification, and what it says | Not run yet | | |
| C5. Video call instead of voice call | Not run yet | | |

### Extra findings

- **Media "current session" is not Spotify.** Windows reported 3 sessions (Spotify.exe, Chrome, Brave) and chose Brave as the current one. The product must pick the Spotify session by its app id (`Spotify.exe`), not use Windows' current session (FR-7, FR-10).
- **The push event needs identity.** It works with the sparse package and fails without it. Polling works in both cases.
- **The identity package scripts work end to end on this PC:** certificate created non-exportable in `CurrentUser\My` and trusted in `LocalMachine\TrustedPeople`, package built with `makeappx`, signed with `signtool` by thumbprint (no `.pfx`, no password), registered with `Add-AppxPackage -ExternalLocation`, status `Ok`.

## How to run the tests

Build everything once, in a normal PowerShell:

```powershell
cd "D:\Claude Project\WinIsland\spikes"
dotnet build NotificationProbe\NotificationProbe.csproj -c Release
dotnet build OverlayProbe\OverlayProbe.csproj -c Release
dotnet build MediaProbe\MediaProbe.csproj -c Release
```

The programs are then in each project's `bin\Release\net10.0-windows10.0.26100.0\` folder. Below, `<probe folder>` means that folder for the program named.

### G1 to G5. Glass look (about 15 minutes)

GlassProbe shows a glass pill at the top center: Windows' own blur (acrylic) behind it, a dark tint, a thin light edge, and a clock. Windows draws the blur; nothing captures the screen.

1. Build it once: `dotnet build "D:\Claude Project\WinIsland\spikes\GlassProbe\GlassProbe.csproj" -c Release`
2. Quit WinIsland from the tray first, so the two pills do not sit on top of each other.
3. Run `& "D:\Claude Project\WinIsland\spikes\GlassProbe\bin\Release\net10.0-windows10.0.26100.0\GlassProbe.exe"`.
4. On the desktop, with a colourful window or wallpaper behind the pill:
   - G1: is the pill blurred glass, or plain black or plain grey? Click another window: does the glass stay, or turn flat? (The island is never the active window, so it must stay.)
   - G2: look closely at the round ends. Smooth, or visibly stair-stepped?
   - G3: click the pill a few times. It grows to 420 x 190 and back. Does the blur follow the shape smoothly, or flicker, lag or show black corners? The expanded pill shows a status line; tell Claude what it says.
5. G4: start a game in borderless mode, play a minute with the pill over it. Is the glass still there, does the game keep keyboard focus, any stutter?
6. G5: over a bright scene and a dark scene, is the white clock easy to read? Then close it (right-click) and try a darker tint: add `--tint 0.55` to the command. Compare.
7. If G1 shows no glass, also try `--keep-active` (keeps the frame looking active, no keyboard focus), `--accent acrylic` and `--accent blur` (the older, undocumented blur that does not depend on activation), and say which (if any) shows glass. `--backdrop mica`, `--backdrop tabbed` and `--gpu` are extra options.
8. Right-click the pill to close it. Start WinIsland again from the Start menu.

### C1 to C5. Incoming calls (about 10 minutes, needs a second phone or a friend)

NotificationProbe prints every Windows notification. With `--show-text` it also prints the text, in its own window only, never to a file.

1. Run `& "D:\Claude Project\WinIsland\spikes\NotificationProbe\bin\Release\net10.0-windows10.0.26100.0\NotificationProbe.exe" --show-text`. WinIsland can keep running.
2. With **WhatsApp desktop** open, have someone voice call you. Let it ring about 10 seconds, then **decline** it. Then a second call that you **let ring out** (missed). If you can, a third that you **answer** and hang up after a few seconds.
3. Do the same with only **WhatsApp Web** (the Chrome app) open, WhatsApp desktop fully closed. If Web shows no call at all, say so; that is a result too.
4. One **video call** to either app.
5. For each call, record: whether an `ADDED` line appeared and its `app=`; the **fixed words** of the text (for example "Incoming voice call"), never the caller's name; whether and when a `REMOVED` line came (when you declined, when it stopped ringing, when you answered); and what a missed call left behind.
6. Press Ctrl+C to stop the probe.

### S1. Package identity (about 10 minutes)

Goal: find out whether the `NotificationChanged` event works once the probe has package identity.

1. Open PowerShell with **Run as administrator** and run:
   `& "D:\Claude Project\WinIsland\packaging\New-DevCertificate.ps1"`
   Look for: "Trusted the certificate in LocalMachine\TrustedPeople".
2. Close the admin window. In a **normal** PowerShell run:
   ```powershell
   cd "D:\Claude Project\WinIsland\packaging"
   .\Build-IdentityPackage.ps1 -PackageName WinIsland.Spike.NotificationProbe -ApplicationId NotificationProbe -Executable NotificationProbe.exe -DisplayName "WinIsland NotificationProbe"
   .\Register-IdentityPackage.ps1 -PackagePath .\out\WinIsland.Spike.NotificationProbe.msix -ExternalLocation "..\spikes\NotificationProbe\bin\Release\net10.0-windows10.0.26100.0"
   ```
   Look for: "Built and signed" and "Registered".
3. Run `& "<NotificationProbe folder>\NotificationProbe.exe"` (no arguments) and wait 5 seconds, then press Ctrl+C.
4. Record:
   - The "Package identity:" line (should now say YES).
   - The "NotificationChanged event:" line: "subscribed OK" or "FAILED" with the error code.
   - Whether a test message (send yourself a Discord DM from another account, or any app notification) shows `via=event` or only `via=poll`.
5. Paste those lines to Claude (no message text needed).

Leave the package registered for U1 to U3 and U7. To undo later: `.\Unregister-IdentityPackage.ps1 -PackageName WinIsland.Spike.NotificationProbe`.

### U2. Banners off (about 5 minutes)

You need someone to send you a WhatsApp message and a Discord DM, or a second account.

1. Windows Settings > System > Notifications > **WhatsApp**: turn **Show notification banners** off, keep **Show notifications in notification center** on. Do the same for **Discord**.
2. Run `NotificationProbe.exe` (no arguments) and leave it open.
3. Have one WhatsApp message and one Discord DM sent to you.
4. Look for one `ADDED` line per message with `app="WhatsApp"` or `app="Discord"`, and its `delay=` value.
5. Record: did each one arrive (yes or no), the delay, and `via=event` or `via=poll`.

### U3. Removed after reading (about 5 minutes)

Continue from U2 with the probe still running.

1. After an `ADDED` line for WhatsApp, open WhatsApp desktop and open that chat so the message is read.
2. Wait 10 seconds. Look for a `REMOVED` line with the same `id`.
3. Do the same for Discord (open the DM).
4. Also try reading on your **phone** instead of the PC, for one message each.
5. Record per app: removed after reading on PC (yes or no, seconds), removed after reading on phone (yes or no).

### U7. WhatsApp details (about 5 minutes)

1. Run `NotificationProbe.exe --show-text`. Message text is shown only in this window, never saved.
2. Have one contact send you **3 short messages in a row**.
3. Look at the ids: 3 `ADDED` lines with different ids means separate notifications. `UPDATED`, or `REMOVED` followed by `ADDED`, means WhatsApp replaces one notification.
4. Have them send **one long message** (about 500 characters, for example a pasted paragraph). Compare `textLengths` with the real length, and check if the text ends early.
5. If possible, also receive a message in a **group chat**, and note how the sender and group name are split across `text[0]`, `text[1]`, and so on.
6. Record: the `app=` and `aumid=` values, separate or replaced, longest text length seen and whether it was cut off, and the group chat text layout (describe it, for example "text[0] = group name, text[1] = sender: message"). Do not paste message content.

### U1. During a game with Do Not Disturb (about 10 minutes, one game is enough)

1. Windows Settings > System > Notifications > **Turn on do not disturb automatically**: turn on **When playing a game** and **When using an app in full-screen mode**.
2. Run `NotificationProbe.exe` (no arguments).
3. Start one game in **borderless** mode and play for a minute.
4. Have a WhatsApp message and a Discord DM sent to you while in the game.
5. Alt+Tab out, check the probe for `ADDED` lines and the notification center for the Do Not Disturb icon (a bell with "z").
6. If DND did not turn on automatically in borderless mode, also repeat once with DND turned on by hand (notification center > Do not disturb), to test the DND part on its own.
7. Record: game used, was DND active (auto or by hand), did each message arrive, delay.

### U4 and U5. Overlay and hotkeys in each game (about 5 minutes per game)

Do this in Forza Horizon 6, Skyrim, Honkai Star Rail, and Zenless Zone Zero, each in **borderless** mode.

1. Run `& "<OverlayProbe folder>\OverlayProbe.exe"`. A black pill appears at the top center. It shows hotkey status, for example "Ctrl+Alt+I: OK, pressed 0x".
2. Start the game.
3. **U4 visible:** is the pill visible over the game? Check during gameplay, in menus, and after a loading screen.
4. **U4 focus:** while walking or driving, click the pill 3 times, then keep pressing a movement key right away. Does the character keep moving without clicking back into the game? Read the pill: "Focus kept: YES" or "NO".
5. **U5:** in the game, press Ctrl+Alt+I three times, Ctrl+Alt+H three times, Alt+X three times. Read the counters on the pill. Also note if the game itself reacted to any of these keys.
6. If the pill disappeared behind the game in step 3, close it (right-click the pill), run it again with `OverlayProbe.exe --reassert-topmost`, and repeat steps 3 to 5.
7. Optional, one game: note the in-game FPS with the pill running and after closing it (right-click).
8. Record per game: visible (yes, no, or only with re-assert), focus kept (yes or no), each hotkey count, game reactions, FPS if measured.

Anti-cheat note: OverlayProbe does not touch the game process. It is an ordinary topmost window plus standard Windows hotkeys. Honkai Star Rail and Zenless Zone Zero have anti-cheat. If either game shows a warning, stop and note it.

### U6 and B3. Spotify controls (about 5 minutes)

1. Start Spotify desktop and play a song.
2. Run `& "<MediaProbe folder>\MediaProbe.exe"`.
3. Press each key and, for each one, note what the probe prints (`ACCEPTED` or `REJECTED`) **and** what Spotify actually did:
   - `S` (seek +30 s), then `A` (seek -30 s): did the song position jump?
   - `F` twice: did Spotify's shuffle button turn on and off?
   - `R` three times: did Spotify's repeat button cycle through its states?
   - `P`, `N`, `B`: play/pause, next, previous.
4. Press `Q` to quit.
5. Record per command: accepted or rejected, and whether Spotify really changed.

## Not covered

- Skyrim (U4, U5): not installed, skipped by Diva.
- Reading a message on the phone instead of the PC (U3): not tested.
- HSR at 2560x1440 fullscreen without the pill (control for the first lag): not tested; the lag did not return at 1920x1080 with the pill on.
- Discord under Do Not Disturb (U1): not reported separately.
