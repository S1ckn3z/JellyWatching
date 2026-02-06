using System;
using Jellyfin.Plugin.JellyWatching.Configuration;
using Jellyfin.Plugin.JellyWatching.Models;

namespace Jellyfin.Plugin.JellyWatching.Services;

/// <summary>
/// Resolves effective settings by merging global config, per-user overrides, and night mode.
/// </summary>
public static class SettingsResolver
{
    /// <summary>
    /// Resolves the effective settings for a given user.
    /// Chain: Global defaults -> Per-user overrides -> Night mode overrides.
    /// </summary>
    /// <param name="config">The global plugin configuration.</param>
    /// <param name="userId">The user ID to resolve settings for.</param>
    /// <returns>A fully resolved <see cref="EffectiveSettings"/> object.</returns>
    public static EffectiveSettings Resolve(PluginConfiguration config, string userId)
    {
        var effective = new EffectiveSettings
        {
            Enabled = config.EnablePlugin,
            EpisodeThreshold = config.EpisodeThreshold,
            TimeThresholdMinutes = config.TimeThresholdMinutes,
            ThresholdMode = config.ThresholdMode,
            CustomMessage = config.CustomMessage,
            AutoPauseEnabled = config.AutoPauseEnabled,
        };

        // Apply per-user overrides
        var userSettings = config.GetUserSettings(userId);
        if (userSettings is not null)
        {
            effective.Enabled = userSettings.Enabled;

            if (userSettings.EpisodeThreshold.HasValue)
            {
                effective.EpisodeThreshold = userSettings.EpisodeThreshold.Value;
            }

            if (userSettings.TimeThresholdMinutes.HasValue)
            {
                effective.TimeThresholdMinutes = userSettings.TimeThresholdMinutes.Value;
            }

            if (userSettings.ThresholdMode.HasValue)
            {
                effective.ThresholdMode = userSettings.ThresholdMode.Value;
            }

            if (userSettings.CustomMessage is not null)
            {
                effective.CustomMessage = userSettings.CustomMessage;
            }

            if (userSettings.AutoPauseEnabled.HasValue)
            {
                effective.AutoPauseEnabled = userSettings.AutoPauseEnabled.Value;
            }
        }

        // Apply night mode overrides if active
        bool nightModeEnabled = userSettings?.NightModeEnabled ?? config.NightModeEnabled;
        if (nightModeEnabled && IsNightModeActive(config, userSettings))
        {
            int nightEpisode = userSettings?.NightModeEpisodeThreshold ?? config.NightModeEpisodeThreshold;
            int nightTime = userSettings?.NightModeTimeThresholdMinutes ?? config.NightModeTimeThresholdMinutes;

            effective.EpisodeThreshold = nightEpisode;
            effective.TimeThresholdMinutes = nightTime;
        }

        return effective;
    }

    private static bool IsNightModeActive(PluginConfiguration config, UserSettings? userSettings)
    {
        int startHour = userSettings?.NightModeStartHour ?? config.NightModeStartHour;
        int endHour = userSettings?.NightModeEndHour ?? config.NightModeEndHour;
        int currentHour = DateTime.Now.Hour;

        if (startHour <= endHour)
        {
            // Same-day window, e.g., 08-18
            return currentHour >= startHour && currentHour < endHour;
        }

        // Overnight window, e.g., 22-06
        return currentHour >= startHour || currentHour < endHour;
    }
}
