# JellyWatching

A Jellyfin plugin that pauses playback after consecutive episodes and asks "Are you still watching?" — just like Netflix.

## Features

- **Episode counting** — Tracks consecutive episode playback per user and device
- **Auto-pause** — Pauses playback and displays a confirmation message after the configured threshold
- **Flexible thresholds** — Trigger by episode count, watch time, or both
- **Night mode** — Stricter thresholds during configurable nighttime hours
- **Per-user overrides** — Different settings for each user
- **Activity detection** — Seeking or changing audio tracks resets the timer
- **Custom messages** — Configurable prompt text

## Installation

### Via Repository (recommended)

1. In Jellyfin, go to **Dashboard → Plugins → Repositories**
2. Click **Add** and enter:
   - **Name:** `JellyWatching`
   - **URL:** `https://S1ckn3z.github.io/JellyWatching/manifest.json`
3. Go to the **Catalog** tab, find **JellyWatching** under General, and click **Install**
4. Restart Jellyfin

### Manual Installation

1. Download the latest `jellywatching_*.zip` from [Releases](../../releases/latest)
2. Extract into your Jellyfin plugins directory (e.g. `<data>/plugins/JellyWatching/`)
3. Restart Jellyfin

## Configuration

After installation, go to **Dashboard → Plugins → JellyWatching** to configure:

| Setting | Default | Description |
|---------|---------|-------------|
| Enable Plugin | `true` | Global on/off switch |
| Episode Threshold | `3` | Number of consecutive episodes before prompting |
| Time Threshold | `120 min` | Watch time before prompting |
| Threshold Mode | Episode Only | `EpisodeOnly`, `TimeOnly`, or `Both` |
| Session Timeout | `30 min` | Inactivity timeout before session cleanup |
| Custom Message | "Are you still watching? Press Play to continue." | Message shown to the user |
| Auto Pause | `true` | Automatically pause playback on intervention |
| Activity Detection | `true` | Reset timer on seek/track change |

### Night Mode

| Setting | Default | Description |
|---------|---------|-------------|
| Night Mode Enabled | `false` | Enable stricter thresholds at night |
| Start Hour | `22` | Hour when night mode begins (0-23) |
| End Hour | `6` | Hour when night mode ends (0-23) |
| Night Episode Threshold | `2` | Episode threshold during night hours |
| Night Time Threshold | `60 min` | Time threshold during night hours |

## Requirements

- Jellyfin Server **10.11.x**

## Building from Source

```bash
dotnet build -c Release
```

The compiled DLL will be in `Jellyfin.Plugin.JellyWatching/bin/Release/net9.0/`.

## License

This plugin is licensed under the [GNU General Public License v3.0](LICENSE).
