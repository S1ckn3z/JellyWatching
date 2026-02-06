namespace Jellyfin.Plugin.JellyWatching.Models;

/// <summary>
/// Per-user override settings. Null fields fall back to the global configuration.
/// </summary>
public class UserSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether JellyWatching is enabled for this user.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the per-user episode threshold override.
    /// </summary>
    public int? EpisodeThreshold { get; set; }

    /// <summary>
    /// Gets or sets the per-user time threshold override in minutes.
    /// </summary>
    public int? TimeThresholdMinutes { get; set; }

    /// <summary>
    /// Gets or sets the per-user threshold mode override.
    /// </summary>
    public ThresholdMode? ThresholdMode { get; set; }

    /// <summary>
    /// Gets or sets the per-user custom message override.
    /// </summary>
    public string? CustomMessage { get; set; }

    /// <summary>
    /// Gets or sets the per-user auto-pause override.
    /// </summary>
    public bool? AutoPauseEnabled { get; set; }

    /// <summary>
    /// Gets or sets the per-user night mode enabled override.
    /// </summary>
    public bool? NightModeEnabled { get; set; }

    /// <summary>
    /// Gets or sets the per-user night mode start hour override.
    /// </summary>
    public int? NightModeStartHour { get; set; }

    /// <summary>
    /// Gets or sets the per-user night mode end hour override.
    /// </summary>
    public int? NightModeEndHour { get; set; }

    /// <summary>
    /// Gets or sets the per-user night mode episode threshold override.
    /// </summary>
    public int? NightModeEpisodeThreshold { get; set; }

    /// <summary>
    /// Gets or sets the per-user night mode time threshold override in minutes.
    /// </summary>
    public int? NightModeTimeThresholdMinutes { get; set; }
}
