namespace Jellyfin.Plugin.JellyWatching.Models;

/// <summary>
/// Determines how thresholds are evaluated for triggering the "still watching?" prompt.
/// </summary>
public enum ThresholdMode
{
    /// <summary>
    /// Trigger based on episode count only.
    /// </summary>
    EpisodeOnly,

    /// <summary>
    /// Trigger based on elapsed watch time only.
    /// </summary>
    TimeOnly,

    /// <summary>
    /// Trigger when both episode count AND time thresholds are exceeded.
    /// </summary>
    EpisodeAndTime,

    /// <summary>
    /// Trigger when either episode count OR time threshold is exceeded.
    /// </summary>
    EpisodeOrTime
}
