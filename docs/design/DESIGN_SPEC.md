# WinIsland Design Spec (v1.1)

Save as `docs/design/DESIGN_SPEC.md`. Save the prototype next to it as `docs/design/prototype.html` (open it in a browser to see and try the real motion). Source: the approved prototype, 30 Oct 2026, updated for v1.1 and approved by Diva on 1 Nov 2026 (section 9). If this spec and the prototype disagree, the prototype wins for look and motion, and the PRD wins for behavior rules. Report the difference to Diva. `winisland-design-boards.html` is a snapshot of v1.0 only; for v1.1 states use the prototype.

All sizes are in device-independent pixels (DIPs, what WPF uses), measured at the base scale for a 2560 DIP wide display. Values were read from the prototype code, so they are exact for the prototype. How well they carry over to WPF is not yet tested.

## 1. Placement and scale

- Top center of the primary monitor, 12 DIPs below the top edge. Not movable.
- Auto scale = display width in DIPs divided by 2560, limited to 0.85 at the low end and 1.25 at the high end. Multiply every size and the 12 DIP offset by it. No manual size setting.
- Dark only. Pill background is solid black `#000000`. No light theme.

## 2. Sizes of each state

| State | Width | Height | Corner radius |
| --- | --- | --- | --- |
| Compact, idle (clock) | 120 | 34 | 17 |
| Compact, music | 280 | 36 | 18 |
| Expanded (music controls) | 420 | 168 | 28 |
| Notification banner | 420 | 72 | 36 |
| Permission prompt | 360 | 52 | 26 |
| Notification list, 2 or 3 items | 420 | 24 + 50 per item + 8 between items (190 for 3 items) | 28 |
| Notification list, 1 item (full text) | 420 | 24 + 22 + 18 per text line (1 to 3 lines) + 6, for example 106 for three lines | 28 |

The corner radius is half the height for the compact and banner states (a true pill). Hotkey conflict banner: 400 wide, 52 high, radius 26. A 20 warning icon in amber #FBBF24, the message at 13, then a Change button styled like the Allow button. It shows for 10 seconds. The prototype estimates text lines at about 44 characters per line; the WPF app measures the real wrapped text instead and uses the same formula (88 for two lines, 106 for three). The "about 98" on design board 8 is superseded by this formula. [Confirmed by Diva, 31 Oct 2026]

## 3. Fonts and text

- Font: Segoe UI Variable, falling back to Segoe UI, then the system font.
- Clock: 15, semibold (600), tabular numbers, 24-hour HH:MM with no seconds, centered.
- Music title in compact: 13, regular, one line, cut with an ellipsis. Changed 31 Oct 2026 (Diva's request): a title that does not fit scrolls sideways instead, in the compact and the expanded view, after a 1.5 s pause at the start of each loop, at about 30 DIPs per second. With Windows animation effects off it is cut with an ellipsis as before.
- Expanded: title 16 semibold, artist 13, time labels 11 with tabular numbers.
- Notification and list: sender 14 semibold, preview 13 (one line with ellipsis, except the single full-text view: up to 3 lines, line height 18). Badge text 12 semibold.
- Permission prompt: text 13. Allow button text 13 semibold.

## 4. Colors

| Use | Color |
| --- | --- |
| Pill background | `#000000` |
| Primary text, icons | `#FFFFFF` |
| Secondary text (artist, preview, time labels) | `#A6ABB6` |
| Progress track | `#3A3A3C` |
| Progress fill | `#FFFFFF` |
| Equalizer bars | `#4ADE80` |
| Notification badge background | `#2C2C2E` |
| WhatsApp icon circle | `#30D158` (v1.4, was `#177A40`) |
| Discord icon circle | `#4752C4` |
| Album art placeholder | gradient 135 degrees, `#7C3AED` to `#06B6D4` (replace with real artwork) |

The prototype also draws a soft shadow under the pill (0 8 30, 40 percent black). This is optional in the real overlay, which is a transparent window. App icons and artwork: the letter circles and the gradient are stand-ins. In the real app, use the app logo from the notification when Windows provides it (fallback: colored circle with the first letter), and the album art from the music player (fallback: the gradient).

## 5. Layout of each state

- **Compact, music:** left padding 6, right padding 12. Artwork 24 square, radius 6. 10 gap. Title fills the middle. Equalizer on the right: three bars, 3 wide, 3 gap, up to 16 high.
- **Expanded:** padding 16, vertical gap 10. Top row: artwork 64 square, radius 12, 14 gap, then title and artist. Middle row: elapsed time, progress bar (4 high, radius 2), total time, 8 gap. Bottom row: previous, play or pause, next, centered, 16 gap. Each button is 44 across with a 22 icon. Play or pause is a white circle with a black icon. The other two have no background. Seek: the progress bar has a 16 high touch area. On hover it grows from 4 to 6 high and shows a white 10 round thumb at the end of the fill. Click or drag to seek. Mouse only: no arrow keys, because the island never has keyboard focus (PRD FR-3, FR-11; confirmed by Diva 31 Oct 2026). If seek is not supported, none of this appears and the bar stays 4 high.
- **Notification banner:** 16 side padding, 12 gap. Icon circle 40 with the app letter, centered bold 16. Sender above preview. Badge on the right (+N), radius 12, padding 3 by 9. The badge is hidden when there is only one.
- **Notification list:** padding 12 by 16, 8 between rows. Each row 50 high with a 36 icon circle, 12 gap. Newest first.
- **Permission prompt:** left padding 20, right padding 10, 12 gap. Allow button: white, black text, radius 16, padding 7 by 16.

## 6. Motion

The whole feel is a spring-like morph, similar to the iPhone Dynamic Island.

- **Shape change** (width, height, corner radius): 0.6 seconds, easing cubic-bezier(0.34, 1.4, 0.56, 1). This curve overshoots slightly and settles.
- **Content swap:** the old content fades out, scales down to 0.88, and blurs by 10 px, over 0.2 to 0.35 seconds. The new content starts 0.14 seconds later and fades in from blur 10 to 0, scale 0.88 to 1, opacity 0 to 1, over 0.3 to 0.5 seconds, with the same easing curve on the scale. Content therefore appears after the shape has started to grow.
- **Hide and show:** the pill fades over 0.3 seconds and scales between 0.4 and 1 while moving 14 up, over 0.5 seconds, with the same spring curve. Hidden means opacity 0 and no mouse interaction.
- **Equalizer:** bars animate between 4 and 16 high, 0.9 seconds, alternating, with delays of 0, minus 0.3 and minus 0.6 seconds. They stop when music is paused.
- **Swipe to hide a banner:** the banner slides 90 to the side while fading out, over 0.2 seconds. A horizontal drag of more than 40 counts as a swipe.
- **Reduced motion:** follow the Windows Animation effects setting. When it is off, skip all of this and change states instantly. The prototype follows the browser setting and has a button to simulate it.
- WPF note, not yet tested: WPF easing curves (KeySpline) may not allow the overshoot values. If not, use a spring or back-ease equivalent and compare with the prototype side by side.

## 7. Timings

- Notification banner: 6 seconds, then hides. Opening the list stops the timer.
- Expanded island or list closes 2 seconds after the pointer leaves it, or immediately when the open island is clicked outside its buttons.
- Progress bar and elapsed time update once per second.
- Clock updates every second but only shows hours and minutes.

## 8. What the prototype shows that is placeholder only

Track names, artist names, sender names, message text, and album art are fake. The monitor selector and the readable zoom view are prototype tools, not part of the app.

## 9. v1.1 changes (PRD FR-21 to FR-26)

Approved by Diva on 1 Nov 2026 after reviewing the prototype. Where this section and sections 1 to 8 differ, this section wins for v1.1.

### Sizes

| State | Width | Height | Corner radius |
| --- | --- | --- | --- |
| Expanded, music page | 420 | 190 | 28 |
| Expanded, notifications page | 420 | 190 | 28 |
| Notification banner | 420 | 72 | 36 (unchanged) |

The separate notification list from v1.0 (the two "Notification list" rows in section 2) is replaced by the notifications page. Both pages are the same size, so switching pages never changes the island's shape.

### Page track and dots

- The two pages sit on a vertical track inside the island, each 190 high. Page 1 (music) is on top, page 2 (notifications) below. Switching moves the track up or down by 190; the island clips what is outside.
- Page dots: at the right edge, 11 from the right, centered vertically, 5 apart. Each dot is 6 wide; the current one is 14 high and white, the other 6 high and white at 35 percent. The click area reaches 5 beyond each dot.
- Music page content keeps 30 free on the right for the dots (16 on the left). On the notifications page the cards run under the dots instead (see Notification cards).

### Music page

- Rows centered vertically with a 14 gap: top row (artwork 64, radius 12, 14 gap, title 16 semibold and artist 13, equalizer on the right), progress row (unchanged from v1.0), controls row (unchanged sizes: three 44 buttons with 22 icons, 16 apart).
- New: the equalizer moves into the top row on the right, in the accent colour. The compact pill keeps its green equalizer.
- New look of the controls: play or pause is a 44 circle filled with the accent at 22 percent, with the icon in the accent. Previous and next have no background and the icon in the accent. Progress fill and the seek thumb use the accent; the track is white at 18 percent.
- Secondary text (artist, time labels) is white at 68 percent over the background colour.
- Paused: same layout, play icon, equalizer stopped.
- Empty view (Spotify closed): artwork placeholder (`#2C2C2E` square with a `#8E8E93` music note), title "Not playing" and no second line (Diva, 1 Nov 2026: cleaner), no progress row, no equalizer, controls at 35 percent opacity and not clickable. Background black.
- Missing artwork while a song plays: the same placeholder, black background, white accent.

### Colours from the artwork

The prototype works these out from the real cover pixels with the steps below; the app does the same in Core.

1. Read the artwork at 64 x 64. For each pixel, colourfulness = saturation x (1 - |2 x lightness - 1|). Each pixel votes for its hue (36 buckets of 10 degrees) with its colourfulness squared, so a small strong colour beats a large pale area.
2. If the average colourfulness is under 0.06 (black and white or grey artwork), the cover is neutral: background hue 240, saturation 0.04, accent white.
3. Otherwise take the winning bucket's weighted hue and saturation.
4. Background: that hue, saturation at most 0.55, lightness 0.16. Darken in steps of 0.01 until the secondary text (white at 68 percent over the background) reaches 4.5 to 1.
5. Accent: same hue, saturation between 0.6 and 0.9, lightness 0.62. Lighten in steps of 0.02 (up to 0.9) until it reaches 3 to 1 against the background.
6. Accent soft (play button fill): the accent at 22 percent.

Results with the prototype's sample covers: red cover gives background `#3F1216` and accent `#E85461`; blue gives `#12273F` and `#5498E8`; a pink shape on cream gives `#3F121D` and `#E15B7B`; black and white stripes give `#27272A` and white. Title contrast is 14.9:1 or more, grey text 7.7:1 or more and accent 4.5:1 or more in all four.

Gradient [Diva, 1 Nov 2026]: the background colour shows fully at the top and fades to black at the bottom: over the background colour lies black at 0 percent at the top, 60 percent at 60 percent of the height, and 100 percent at the bottom. The controls therefore sit on near black, and the accent keeps the artwork's colour. The gradient only darkens, so every contrast above stays at least as high.

Only the open music page is coloured. The compact pill, banner and notifications page stay black. When the song changes, the background and accents fade over 0.45 seconds; moving between pages fades the background and the gradient between the colour and black over the same 0.45 seconds.

### Notification cards

- Card: `#1C1C1E`, radius 18, 52 high, padding 12 on the left and 22 on the right, 10 gap. App icon circle 32. Top line: sender 14 semibold (ellipsis) and, on the right, the arrival time 11 in `#A6ABB6`, 24-hour HH:MM. Second line: preview 13 in `#A6ABB6`, one line with ellipsis. Text never runs past the card: long text is cut with an ellipsis.
- Notifications page: up to 3 cards, 6 apart, centered vertically, newest first. The cards run edge to edge, 10 from the island's sides [Diva, 1 Nov 2026], so their radius 18 follows the island's radius 28 (28 minus 10). The page dots sit on top of the cards' right padding; the wider right padding (22) keeps the time and the preview at least 6 away from the dots.
- One notification: the card sits at the top (10 from the top), grows to fit, padding 12 (22 on the right), and shows up to 3 lines of preview at line height 18.
- None: "No notifications", 13, `#A6ABB6`, centered.
- Banner: the 420 x 72 black pill holds one card inset by 8: 56 high, radius 28 (the pill's 36 minus 8), padding 0 14 0 10, icon circle 36, sender and preview, and the +N badge on the right (`#3A3A3C` so it stands out on the card, radius 12, padding 3 by 9, text 12 semibold). The banner shows no time: it is always "now". The card fills the pill's width and never grows past it.
- App icons: the WhatsApp and Discord logos are centered in their circle, horizontally and vertically. The prototype's letters are stand-ins.
- WhatsApp icon (desktop and Web, Diva 1 Nov 2026): always the `#30D158` circle (v1.4, Diva 2 Nov 2026: brighter, the same green as the call symbol; was `#177A40`) with the white WhatsApp logo at 58 percent of the circle, drawn by WinIsland, whatever logo Windows sends.
- Clicking a card does nothing.

### Motion

- Page switch: the track moves 190 over 0.5 seconds with cubic-bezier(0.3, 1.12, 0.5, 1), a softer spring than the shape change, so the next page does not visibly bounce past the edge. The current dot grows from 6 to 14 over 0.3 seconds.
- One gesture moves one page: scroll steps less than 300 ms apart count as one gesture. Down goes to notifications, up to music. Sideways scrolling is ignored.
- With Windows animation effects off, the page switch and colour fades are instant.

### Colours added in v1.1

| Use | Color |
| --- | --- |
| Notification card | `#1C1C1E` |
| Badge on a banner card | `#3A3A3C` |
| Artwork placeholder | `#2C2C2E` with a `#8E8E93` note (replaces the v1.0 gradient) |
| Progress track on the music page | white at 18 percent |
| Secondary text on the music page | white at 68 percent over the background |
| Page dots | white, and white at 35 percent |

## 10. v1.3 additions

Approved by Diva on 2 Nov 2026 after reviewing the prototype, together with every [Proposed] item in PRD FR-30 to FR-34. Where this section and earlier sections differ, this section wins for v1.3.

### Page dots (FR-31)

- Hidden (opacity 0) while the island is still. They fade in over 0.2 seconds when the island expands and on every page change (scroll, Alt+Up or Alt+Down, a dot click), and fade out over 0.2 seconds 1.5 seconds after the last change.
- The pointer does not show them (Diva, 2 Nov 2026, after testing): only expanding and a page change do. Hidden dots can still be clicked.
- With Windows animation effects off they appear and disappear at once.

### Unread dot (FR-33)

- A 6 x 6 white circle.
- Clock: 12 from the pill's right edge, centered vertically; the clock stays centered in the pill.
- Compact music pill: the last item in the row, 10 after the equalizer (the row's normal gap), so the title gets 16 less room.
- It pops in over 0.35 seconds (scale 0.4 to 1 with the shape spring, opacity over 0.25 seconds) and fades out over 0.25 seconds.

### Discord icon (FR-30)

- Always the `#5865F2` circle with the white Discord logo (Simple Icons, CC0) at 58 percent of the circle, centered, drawn by WinIsland, like WhatsApp. It replaces the `#4752C4` letter circle.

### Island size (FR-32)

| Setting | Scale | Expanded | Clock pill |
| --- | --- | --- | --- |
| Small | 0.85 | 357 x 162 | 102 x 29 |
| Medium (default) | 1.00 | 420 x 190 | 120 x 34 |
| Large | 1.15 | 483 x 219 | 138 x 39 |
| Extra large | 1.30 | 546 x 247 | 156 x 44 |

Sizes are before the monitor's automatic scale (FR-1), which multiplies them. Everything inside scales by the same factor, so every value in this spec stays true in proportion.

A size change grows or shrinks the whole island from its top center over 0.6 seconds with the shape spring, cubic-bezier(0.34, 1.4, 0.56, 1) (Diva, 2 Nov 2026). With Windows animation effects off it changes at once.

### App icon (FR-34), concept B "Mono pill"

On a 256 x 256 grid:

- Tile: rounded square at (11, 11), 234 x 234, radius 54, fill `#0B0B0C`, outline `#3A3A3C` 6 wide.
- Pill: (36, 98), 184 x 60, radius 30, white.
- Artwork square inside the pill: (54, 112), 32 x 32, radius 9, `#000000`.
- Equalizer bars inside the pill, each 8 wide with radius 3, `#000000`: x 166 from y 120 to 144, x 180 from y 112 to 144, x 194 from y 126 to 144.
- The tray icon stays the plain white pill (drawn in code, see `TrayIconImage.cs`), which matches this icon.

## 11. v1.4 additions (draft, not approved yet)

Stacks per chat on the notifications page (PRD FR-35). [Proposed, 2 Nov 2026]

- Page cards are 48 high instead of 52, so three stacks still fit in the 190 high island. The island size does not change.
- A stack shows "N messages" (11, `#A6ABB6`) before the time on the top line, "99+" above 99.
- The cards behind a stack peek out under it, like the iPhone: one card for 2 messages, two for 3 or more.
  - First card behind: `#3A3A3C`, 8 narrower on each side, radius 14, shows 4 below the card.
  - Second card behind: `#2E2E31`, 16 narrower on each side, radius 12, shows 4 below the first.
  - The space below a stack grows by what peeks out (4 or 8), so the 6 gap to the next card stays clear.
- Worst case, three stacks of 3 or more: 3 x 48 + 3 x 8 + 2 x 6 = 180, inside the 190 island.

Opening a stack (PRD FR-35, click to see the last 3 messages). [Proposed, 2 Nov 2026]

- The chat view fills the notifications page, centered, 6 between rows.
- Header, 36 high: a back chevron 20 in `#A6ABB6`, the app icon circle 32, the chat name 14 semibold (ellipsis), and "N messages" 11 in `#A6ABB6` on the right.
- Up to 3 message rows, newest first: `#1C1C1E`, radius 14, 34 high, padding 12 left and 22 right. Text 13 in `#E5E5EA`, one line with ellipsis; time 11 in `#A6ABB6` on the right.
- Total height 36 + 3 x 34 + 3 x 6 = 156, inside the 190 island.
- Clicking anywhere on the view, or Alt+Left, goes back to the list.
- Opening: the list slides 24 to the left while fading out over 0.15 seconds (ease-in). Then the chat view slides in from 40 on the right over 0.5 seconds with the page-switch spring cubic-bezier(0.3, 1.12, 0.5, 1), fully visible halfway. Its header and rows follow, each from 16 to the right over 0.35 seconds with the same spring, 0.04 seconds apart.
- Going back is the mirror image: the chat slides right and out, the list comes in from the left.
- A card and the cards peeking out behind it move and fade as one piece, and the peeking cards always stay behind the card, also mid-animation. [Fixes a grey box Diva saw flash on going back, 2 Nov 2026]
- With Windows animation effects off, the change is instant.

Glass look (PRD FR-37). [Confirmed in spike rounds 5 to 10, 2 Nov 2026]

- Settings "Look": Solid (default, today's `#000000`) or "Glass (beta)", with the note "Glass is in beta: it may not look right in every game." in 12 `#A6ABB6` under the label. The beta label stays after the release (Diva, 2 Nov 2026).
- Glass background: Windows' host backdrop blur of what is behind the island, no tint, no edge line. The blur has the island's exact rounded shape with smooth edges and follows every shape change in the same frame.
- Text on Glass gets a small soft shadow: black, blur radius 3, 1 down, 45 percent opacity. A larger glow (radius 8, 70 percent) looked like a dark box around each line in the prototype. [Diva, 2 Nov 2026]
- Second lines on Glass (message previews, times, "N messages", the back chevron) are white at 85 percent instead of `#A6ABB6`, which disappears over bright games. [Diva, 2 Nov 2026]
- Cards on Glass (banners, notification cards, opened chat rows): white at 14 percent instead of `#1C1C1E`. Peeking stacked cards: white at 10 and 6 percent, drawn only where they peek out below the card, because the card above is see-through.
- Music page on Glass: the artwork colour while a song with artwork is shown; glass when Spotify is closed or there is no artwork.
- Unchanged on Glass: the page dots and the unread dot.
- The prototype imitates the blur with CSS `backdrop-filter: blur(22px) saturate(1.3)`; the app uses Windows' own blur, which looks lighter and smoother. "Game scene: Bright" in the prototype puts a bright scene behind the island to check readability.

Call banner (PRD FR-38). [Proposed, 2 Nov 2026]

- Same size and card as a message banner: 420 x 72, radius 36, card radius 28.
- Left: the WhatsApp icon circle 36. Middle: caller name 14 semibold, under it "Incoming voice call" or "Incoming video call" 13 in `#A6ABB6` (near-white on Glass). No time, no +N badge.
- Right: a green `#30D158` circle 36 with a white phone symbol 20 for a voice call, or a white video camera symbol 20 for a video call. It pulses gently: a green ring grows from 0 to 7 and fades out, every 1.6 seconds. No pulse with Windows animation effects off.
- It shows over message banners and stays until the call notification is removed. The banner that was showing before comes back afterwards.
- The banner card sits 8 inside the island, like a message banner.
- Notifications page while ringing: the call is the top card (48 high, same layout, call circle 30 with a 16 symbol at the right), followed by the 2 newest notifications.

## 12. v1.5 additions (draft, not approved yet)

Volume of the music app (PRD FR-39). [Confirmed 3 Nov 2026: hidden until clicked; horizontal]

- The music page keeps its rows and gaps (14). A speaker button, 44 with a 20 symbol in the accent, sits at the right end of the controls row, vertically centered with the other buttons. Its symbol follows the level: muted at 0, one wave under 50 percent, two waves above.
- Clicking it fills it like the play button (accent at 22 percent) and swaps the progress row for the volume row in the same place: a quiet speaker symbol 14, the slider, a loud speaker symbol 14, 8 apart. The slider has the progress bar's shape (16 high hit area, 4 high track in white at 18 percent, a 10 thumb on hover) but is white for every artwork: symbols, fill and thumb `#FFFFFF`, so it never looks like the song's progress, which stays in the accent (Diva, 3 Nov 2026).
- The two rows share one place and cross over the same way in both directions, like the island's content swap: the leaving row fades out, blurs by 4 and shrinks to 0.96, the arriving row does the reverse, over 0.3 s on the ease curve (Diva, 3 Nov 2026: the same both ways). Instant with Windows animation effects off.
- The volume bar is the progress bar exactly: same length and place (Diva, 3 Nov 2026). The two time labels sit in fixed 30 wide slots (the left one aligned left, the right one aligned right), and the speaker symbols take the same slots, aligned the same way, measured by their drawn shape. So switching only changes the slot contents and the fill colour.
- It swaps back 3 seconds after the last touch, on a second click, or when the island closes. Ctrl+Up and Ctrl+Down also open it.
- No speaker button in the empty view; the slider is dimmed to 35 percent while the app has no sound open with Windows.

Camera and microphone dots (PRD FR-40). [Proposed, 3 Nov 2026]

- 6 across, the unread dot's size and pop. Microphone `#FF9F0A`, camera `#30D158`; with both in use, the camera colour.
- On the compact clock 12 from the right edge, the unread dot then sits 22 from the right edge (4 between them). On the compact music pill it is the last item, after the unread dot.
- Not shown on banners or the open island.

Artwork from every app (PRD FR-41). [Confirmed 3 Nov 2026]

- Artwork that is not square shows whole inside the artwork square (64 on the music page, 24 on the pill), centered on black, with the square's usual radius. Its colours come from the picture only.

Other apps' logos (PRD FR-42). [Proposed, 3 Nov 2026]

- Like WhatsApp and Discord: the circle in the app's colour, the logo in white at 58 percent. Claude: `#D97757` with the white mark (Diva, 3 Nov 2026). The colour is the logo's main colour; a logo that is a full square fills the circle as it is, clipped round. No logo: the letter circle as before.

Tray icon (PRD FR-43). [Confirmed 3 Nov 2026]

- The tray uses `WinIsland.ico` (concept B, "Mono pill") instead of the white pill drawn in code.
