<p align="center">
  <img src="Jellyfin.Plugin.JellyWatching/logo.svg" alt="JellyWatching Logo" width="160"/>
</p>

<h1 align="center">JellyWatching</h1>

<p align="center">
  <em>Are you still watching?</em><br/>
  A Jellyfin plugin that pauses playback after consecutive episodes — just like the streaming giants.
</p>

<p align="center">
  <a href="https://github.com/S1ckn3z/JellyWatching/actions/workflows/build.yaml">
    <img src="https://github.com/S1ckn3z/JellyWatching/actions/workflows/build.yaml/badge.svg" alt="Build"/>
  </a>
  <a href="https://github.com/S1ckn3z/JellyWatching/releases/latest">
    <img src="https://img.shields.io/github/v/release/S1ckn3z/JellyWatching?label=latest" alt="Latest Release"/>
  </a>
  <a href="LICENSE">
    <img src="https://img.shields.io/github/license/S1ckn3z/JellyWatching" alt="License"/>
  </a>
</p>

---

## How It Works

JellyWatching tracks consecutive episode playback **per user and device**. Once a configurable threshold is reached, it pauses playback and displays a message asking the viewer to confirm they're still there. When the user presses play, the counters reset and watching continues normally.

## Features

- **Episode & time thresholds** — Trigger after a number of episodes, a duration of watch time, or both
- **Auto-pause** — Automatically pauses playback when the threshold is hit
- **Night mode** — Apply stricter thresholds during nighttime hours
- **Per-user overrides** — Customize behavior for individual users
- **Activity detection** — Seeking or switching audio/subtitle tracks proves the user is active and resets the timer
- **Custom messages** — Change the prompt text to whatever you like
- **Web config UI** — Configure everything from the Jellyfin dashboard

## Installation

### Repository (recommended)

1. In Jellyfin, go to **Dashboard → Plugins → Repositories**
2. Add a new repository:

   | Field | Value |
   |-------|-------|
   | Name  | `JellyWatching` |
   | URL   | `https://s1ckn3z.github.io/JellyWatching/manifest.json` |

3. Go to **Catalog** → find **JellyWatching** under *General* → click **Install**
4. Restart Jellyfin

### Manual

1. Download `jellywatching_*.zip` from the [latest release](https://github.com/S1ckn3z/JellyWatching/releases/latest)
2. Extract into your Jellyfin plugins directory:
   - **Windows:** `%LOCALAPPDATA%\jellyfin\plugins\JellyWatching\`
   - **Linux:** `~/.local/share/jellyfin/plugins/JellyWatching/`
   - **Docker:** `/config/plugins/JellyWatching/`
3. Restart Jellyfin

## Configuration

After installation, navigate to **Dashboard → Plugins → JellyWatching**.

### General

| Setting | Default | Description |
|---------|---------|-------------|
| Enable Plugin | On | Global on/off switch |
| Episode Threshold | `3` | Episodes before prompting |
| Time Threshold | `120 min` | Watch time before prompting |
| Threshold Mode | Episode Only | `EpisodeOnly` · `TimeOnly` · `EpisodeAndTime` · `EpisodeOrTime` |
| Session Timeout | `30 min` | Idle time before a session is cleaned up |
| Custom Message | *Are you still watching? Press Play to continue.* | Text shown to the viewer |
| Auto Pause | On | Pause playback when the prompt appears |
| Activity Detection | On | Reset timer when the user seeks or changes tracks |

### Night Mode

Apply stricter limits during specific hours — useful for preventing kids from watching all night.

| Setting | Default | Description |
|---------|---------|-------------|
| Enabled | Off | Activate night mode |
| Start Hour | `22` | When night mode kicks in (0-23) |
| End Hour | `6` | When night mode ends (0-23) |
| Episode Threshold | `2` | Episodes allowed during night hours |
| Time Threshold | `60 min` | Watch time allowed during night hours |

### Per-User Overrides

Every setting above can be overridden per user through the plugin configuration page. Users without overrides inherit the global defaults.

## Requirements

| Component | Version |
|-----------|---------|
| Jellyfin Server | **10.11.x** |
| .NET | 9.0 |

## Building from Source

```bash
git clone https://github.com/S1ckn3z/JellyWatching.git
cd JellyWatching
dotnet build -c Release
```

The compiled DLL will be at `Jellyfin.Plugin.JellyWatching/bin/Release/net9.0/Jellyfin.Plugin.JellyWatching.dll`.

## License

This project is licensed under the [GNU General Public License v3.0](LICENSE).
