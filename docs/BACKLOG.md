# WinIsland Backlog (Next Release)

Status: notes only, not a PRD. Written 30 Oct 2026 after MVP v1.0 was approved. Updated 1 Nov 2026 with your additions and changes after using v1.0.0-beta.1. When you are ready, these become a separate PRD, and the design goes into the same design artifact as v1.0.

Labels: **[Confirmed]** you asked for it. **[Verified]** checked against Microsoft docs or tested. **[Unverified]** to be tested. **[Proposed]** my suggestion.

Size rule [Confirmed]: the island may change size for these features, but every size change is confirmed with you first, and it must not get much bigger.

## Features you want next

F1 to F5 and the first-song artwork issue moved into v1.1 of `docs/PRD.md` on 1 Nov 2026 (FR-21 to FR-27). The PRD is now the source of truth for them; the notes below stay for history. F6 stays here.

### F1. Paused music stays one click away [Confirmed]

Today, when Spotify is paused, the island goes back to the clock, and the only way to play again is to open Spotify. Instead:

- The idle island still shows the clock.
- Clicking it (or Ctrl+Alt+I) expands it to the music controls, in the paused state, so you can press play right there.
- Changes the v1.0 rule that clicking the clock does nothing.

### F2. Empty music view when there is nothing to show [Confirmed]

When Spotify is closed and there is no last song, the expanded music view shows an empty default instead of nothing.

- [Proposed] A plain artwork placeholder, "Not playing" as the title, and dimmed controls that do nothing.
- [Confirmed 1 Nov 2026] While Spotify is open but stopped, its media session usually still reports the last song: show that song, paused. Once Spotify is closed there is no session left, so the controls could not work; show the empty view.
- [Unverified] What Spotify reports after it has been open a long time with nothing played. Check during the build.

### F3. Two pages in the expanded island: music and notifications [Confirmed]

Replaces the old F1 (recent notifications when idle) and F2 (music controls plus notifications) ideas. Also covers your wish to keep notifications around, like on the iPhone, instead of only the quick +N badge.

- Expanding opens the music page (playing, paused, or empty, see F1 and F2). Clicking a notification banner opens the notifications page.
- The notifications page shows the 3 most recent notifications, newest first, all apps, the same list as v1.0. Memory only, as in v1.0.
- Switching pages is vertical: mouse scroll wheel or two-finger swipe up and down on a touchpad, with small page dots at the right edge.
- Hotkeys Alt+Up and Alt+Down switch pages. Like the music keys, they are registered only while the island is expanded, so they do not take those keys from games or other apps the rest of the time.
- Size [Confirmed]: both pages are 420 x 190 (today the music view is 420 x 168, the list is up to 420 x 190). Same size on both pages, so switching only slides the content and the island never jumps in size.
- [Proposed] An empty notifications page says "No notifications".
- [Proposed] A single notification shows its full text, up to 3 lines, in its card.
- Needs a design pass in the prototype: page dots, slide motion, spacing at 190 high.

### F4. Colours from the artwork [Confirmed]

The music view takes its colours from the song's artwork, similar to your reference (dark red island for a red cover), not an exact copy.

- Solid dark background tinted with the artwork's main colour [Confirmed]. Not see-through, so text stays readable over bright and dark games.
- Accent colour for the play or pause button, previous and next, the progress bar and the equalizer. You said this part can come later if it is hard. [Confirmed 1 Nov 2026] Do it in the same step; picking a brighter version of the same colour is not much extra work.
- [Proposed] Text stays white and light grey, and the background is darkened until it meets the normal contrast rule for small text (4.5 to 1).
- [Proposed] No artwork means black, as in v1.0. When the song changes, the colour fades over.
- [Proposed] The colour picking is plain logic in Core, with unit tests.

### F5. Notification cards [Confirmed]

Each notification sits on its own rounded grey card inside the black island, like your reference: app icon, sender in bold, preview below, time on the right.

- Applies to the notifications page (F3). [Confirmed 1 Nov 2026] Also to the banner, so they look the same.
- Exact card colour, radius, and spacing get decided in the prototype design pass.

### F6. Glass or blur look [Confirmed]

A setting to switch the island from solid black to a glass or blur style, so it is easier to see in games.

- [Proposed] Frosted dark glass: a dark tint at roughly 40 to 55 percent opacity, strong background blur, and a thin light edge.
- [Proposed] Text: keep white for the clock and titles, and light grey for secondary text. Dark text on glass fails over dark game scenes, and over bright scenes the dark tint keeps white text readable.
- [Unverified] Windows 11 supports blur and acrylic backdrops, but doing it on a transparent, pill-shaped, always-on-top window may have limits. Needs a spike.
- Open question: how it combines with the artwork colours from F4.
- Test over bright and dark scenes in your four games before approving.

## Removed

- **Shuffle and repeat buttons** (was F3). Removed 1 Nov 2026 by you: the controls stay as they are now, to keep the view clean.
- **Open the real app from a notification** (was F4). Removed 1 Nov 2026 by you: an accidental click during a game would pull you out of it. You plan a different feature in its place later.
- **Recent notifications when idle** and **music controls plus notifications** (were F1 and F2). Replaced by the two pages in F3.

## Known issues to fix

- Artwork sometimes does not show for the first song after WinIsland starts (seen in your v1.0 tests).

## Other ideas noted

- Group notifications by category, like the iPhone.
- Optional setting to also remove a notification from Windows when you hide it. v1.0 never deletes anything from Windows.
- Dot indicator on the idle island when there are unread notifications. May be unnecessary once F3 exists.

## Suggested order [Proposed]

1. The first-song artwork issue, F1 and F2, because they are small and fix daily annoyances.
2. F3 and F5 together, after the prototype design pass, because both change the expanded island.
3. F4, after F3, so the colours are designed on the final layout.
4. F6, after a blur spike.
