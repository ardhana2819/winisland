# Spikes

Small, throwaway test programs for Phase 1. Each one answers a technical question from `docs/PRD.md` before we build the real feature.

| Spike | Answers | What it is |
|---|---|---|
| `NotificationProbe/` | S1, U1, U2, U3, U7, W1 to W7, C1 to C5 | Console program that reads Windows notifications (event and polling) and prints app, ids, and text lengths. Message text only with `--show-text`, console only. |
| `OverlayProbe/` | U4, U5 | Black topmost, non-activating pill at the top center. Shows click focus result and hotkey status. Right-click to close. |
| `GlassProbe/` | G1 to G5 (v1.4) | A glass pill with Windows' own blurred backdrop, a dark tint and a clock; click to change its shape. Tests the glass look of backlog F6. Right-click to close. |
| `MediaProbe/` | U6, B3 | Console program that reads the Spotify media session and tries each control. `--report` reads only. |
| `EmojiProbe/` | E1 to E3 (v1.8) | Console program that draws sample lines with Windows' color emoji layers (COLR version 0) over WPF's own layout, to PNG files, and prints time and memory. |
| `measure-idle.ps1` | Idle CPU and memory | Samples a running process's CPU and memory for a number of seconds. |

How to build and run each test: see `docs/spike-results.md`.

Rules:

- Spikes are not part of the product. Product code under `src/` must never reference anything in this folder.
- Spikes are not in `WinIsland.slnx`, so CI does not build them and they cost no Actions minutes. They still use the shared rules in `Directory.Build.props` (warnings are errors).
- Results go in `docs/spike-results.md`.
- Spikes can be deleted once their results are recorded and the real feature exists.
