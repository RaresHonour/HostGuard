using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

/// <summary>
/// HostGuard sidebar panel UI.
/// A left-side panel in the lobby with paginated settings.
/// Reference: hostguard panel.png drawing by user.
/// </summary>
public static class HostGuardUI
{
    // State
    private static GameObject? _lobbyButton;
    private static GameObject? _panel;
    private static GameObject? _rowsContainer;
    private static GameObject? _presetsContainer;
    private static readonly List<Action> _refreshCbs = new();
    private static List<Action<Transform>> _rowDefs = new();
    private static int _page;
    private static int _presetNum = 1;
    private static bool _settingsTab = true;
    private static TextMeshPro? _pageTMP;

    // How many setting rows fit in the panel
    private const int PageSize = 11;

    // We compute layout from the camera at runtime so it works at any resolution
    private static float _camLeft, _camRight, _camTop, _camBot;
    private static bool _camReady;

    // Reference SpriteRenderer from HUD to copy rendering settings
    private static Material? _hudSpriteMaterial;
    private static string _hudSortingLayer = "";
    private static bool _sortingLayerDiscovered;

    public static bool IsPanelOpen => _panel != null && _panel.activeSelf;

    // Called from HudManager.Start
    public static void CreateLobbyButton(Transform hudParent)
    {
        if (_lobbyButton != null) Object.Destroy(_lobbyButton);

        _lobbyButton = new GameObject("HG_Button");
        _lobbyButton.transform.SetParent(hudParent);
        _lobbyButton.layer = 5;
        // Will be repositioned in UpdateVisibility once camera is ready
        _lobbyButton.transform.localPosition = new Vector3(-3.8f, -2.5f, -50f);
        _lobbyButton.transform.localScale = Vector3.one;

        MakeBg(_lobbyButton.transform, Vector3.zero, 1.1f, 0.38f, new Color(0.18f, 0.55f, 0.18f, 1f), 10);
        MakeLabel(_lobbyButton.transform, "HostGuard", Vector3.zero, 2.2f, Color.white, TextAlignmentOptions.Center, 11);

        var c = _lobbyButton.AddComponent<BoxCollider2D>();
        c.size = new Vector2(1.1f, 0.38f);
        var p = AddButton(_lobbyButton);
        p.OnClick.AddListener((Action)Toggle);

        _lobbyButton.SetActive(false);
    }

    // Called from HudManager.Update
    public static void UpdateVisibility()
    {
        if (_lobbyButton == null) return;

        // Compute camera bounds once
        if (!_camReady)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                float h = cam.orthographicSize;
                float w = h * cam.aspect;
                _camLeft = -w;
                _camRight = w;
                _camTop = h;
                _camBot = -h;
                _camReady = true;

                _lobbyButton.transform.localPosition = new Vector3(_camLeft + 0.65f, _camBot + 0.25f, -50f);
                HostGuardPlugin.Logger.LogInfo($"[HG UI] Camera bounds: L={_camLeft:F1} R={_camRight:F1} T={_camTop:F1} B={_camBot:F1}");
            }
        }

        // Discover rendering settings from existing HUD sprites (once)
        if (!_sortingLayerDiscovered && HudManager.Instance != null)
        {
            foreach (var sr in HudManager.Instance.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (sr.sprite != null && sr.enabled && sr.gameObject.activeInHierarchy)
                {
                    _hudSortingLayer = sr.sortingLayerName ?? "";
                    _hudSpriteMaterial = sr.material;
                    _sortingLayerDiscovered = true;
                    HostGuardPlugin.Logger.LogInfo($"[HG UI] HUD sprite: layer='{_hudSortingLayer}' order={sr.sortingOrder} material='{sr.material?.name}' renderQueue={sr.material?.renderQueue}");
                    break;
                }
            }
            if (!_sortingLayerDiscovered)
            {
                _sortingLayerDiscovered = true; // don't keep trying
                HostGuardPlugin.Logger.LogWarning("[HG UI] No active HUD SpriteRenderer found, using defaults.");
            }
        }

        bool show = AmongUsClient.Instance != null
                 && AmongUsClient.Instance.AmHost
                 && !AmongUsClient.Instance.IsGameStarted;

        if (_lobbyButton.activeSelf != show)
            _lobbyButton.SetActive(show);

        // Freeze player while panel open
        if (IsPanelOpen && PlayerControl.LocalPlayer != null)
        {
            PlayerControl.LocalPlayer.moveable = false;
            try { PlayerControl.LocalPlayer.NetTransform.Halt(); } catch { }
        }
    }

    private static void Toggle()
    {
        if (_panel == null) BuildPanel();
        if (_panel == null) return;
        bool open = !_panel.activeSelf;
        _panel.SetActive(open);
        if (!open) Unfreeze();
    }

    private static void Close()
    {
        if (_panel != null) _panel.SetActive(false);
        Unfreeze();
    }

    private static void Unfreeze()
    {
        if (PlayerControl.LocalPlayer != null)
            PlayerControl.LocalPlayer.moveable = true;
    }

    // ==================== PANEL BUILD ====================

    private static void BuildPanel()
    {
        if (_panel != null) Object.Destroy(_panel);
        _refreshCbs.Clear();
        _rowDefs.Clear();
        _page = 0;

        if (!_camReady) return;

        // Force sorting layer discovery before building
        if (!_sortingLayerDiscovered && HudManager.Instance != null)
        {
            foreach (var sr in HudManager.Instance.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (sr.sprite != null && sr.enabled)
                {
                    _hudSortingLayer = sr.sortingLayerName ?? "";
                    _hudSpriteMaterial = sr.material;
                    _sortingLayerDiscovered = true;
                    HostGuardPlugin.Logger.LogInfo($"[HG UI] Forced discovery: layer='{_hudSortingLayer}' material='{sr.material?.name}'");
                    break;
                }
            }
            _sortingLayerDiscovered = true;
        }

        _panel = new GameObject("HG_Panel");
        _panel.transform.SetParent(_lobbyButton!.transform.parent);
        _panel.transform.localPosition = Vector3.zero;
        _panel.transform.localScale = Vector3.one;

        // Panel dimensions from camera bounds
        // Left edge of screen + small margin, ~33% of screen width, ~75% of screen height
        float screenW = _camRight - _camLeft;
        float screenH = _camTop - _camBot;

        float panelW = screenW * 0.30f;
        float panelH = screenH * 0.72f;
        float panelLeft = _camLeft + screenW * 0.02f; // 2% margin from left edge
        float panelCenterX = panelLeft + panelW / 2f;
        float panelCenterY = 0f; // vertically centered

        float panelTop = panelCenterY + panelH / 2f;
        float panelBot = panelCenterY - panelH / 2f;

        HostGuardPlugin.Logger.LogInfo($"[HG UI] Panel: cx={panelCenterX:F2} cy={panelCenterY:F2} w={panelW:F2} h={panelH:F2}");

        // Border
        MakeBg(_panel.transform, new Vector3(panelCenterX, panelCenterY, -5f),
            panelW + 0.06f, panelH + 0.06f, new Color(0.4f, 0.4f, 0.45f, 1f), 490);

        // Background
        MakeBg(_panel.transform, new Vector3(panelCenterX, panelCenterY, -4.9f),
            panelW, panelH, new Color(0.11f, 0.11f, 0.14f, 1f), 491);

        // Title
        MakeLabel(_panel.transform, "HostGuard Settings",
            new Vector3(panelCenterX, panelTop - 0.22f, -100f), 2.0f, Color.white, TextAlignmentOptions.Center, 500);

        // Close X
        var closeObj = MakeLabel(_panel.transform, "X",
            new Vector3(panelLeft + panelW - 0.18f, panelTop - 0.22f, -100f), 1.8f, new Color(1f, 0.3f, 0.3f), TextAlignmentOptions.Center, 500);
        var closeCol = closeObj.AddComponent<BoxCollider2D>();
        closeCol.size = new Vector2(0.3f, 0.3f);
        AddButton(closeObj).OnClick.AddListener((Action)Close);

        // Tabs
        float tabY = panelTop - 0.55f;
        MakeTabBtn(_panel.transform, "Settings", panelCenterX - panelW * 0.2f, tabY, true, () => SwitchTab(true));
        MakeTabBtn(_panel.transform, "Presets", panelCenterX + panelW * 0.2f, tabY, false, () => SwitchTab(false));

        // Page nav at bottom
        float navY = panelBot + 0.2f;
        var prevObj = MakeLabel(_panel.transform, "< Prev", new Vector3(panelCenterX - panelW * 0.3f, navY, -100f),
            1.3f, Color.white, TextAlignmentOptions.Center, 500);
        prevObj.AddComponent<BoxCollider2D>().size = new Vector2(0.7f, 0.25f);
        AddButton(prevObj).OnClick.AddListener((Action)(() => FlipPage(-1)));

        var pageObj = MakeLabel(_panel.transform, "1/1", new Vector3(panelCenterX, navY, -100f),
            1.3f, new Color(0.5f, 0.5f, 0.5f), TextAlignmentOptions.Center, 500);
        _pageTMP = pageObj.GetComponent<TextMeshPro>();

        var nextObj = MakeLabel(_panel.transform, "Next >", new Vector3(panelCenterX + panelW * 0.3f, navY, -100f),
            1.3f, Color.white, TextAlignmentOptions.Center, 500);
        nextObj.AddComponent<BoxCollider2D>().size = new Vector2(0.7f, 0.25f);
        AddButton(nextObj).OnClick.AddListener((Action)(() => FlipPage(1)));

        // Build row definitions
        DefineRows();

        // Settings rows container
        _rowsContainer = new GameObject("Rows");
        _rowsContainer.transform.SetParent(_panel.transform);
        _rowsContainer.transform.localPosition = Vector3.zero;
        _rowsContainer.transform.localScale = Vector3.one;

        // Store layout info for rendering
        _panelCX = panelCenterX;
        _panelLeft = panelLeft;
        _panelW = panelW;
        _contentTop = panelTop - 0.95f;

        RenderPage();

        // Presets container
        _presetsContainer = new GameObject("Presets");
        _presetsContainer.transform.SetParent(_panel.transform);
        _presetsContainer.transform.localPosition = Vector3.zero;
        _presetsContainer.transform.localScale = Vector3.one;
        BuildPresets();
        _presetsContainer.SetActive(false);

        _panel.SetActive(false);
    }

    // Stored layout values used by RenderPage
    private static float _panelCX, _panelLeft, _panelW, _contentTop;

    private static void MakeTabBtn(Transform parent, string label, float x, float y, bool active, Action onClick)
    {
        var obj = new GameObject("Tab_" + label);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = new Vector3(x, y, -100f);
        obj.transform.localScale = Vector3.one;

        float tw = _panelW > 0 ? _panelW * 0.35f : 0.9f;
        MakeBg(obj.transform, Vector3.zero, tw, 0.25f, active ? new Color(0.2f, 0.55f, 0.2f, 1f) : new Color(0.25f, 0.25f, 0.28f, 1f), 500);
        MakeLabel(obj.transform, label, Vector3.zero, 1.4f, Color.white, TextAlignmentOptions.Center, 501);

        obj.AddComponent<BoxCollider2D>().size = new Vector2(tw, 0.25f);
        AddButton(obj).OnClick.AddListener((Action)onClick);
    }

    private static void SwitchTab(bool settings)
    {
        _settingsTab = settings;
        if (_rowsContainer != null) _rowsContainer.SetActive(settings);
        if (_presetsContainer != null) { _presetsContainer.SetActive(!settings); if (!settings) RefreshPresets(); }
        // Update visuals
        UpdateTabVisual("Tab_Settings", settings);
        UpdateTabVisual("Tab_Presets", !settings);
        // Show/hide page nav
        // Page nav objects are direct children of _panel, we just control their visibility via the page label
    }

    private static void UpdateTabVisual(string name, bool active)
    {
        if (_panel == null) return;
        var t = _panel.transform.Find(name);
        if (t == null) return;
        var bg = t.Find("BG")?.GetComponent<SpriteRenderer>();
        if (bg != null) bg.color = active ? new Color(0.2f, 0.55f, 0.2f, 1f) : new Color(0.25f, 0.25f, 0.28f, 1f);
    }

    private static void FlipPage(int dir)
    {
        int maxP = Math.Max(0, (_rowDefs.Count - 1) / PageSize);
        _page = Mathf.Clamp(_page + dir, 0, maxP);
        RenderPage();
    }

    private static void RenderPage()
    {
        if (_rowsContainer == null) return;
        _refreshCbs.Clear();

        // Destroy old rows
        for (int i = _rowsContainer.transform.childCount - 1; i >= 0; i--)
            Object.Destroy(_rowsContainer.transform.GetChild(i).gameObject);

        int start = _page * PageSize;
        int end = Math.Min(start + PageSize, _rowDefs.Count);
        float y = _contentTop;

        for (int i = start; i < end; i++)
        {
            var row = new GameObject("R");
            row.transform.SetParent(_rowsContainer.transform);
            row.transform.localPosition = new Vector3(_panelLeft + 0.12f, y, -100f);
            row.transform.localScale = Vector3.one;
            _rowDefs[i](row.transform);
            y -= 0.26f;
        }

        int maxP = Math.Max(1, (_rowDefs.Count - 1) / PageSize + 1);
        if (_pageTMP != null) _pageTMP.text = $"{_page + 1}/{maxP}";
    }

    // ==================== ROW DEFINITIONS ====================

    private static void DefineRows()
    {
        _rowDefs.Clear();
        H("NAME FILTER");
        R("Default Names", HostGuardConfig.KickDefaultNames, HostGuardConfig.BanForDefaultName);
        R("Strict Casing", HostGuardConfig.StrictDefaultNameCasing, null);
        R("Bad Names", HostGuardConfig.BanForBadName, null);
        H("CHAT FILTER");
        R("Banned Words", HostGuardConfig.BanForBannedWords, null);
        R("Contains Mode", HostGuardConfig.ContainsMode, null);
        H("BOT PROTECTION");
        R("Known Bots", HostGuardConfig.BanKnownBots, null);
        R("Cosmetic Detect", HostGuardConfig.CosmeticDetectionEnabled, HostGuardConfig.BanForSuspiciousCosmetics);
        H("FLOOD PROTECTION");
        R("Flood Protect", HostGuardConfig.FloodProtectionEnabled, null);
        R("Meeting Spam", HostGuardConfig.MeetingSpamKick, null);
        R("Auto-Lock", HostGuardConfig.AutoLockOnFlood, null);
        N("Join Threshold", HostGuardConfig.FloodJoinThreshold, 1, 20, 1);
        N("Join Window", HostGuardConfig.FloodJoinWindowSeconds, 1, 30, 1);
        N("Leave Thresh", HostGuardConfig.RapidLeaveThreshold, 1, 10, 1);
        N("Mtg Threshold", HostGuardConfig.MeetingSpamThreshold, 1, 10, 1);
        N("Mtg Window", HostGuardConfig.MeetingSpamWindowSeconds, 5, 60, 5);
        N("Lock Duration", HostGuardConfig.AutoLockDurationSeconds, 0, 300, 5);
        H("ANTI-CHEAT");
        R("Anti-Cheat", HostGuardConfig.AntiCheatEnabled, HostGuardConfig.BanOnInvalidRpc);
        N("Chat Limit", HostGuardConfig.ChatRateLimit, 1, 20, 1);
        N("Limit Window", HostGuardConfig.ChatRateLimitWindowSeconds, 1, 30, 1);
        H("GENERAL");
        R("Auto-Start", HostGuardConfig.AutoStartEnabled, null);
        N("Start Players", HostGuardConfig.AutoStartPlayerCount, 0, 15, 1);
        R("Rules on Start", HostGuardConfig.SendRulesOnLobbyStart, null);
        R("Join Notifs", HostGuardConfig.VerboseJoinNotifications, null);
        H("LISTS");
        _rowDefs.Add(p =>
        {
            int wl = HostGuardConfig.GetWhitelistedCodes().Count;
            int bl = Blacklist.GetAll().Count;
            MakeLabel(p, $"WL: {wl} | BL: {bl}", new Vector3(0f, 0f, 0f), 1.1f,
                new Color(0.5f, 0.5f, 0.5f), TextAlignmentOptions.Left, 501);
        });
    }

    // Row definition helpers — these add lambdas to _rowDefs
    private static void H(string title)
    {
        _rowDefs.Add(p => MakeLabel(p, title, Vector3.zero, 1.2f,
            new Color(0.9f, 0.9f, 0.25f), TextAlignmentOptions.Left, 501));
    }

    private static void R(string label, ConfigEntry<bool> cfg, ConfigEntry<bool>? banCfg)
    {
        _rowDefs.Add(p =>
        {
            // All positions relative to row origin (which is at panel left edge + margin)
            MakeLabel(p, label, new Vector3(0f, 0f, 0f), 1.05f, Color.white, TextAlignmentOptions.Left, 501);

            // ON/OFF toggle — positioned relative to panel width
            float togX = _panelW - 0.9f; // near right edge
            MakeToggle(p, cfg, new Vector3(togX, 0f, 0f), "ON", "OFF", Color.green, Palette.ImpostorRed);

            // BAN/KICK toggle
            if (banCfg != null)
            {
                MakeToggle(p, banCfg, new Vector3(togX + 0.45f, 0f, 0f), "BAN", "KICK",
                    new Color(0.85f, 0.25f, 0.1f), new Color(0.85f, 0.7f, 0.1f));
            }
        });
    }

    private static void N(string label, ConfigEntry<int> cfg, int min, int max, int step)
    {
        _rowDefs.Add(p =>
        {
            MakeLabel(p, label, Vector3.zero, 1.05f, Color.white, TextAlignmentOptions.Left, 501);

            float x = _panelW - 0.9f;
            int v = cfg.Value;
            var valObj = MakeLabel(p, v.ToString(), new Vector3(x, 0f, 0f), 1.15f, Color.white, TextAlignmentOptions.Center, 502);
            var vt = valObj.GetComponent<TextMeshPro>();

            var minObj = MakeLabel(p, "<", new Vector3(x - 0.25f, 0f, 0f), 1.3f, Palette.ImpostorRed, TextAlignmentOptions.Center, 502);
            minObj.AddComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.2f);
            AddButton(minObj).OnClick.AddListener((Action)(() => { v = Math.Max(min, v - step); cfg.Value = v; if (vt) vt.text = v.ToString(); }));

            var plusObj = MakeLabel(p, ">", new Vector3(x + 0.25f, 0f, 0f), 1.3f, Color.green, TextAlignmentOptions.Center, 502);
            plusObj.AddComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.2f);
            AddButton(plusObj).OnClick.AddListener((Action)(() => { v = Math.Min(max, v + step); cfg.Value = v; if (vt) vt.text = v.ToString(); }));
        });
    }

    private static void MakeToggle(Transform p, ConfigEntry<bool> cfg, Vector3 pos,
        string onTxt, string offTxt, Color onCol, Color offCol)
    {
        var obj = new GameObject("Tog");
        obj.transform.SetParent(p);
        obj.transform.localPosition = pos;
        obj.transform.localScale = Vector3.one;

        var bgObj = MakeBg(obj.transform, Vector3.zero, 0.38f, 0.16f, cfg.Value ? onCol : offCol, 501);
        var bgSR = bgObj.GetComponent<SpriteRenderer>();
        var txtObj = MakeLabel(obj.transform, cfg.Value ? onTxt : offTxt, Vector3.zero, 1.0f, Color.white, TextAlignmentOptions.Center, 502);
        var txtTMP = txtObj.GetComponent<TextMeshPro>();

        obj.AddComponent<BoxCollider2D>().size = new Vector2(0.38f, 0.16f);
        AddButton(obj).OnClick.AddListener((Action)(() =>
        {
            cfg.Value = !cfg.Value;
            if (bgSR) bgSR.color = cfg.Value ? onCol : offCol;
            if (txtTMP) txtTMP.text = cfg.Value ? onTxt : offTxt;
        }));
        _refreshCbs.Add(() =>
        {
            if (bgSR) bgSR.color = cfg.Value ? onCol : offCol;
            if (txtTMP) txtTMP.text = cfg.Value ? onTxt : offTxt;
        });
    }

    // ==================== PRESETS ====================

    private static void BuildPresets()
    {
        if (_presetsContainer == null) return;
        var saveObj = MakeLabel(_presetsContainer.transform, "[ Save Settings ]",
            new Vector3(_panelCX, _contentTop, -100f), 1.5f, Color.green, TextAlignmentOptions.Center, 501);
        saveObj.AddComponent<BoxCollider2D>().size = new Vector2(1.5f, 0.25f);
        AddButton(saveObj).OnClick.AddListener((Action)(() =>
        {
            PresetManager.SavePreset("Preset " + _presetNum++);
            RefreshPresets();
        }));
    }

    private static void RefreshPresets()
    {
        if (_presetsContainer == null) return;
        for (int i = _presetsContainer.transform.childCount - 1; i >= 1; i--)
            Object.Destroy(_presetsContainer.transform.GetChild(i).gameObject);

        var presets = PresetManager.GetPresetNames();
        float y = _contentTop - 0.35f;
        foreach (var pr in presets)
        {
            string n = pr;
            MakeLabel(_presetsContainer.transform, n, new Vector3(_panelLeft + 0.15f, y, -100f),
                1.1f, Color.white, TextAlignmentOptions.Left, 501);

            var loadObj = MakeLabel(_presetsContainer.transform, "Load",
                new Vector3(_panelCX + _panelW * 0.2f, y, -100f), 1.0f, Color.green, TextAlignmentOptions.Center, 501);
            loadObj.AddComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.2f);
            AddButton(loadObj).OnClick.AddListener((Action)(() =>
            {
                if (PresetManager.LoadPreset(n)) foreach (var cb in _refreshCbs) cb();
            }));

            var delObj = MakeLabel(_presetsContainer.transform, "X",
                new Vector3(_panelCX + _panelW * 0.4f, y, -100f), 1.0f, Palette.ImpostorRed, TextAlignmentOptions.Center, 501);
            delObj.AddComponent<BoxCollider2D>().size = new Vector2(0.25f, 0.2f);
            AddButton(delObj).OnClick.AddListener((Action)(() => { PresetManager.DeletePreset(n); RefreshPresets(); }));

            y -= 0.28f;
        }
        if (presets.Count == 0)
            MakeLabel(_presetsContainer.transform, "No presets yet.",
                new Vector3(_panelCX, _contentTop - 0.5f, -100f), 1.2f, new Color(0.4f, 0.4f, 0.4f), TextAlignmentOptions.Center, 501);
    }

    // ==================== PRIMITIVES ====================

    private static GameObject MakeBg(Transform parent, Vector3 pos, float w, float h, Color color, int order)
    {
        var obj = new GameObject("BG");
        obj.transform.SetParent(parent);
        obj.transform.localPosition = pos;
        obj.transform.localScale = new Vector3(w, h, 1f);
        var sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = GetSquare();
        sr.color = color;
        sr.sortingOrder = order;

        // Copy material from a known-visible HUD sprite
        if (_hudSpriteMaterial != null)
            sr.material = _hudSpriteMaterial;
        if (!string.IsNullOrEmpty(_hudSortingLayer))
            sr.sortingLayerName = _hudSortingLayer;

        return obj;
    }

    private static GameObject MakeLabel(Transform parent, string text, Vector3 pos,
        float size, Color color, TextAlignmentOptions align, int order)
    {
        var obj = new GameObject("L");
        obj.transform.SetParent(parent);
        obj.transform.localPosition = pos;
        obj.transform.localScale = Vector3.one;
        var tmp = obj.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = color;
        tmp.sortingOrder = order;
        tmp.enableWordWrapping = false;
        tmp.fontStyle = FontStyles.Normal;
        tmp.overflowMode = TextOverflowModes.Overflow;
        var rt = tmp.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(3f, 0.3f);
        // Set pivot based on alignment so text renders from the object's position
        if (align == TextAlignmentOptions.Left)
            rt.pivot = new Vector2(0f, 0.5f);
        else if (align == TextAlignmentOptions.Right)
            rt.pivot = new Vector2(1f, 0.5f);
        else
            rt.pivot = new Vector2(0.5f, 0.5f);
        return obj;
    }

    private static PassiveButton AddButton(GameObject obj)
    {
        var p = obj.AddComponent<PassiveButton>();
        p.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        p.OnMouseOut = new UnityEvent();
        p.OnMouseOver = new UnityEvent();
        return p;
    }

    private static Sprite? _sqSpr;
    private static Sprite GetSquare()
    {
        if (_sqSpr != null) return _sqSpr;
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _sqSpr = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _sqSpr;
    }

    public static void Cleanup()
    {
        if (_panel != null) Object.Destroy(_panel);
        if (_lobbyButton != null) Object.Destroy(_lobbyButton);
        _panel = null; _lobbyButton = null; _rowsContainer = null; _presetsContainer = null;
        _refreshCbs.Clear(); _rowDefs.Clear(); _camReady = false;
    }
}
