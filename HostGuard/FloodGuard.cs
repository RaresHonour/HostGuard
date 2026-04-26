using System;
using System.Collections.Generic;
using System.Linq;

public static class FloodGuard
{
    // Join flood tracking
    private static readonly Queue<DateTime> _recentJoins = new();
    private static DateTime _floodLockoutUntil = DateTime.MinValue;

    // Join-leave tracking
    private static readonly Dictionary<int, DateTime> _joinTimes = new();
    private static readonly Queue<DateTime> _rapidLeaves = new();
    private static DateTime _leaveLockoutUntil = DateTime.MinValue;

    // Meeting spam tracking
    private static readonly Dictionary<byte, Queue<DateTime>> _meetingCalls = new();

    // Session blacklist — friend codes banned during this session due to flood/attack
    // Separate from the permanent blacklist file. Cleared when lobby resets.
    private static readonly HashSet<string> _sessionBlacklist = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds a friend code to the session blacklist. These players get instantly banned on rejoin.
    /// </summary>
    public static void SessionBan(string friendCode)
    {
        if (!string.IsNullOrEmpty(friendCode) && friendCode.Contains('#'))
            _sessionBlacklist.Add(friendCode);
    }

    /// <summary>
    /// Checks if a friend code is session-blacklisted.
    /// </summary>
    public static bool IsSessionBanned(string friendCode)
    {
        return !string.IsNullOrEmpty(friendCode) && _sessionBlacklist.Contains(friendCode);
    }

    public static int SessionBlacklistCount => _sessionBlacklist.Count;

    /// <summary>
    /// Records a player join and returns true if the join should be blocked (flood detected).
    /// </summary>
    public static bool ShouldBlockJoin(int clientId)
    {
        var now = DateTime.UtcNow;

        // Track join time for leave detection
        _joinTimes[clientId] = now;

        // Check if we're in lockout from either flood type
        if (now < _floodLockoutUntil || now < _leaveLockoutUntil)
            return true;

        // Add to recent joins and clean old entries
        _recentJoins.Enqueue(now);
        int windowSeconds = HostGuardConfig.FloodJoinWindowSeconds.Value;
        while (_recentJoins.Count > 0 && (now - _recentJoins.Peek()).TotalSeconds > windowSeconds)
            _recentJoins.Dequeue();

        // Check flood threshold
        if (_recentJoins.Count >= HostGuardConfig.FloodJoinThreshold.Value)
        {
            _floodLockoutUntil = now.AddSeconds(10);
            HostGuardPlugin.Logger.LogWarning($"[FloodGuard] Join flood detected! {_recentJoins.Count} joins in {windowSeconds}s. Blocking new joins for 10s.");
            _recentJoins.Clear();
            LobbyLock.AutoLock();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Called when a player leaves. Detects rapid join-leave patterns.
    /// </summary>
    public static void OnPlayerLeft(int clientId)
    {
        if (!_joinTimes.TryGetValue(clientId, out var joinTime))
            return;

        _joinTimes.Remove(clientId);
        var now = DateTime.UtcNow;

        // Check if this was a rapid leave (within 1 second of joining)
        if ((now - joinTime).TotalSeconds < 1.0)
        {
            _rapidLeaves.Enqueue(now);

            // Clean old rapid leaves (10 second window)
            while (_rapidLeaves.Count > 0 && (now - _rapidLeaves.Peek()).TotalSeconds > 10)
                _rapidLeaves.Dequeue();

            if (_rapidLeaves.Count >= HostGuardConfig.RapidLeaveThreshold.Value)
            {
                _leaveLockoutUntil = now.AddSeconds(10);
                HostGuardPlugin.Logger.LogWarning($"[FloodGuard] Rapid join-leave detected! {_rapidLeaves.Count} in 10s. Blocking new joins for 10s.");
                ChatHelper.SendLocalMessage($"[Flood] Rapid join-leave attack detected. Blocking new joins for 10s.");
                _rapidLeaves.Clear();
                LobbyLock.AutoLock();
            }
        }
    }

    /// <summary>
    /// Records a meeting call and returns true if it should be blocked (spam detected).
    /// </summary>
    public static bool IsMeetingSpam(byte playerId)
    {
        if (!HostGuardConfig.MeetingSpamKick.Value)
            return false;

        var now = DateTime.UtcNow;

        if (!_meetingCalls.TryGetValue(playerId, out var calls))
        {
            calls = new Queue<DateTime>();
            _meetingCalls[playerId] = calls;
        }

        calls.Enqueue(now);
        int windowSeconds = HostGuardConfig.MeetingSpamWindowSeconds.Value;
        while (calls.Count > 0 && (now - calls.Peek()).TotalSeconds > windowSeconds)
            calls.Dequeue();

        return calls.Count >= HostGuardConfig.MeetingSpamThreshold.Value;
    }

    /// <summary>
    /// Resets all tracking state (call when lobby resets).
    /// </summary>
    public static void Reset()
    {
        _recentJoins.Clear();
        _joinTimes.Clear();
        _rapidLeaves.Clear();
        _meetingCalls.Clear();
        _sessionBlacklist.Clear();
        _floodLockoutUntil = DateTime.MinValue;
        _leaveLockoutUntil = DateTime.MinValue;
    }
}
