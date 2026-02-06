using Jellyfin.Plugin.JellyWatching.Models;

namespace Jellyfin.Plugin.JellyWatching.Services;

/// <summary>
/// Resolved, flat settings object with no nullable fields. Produced per event by <see cref="SettingsResolver"/>.
/// </summary>
public class EffectiveSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether JellyWatching is enabled for this user.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the resolved episode threshold.
    /// </summary>
    public int EpisodeThreshold { get; set; }

    /// <summary>
    /// Gets or sets the resolved time threshold in minutes.
    /// </summary>
    public int TimeThresholdMinutes { get; set; }

    /// <summary>
    /// Gets or sets the resolved threshold mode.
    /// </summary>
    public ThresholdMode ThresholdMode { get; set; }

    /// <summary>
    /// Gets or sets the resolved custom message.
    /// </summary>
    public string CustomMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether auto-pause is enabled.
    /// </summary>
    public bool AutoPauseEnabled { get; set; }
}
