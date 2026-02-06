using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.JellyWatching.Models;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Session;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JellyWatching.Services;

/// <summary>
/// Monitors consecutive episode playback and intervenes when the threshold is reached.
/// </summary>
public sealed class WatchingMonitorService : IHostedService, IDisposable
{
    /// <summary>
    /// Seek detection threshold: 30 seconds in ticks.
    /// </summary>
    private const long SeekThresholdTicks = 300_000_000;

    private readonly ISessionManager _sessionManager;
    private readonly ILogger<WatchingMonitorService> _logger;
    private readonly ConcurrentDictionary<string, UserWatchingSession> _sessions = new();
    private Timer? _cleanupTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="WatchingMonitorService"/> class.
    /// </summary>
    /// <param name="sessionManager">The session manager.</param>
    /// <param name="logger">The logger.</param>
    public WatchingMonitorService(ISessionManager sessionManager, ILogger<WatchingMonitorService> logger)
    {
        _sessionManager = sessionManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("JellyWatching monitor service starting");

        _sessionManager.PlaybackStart += OnPlaybackStart;
        _sessionManager.PlaybackProgress += OnPlaybackProgress;
        _sessionManager.PlaybackStopped += OnPlaybackStopped;

        _cleanupTimer = new Timer(CleanupStaleSessions, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("JellyWatching monitor service stopping");

        _sessionManager.PlaybackStart -= OnPlaybackStart;
        _sessionManager.PlaybackProgress -= OnPlaybackProgress;
        _sessionManager.PlaybackStopped -= OnPlaybackStopped;

        _cleanupTimer?.Change(Timeout.Infinite, Timeout.Infinite);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }

    private static string GetSessionKey(SessionInfo session)
    {
        return $"{session.UserId}_{session.DeviceId}";
    }

    private static bool ShouldIntervene(EffectiveSettings settings, int episodesSinceConfirmation, double minutesSinceConfirmation)
    {
        bool episodeExceeded = episodesSinceConfirmation > settings.EpisodeThreshold;
        bool timeExceeded = minutesSinceConfirmation > settings.TimeThresholdMinutes;

        return settings.ThresholdMode switch
        {
            ThresholdMode.EpisodeOnly => episodeExceeded,
            ThresholdMode.TimeOnly => timeExceeded,
            ThresholdMode.EpisodeAndTime => episodeExceeded && timeExceeded,
            ThresholdMode.EpisodeOrTime => episodeExceeded || timeExceeded,
            _ => episodeExceeded
        };
    }

    private void OnPlaybackStart(object? sender, PlaybackProgressEventArgs e)
    {
        var config = Plugin.Instance?.Configuration;
        if (config is null || !config.EnablePlugin)
        {
            return;
        }

        var item = e.MediaInfo;
        if (item is null || !item.SeriesId.HasValue || item.SeriesId.Value == Guid.Empty)
        {
            return;
        }

        var session = e.Session;
        if (session is null)
        {
            return;
        }

        var key = GetSessionKey(session);
        var userId = session.UserId.ToString("N");
        var effective = SettingsResolver.Resolve(config, userId);

        if (!effective.Enabled)
        {
            _logger.LogDebug("JellyWatching: Plugin disabled for user {UserId}, skipping", userId);
            return;
        }

        var seriesId = item.SeriesId.Value;
        var watchSession = _sessions.GetOrAdd(key, _ => new UserWatchingSession());

        lock (watchSession)
        {
            watchSession.LastActivity = DateTime.UtcNow;

            // Reset activity tracking fields for the new episode
            watchSession.LastPositionTicks = null;
            watchSession.LastAudioStreamIndex = null;
            watchSession.LastSubtitleStreamIndex = null;

            if (watchSession.CurrentSeriesId != seriesId)
            {
                // Series changed, reset
                watchSession.CurrentSeriesId = seriesId;
                watchSession.EpisodeCount = 1;
                watchSession.LastConfirmationCount = 0;
                watchSession.WatchingSince = DateTime.UtcNow;
                watchSession.State = WatchingState.Watching;
                _logger.LogDebug("JellyWatching: New series for {Key}, count reset to 1", key);
                return;
            }

            // Same series, increment
            watchSession.EpisodeCount++;
            int episodesSinceConfirmation = watchSession.EpisodeCount - watchSession.LastConfirmationCount;
            double minutesSinceConfirmation = (DateTime.UtcNow - watchSession.WatchingSince).TotalMinutes;

            _logger.LogDebug(
                "JellyWatching: {Key} - episodes since confirm: {Episodes}, minutes: {Minutes:F1}",
                key,
                episodesSinceConfirmation,
                minutesSinceConfirmation);

            if (ShouldIntervene(effective, episodesSinceConfirmation, minutesSinceConfirmation))
            {
                watchSession.State = WatchingState.PendingConfirmation;
                _logger.LogInformation(
                    "JellyWatching: Threshold reached for {Key} (mode={Mode}, episodes={Episodes}, minutes={Minutes:F1}), intervening",
                    key,
                    effective.ThresholdMode,
                    episodesSinceConfirmation,
                    minutesSinceConfirmation);
                _ = InterventionAsync(session, effective);
            }
            else
            {
                watchSession.State = watchSession.LastConfirmationCount > 0
                    ? WatchingState.Confirmed
                    : WatchingState.Watching;
            }
        }
    }

    private void OnPlaybackProgress(object? sender, PlaybackProgressEventArgs e)
    {
        var config = Plugin.Instance?.Configuration;
        if (config is null || !config.EnablePlugin)
        {
            return;
        }

        var session = e.Session;
        if (session is null)
        {
            return;
        }

        var key = GetSessionKey(session);

        if (!_sessions.TryGetValue(key, out var watchSession))
        {
            return;
        }

        var userId = session.UserId.ToString("N");
        var effective = SettingsResolver.Resolve(config, userId);

        lock (watchSession)
        {
            watchSession.LastActivity = DateTime.UtcNow;

            // Activity detection: seek and track changes reset the time counter
            if (config.ActivityDetectionEnabled)
            {
                DetectActivity(watchSession, e, key);
            }

            // Check time threshold during playback (for TimeOnly and EpisodeOrTime modes)
            if (watchSession.State == WatchingState.Watching || watchSession.State == WatchingState.Confirmed)
            {
                if (effective.ThresholdMode == ThresholdMode.TimeOnly || effective.ThresholdMode == ThresholdMode.EpisodeOrTime)
                {
                    int episodesSinceConfirmation = watchSession.EpisodeCount - watchSession.LastConfirmationCount;

                    // Cooldown: skip time-based checks on the same episode the user just confirmed on
                    if (episodesSinceConfirmation < 1)
                    {
                        return;
                    }

                    double minutesSinceConfirmation = (DateTime.UtcNow - watchSession.WatchingSince).TotalMinutes;

                    if (ShouldIntervene(effective, episodesSinceConfirmation, minutesSinceConfirmation))
                    {
                        watchSession.State = WatchingState.PendingConfirmation;
                        _logger.LogInformation(
                            "JellyWatching: Time threshold reached for {Key} ({Minutes:F1} min), intervening",
                            key,
                            minutesSinceConfirmation);
                        _ = InterventionAsync(session, effective);
                        return;
                    }
                }
            }

            if (watchSession.State == WatchingState.PendingConfirmation && !e.IsPaused)
            {
                // User unpaused - they confirmed they're still watching
                watchSession.State = WatchingState.Confirmed;
                watchSession.LastConfirmationCount = watchSession.EpisodeCount;
                watchSession.WatchingSince = DateTime.UtcNow;
                _logger.LogInformation("JellyWatching: User confirmed for {Key}, counters reset", key);
            }
        }
    }

    private void DetectActivity(UserWatchingSession watchSession, PlaybackProgressEventArgs e, string key)
    {
        var playState = e.Session?.PlayState;
        if (playState is null)
        {
            return;
        }

        bool activityDetected = false;

        // Seek detection: position jumped more than 30 seconds
        long? currentTicks = playState.PositionTicks;
        if (currentTicks.HasValue && watchSession.LastPositionTicks.HasValue)
        {
            long delta = Math.Abs(currentTicks.Value - watchSession.LastPositionTicks.Value);
            if (delta > SeekThresholdTicks)
            {
                activityDetected = true;
                _logger.LogDebug("JellyWatching: Seek detected for {Key} (delta={Delta} ticks)", key, delta);
            }
        }

        // Audio track change detection (including turning off: value → null)
        int? currentAudio = playState.AudioStreamIndex;
        if (watchSession.LastAudioStreamIndex.HasValue && currentAudio != watchSession.LastAudioStreamIndex)
        {
            activityDetected = true;
            _logger.LogDebug("JellyWatching: Audio track change detected for {Key}", key);
        }

        // Subtitle track change detection (including turning off: value → null)
        int? currentSubtitle = playState.SubtitleStreamIndex;
        if (watchSession.LastSubtitleStreamIndex.HasValue && currentSubtitle != watchSession.LastSubtitleStreamIndex)
        {
            activityDetected = true;
            _logger.LogDebug("JellyWatching: Subtitle track change detected for {Key}", key);
        }

        // Update tracked values
        watchSession.LastPositionTicks = currentTicks;
        watchSession.LastAudioStreamIndex = currentAudio;
        watchSession.LastSubtitleStreamIndex = currentSubtitle;

        // Activity resets the time counter only, NOT the episode count
        if (activityDetected)
        {
            watchSession.WatchingSince = DateTime.UtcNow;
            _logger.LogInformation("JellyWatching: Activity detected for {Key}, timer reset", key);
        }
    }

    private void OnPlaybackStopped(object? sender, PlaybackStopEventArgs e)
    {
        var session = e.Session;
        if (session is null)
        {
            return;
        }

        var key = GetSessionKey(session);

        if (!e.PlayedToCompletion)
        {
            // Manual stop - remove session
            _sessions.TryRemove(key, out _);
            _logger.LogDebug("JellyWatching: Manual stop for {Key}, session removed", key);
        }
    }

    private async Task InterventionAsync(SessionInfo session, EffectiveSettings settings)
    {
        try
        {
            // Small delay to let the episode start playing
            await Task.Delay(2000).ConfigureAwait(false);

            if (settings.AutoPauseEnabled)
            {
                await _sessionManager.SendPlaystateCommand(
                    session.Id,
                    session.Id,
                    new PlaystateRequest
                    {
                        Command = PlaystateCommand.Pause
                    },
                    CancellationToken.None).ConfigureAwait(false);

                _logger.LogInformation("JellyWatching: Paused session {SessionId}", session.Id);
            }

            await _sessionManager.SendGeneralCommand(
                session.Id,
                session.Id,
                new GeneralCommand
                {
                    Name = GeneralCommandType.DisplayMessage,
                    Arguments =
                    {
                        ["Header"] = "JellyWatching",
                        ["Text"] = settings.CustomMessage,
                        ["TimeoutMs"] = "0"
                    }
                },
                CancellationToken.None).ConfigureAwait(false);

            _logger.LogInformation("JellyWatching: Message sent to session {SessionId}", session.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JellyWatching: Error during intervention for session {SessionId}", session.Id);
        }
    }

    private void CleanupStaleSessions(object? state)
    {
        var config = Plugin.Instance?.Configuration;
        var timeout = config?.SessionTimeoutMinutes ?? 30;
        var cutoff = DateTime.UtcNow.AddMinutes(-timeout);

        foreach (var kvp in _sessions)
        {
            if (kvp.Value.LastActivity < cutoff)
            {
                _sessions.TryRemove(kvp.Key, out _);
                _logger.LogDebug("JellyWatching: Cleaned up stale session {Key}", kvp.Key);
            }
        }
    }
}
