using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

public static class HostGuardSettingsPanel
{
    public static GameObject? Instance;
    private static readonly List<Action> _refreshCallbacks = new();

    public static GameObject Create(Transform parent)
    {
        var panel = new GameObject("HostGuardSettingsPanel");
        panel.transform.SetParent(parent);
        panel.transform.localPosition = new Vector3(0f, 0f, -1f);
        panel.transform.localScale = Vector3.one;

        var content = new GameObject("Content");
        content.transform.SetParent(panel.transform);
        content.transform.localPosition = Vector3.zero;
        content.transform.localScale = Vector3.one;

        float y = 1.8f;

        // === NAME FILTER ===
        UIFactory.CreateHeader(content.transform, "NAME FILTER", new Vector3(0f, y, -2f));
        y -= 0.45f;
        CreateToggleRow(content.transform, "Kick Default Names", HostGuardConfig.KickDefaultNames, ref y);
        CreateToggleRow(content.transform, "Ban Default Names", HostGuardConfig.BanForDefaultName, ref y);
        CreateToggleRow(content.transform, "Strict Casing", HostGuardConfig.StrictDefaultNameCasing, ref y);
        CreateToggleRow(content.transform, "Ban Bad Names", HostGuardConfig.BanForBadName, ref y);
        CreateTextRow(content.transform, "Bad Name Words", HostGuardConfig.BadNameWords, ref y);

        // === CHAT FILTER ===
        UIFactory.CreateHeader(content.transform, "CHAT FILTER", new Vector3(0f, y, -2f));
        y -= 0.45f;
        CreateToggleRow(content.transform, "Ban for Banned Words", HostGuardConfig.BanForBannedWords, ref y);
        CreateToggleRow(content.transform, "Contains Mode", HostGuardConfig.ContainsMode, ref y);
        CreateTextRow(content.transform, "Banned Words", HostGuardConfig.BannedWords, ref y);

        // === BOT PROTECTION ===
        UIFactory.CreateHeader(content.transform, "BOT PROTECTION", new Vector3(0f, y, -2f));
        y -= 0.45f;
        CreateToggleRow(content.transform, "Ban Known Bots", HostGuardConfig.BanKnownBots, ref y);
        CreateToggleRow(content.transform, "Cosmetic Detection", HostGuardConfig.CosmeticDetectionEnabled, ref y);
        CreateToggleRow(content.transform, "Ban Suspicious Cosmetics", HostGuardConfig.BanForSuspiciousCosmetics, ref y);
        CreateTextRow(content.transform, "Known Bot Names", HostGuardConfig.KnownBotNames, ref y);
        CreateTextRow(content.transform, "Known Bot URLs", HostGuardConfig.KnownBotUrls, ref y);

        // === FLOOD PROTECTION ===
        UIFactory.CreateHeader(content.transform, "FLOOD PROTECTION", new Vector3(0f, y, -2f));
        y -= 0.45f;
        CreateToggleRow(content.transform, "Enabled", HostGuardConfig.FloodProtectionEnabled, ref y);
        CreateToggleRow(content.transform, "Meeting Spam Kick", HostGuardConfig.MeetingSpamKick, ref y);
        CreateToggleRow(content.transform, "Auto-Lock on Flood", HostGuardConfig.AutoLockOnFlood, ref y);
        CreateNumberRow(content.transform, "Join Threshold", HostGuardConfig.FloodJoinThreshold, 1, 20, 1, ref y);
        CreateNumberRow(content.transform, "Join Window (sec)", HostGuardConfig.FloodJoinWindowSeconds, 1, 30, 1, ref y);
        CreateNumberRow(content.transform, "Rapid Leave Threshold", HostGuardConfig.RapidLeaveThreshold, 1, 10, 1, ref y);
        CreateNumberRow(content.transform, "Meeting Spam Threshold", HostGuardConfig.MeetingSpamThreshold, 1, 10, 1, ref y);
        CreateNumberRow(content.transform, "Meeting Window (sec)", HostGuardConfig.MeetingSpamWindowSeconds, 5, 60, 5, ref y);
        CreateNumberRow(content.transform, "Auto-Lock Duration (sec)", HostGuardConfig.AutoLockDurationSeconds, 0, 300, 5, ref y);

        // === ANTI-CHEAT ===
        UIFactory.CreateHeader(content.transform, "ANTI-CHEAT", new Vector3(0f, y, -2f));
        y -= 0.45f;
        CreateToggleRow(content.transform, "Enabled", HostGuardConfig.AntiCheatEnabled, ref y);
        CreateToggleRow(content.transform, "Ban on Invalid RPC", HostGuardConfig.BanOnInvalidRpc, ref y);
        CreateNumberRow(content.transform, "Chat Rate Limit", HostGuardConfig.ChatRateLimit, 1, 20, 1, ref y);
        CreateNumberRow(content.transform, "Rate Limit Window (sec)", HostGuardConfig.ChatRateLimitWindowSeconds, 1, 30, 1, ref y);

        // === GENERAL ===
        UIFactory.CreateHeader(content.transform, "GENERAL", new Vector3(0f, y, -2f));
        y -= 0.45f;
        CreateToggleRow(content.transform, "Auto-Start", HostGuardConfig.AutoStartEnabled, ref y);
        CreateNumberRow(content.transform, "Auto-Start Players", HostGuardConfig.AutoStartPlayerCount, 0, 15, 1, ref y);
        CreateToggleRow(content.transform, "Send Rules on Start", HostGuardConfig.SendRulesOnLobbyStart, ref y);
        CreateToggleRow(content.transform, "Join Notifications", HostGuardConfig.VerboseJoinNotifications, ref y);
        CreateTextRow(content.transform, "Rules Message", HostGuardConfig.RulesMessage, ref y);
        CreateTextRow(content.transform, "Ban List URL", HostGuardConfig.BanListUrl, ref y);

        // === WHITELIST / BLACKLIST ===
        UIFactory.CreateHeader(content.transform, "WHITELIST / BLACKLIST", new Vector3(0f, y, -2f));
        y -= 0.45f;

        // Whitelist display
        var wlCodes = HostGuardConfig.GetWhitelistedCodes();
        string wlText = wlCodes.Count > 0 ? string.Join(", ", wlCodes) : "(empty)";
        UIFactory.CreateTextDisplay(content.transform, "Whitelist", $"Whitelisted ({wlCodes.Count})",
            wlText, new Vector3(0f, y, -2f));
        y -= 0.45f;

        // Blacklist display
        var blCodes = Blacklist.GetAll();
        string blText = blCodes.Count > 0 ? string.Join(", ", blCodes) : "(empty)";
        UIFactory.CreateTextDisplay(content.transform, "Blacklist", $"Blacklisted ({blCodes.Count})",
            blText, new Vector3(0f, y, -2f));
        y -= 0.45f;

        Instance = panel;
        HostGuardPlugin.Logger.LogInfo($"[HostGuard UI] Settings panel created with {_refreshCallbacks.Count} toggles.");
        return panel;
    }

    private static void CreateToggleRow(Transform parent, string label, ConfigEntry<bool> config, ref float y)
    {
        var toggle = UIFactory.CreateToggle(parent, label, label, config.Value,
            newVal => { config.Value = newVal; },
            new Vector3(0f, y, -2f));

        var btn = toggle;
        _refreshCallbacks.Add(() =>
        {
            btn.onState = config.Value;
            btn.Background.color = config.Value ? Color.green : Palette.ImpostorRed;
        });

        y -= 0.5f;
    }

    private static void CreateNumberRow(Transform parent, string label, ConfigEntry<int> config,
        int min, int max, int step, ref float y)
    {
        UIFactory.CreateNumberStepper(parent, label, label, config.Value, min, max, step,
            newVal => { config.Value = newVal; },
            new Vector3(0f, y, -2f));
        y -= 0.5f;
    }

    private static void CreateTextRow(Transform parent, string label, ConfigEntry<string> config, ref float y)
    {
        UIFactory.CreateTextDisplay(parent, label, label, config.Value, new Vector3(0f, y, -2f));
        y -= 0.45f;
    }

    public static void RefreshAll()
    {
        foreach (var cb in _refreshCallbacks)
            cb();
    }

    public static void Cleanup()
    {
        _refreshCallbacks.Clear();
        Instance = null;
    }
}
