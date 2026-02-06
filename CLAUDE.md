# CLAUDE.md - JellyWatching Development Guide

## Project Overview

JellyWatching is a Jellyfin plugin that monitors consecutive episode playback and prompts users with "Are you still watching?" after configurable thresholds — similar to Netflix.

- **Target:** Jellyfin Server 10.11.x, .NET 9.0
- **NuGet:** Jellyfin.Controller 10.11.6, Jellyfin.Model 10.11.6
- **License:** GPLv3
- **Repository:** https://github.com/S1ckn3z/JellyWatching

## Build & Test

```bash
# Build (must produce 0 warnings — TreatWarningsAsErrors is active)
dotnet build -c Release

# Publish (for packaging)
dotnet publish Jellyfin.Plugin.JellyWatching/Jellyfin.Plugin.JellyWatching.csproj -c Release -o ./release
```

There are no unit tests yet. Verify changes by building and deploying to a local Jellyfin instance.

## Architecture

### Key Components

| File | Purpose |
|------|---------|
| `Plugin.cs` | Main plugin class, registers config page, holds singleton `Instance` |
| `PluginServiceRegistrator.cs` | Separate class (parameterless ctor!) that registers `WatchingMonitorService` as `IHostedService` |
| `Services/WatchingMonitorService.cs` | Core logic: listens to `PlaybackStart`/`PlaybackProgress`/`PlaybackStopped`, tracks sessions, triggers intervention |
| `Services/SettingsResolver.cs` | Merges global config → per-user overrides → night mode into `EffectiveSettings` |
| `Services/EffectiveSettings.cs` | Flat resolved settings object (no nullable fields) |
| `Configuration/PluginConfiguration.cs` | All configurable options with defaults |
| `Configuration/configPage.html` | Admin UI (embedded resource) |
| `Models/` | `UserWatchingSession`, `UserSettings`, `UserSettingsEntry`, `ThresholdMode`, `WatchingState` |

### Session Tracking Flow

1. `PlaybackStart` → If episode belongs to a series, get/create `UserWatchingSession` keyed by `{UserId}_{DeviceId}`
2. Same series → increment `EpisodeCount`, check thresholds via `ShouldIntervene()`
3. Threshold reached → set state to `PendingConfirmation`, call `InterventionAsync()` (pause + display message)
4. `PlaybackProgress` with `IsPaused=false` while `PendingConfirmation` → user confirmed, reset counters
5. `PlaybackStopped` without completion → remove session
6. Cleanup timer runs every 5 minutes, removes sessions older than `SessionTimeoutMinutes`

### Activity Detection

Detects user activity during playback progress events:
- **Seek:** Position jumped > 30 seconds (300M ticks)
- **Audio track change:** `AudioStreamIndex` changed
- **Subtitle track change:** `SubtitleStreamIndex` changed

Activity resets the time counter only, NOT the episode count.

## Critical Rules

### Jellyfin Plugin API

- `IPluginServiceRegistrator` MUST be a SEPARATE class with a parameterless constructor. Jellyfin uses `Activator.CreateInstance()` before DI is ready. Never put it on the Plugin class.
- The Plugin class CANNOT also be an `IHostedService`.
- `IPluginServiceRegistrator` lives in `MediaBrowser.Controller.Plugins` (not `MediaBrowser.Common.Plugins`).
- Method signature: `RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)`
- If the plugin fails to load, Jellyfin sets `meta.json` status to `"Malfunctioned"` — must manually reset to `"Active"` after fixing.

### Code Quality

- `TreatWarningsAsErrors` is active — the build must produce **zero warnings**.
- StyleCop analyzers are included. SA1600 is disabled in the ruleset, but keep XML doc comments on public members.
- Use file-scoped namespaces (`namespace Foo;` not `namespace Foo { }`).
- All concurrent session access uses `lock (watchSession)` — the `ConcurrentDictionary` only protects the dictionary itself, not the session objects.

### What NOT to Do

- Do not add `IPluginServiceRegistrator` to the Plugin class.
- Do not use `async void` — use `async Task` and fire-and-forget with `_ = MethodAsync()`.
- Do not store secrets or credentials in the repository.
- Do not commit `bin/`, `obj/`, `.vs/`, `.vscode/`, `docker/`, `docker-compose.yml`, or `.claude/` directories.

## CI/CD

### Workflows (`.github/workflows/`)

- **`build.yaml`** — Runs on push/PR to `main`. Builds in Release mode.
- **`publish.yaml`** — Runs on GitHub Release (`published`). Three steps:
  1. `dotnet publish` → ZIP the DLL → compute MD5
  2. Upload ZIP + MD5 as release assets
  3. Generate `manifest.json` on `gh-pages` via `Kevinjil/jellyfin-plugin-repo-action@v0.3.0`

### Releasing a New Version

1. Bump version in `build.yaml` (JPRM) and `Directory.Build.props` (all three fields)
2. Update changelog in `build.yaml`
3. Commit, push to `main`
4. Create a GitHub Release with tag `vX.X.X.X`
5. Workflow runs automatically — ZIP appears as release asset, manifest updates on gh-pages

### Plugin Repository URL

```
https://s1ckn3z.github.io/JellyWatching/manifest.json
```

## File Layout

```
JellyWatching/
├── .github/workflows/        # CI + release workflows
├── Jellyfin.Plugin.JellyWatching/
│   ├── Configuration/        # PluginConfiguration + configPage.html
│   ├── Models/               # Data models and enums
│   ├── Services/             # WatchingMonitorService, SettingsResolver, EffectiveSettings
│   ├── Plugin.cs             # Main plugin entry point
│   ├── PluginServiceRegistrator.cs
│   ├── logo.svg              # Plugin logo
│   └── *.csproj
├── build.yaml                # JPRM metadata (version, GUID, artifacts)
├── Directory.Build.props     # Shared version properties
├── jellyfin.ruleset          # Code analysis rules
└── CLAUDE.md                 # This file
```

## Plugin GUID

```
a4c0e2f8-3b7d-4e6a-9f1c-8d5e2b0a7c34
```

Do not change this — it identifies the plugin across installations.
