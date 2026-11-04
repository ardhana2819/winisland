# Performance Results

This file keeps the memory and CPU measurements of the real app, so later versions can be compared against them (PRD goal G4, AC-16: under 150 MB working set, near-zero CPU when idle). Add a new section when a release or a heavier feature changes the numbers.

Labels follow `docs/spike-results.md`: "Claude ran" means Claude Code ran it on Diva's PC during the session. Nothing here is estimated.

Test machine (Diva's PC): Intel Core i5-9400F (6 cores), NVIDIA GeForce GTX 1650 Super (4 GB), 16 GB DDR4 2666 MHz, Windows 11 Pro 24H2 (build 26100).

## v1.5 before release (4 Nov 2026)

**Summary:** v1.5 still meets AC-30 at idle, but with little room left. Glass, clock showing, music paused: working set 142.1 to 147.0 MB on average (peaks up to 151.8 MB), private memory 50.0 to 53.9 MB, CPU 0.08 to 0.34 % on average in the runs without a notification. Compared with v1.4 Glass (136.9 to 140.7 MB), the working set is about 5 MB higher; private memory is the same (v1.4: 51.2 to 54.5 MB), so the extra is probably shared Windows code that v1.5 loads (the audio session API for the volume and the microphone dot), not WinIsland's own data. This is the point the v1.1 decision below named: idle memory is near 150 MB again.

### Method

Claude ran, 4 Nov 2026, on `main` at `bf79caa` (Release build, run from the build folder with package identity for notifications):

1. Stopped `WinIsland.App`, started it with Look Glass and size Extra large, as Diva has it. Startup CPU in the first 6 s: 3297 ms. Waited 6 s more.
2. Ran `spikes/measure-idle.ps1 -ProcessName WinIsland.App -Seconds 30` four times in a row. Spotify and Brave were paused (checked through the media session API), the island showed the clock, Diva left the PC alone.
3. WhatsApp messages arrived during runs 2 and 4 (log: 11:11:31, 11:11:46, 11:12:56), so those runs include banner animations; runs 1 and 3 are true idle.

### Raw data

| Run | Notifications during the run | CPU avg / peak (%) | Working set avg / peak (MB) | Private avg / peak (MB) |
|---|---|---|---|---|
| 1 | none | 0.08 / 0.52 | 142.1 / 151.8 | 53.9 / 63.6 |
| 2 | 2 banners | 1.19 / 13.40 | 146.3 / 149.8 | 53.6 / 56.2 |
| 3 | none | 0.15 / 3.57 | 147.0 / 148.6 | 52.7 / 54.4 |
| 4 | 1 banner | 0.34 / 8.74 | 144.2 / 150.0 | 50.0 / 55.2 |

### What it means

- Idle CPU is near zero (0.08 and 0.15 % in the quiet runs). A banner costs a short peak of 9 to 13 %, then CPU falls back.
- With music playing (3 Nov 2026, Spotify, measured during Phase 19) the working set went up and down between 141.7 and 152.5 MB in 30 s runs, rising until .NET's clean-up and then dropping: memory in use, not a leak.
- The microphone check (FR-40, once a second) is not the cause: a build with the check turned off measured the same (146.6 to 150.4 MB), and letting go of the Windows audio objects at once did not change it either. Both experiments were undone.
- The one setting that lowered memory in v1.1, turning off tiered compilation (about 13 MB less), is the option if Diva wants more room. Not adopted yet: her decision.

## v1.4 Glass look vs Solid (2 Nov 2026)

**Summary:** the Glass look (FR-37) costs nothing measurable at idle. Glass: working set 136.9 to 140.7 MB, private memory 51.2 to 54.5 MB, CPU 0.38 to 0.64 % on average. Solid: 133.4 to 143.6 MB, 50.0 to 59.3 MB, 0.48 to 1.91 %. Both meet AC-30 (under 150 MB working set) and stay in the v1.1 range. While Diva was actively testing (animations, a game running) one 60 s sample showed 154.9 MB average and 3.72 % CPU; that is use, not idle.

### Method

Claude ran, 2 Nov 2026, on branch `feat/v1.4-glass-and-call` at `e2d817e` (Release build, run from the build folder with package identity for notifications):

1. Three rounds; in each, Glass first, then Solid. Before each run: stopped `WinIsland.App`, set `"Look"` in `settings.json`, started it, waited 15 s.
2. Ran `spikes/measure-idle.ps1 -ProcessName WinIsland.App -Seconds 30`. The island sat idle with the clock showing and the PC untouched (Diva left it alone).
3. Afterwards `"Look"` was set back to Glass, as Diva had it.

### Raw data

| Look | Run | CPU avg / peak (%) | Working set avg / peak (MB) | Private avg / peak (MB) |
|---|---|---|---|---|
| Glass | 1 | 0.57 / 2.07 | 139.5 / 150.0 | 51.9 / 62.8 |
| Solid | 1 | 1.91 / 15.97 | 143.6 / 147.7 | 59.3 / 63.0 |
| Glass | 2 | 0.64 / 1.81 | 140.7 / 143.0 | 54.5 / 56.8 |
| Solid | 2 | 0.57 / 2.31 | 133.4 / 144.2 | 50.0 / 61.1 |
| Glass | 3 | 0.38 / 1.29 | 136.9 / 142.1 | 51.2 / 56.7 |
| Solid | 3 | 0.48 / 1.55 | 140.8 / 146.6 | 57.0 / 62.8 |

### What it means

- The differences between Glass and Solid are smaller than the 15 MB the same setting moves between runs (see v1.1 below), so they are noise.
- The blur window follows the island only while it moves, and refreshes the blur every 2 s; neither shows up in idle CPU.
- Solid run 1 had one CPU spike (16 %), a one-off like the v1.1 baseline's; the other runs are under 2.5 %.

## v1.1.0-beta.1: .NET runtime settings (1 Nov 2026)

**Summary:** v1.1.0-beta.1 meets G4 with room to spare: idle working set 128 to 143 MB, private memory 49 to 61 MB, CPU under 1 % on average. Of the .NET runtime settings tried, only turning off tiered compilation lowered memory (about 122 MB working set, 46 MB private) without costing CPU. **Decision (Diva, 1 Nov 2026): change nothing.** Memory is within target and does not affect smoothness at this level. Revisit if a new feature pushes idle memory toward 150 MB.

### Method

Claude ran, 1 Nov 2026:

1. Built `dotnet build WinIsland.slnx -c Release -warnaserror` from `main` at `255d807` (v1.1.0-beta.1).
2. Copied the build output once per setting and added the setting to `WinIsland.App.runtimeconfig.json` (`configProperties`). This has the same effect as the matching property in `WinIsland.App.csproj`.
3. For each run: stopped any running `WinIsland.App`, started that copy, waited 6 s, recorded CPU time used so far ("startup CPU"), then ran `spikes/measure-idle.ps1 -ProcessName WinIsland.App -Seconds 20`. The island sat idle with the clock showing; nothing was clicked.
4. Round 1 ran every setting twice in a row. Round 2 ran the four main candidates four times each, interleaved, so order and file-cache effects spread evenly.

The app ran without package identity (from the build folder), as during development.

### Results

Ranges over all runs (average values per run; "CPU peak" is the highest single second).

| Setting (`runtimeconfig.json`) | Runs | Working set (MB) | Private memory (MB) | CPU average | CPU peak | Startup CPU (first 6 s) |
|---|---|---|---|---|---|---|
| Baseline (no change) | 6 | 127.6 to 143.4 | 49.1 to 61.2 | 0.48 to 2.67 % | 1.8 to 14.3 % | 2.5 to 3.3 s |
| `System.Runtime.TieredCompilation: false` | 6 | 121.1 to 122.7 | 45.3 to 47.0 | 0.32 to 0.81 % | 0.8 to 2.0 % | 1.5 to 1.8 s |
| `System.Runtime.TieredCompilation.QuickJit: false` | 6 | 126.3 to 140.2 | 48.6 to 60.4 | 0.42 to 0.83 % | 0.8 to 3.0 % | 2.2 to 3.9 s |
| `System.GC.Concurrent: false` | 6 | 128.6 to 143.0 | 49.7 to 61.3 | 0.48 to 2.26 % | 1.6 to 12.4 % | 2.7 to 2.9 s |
| `System.GC.ConserveMemory: 5` | 2 | 131.1 to 143.2 | 52.4 to 61.4 | 0.57 to 0.85 % | 1.6 to 2.6 % | Not measured |
| `System.GC.ConserveMemory: 9` | 2 | 131.5 to 142.8 | 53.2 to 61.5 | 0.63 to 0.84 % | 1.8 to 2.8 % | Not measured |
| `System.Runtime.TieredPGO: false` | 2 | 128.5 to 143.2 | 48.9 to 62.4 | 0.63 to 1.85 % | 2.3 to 10.4 % | Not measured |

### What it means

- The baseline moves by about 15 MB between identical runs, so any difference smaller than that is noise.
- The garbage collector settings (`Concurrent`, `ConserveMemory`) and `TieredPGO` change nothing measurable. The app's managed heap is small, so the collector has little to save.
- `QuickJit: false` is inconsistent: some runs lower, some equal to the baseline.
- `TieredCompilation: false` was the only consistent gain: about 13 MB working set and 8 MB private memory below the baseline average (122.0 vs 135.2 MB, 46.3 vs 54.4 MB), in all 6 runs, with no extra idle CPU and less startup CPU (no background recompiling). Not measured: whether the first opening of each page gets a short delay, because code is then compiled fully on first use. This was not adopted, see the decision above.

### Raw data

Round 1 (sequential, 2 runs each):

| Setting | Run | CPU avg / peak (%) | Working set avg / peak (MB) | Private avg / peak (MB) |
|---|---|---|---|---|
| Baseline | 1 | 0.84 / 14.28 | 143.4 / 144.1 | 61.2 / 61.9 |
| Baseline | 2 | 2.67 / 12.06 | 134.6 / 139.8 | 53.6 / 59.2 |
| GC Concurrent false | 1 | 0.68 / 2.07 | 143.0 / 144.1 | 61.3 / 62.4 |
| GC Concurrent false | 2 | 0.61 / 2.31 | 131.1 / 132.3 | 52.5 / 53.4 |
| ConserveMemory 5 | 1 | 0.85 / 2.59 | 143.2 / 144.4 | 61.4 / 62.4 |
| ConserveMemory 5 | 2 | 0.57 / 1.55 | 131.1 / 132.4 | 52.4 / 54.0 |
| ConserveMemory 9 | 1 | 0.84 / 2.84 | 142.8 / 144.1 | 61.5 / 62.6 |
| ConserveMemory 9 | 2 | 0.63 / 1.80 | 131.5 / 132.7 | 53.2 / 54.3 |
| TieredPGO false | 1 | 0.63 / 2.26 | 143.2 / 144.8 | 62.4 / 64.1 |
| TieredPGO false | 2 | 1.85 / 10.35 | 128.5 / 133.5 | 48.9 / 52.3 |
| TieredCompilation false | 1 | 0.62 / 1.28 | 122.7 / 123.7 | 47.0 / 48.0 |
| TieredCompilation false | 2 | 0.59 / 1.04 | 121.9 / 124.0 | 46.2 / 48.2 |
| QuickJit false | 1 | 0.42 / 0.77 | 131.4 / 141.6 | 51.0 / 61.3 |
| QuickJit false | 2 | 0.54 / 1.04 | 126.9 / 127.9 | 49.1 / 50.1 |

Round 2 (interleaved, 4 runs each):

| Setting | Run | Startup CPU (ms) | CPU avg / peak (%) | Working set avg / peak (MB) | Private avg / peak (MB) |
|---|---|---|---|---|---|
| Baseline | 1 | 3109 | 0.72 / 2.32 | 135.1 / 136.4 | 53.2 / 54.2 |
| Baseline | 2 | 2703 | 0.77 / 2.07 | 131.4 / 132.4 | 52.1 / 53.0 |
| Baseline | 3 | 3344 | 0.48 / 1.80 | 139.1 / 143.5 | 57.4 / 61.8 |
| Baseline | 4 | 2453 | 0.64 / 2.07 | 127.6 / 129.7 | 49.1 / 51.1 |
| TieredCompilation false | 1 | 1641 | 0.81 / 2.04 | 121.1 / 123.7 | 45.3 / 47.8 |
| TieredCompilation false | 2 | 1781 | 0.57 / 1.29 | 122.0 / 122.9 | 46.4 / 47.2 |
| TieredCompilation false | 3 | 1547 | 0.32 / 0.78 | 122.0 / 122.9 | 46.3 / 47.3 |
| TieredCompilation false | 4 | 1688 | 0.73 / 2.00 | 122.1 / 123.1 | 46.5 / 47.4 |
| QuickJit false | 1 | 2188 | 0.76 / 1.29 | 126.3 / 127.2 | 48.6 / 49.5 |
| QuickJit false | 2 | 3891 | 0.57 / 1.29 | 140.1 / 141.1 | 60.4 / 61.3 |
| QuickJit false | 3 | 3656 | 0.50 / 1.29 | 140.2 / 141.1 | 60.4 / 61.3 |
| QuickJit false | 4 | 3828 | 0.83 / 3.01 | 135.3 / 141.7 | 55.1 / 61.8 |
| GC Concurrent false | 1 | 2719 | 0.91 / 1.81 | 131.2 / 132.3 | 52.3 / 53.2 |
| GC Concurrent false | 2 | 2875 | 0.48 / 1.55 | 128.6 / 131.7 | 49.7 / 52.8 |
| GC Concurrent false | 3 | 2688 | 0.64 / 2.58 | 131.4 / 132.6 | 52.2 / 53.2 |
| GC Concurrent false | 4 | 2797 | 2.26 / 12.41 | 134.5 / 140.4 | 53.9 / 58.3 |
