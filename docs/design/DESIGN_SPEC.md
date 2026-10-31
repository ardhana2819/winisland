# WinIsland Design Spec (MVP v1.0)

Save as `docs/design/DESIGN_SPEC.md`. Save the prototype next to it as `docs/design/prototype.html` (open it in a browser to see and try the real motion). Source: the approved prototype, 30 Oct 2026. If this spec and the prototype disagree, the prototype wins for look and motion, and the PRD wins for behavior rules. Report the difference to Diva.

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
- Music title in compact: 13, regular, one line, cut with an ellipsis.
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
| WhatsApp icon circle | `#177A40` |
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
