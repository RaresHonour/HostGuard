using System;

public static class LobbyLock
{
    private static bool _manuallyLocked = false;
    private static DateTime _autoUnlockAt = DateTime.MinValue;

    /// <summary>
    /// Returns true if the lobby is currently locked (manually or by auto-lock).
    /// </summary>
    public static bool IsLocked => _manuallyLocked || DateTime.UtcNow < _autoUnlockAt;

    /// <summary>
    /// Manually lock the lobby (sets it to private).
    /// </summary>
    public static void Lock()
    {
        _manuallyLocked = true;
        SetLobbyPrivate(true);
        HostGuardPlugin.Logger.LogInfo("[HostGuard] Lobby manually locked.");
    }

    /// <summary>
    /// Manually unlock the lobby (sets it back to public).
    /// </summary>
    public static void Unlock()
    {
        _manuallyLocked = false;
        _autoUnlockAt = DateTime.MinValue;
        SetLobbyPrivate(false);
        HostGuardPlugin.Logger.LogInfo("[HostGuard] Lobby unlocked.");
    }

    /// <summary>
    /// Auto-lock the lobby due to a flood attack. Will auto-unlock after the configured duration.
    /// </summary>
    public static void AutoLock()
    {
        if (!HostGuardConfig.AutoLockOnFlood.Value) return;
        if (_manuallyLocked) return; // already locked

        int duration = HostGuardConfig.AutoLockDurationSeconds.Value;
        if (duration > 0)
            _autoUnlockAt = DateTime.UtcNow.AddSeconds(duration);
        else
            _autoUnlockAt = DateTime.MaxValue; // stay locked until manual unlock

        SetLobbyPrivate(true);
        string durationMsg = duration > 0 ? $" Auto-unlock in {duration}s." : " Use !unlock to reopen.";
        ChatHelper.SendLocalMessage($"[Flood] Lobby auto-locked!{durationMsg}");
        HostGuardPlugin.Logger.LogWarning($"[HostGuard] Lobby auto-locked for {(duration > 0 ? $"{duration}s" : "indefinitely")}.");
    }

    /// <summary>
    /// Called periodically to check if auto-lock has expired.
    /// </summary>
    public static void CheckAutoUnlock()
    {
        if (!_manuallyLocked && _autoUnlockAt != DateTime.MinValue && DateTime.UtcNow >= _autoUnlockAt)
        {
            _autoUnlockAt = DateTime.MinValue;
            SetLobbyPrivate(false);
            ChatHelper.SendLocalMessage("[Flood] Lobby auto-unlocked.");
            HostGuardPlugin.Logger.LogInfo("[HostGuard] Lobby auto-unlocked after cooldown.");
        }
    }

    /// <summary>
    /// Reset state (call when leaving a lobby).
    /// </summary>
    public static void Reset()
    {
        _manuallyLocked = false;
        _autoUnlockAt = DateTime.MinValue;
    }

    private static void SetLobbyPrivate(bool isPrivate)
    {
        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            AmongUsClient.Instance.ChangeGamePublic(!isPrivate);
        }
    }
}
