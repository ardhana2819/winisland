# Spikes

Small, throwaway test programs for Phase 1. Each one answers a technical question from `docs/PRD.md` before we build the real feature.

| Spike | Answers | What it is |
|---|---|---|
| `NotificationProbe/` | S1, U1, U2, U3, U7 | Console program that reads Windows notifications (event and polling) and prints app, ids, and text lengths. Message text only with `--show-text`, console only. |
| `OverlayProbe/` | U4, U5 | Black topmost, non-activating pill at the top center. Shows click focus result and hotkey status. Right-click to close. |
| `MediaProbe/` | U6, B3 | Console program that reads the Spotify media session and tries each control. `--report` reads only. |
| `measure-idle.ps1` | Idle CPU and memory | Samples a running process's CPU and memory for a number of seconds. |

How to build and run each test: see `docs/spike-results.md`.

Rules:

- Spikes are not part of the product. Product code under `src/` must never reference anything in this folder.
- Spikes are not in `WinIsland.slnx`, so CI does not build them and they cost no Actions minutes. They still use the shared rules in `Directory.Build.props` (warnings are errors).
- Results go in `docs/spike-results.md`.
- Spikes can be deleted once their results are recorded and the real feature exists.
