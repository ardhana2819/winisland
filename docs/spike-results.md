# Phase 1 Spike Results

Branch: `spike/platform-checks`. Spike programs are in `spikes/`, identity packaging in `packaging/`.

Labels: **Pass**, **Fail**, **Partly**, **Not tested**. "Claude ran" means Claude Code ran it on Diva's PC during the session. "Diva ran" means Diva ran it and reported the result. Nothing is marked Pass or Fail without a real run.

Privacy: this file records behavior only, never message content.

Test machine (Diva's PC): Intel Core i5-9400F (6 cores), NVIDIA GeForce GTX 1650 Super (4 GB), 16 GB DDR4 2666 MHz, Windows 11 Pro 24H2 (build 26100), 1920x1080 in games.

## Results

| Test | Result | Evidence | What it means for the PRD |
|---|---|---|---|
| S1. Does the notification listener need package identity? | **Pass** (answered) | **Unpackaged** (Claude ran, 31 Oct 2026): identity NO, access `Allowed`, polling with `GetNotificationsAsync` returned 2 existing toasts with app name, AUMID and text. `NotificationChanged` subscribe FAILED, `COMException 0x80070490` (Element not found). **With identity** (Diva approved the admin certificate step via UAC, Claude ran the rest): identity YES (`WinIsland.Spike.NotificationProbe_1.0.0.0_x64__2k9dtv6xm75sy`), access `Allowed`, `NotificationChanged` subscribed OK. A local test toast sent from PowerShell arrived as `ADDED via=event` with 0.1 s delay, and removing it from Windows arrived as `REMOVED via=event` within milliseconds. | Identity is **not needed to read** notifications, but **is needed for the instant push event**. Without identity the app must poll (up to 1 s delay, still inside the 2 s goal G2). Recommendation for Phase 4: use the event when identity is present and fall back to polling when it is not, so the app works either way and setup friction (PRD risk 4) becomes optional. |
| U1. WhatsApp and Discord reach the listener during a game with DND on | **Pass** (Do Not Disturb turned on by hand) | Diva ran, 31 Oct 2026, NotificationProbe with identity: with Do Not Disturb on, WhatsApp notifications still arrived in the probe (`ADDED via=event`); Windows showed no banner. The automatic "when playing a game" rule was not checked separately; it only decides when DND turns on, not what the listener receives. Discord under DND: not reported separately. | The listener keeps working during DND, so section 13 "DND on" is covered. DND is also a working way to hide Windows banners while WinIsland still gets the notifications (fallback for U2). |
| U2. Listener still receives them with banners off | **Not tested yet** (first run tested a different setting) | Diva ran, 31 Oct 2026: the first run used **WhatsApp's own in-app setting** "Show notification banner: Never". With that, WhatsApp creates no Windows notification at all: nothing reached the probe (3 senders, 3 messages each, also not via polling) and nothing appeared in the Windows notification center. With the in-app setting back on, messages arrived at once. The **Windows** setting (Settings > System > Notifications > app > "Show notification banners" off, notification center on), which is what U2 asks about: Not tested yet. | First-run setup (README) must tell users to keep WhatsApp's and Discord's own notification settings on and hide banners only in Windows (or with DND, see U1). AC-4 depends on the Windows-setting retest. |
| U3. Apps remove the notification after reading, listener reports removal | **Pass** (read on the PC) | Diva ran, 31 Oct 2026, NotificationProbe with identity, screenshot: after opening and reading the chat in the app, the notification was removed from Windows and the probe printed `REMOVED ... via=event`. **WhatsApp desktop**: ids 468, 469, 473, 476 removed after reading. **Discord desktop**: two DMs (474, 475) removed together when the DM was opened. Reading on the phone instead: Not tested. | FR-16 and AC-5 "dismiss on read" can be built for both apps on PC reads. |
| U4. Topmost non-activating pill stays visible and game keeps focus | **Pass** (3 of the 4 PRD games, AC-3 needs 2) | Claude ran: launching OverlayProbe did not change the foreground window. Diva ran the rest on 31 Oct 2026, with screenshots. **Forza Horizon 6** (1920x1080, game counter 55 FPS, GPU 99 %): pill visible from launch through loading to racing; Forza shows a pointer while driving, so the pill could be clicked directly (18 clicks, "Focus kept: YES"); pressing W right after a click moved the car at once, even when clicking while driving. **This proves the game keeps keyboard focus.** Smooth in fast driving, a busy city at night, and with Spotify playing. **Zenless Zone Zero** (fullscreen, resolution not noted): pill visible from launch through combat; clicking the pill and pressing hotkeys during combat did not disturb combat. The game stuttered and paused for a while right after launch (no freeze), then ran smoothly; closing and reopening the pill mid-game stayed smooth. No anti-cheat warning. **Honkai Star Rail**: windowed, pill visible, "Focus kept: YES", stable. First fullscreen attempt at 2560x1440 lagged heavily and the game once showed "stopped working". Retest at 1920x1080 fullscreen: smooth from launch to combat (including auto battle at 2x speed), with Spotify playing, skipping tracks via the Start menu, and closing and reopening the pill mid-game. No anti-cheat warning. **GTA San Andreas Definitive Edition** (extra game, windowed fullscreen): visible and smooth through a long session; it hides the cursor, so Start was needed before clicking. **Skyrim**: Not tested (not installed; skipped by Diva). **Forza low memory warning**: appeared once with the pill on, game kept running; not seen again in later Forza, HSR or ZZZ sessions. Claude measured the pill's GPU memory with the Windows "GPU Process Memory" counters: 2.0 MB dedicated, 1.1 MB shared (about 0.05 % of the 4 GB card). | FR-3 and AC-3 are met with the non-activating window technique: visible over the games, game keeps focus. The HSR 1440p lag was not isolated (no run at 1440p without the pill), but it went away at 1080p with the pill on, which points to GPU load on this machine rather than the overlay. The pill is unlikely to cause low memory warnings, but Diva wants the real island as light as possible: Phase 2 measures the real app's RAM, GPU memory and CPU (G4) and compares lighter rendering options. **Clicking the island is impractical in games that hide the cursor** (GTA), so hotkey 1 (FR-5) is the main way to expand it in game. |
| U5. Hotkeys fire in games, conflicts | **Pass** | Claude ran: Ctrl+Alt+I, Ctrl+Alt+H, Alt+X all free on this PC; while the probe ran, a second registration got error 1409, proving the probe owns them. Diva ran in game, with screenshots: all three registered and counted presses in **Forza Horizon 6** (I 13x, H 12x, X 16x), **Honkai Star Rail** (I 2x, H 3x, X 5x), **GTA San Andreas DE** (I 10x, H 10x, X 14x), and **Zenless Zone Zero** (all worked during combat). GTA also reacts to the Alt key on its own (shows stats) when Alt+X is pressed; Diva judged this not a blocker. | FR-5 defaults work. Known limitation: modifier keys (Alt, Ctrl) still reach the game before the full combination is caught, so a game that binds Alt alone also reacts. |
| U6. Spotify accepts seek | **Partly** | Claude ran `MediaProbe --report` (read only): session `Spotify.exe` reports `seek(playbackPosition)=True`, seek range 00:00 to 08:28 for the current track. Actually seeking: Not tested (it changes Diva's music, so Diva runs it). | Spotify advertises seek support. If the seek command works, FR-11 can be in MVP. |
| U7. WhatsApp desktop notification identity, grouping, truncation | **Pass** | Diva ran, 31 Oct 2026, `--show-text`, screenshots (content not recorded). App name `WhatsApp`, AUMID `5319275A.WhatsAppDesktop_cv1g1gvanyjgm!App`; Discord: `Discord`, AUMID `com.squirrel.Discord.Discord`. Both arrive `via=event` with 0.0 to 0.2 s delay. **Text layout**: `text[0]` = chat name (person or group), `text[1]` = message; in groups `text[1]` starts with the sender name and a colon. Image-only messages: one text, "WhatsApp Image <date> at <time>". **Long text is not cut**: a long message arrived complete at 3,214 characters. **One notification per chat**: three short messages in a row from one chat each removed the previous notification and added a new one with a new id (`REMOVED 511`, `ADDED 512`, `REMOVED 512`, `ADDED 513`, each within about 50 ms); a long message was also replaced by the next message in the same chat. Discord instead kept two DMs as two separate notifications (474 and 475). Emoji show as `??` in the console (display only). | FR-13 maps `text[0]` to sender or chat and `text[1]` to preview. FR-15 full text up to 3 lines is possible; WinIsland does the cutting. **Design input for Phase 4**: a WhatsApp `REMOVED` can mean "read" (U3) or "replaced by a newer message in the same chat". Proposed rule, to be approved before Phase 4: a removal followed within 1 s by a new notification from the same app and chat is a replacement (keep the older message in the list, add the new one); a removal with no follow-up is a read. Image-only messages use the "app and sender only" case from section 13. |
| B3 (backlog, observe only). Spotify shuffle and repeat | **Partly** | Claude ran `MediaProbe --report`: `shuffle=True repeat=True` enabled, shuffle currently on, repeat `None`. Commands: Not tested. | Observe only. Input for backlog F3. |
| Idle CPU and memory of the test window | **Pass for CPU, borderline for memory** | Claude ran `measure-idle.ps1 -ProcessName OverlayProbe -Seconds 60` (6 logical cores, Release build, desktop idle): CPU average 0.01 %, peak 0.50 %. Working set average 101.3 MB, peak 101.4 MB. Private memory average 52.7 MB. | CPU meets G4. Working set is just over the proposed 100 MB target. Most of it is shared WPF and .NET libraries; private memory is about half. Phase 2 should measure the real app and decide whether the target means working set or private memory. |

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

## Results to record

Send these to Claude, or fill them in here:

| Test | Your result |
|---|---|
| U2: WhatsApp arrived? Discord arrived? delays | |
| U3: removed after read on PC / on phone, per app | |
| U7: app name and AUMID, separate or replaced, long text cut off?, group layout | |
| U1: game, DND auto or by hand, messages arrived?, delays | |
| U4 Forza Horizon 6: visible, focus kept | |
| U4 Skyrim: visible, focus kept | |
| U4 Honkai Star Rail: visible, focus kept | |
| U4 Zenless Zone Zero: visible, focus kept | |
| U5 per game: hotkey counts, game reactions | |
| U6: seek accepted? did it move? | |
| B3: shuffle and repeat accepted? did they change? | |
