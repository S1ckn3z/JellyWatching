namespace Jellyfin.Plugin.JellyWatching.Models;

/// <summary>
/// XML-serializable wrapper for per-user settings (XmlSerializer cannot serialize Dictionary).
/// </summary>
public class UserSettingsEntry
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user display name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the per-user settings.
    /// </summary>
    public UserSettings Settings { get; set; } = new();
}
