namespace Jellyfin.Plugin.JellyWatching.Models;

/// <summary>
/// Represents the current watching state for a user session.
/// </summary>
public enum WatchingState
{
    /// <summary>
    /// No active watching session.
    /// </summary>
    Idle,

    /// <summary>
    /// User is actively watching episodes.
    /// </summary>
    Watching,

    /// <summary>
    /// Threshold reached, waiting for user to confirm they are still watching.
    /// </summary>
    PendingConfirmation,

    /// <summary>
    /// User has confirmed they are still watching.
    /// </summary>
    Confirmed
}
