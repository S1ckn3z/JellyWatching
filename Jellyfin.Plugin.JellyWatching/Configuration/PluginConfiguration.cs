using System.Collections.ObjectModel;
using System.Linq;
using Jellyfin.Plugin.JellyWatching.Models;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.JellyWatching.Configuration;

/// <summary>
/// Plugin configuration for JellyWatching.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        EnablePlugin = true;
        EpisodeThreshold = 3;
        TimeThresholdMinutes = 120;
        ThresholdMode = ThresholdMode.EpisodeOnly;
        SessionTimeoutMinutes = 30;
        CustomMessage = "Are you still watching? Press Play to continue.";
        AutoPauseEnabled = true;
        ActivityDetectionEnabled = true;
        NightModeEnabled = false;
        NightModeStartHour = 22;
        NightModeEndHour = 6;
        NightModeEpisodeThreshold = 2;
        NightModeTimeThresholdMinutes = 60;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the plugin is globally enabled.
    /// </summary>
    public bool EnablePlugin { get; set; }

    /// <summary>
    /// Gets or sets the number of consecutive episodes before a prompt is shown.
    /// </summary>
    public int EpisodeThreshold { get; set; }

    /// <summary>
    /// Gets or sets the time threshold in minutes before a prompt is shown.
    /// </summary>
    public int TimeThresholdMinutes { get; set; }

    /// <summary>
    /// Gets or sets the threshold mode determining how triggers are evaluated.
    /// </summary>
    public ThresholdMode ThresholdMode { get; set; }

    /// <summary>
    /// Gets or sets the session inactivity timeout in minutes for cleanup.
    /// </summary>
    public int SessionTimeoutMinutes { get; set; }

    /// <summary>
    /// Gets or sets the custom message displayed to the user.
    /// </summary>
    public string CustomMessage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether auto-pause is enabled on intervention.
    /// </summary>
    public bool AutoPauseEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether activity detection (seek, track change) resets the time counter.
    /// </summary>
    public bool ActivityDetectionEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether night mode is enabled globally.
    /// </summary>
    public bool NightModeEnabled { get; set; }

    /// <summary>
    /// Gets or sets the hour when night mode starts (0-23).
    /// </summary>
    public int NightModeStartHour { get; set; }

    /// <summary>
    /// Gets or sets the hour when night mode ends (0-23).
    /// </summary>
    public int NightModeEndHour { get; set; }

    /// <summary>
    /// Gets or sets the episode threshold during night mode.
    /// </summary>
    public int NightModeEpisodeThreshold { get; set; }

    /// <summary>
    /// Gets or sets the time threshold in minutes during night mode.
    /// </summary>
    public int NightModeTimeThresholdMinutes { get; set; }

    /// <summary>
    /// Gets the list of per-user settings entries.
    /// </summary>
    public Collection<UserSettingsEntry> UserSettingsList { get; } = new();

    /// <summary>
    /// Gets the per-user settings for a specific user, or null if no override exists.
    /// </summary>
    /// <param name="userId">The user ID to look up.</param>
    /// <returns>The user settings if found, otherwise null.</returns>
    public UserSettings? GetUserSettings(string userId)
    {
        return UserSettingsList.FirstOrDefault(e => e.UserId == userId)?.Settings;
    }
}
