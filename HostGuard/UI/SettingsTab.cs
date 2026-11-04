using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public static class SettingsTab
{
    private static bool _showExactNames = false;
    private static bool _showContainsNames = false;
    private static bool _showExactWords = false;
    private static bool _showContainsWords = false;
    private static string? _addingToList = null;
    private static string _addInput = "";
    private static TextMeshPro? _addInputTmp = null;
    private static Action? _addConfirmAction = null;

    public static float Render(GameObject rowsContainer, float panelLeft, float panelW, float panelCX, float contentTop, Action rebuildSettings)
    {
        float y = contentTop;
        float rowH = 0.26f;

        // --- NAME FILTER ---
        HdrToggle(rowsContainer, panelLeft, panelW, y, "NAME FILTER", HostGuardConfig.NameFilterEnabled); y -= 0.3f;
        Row(rowsContainer, panelLeft, panelW, y, "Default Names", HostGuardConfig.KickDefaultNames, HostGuardConfig.BanForDefaultName); y -= rowH;
        Row(rowsContainer, panelLeft, panelW, y, "  Auto-Blacklist", HostGuardConfig.AutoBlacklistDefaultName, null); y -= rowH;
        Row(rowsContainer, panelLeft, panelW, y, "Strict Casing", HostGuardConfig.StrictDefaultNameCasing, null); y -= rowH;
        Row(rowsContainer, panelLeft, panelW, y, "Bad Names", HostGuardConfig.BanForBadName, null); y -= rowH;
        Row(rowsContainer, panelLeft, panelW, y, "  Auto-Blacklist", HostGuardConfig.AutoBlacklistBadName, null); y -= rowH;
        y = RenderWordList(rowsContainer, panelLeft, panelW, y, "Exact Match Names", HostGuardConfig.GetBadNameWordsList(), ref _showExactNames, "exactNames", HostGuardConfig.AddBadNameWord, HostGuardConfig.RemoveBadNameWord, rebuildSettings);
        y = RenderWordList(rowsContainer, panelLeft, panelW, y, "Contains Names", HostGuardConfig.GetContainsBannedNames(), ref _showContainsNames, "containsNames", HostGuardConfig.AddContainsBannedName, HostGuardConfig.RemoveContainsBannedName, rebuildSettings);

        // --- CHAT FILTER ---
        HdrToggle(rowsContainer, panelLeft, panelW, y, "CHAT FILTER", HostGuardConfig.ChatFilterEnabled); y -= 0.3f;
        BanKickRow(rowsContainer, panelLeft, panelW, y, "Banned Words", HostGuardConfig.BanForBannedWords); y -= rowH;
        Row(rowsContainer, panelLeft, panelW, y, "  Auto-Blacklist", HostGuardConfig.AutoBlacklistBannedWord, null); y -= rowH;
        y = RenderWordList(rowsContainer, panelLeft, panelW, y, "Exact Match Words", HostGuardConfig.GetBannedWordsList(), ref _showExactWords, "exactWords", HostGuardConfig.AddBannedWord, HostGuardConfig.RemoveBannedWord, rebuildSettings);
        y = RenderWordList(rowsContainer, panelLeft, panelW, y, "Contains Words", HostGuardConfig.GetContainsBannedWords(), ref _showContainsWords, "containsWords", HostGuardConfig.AddContainsBannedWord, HostGuardConfig.RemoveContainsBannedWord, rebuildSettings);

        // --- BOT PROTECTION ---
        HdrToggle(rowsContainer, panelLeft, panelW, y, "BOT PROTECTION", HostGuardConfig.BotProtectionEnabled); y -= 0.3f;
        Row(rowsContainer, panelLeft, panelW, y, "Known Bots Ban", HostGuardConfig.BanKnownBots, null); y -= rowH;
        Row(rowsContainer, panelLeft, panelW, y, "  Auto-Blacklist", HostGuardConfig.AutoBlacklistKnownBot, null); y -= rowH;
        Row(rowsContainer, panelLeft, panelW, y, "Cosmetic Detect", HostGuardConfig.CosmeticDetectionEnabled, HostGuardConfig.BanForSuspiciousCosmetics); y -= rowH;
        TxtRow(rowsContainer, panelLeft, panelW, y, "Bot Names (comma-separated)", HostGuardConfig.KnownBotNames); y -= rowH;
        TxtRow(rowsContainer, panelLeft, panelW, y, "Bot URLs (comma-separated)", HostGuardConfig.KnownBotUrls); y -= rowH;

        // --- FLOOD PROTECTION ---
        HdrToggle(rowsContainer, panelLeft, panelW, y, "FLOOD PROTECTION", HostGuardConfig.FloodProtectionEnabled); y -= 0.3f;
        Row(rowsContainer, panelLeft, panelW, y, "Meeting Spam", HostGuardConfig.MeetingSpamKick, null); y -= rowH;
        Row(rowsContainer, panelLeft, panelW, y, "Auto-Lock", HostGuardConfig.AutoLockOnFlood, null); y -= rowH;
        NumRow(rowsContainer, panelLeft, panelW, y, "Join Threshold", HostGuardConfig.FloodJoinThreshold, 1, 20, 1); y -= rowH;
        NumRow(rowsContainer, panelLeft, panelW, y, "Join Window", HostGuardConfig.FloodJoinWindowSeconds, 1, 30, 1); y -= rowH;
        NumRow(rowsContainer, panelLeft, panelW, y, "Leave Thresh", HostGuardConfig.RapidLeaveThreshold, 1, 10, 1); y -= rowH;
        NumRow(rowsContainer, panelLeft, panelW, y, "Mtg Threshold", HostGuardConfig.MeetingSpamThreshold, 1, 10, 1); y -= rowH;
        NumRow(rowsContainer, panelLeft, panelW, y, "Mtg Window", HostGuardConfig.MeetingSpamWindowSeconds, 5, 60, 5); y -= rowH;
        NumRow(rowsContainer, panelLeft, panelW, y, "Lock Duration", HostGuardConfig.AutoLockDurationSeconds, 0, 300, 5); y -= rowH;

        // --- ANTI-CHEAT ---
        HdrToggle(rowsContainer, panelLeft, panelW, y, "ANTI-CHEAT", HostGuardConfig.AntiCheatEnabled); y -= 0.3f;
        BanKickRow(rowsContainer, panelLeft, panelW, y, "Invalid RPC", HostGuardConfig.BanOnInvalidRpc); y -= rowH;
        Row(rowsContainer, panelLeft, panelW, y, "  Auto-Blacklist", HostGuardConfig.AutoBlacklistInvalidRpc, null); y -= rowH;
        NumRow(rowsContainer, panelLeft, panelW, y, "Chat Limit", HostGuardConfig.ChatRateLimit, 1, 20, 1); y -= rowH;
        NumRow(rowsContainer, panelLeft, panelW, y, "Limit Window", HostGuardConfig.ChatRateLimitWindowSeconds, 1, 30, 1); y -= rowH;

        // --- MIN LEVEL ---
        HdrToggle(rowsContainer, panelLeft, panelW, y, "MIN LEVEL", HostGuardConfig.MinLevelEnabled); y -= 0.3f;
        NumRow(rowsContainer, panelLeft, panelW, y, "Min Level", HostGuardConfig.MinLevel, 0, 100, 1); y -= rowH;
        BanKickRow(rowsContainer, panelLeft, panelW, y, "Low Level", HostGuardConfig.BanForLowLevel); y -= rowH;
        Row(rowsContainer, panelLeft, panelW, y, "  Auto-Blacklist", HostGuardConfig.AutoBlacklistLowLevel, null); y -= rowH;

        // --- GENERAL ---
        HostGuardUI.MakeLabel(rowsContainer.transform, "GENERAL", new Vector3(panelLeft + 0.12f, y, -100f),
            1.2f, HostGuardUI.HdrColor, TextAlignmentOptions.Left, 501); y -= 0.3f;
        Row(rowsContainer, panelLeft, panelW, y, "Auto-Start", HostGuardConfig.AutoStartEnabled, null); y -= rowH;
        NumRow(rowsContainer, panelLeft, panelW, y, "Start Players", HostGuardConfig.AutoStartPlayerCount, 0, 15, 1); y -= rowH;
        Row(rowsContainer, panelLeft, panelW, y, "Rules on Start", HostGuardConfig.SendRulesOnLobbyStart, null); y -= rowH;
        Row(rowsContainer, panelLeft, panelW, y, "Join Notifs", HostGuardConfig.VerboseJoinNotifications, null); y -= rowH;
        TxtRow(rowsContainer, panelLeft, panelW, y, "Ban List URL", HostGuardConfig.BanListUrl); y -= rowH;

        // --- UI ---
        HostGuardUI.MakeLabel(rowsContainer.transform, "UI", new Vector3(panelLeft + 0.12f, y, -100f),
            1.2f, HostGuardUI.HdrColor, TextAlignmentOptions.Left, 501); y -= 0.3f;
        Row(rowsContainer, panelLeft, panelW, y, "Notifications", HostGuardConfig.ShowNotifications, null); y -= rowH;

        // --- LOBBY ---
        HostGuardUI.MakeLabel(rowsContainer.transform, "LOBBY", new Vector3(panelLeft + 0.12f, y, -100f),
            1.2f, HostGuardUI.HdrColor, TextAlignmentOptions.Left, 501); y -= 0.3f;
        Row(rowsContainer, panelLeft, panelW, y, "Auto Return", HostGuardConfig.AutoReturnToLobby, null); y -= rowH;
        FloatRow(rowsContainer, panelLeft, panelW, y, "Return Delay", HostGuardConfig.AutoReturnDelay, 0f, 30f, 0.5f); y -= rowH;
        Row(rowsContainer, panelLeft, panelW, y, "Auto Start If Closing", HostGuardConfig.AutoStartIfClosing, null); y -= rowH;
        NumRow(rowsContainer, panelLeft, panelW, y, "Auto Start Threshold (s)", HostGuardConfig.AutoStartThreshold, 5, 120, 5); y -= rowH;
        NumRow(rowsContainer, panelLeft, panelW, y, "Min Players", HostGuardConfig.MinPlayersToAutoStart, 1, 15, 1); y -= rowH;

        // --- WHITELIST ---
        HostGuardUI.MakeLabel(rowsContainer.transform, "WHITELIST", new Vector3(panelLeft + 0.12f, y, -100f),
            1.2f, HostGuardUI.HdrColor, TextAlignmentOptions.Left, 501); y -= 0.28f;
        var wlCodes = HostGuardConfig.GetWhitelistedCodes().ToList();
        if (wlCodes.Count == 0)
        {
            HostGuardUI.MakeLabel(rowsContainer.transform, "(empty)", new Vector3(panelLeft + 0.12f, y, -100f),
                1.0f, HostGuardUI.Dim, TextAlignmentOptions.Left, 501); y -= 0.22f;
        }
        else
        {
            foreach (var code in wlCodes)
            {
                string c = code;
                HostGuardUI.MakeLabel(rowsContainer.transform, c, new Vector3(panelLeft + 0.12f, y, -100f),
                    0.95f, Color.white, TextAlignmentOptions.Left, 501);
                var rmObj = HostGuardUI.MakeLabel(rowsContainer.transform, "[X]",
                    new Vector3(panelLeft + panelW - 0.3f, y, -100f), 0.95f,
                    new Color(1f, 0.3f, 0.3f), TextAlignmentOptions.Center, 501);
                rmObj.AddComponent<BoxCollider2D>().size = new Vector2(0.3f, 0.2f);
                HostGuardUI.AddButton(rmObj).OnClick.AddListener((Action)(() =>
                {
                    HostGuardConfig.RemoveFromWhitelist(c);
                    rebuildSettings();
                }));
                y -= 0.22f;
            }
        }

        MakeActionBtn(rowsContainer, panelCX, panelW, y, "Export Whitelist", () =>
        {
            var codes = HostGuardConfig.GetWhitelistedCodes();
            var filename = $"whitelist_{SanitizeNickname()}_export.txt";
            File.WriteAllLines(Path.Combine(BepInEx.Paths.ConfigPath, filename), codes);
            ChatHelper.SendLocalMessage($"[HostGuard] Whitelist exported to {filename}");
        }); y -= 0.26f;
        MakeActionBtn(rowsContainer, panelCX, panelW, y, "Import Whitelist", () =>
        {
            var files = Directory.GetFiles(BepInEx.Paths.ConfigPath, "whitelist_*_import.txt");
            if (files.Length == 0)
            {
                ChatHelper.SendLocalMessage("[HostGuard] No import file found. Export a list, rename '_export' to '_import', place it in the BepInEx config folder, then press Import.");
                return;
            }
            var existing = HostGuardConfig.GetWhitelistedCodes();
            int added = 0, dupes = 0;
            foreach (var line in File.ReadAllLines(files[0]))
            {
                var code = line.Trim();
                if (code.Length == 0 || code.Contains(",")) continue;
                if (existing.Contains(code)) { dupes++; continue; }
                HostGuardConfig.AddToWhitelist(code);
                added++;
            }
            ChatHelper.SendLocalMessage($"[HostGuard] Whitelist import: {added} codes added, {dupes} duplicates skipped.");
            rebuildSettings();
        }); y -= 0.26f;
        MakeActionBtn(rowsContainer, panelCX, panelW, y, "Open Folder", () =>
        {
            Process.Start("explorer.exe", BepInEx.Paths.ConfigPath);
        }); y -= 0.26f;

        // --- BLACKLIST ---
        HostGuardUI.MakeLabel(rowsContainer.transform, "BLACKLIST", new Vector3(panelLeft + 0.12f, y, -100f),
            1.2f, HostGuardUI.HdrColor, TextAlignmentOptions.Left, 501); y -= 0.28f;
        var blCodes = Blacklist.GetAll();
        if (blCodes.Count == 0)
        {
            HostGuardUI.MakeLabel(rowsContainer.transform, "(empty)", new Vector3(panelLeft + 0.12f, y, -100f),
                1.0f, HostGuardUI.Dim, TextAlignmentOptions.Left, 501); y -= 0.22f;
        }
        else
        {
            foreach (var code in blCodes)
            {
                string c = code;
                HostGuardUI.MakeLabel(rowsContainer.transform, c, new Vector3(panelLeft + 0.12f, y, -100f),
                    0.95f, Color.white, TextAlignmentOptions.Left, 501);
                var rmObj = HostGuardUI.MakeLabel(rowsContainer.transform, "[X]",
                    new Vector3(panelLeft + panelW - 0.3f, y, -100f), 0.95f,
                    new Color(1f, 0.3f, 0.3f), TextAlignmentOptions.Center, 501);
                rmObj.AddComponent<BoxCollider2D>().size = new Vector2(0.3f, 0.2f);
                HostGuardUI.AddButton(rmObj).OnClick.AddListener((Action)(() =>
                {
                    Blacklist.Remove(c);
                    rebuildSettings();
                }));
                y -= 0.22f;
            }
        }

        MakeActionBtn(rowsContainer, panelCX, panelW, y, "Export Blacklist", () =>
        {
            var codes = Blacklist.GetAll();
            var filename = $"blacklist_{SanitizeNickname()}_export.txt";
            File.WriteAllLines(Path.Combine(BepInEx.Paths.ConfigPath, filename), codes);
            ChatHelper.SendLocalMessage($"[HostGuard] Blacklist exported to {filename}");
        }); y -= 0.26f;
        MakeActionBtn(rowsContainer, panelCX, panelW, y, "Import Blacklist", () =>
        {
            var files = Directory.GetFiles(BepInEx.Paths.ConfigPath, "blacklist_*_import.txt");
            if (files.Length == 0)
            {
                ChatHelper.SendLocalMessage("[HostGuard] No import file found. Export a list, rename '_export' to '_import', place it in the BepInEx config folder, then press Import.");
                return;
            }
            int added = 0, dupes = 0;
            foreach (var line in File.ReadAllLines(files[0]))
            {
                var code = line.Trim();
                if (code.Length == 0 || code.Contains(",")) continue;
                if (Blacklist.Add(code)) added++;
                else dupes++;
            }
            ChatHelper.SendLocalMessage($"[HostGuard] Blacklist import: {added} codes added, {dupes} duplicates skipped.");
            rebuildSettings();
        }); y -= 0.26f;
        MakeActionBtn(rowsContainer, panelCX, panelW, y, "Open Folder", () =>
        {
            Process.Start("explorer.exe", BepInEx.Paths.ConfigPath);
        }); y -= 0.26f;

        return y;
    }

    public static void HandleKeyboardInput(Action rebuildSettings)
    {
        if (_addingToList != null && _addInputTmp != null)
        {
            string typed = Input.inputString;
            for (int i = 0; i < typed.Length; i++)
            {
                char ch = typed[i];
                if (ch == '\b')
                {
                    if (_addInput.Length > 0)
                        _addInput = _addInput.Substring(0, _addInput.Length - 1);
                }
                else if (ch == '\r' || ch == '\n')
                {
                    _addConfirmAction?.Invoke();
                    return;
                }
                else if (ch == 27) // Escape
                {
                    _addingToList = null;
                    _addInputTmp = null;
                    _addConfirmAction = null;
                    rebuildSettings();
                    return;
                }
                else if (!char.IsControl(ch))
                {
                    _addInput += ch;
                }
            }
            _addInputTmp.text = "Add: " + _addInput + "_";
        }
    }

    public static void Reset()
    {
        _addingToList = null;
        _addInput = "";
        _addInputTmp = null;
        _addConfirmAction = null;
        _showExactNames = false;
        _showContainsNames = false;
        _showExactWords = false;
        _showContainsWords = false;
    }

    private static void HdrToggle(GameObject container, float panelLeft, float panelW, float y, string title, ConfigEntry<bool> enableCfg)
    {
        HostGuardUI.MakeLabel(container.transform, title, new Vector3(panelLeft + 0.12f, y, -100f),
            1.2f, HostGuardUI.HdrColor, TextAlignmentOptions.Left, 501);
        MakeSmallToggle(container.transform, enableCfg,
            new Vector3(panelLeft + panelW - 0.5f, y, -100f), "ON", "OFF", Color.green, Palette.ImpostorRed);
    }

    private static void Row(GameObject container, float panelLeft, float panelW, float y, string label, ConfigEntry<bool> cfg, ConfigEntry<bool>? banCfg)
    {
        float lx = panelLeft + 0.12f;
        float togX = panelLeft + panelW - 0.9f;
        HostGuardUI.MakeLabel(container.transform, label, new Vector3(lx, y, -100f), 1.05f, Color.white, TextAlignmentOptions.Left, 501);
        MakeSmallToggle(container.transform, cfg, new Vector3(togX, y, -100f), "ON", "OFF", Color.green, Palette.ImpostorRed);
        if (banCfg != null)
            MakeSmallToggle(container.transform, banCfg, new Vector3(togX + 0.45f, y, -100f), "BAN", "KICK",
                new Color(0.85f, 0.25f, 0.1f), new Color(0.85f, 0.7f, 0.1f));
    }

    private static void BanKickRow(GameObject container, float panelLeft, float panelW, float y, string label, ConfigEntry<bool> banCfg)
    {
        float lx = panelLeft + 0.12f;
        float togX = panelLeft + panelW - 0.9f;
        HostGuardUI.MakeLabel(container.transform, label, new Vector3(lx, y, -100f), 1.05f, Color.white, TextAlignmentOptions.Left, 501);
        MakeSmallToggle(container.transform, banCfg, new Vector3(togX, y, -100f), "BAN", "KICK",
            new Color(0.85f, 0.25f, 0.1f), new Color(0.85f, 0.7f, 0.1f));
    }

    private static float RenderWordList(GameObject container, float panelLeft, float panelW, float y, string title, List<string> items,
        ref bool showState, string listId, Func<string, bool> addFunc, Func<string, bool> removeFunc, Action rebuildSettings)
    {
        float lx = panelLeft + 0.12f;

        HostGuardUI.MakeLabel(container.transform, $"{title} ({items.Count})", new Vector3(lx, y, -100f),
            1.05f, Color.white, TextAlignmentOptions.Left, 501);

        string capturedId = listId;
        var togObj = HostGuardUI.MakeLabel(container.transform, showState ? "[Hide]" : "[Show]",
            new Vector3(panelLeft + panelW - 0.5f, y, -100f), 0.95f,
            new Color(0.5f, 0.8f, 1f), TextAlignmentOptions.Center, 501);
        togObj.AddComponent<BoxCollider2D>().size = new Vector2(0.45f, 0.2f);
        HostGuardUI.AddButton(togObj).OnClick.AddListener((Action)(() =>
        {
            if (capturedId == "exactNames") _showExactNames = !_showExactNames;
            else if (capturedId == "containsNames") _showContainsNames = !_showContainsNames;
            else if (capturedId == "exactWords") _showExactWords = !_showExactWords;
            else if (capturedId == "containsWords") _showContainsWords = !_showContainsWords;
            rebuildSettings();
        }));
        y -= 0.24f;

        if (!showState) return y;

        if (items.Count == 0)
        {
            HostGuardUI.MakeLabel(container.transform, "(empty)", new Vector3(lx + 0.1f, y, -100f),
                0.9f, HostGuardUI.Dim, TextAlignmentOptions.Left, 501);
            y -= 0.2f;
        }
        else
        {
            for (int i = 0; i < items.Count; i++)
            {
                string item = items[i];
                HostGuardUI.MakeLabel(container.transform, item, new Vector3(lx + 0.1f, y, -100f),
                    0.9f, Color.white, TextAlignmentOptions.Left, 501);
                string capturedItem = item;
                Func<string, bool> rmFn = removeFunc;
                var rmObj = HostGuardUI.MakeLabel(container.transform, "[X]",
                    new Vector3(panelLeft + panelW - 0.3f, y, -100f), 0.9f,
                    new Color(1f, 0.3f, 0.3f), TextAlignmentOptions.Center, 501);
                rmObj.AddComponent<BoxCollider2D>().size = new Vector2(0.3f, 0.18f);
                HostGuardUI.AddButton(rmObj).OnClick.AddListener((Action)(() => { rmFn(capturedItem); rebuildSettings(); }));
                y -= 0.2f;
            }
        }

        // Add input or [+ Add] button
        if (_addingToList == capturedId)
        {
            var inputLabel = HostGuardUI.MakeLabel(container.transform, "Add: _",
                new Vector3(lx + 0.1f, y, -100f), 0.9f, Color.yellow, TextAlignmentOptions.Left, 501);
            _addInputTmp = inputLabel.GetComponent<TextMeshPro>();

            Func<string, bool> addFn = addFunc;
            _addConfirmAction = () =>
            {
                if (!string.IsNullOrWhiteSpace(_addInput))
                    addFn(_addInput.Trim());
                _addingToList = null;
                _addInputTmp = null;
                _addConfirmAction = null;
                _addInput = "";
                rebuildSettings();
            };

            var okObj = HostGuardUI.MakeLabel(container.transform, "[OK]",
                new Vector3(panelLeft + panelW - 0.5f, y, -100f), 0.9f,
                Color.green, TextAlignmentOptions.Center, 501);
            okObj.AddComponent<BoxCollider2D>().size = new Vector2(0.35f, 0.18f);
            HostGuardUI.AddButton(okObj).OnClick.AddListener((Action)(() => _addConfirmAction?.Invoke()));

            var cancelObj = HostGuardUI.MakeLabel(container.transform, "[X]",
                new Vector3(panelLeft + panelW - 0.2f, y, -100f), 0.9f,
                Palette.ImpostorRed, TextAlignmentOptions.Center, 501);
            cancelObj.AddComponent<BoxCollider2D>().size = new Vector2(0.25f, 0.18f);
            HostGuardUI.AddButton(cancelObj).OnClick.AddListener((Action)(() =>
            {
                _addingToList = null;
                _addInputTmp = null;
                _addConfirmAction = null;
                _addInput = "";
                rebuildSettings();
            }));
            y -= 0.22f;
        }
        else
        {
            string addId = capturedId;
            var addObj = HostGuardUI.MakeLabel(container.transform, "[+ Add]",
                new Vector3(lx + 0.1f, y, -100f), 0.9f, Color.green, TextAlignmentOptions.Left, 501);
            addObj.AddComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.18f);
            HostGuardUI.AddButton(addObj).OnClick.AddListener((Action)(() =>
            {
                _addingToList = addId;
                _addInput = "";
                rebuildSettings();
            }));
            y -= 0.22f;
        }

        return y;
    }

    private static void TxtRow(GameObject container, float panelLeft, float panelW, float y, string label, ConfigEntry<string> cfg)
    {
        float lx = panelLeft + 0.12f;
        HostGuardUI.MakeLabel(container.transform, label, new Vector3(lx, y, -100f), 1.05f, Color.white, TextAlignmentOptions.Left, 501);
        string display = cfg.Value;
        if (display.Length > 30) display = display.Substring(0, 27) + "...";
        HostGuardUI.MakeLabel(container.transform, display, new Vector3(lx + panelW * 0.45f, y, -100f),
            0.85f, HostGuardUI.Dim, TextAlignmentOptions.Left, 501);
    }

    private static void NumRow(GameObject container, float panelLeft, float panelW, float y, string label, ConfigEntry<int> cfg, int min, int max, int step)
    {
        float lx = panelLeft + 0.12f;
        float numX = panelLeft + panelW - 0.9f;
        HostGuardUI.MakeLabel(container.transform, label, new Vector3(lx, y, -100f), 1.05f, Color.white, TextAlignmentOptions.Left, 501);

        int v = cfg.Value;
        var valObj = HostGuardUI.MakeLabel(container.transform, v.ToString(), new Vector3(numX, y, -100f),
            1.15f, Color.white, TextAlignmentOptions.Center, 502);
        var vt = valObj.GetComponent<TextMeshPro>();

        var minObj = HostGuardUI.MakeLabel(container.transform, "<", new Vector3(numX - 0.25f, y, -100f),
            1.3f, Palette.ImpostorRed, TextAlignmentOptions.Center, 502);
        minObj.AddComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.2f);
        HostGuardUI.AddButton(minObj).OnClick.AddListener((Action)(() => { v = Math.Max(min, v - step); cfg.Value = v; if (vt) vt.text = v.ToString(); }));

        var plusObj = HostGuardUI.MakeLabel(container.transform, ">", new Vector3(numX + 0.25f, y, -100f),
            1.3f, Color.green, TextAlignmentOptions.Center, 502);
        plusObj.AddComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.2f);
        HostGuardUI.AddButton(plusObj).OnClick.AddListener((Action)(() => { v = Math.Min(max, v + step); cfg.Value = v; if (vt) vt.text = v.ToString(); }));
    }

    private static void FloatRow(GameObject container, float panelLeft, float panelW, float y, string label, ConfigEntry<float> cfg, float min, float max, float step)
    {
        float lx = panelLeft + 0.12f;
        float numX = panelLeft + panelW - 0.9f;
        HostGuardUI.MakeLabel(container.transform, label, new Vector3(lx, y, -100f), 1.05f, Color.white, TextAlignmentOptions.Left, 501);

        float v = cfg.Value;
        var valObj = HostGuardUI.MakeLabel(container.transform, v.ToString("F1"), new Vector3(numX, y, -100f),
            1.15f, Color.white, TextAlignmentOptions.Center, 502);
        var vt = valObj.GetComponent<TextMeshPro>();

        var minObj = HostGuardUI.MakeLabel(container.transform, "<", new Vector3(numX - 0.25f, y, -100f),
            1.3f, Palette.ImpostorRed, TextAlignmentOptions.Center, 502);
        minObj.AddComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.2f);
        HostGuardUI.AddButton(minObj).OnClick.AddListener((Action)(() => { v = Math.Max(min, v - step); cfg.Value = v; if (vt) vt.text = v.ToString("F1"); }));

        var plusObj = HostGuardUI.MakeLabel(container.transform, ">", new Vector3(numX + 0.25f, y, -100f),
            1.3f, Color.green, TextAlignmentOptions.Center, 502);
        plusObj.AddComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.2f);
        HostGuardUI.AddButton(plusObj).OnClick.AddListener((Action)(() => { v = Math.Min(max, v + step); cfg.Value = v; if (vt) vt.text = v.ToString("F1"); }));
    }

    private static void MakeActionBtn(GameObject container, float panelCX, float panelW, float y, string label, Action onClick)
    {
        var btn = new GameObject("ActBtn");
        btn.transform.SetParent(container.transform);
        btn.transform.localPosition = new Vector3(panelCX, y, -100f);
        btn.transform.localScale = Vector3.one;
        float bw = panelW * 0.7f;
        HostGuardUI.MakeBg(btn.transform, Vector3.zero, bw, 0.2f, new Color(0.2f, 0.35f, 0.55f, 1f), 501);
        HostGuardUI.MakeLabel(btn.transform, label, Vector3.zero, 1.0f, Color.white, TextAlignmentOptions.Center, 502);
        btn.AddComponent<BoxCollider2D>().size = new Vector2(bw, 0.2f);
        HostGuardUI.AddButton(btn).OnClick.AddListener(onClick);
    }

    private static void MakeSmallToggle(Transform parent, ConfigEntry<bool> cfg, Vector3 pos,
        string onTxt, string offTxt, Color onCol, Color offCol)
    {
        var obj = new GameObject("Tog");
        obj.transform.SetParent(parent);
        obj.transform.localPosition = pos;
        obj.transform.localScale = Vector3.one;
        var bgObj = HostGuardUI.MakeBg(obj.transform, Vector3.zero, 0.38f, 0.16f, cfg.Value ? onCol : offCol, 501);
        var bgSR = bgObj.GetComponent<SpriteRenderer>();
        var txtObj = HostGuardUI.MakeLabel(obj.transform, cfg.Value ? onTxt : offTxt, Vector3.zero, 1.0f, Color.white, TextAlignmentOptions.Center, 502);
        var txtTMP = txtObj.GetComponent<TextMeshPro>();
        obj.AddComponent<BoxCollider2D>().size = new Vector2(0.38f, 0.16f);
        HostGuardUI.AddButton(obj).OnClick.AddListener((Action)(() =>
        {
            cfg.Value = !cfg.Value;
            if (bgSR) bgSR.color = cfg.Value ? onCol : offCol;
            if (txtTMP) txtTMP.text = cfg.Value ? onTxt : offTxt;
        }));
    }

    private static string SanitizeNickname()
    {
        var name = PlayerControl.LocalPlayer?.Data?.PlayerName;
        if (string.IsNullOrWhiteSpace(name)) name = "host";
        return Regex.Replace(name, @"[^a-zA-Z0-9_\-]", "_");
    }
}
