# WinIsland Backlog (Next Release)

Status: notes only, not a PRD. Written 30 Oct 2026 after MVP v1.0 was approved. When you are ready, these become a separate PRD, and the design goes into the same design artifact as v1.0.

Labels: **[Confirmed]** you asked for it. **[Verified]** checked against Microsoft docs. **[Unverified]** to be tested. **[Proposed]** my suggestion.

## Features you want next

### F1. Recent notifications when idle [Confirmed]

When the island is idle and there were earlier notifications, clicking it shows the 3 most recent ones, the same list as v1.0 (newest first, all apps).

- Fixes the v1.0 gap: the list can only be opened from the banner, which disappears after 6 seconds.
- Changes the v1.0 rule that clicking does nothing without music. It would do nothing only when there are no recent notifications.
- Open question: how long does a notification stay in the list if you never read it in the app? Memory only, as in v1.0.

### F2. Music controls plus notifications in the expanded island [Confirmed]

While music plays and notifications exist, expanding shows the music controls with the 3 most recent notifications underneath.

- The expanded island gets taller. Needs a design pass for height and spacing.
- Open question: does tapping the banner still open the list on its own, or does it open this combined view?

### F3. Shuffle and repeat buttons [Confirmed]

- [Verified] The Windows media API has shuffle and repeat commands, and each session reports whether they are enabled.
- [Unverified] Whether the Spotify desktop app supports them through that API. The Spotify Web API is excluded by your decision.
- [Proposed] Test in the technical spike. Show the buttons only if Spotify supports them.

### F4. Open the real app from a notification [Confirmed]

Clicking a notification takes you to WhatsApp or Discord.

- [Unverified] This may pull focus away from the game, or minimize it. Needs testing in borderless mode with your four games.
- [Unverified] Whether Windows lets us open the exact chat, or only the app.
- [Proposed] Make it an option, so tapping can stay read-only.

### F5. Glass or blur look [Confirmed]

A setting to switch the island from solid black to a glass or blur style, so it is easier to see in games.

- [Proposed] Frosted dark glass: a dark tint at roughly 40 to 55 percent opacity, strong background blur, and a thin light edge.
- [Proposed] Text: keep white for the clock and titles, and light grey for secondary text. Dark text on glass fails over dark game scenes, and over bright scenes the dark tint keeps white text readable.
- [Unverified] Windows 11 supports blur and acrylic backdrops, but doing it on a transparent, pill-shaped, always-on-top window may have limits. Needs a spike.
- Test over bright and dark scenes in your four games before approving.

## Other ideas noted

- Group notifications by category, like the iPhone.
- Optional setting to also remove a notification from Windows when you hide it. v1.0 never deletes anything from Windows.
- Dot indicator on the idle island when there are unread notifications. May be unnecessary once F1 exists.

## Suggested order [Proposed]

1. F1 and F2, because they build on the v1.0 list.
2. F5, after a blur spike.
3. F3 and F4, after the spike results on Spotify and app launching.
