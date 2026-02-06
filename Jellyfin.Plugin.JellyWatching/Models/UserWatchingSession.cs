using System;

namespace Jellyfin.Plugin.JellyWatching.Models;

/// <summary>
/// Tracks the watching session for a specific user and device combination.
/// </summary>
public class UserWatchingSession
{
    /// <summary>
    /// Gets or sets the current watching state.
    /// </summary>
    public WatchingState State { get; set; } = WatchingState.Idle;

    /// <summary>
    /// Gets or sets the series ID currently being watched.
    /// </summary>
    public Guid CurrentSeriesId { get; set; }

    /// <summary>
    /// Gets or sets the total number of consecutive episodes watched for the current series.
    /// </summary>
    public int EpisodeCount { get; set; }

    /// <summary>
    /// Gets or sets the episode count at the time of the last confirmation.
    /// </summary>
    public int LastConfirmationCount { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the session started or was last confirmed.
    /// </summary>
    public DateTime WatchingSince { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last activity timestamp.
    /// </summary>
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last known playback position in ticks for seek detection.
    /// </summary>
    public long? LastPositionTicks { get; set; }

    /// <summary>
    /// Gets or sets the last known audio stream index for track change detection.
    /// </summary>
    public int? LastAudioStreamIndex { get; set; }

    /// <summary>
    /// Gets or sets the last known subtitle stream index for track change detection.
    /// </summary>
    public int? LastSubtitleStreamIndex { get; set; }
}
