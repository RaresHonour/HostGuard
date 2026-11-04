using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public static class HostGuardUI
{
    private static GameObject? _lobbyButton;
    private static GameObject? _panel;
    private static GameObject? _rowsContainer;
    private static GameObject? _presetsContainer;
    private static bool _settingsTab = true;

    // Scroll state
    private static float _scrollY = 0f;
    private static float _scrollMax = 0f;
    private static float _totalContentHeight = 0f;

    // Viewport bounds (world Y coordinates for clipping)
    private static float _viewportTop = 0f;
    private static float _viewportBot = 0f;

    internal static readonly Color PanelBg = new(0.12f, 0.12f, 0.15f, 1f);
    internal static readonly Color TabActive = new(0.2f, 0.55f, 0.2f, 1f);
    internal static readonly Color TabInactive = new(0.25f, 0.25f, 0.28f, 1f);
    internal static readonly Color BtnGreen = new(0.2f, 0.6f, 0.2f, 1f);
    internal static readonly Color HdrColor = new(0.9f, 0.9f, 0.25f, 1f);
    internal static readonly Color Dim = new(0.45f, 0.45f, 0.45f, 1f);
    internal static readonly Color SubTabActive = new(0.15f, 0.45f, 0.6f, 1f);
    internal static readonly Color SubTabInactive = new(0.2f, 0.2f, 0.23f, 1f);

    private static float _camLeft, _camRight, _camTop, _camBot;
    private static bool _camReady;
    private static Material? _hudSpriteMaterial;
    private static string _hudSortingLayer = "";
    private static bool _sortingLayerDiscovered;

    internal static Material? HudSpriteMaterial => _hudSpriteMaterial;
    internal static string HudSortingLayer => _hudSortingLayer;

    // Panel layout (computed from camera)
    private static float _panelCX, _panelLeft, _panelW, _panelH, _contentTop;

    public static bool IsPanelOpen => _panel != null && _panel.activeSelf;

    public static void CreateLobbyButton(Transform hudParent)
    {
        if (_lobbyButton != null) Object.Destroy(_lobbyButton);
        _lobbyButton = new GameObject("HG_Button");
        _lobbyButton.transform.SetParent(hudParent);
        _lobbyButton.layer = 5;
        _lobbyButton.transform.localPosition = new Vector3(-3.8f, -2.5f, -50f);
        _lobbyButton.transform.localScale = Vector3.one;
        MakeBg(_lobbyButton.transform, Vector3.zero, 1.1f, 0.38f, BtnGreen, 10);
        MakeLabel(_lobbyButton.transform, "HostGuard", Vector3.zero, 2.2f, Color.white, TextAlignmentOptions.Center, 11);
        var c = _lobbyButton.AddComponent<BoxCollider2D>(); c.size = new Vector2(1.1f, 0.38f);
        var p = AddButton(_lobbyButton); p.OnClick.AddListener((Action)Toggle);
        _lobbyButton.SetActive(false);
    }

    public static void UpdateVisibility()
    {
        if (_lobbyButton == null) return;

        if (!_camReady)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                float h = cam.orthographicSize; float w = h * cam.aspect;
                _camLeft = -w; _camRight = w; _camTop = h; _camBot = -h;
                _camReady = true;
                _lobbyButton.transform.localPosition = new Vector3(_camLeft + 0.65f, _camBot + 0.25f, -50f);
            }
        }

        if (!_sortingLayerDiscovered && HudManager.Instance != null)
        {
            var renderers = HudManager.Instance.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Count; i++)
            {
                var sr = renderers[i];
                if (sr.sprite != null && sr.enabled)
                {
                    _hudSortingLayer = sr.sortingLayerName ?? "";
                    _hudSpriteMaterial = sr.material;
                    break;
                }
            }
            _sortingLayerDiscovered = true;
        }

        bool show = AmongUsClient.Instance != null
                 && AmongUsClient.Instance.AmHost
                 && !AmongUsClient.Instance.IsGameStarted;
        if (_lobbyButton.activeSelf != show) _lobbyButton.SetActive(show);

        if (IsPanelOpen && PlayerControl.LocalPlayer != null)
        {
            PlayerControl.LocalPlayer.moveable = false;
            try { PlayerControl.LocalPlayer.NetTransform.Halt(); } catch { }
        }

        // Scroll when panel open and settings tab visible
        if (IsPanelOpen && _settingsTab && _rowsContainer != null)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0f)
            {
                _scrollY = Mathf.Clamp(_scrollY - scroll * 0.3f, 0f, _scrollMax);
                _rowsContainer.transform.localPosition = new Vector3(0f, _scrollY, 0f);
                ClipToViewport();
            }
        }

        // Delegate keyboard input to active sub-views
        if (IsPanelOpen)
        {
            if (_settingsTab)
                SettingsTab.HandleKeyboardInput(() => RebuildSettings());
            else
                PresetsTab.HandleKeyboardInput(() => BuildPresets());
        }
    }

    private static void Toggle()
    {
        if (_panel == null) BuildPanel();
        if (_panel == null) return;
        bool open = !_panel.activeSelf;
        _panel.SetActive(open);
        if (open && _settingsTab)
            RebuildSettings();
        if (!open && PlayerControl.LocalPlayer != null)
            PlayerControl.LocalPlayer.moveable = true;
    }

    private static void Close()
    {
        if (_panel != null) _panel.SetActive(false);
        if (PlayerControl.LocalPlayer != null) PlayerControl.LocalPlayer.moveable = true;
    }

    // ==================== BUILD PANEL ====================

    private static void BuildPanel()
    {
        if (_panel != null) Object.Destroy(_panel);
        _scrollY = 0f;

        if (!_camReady) return;

        if (!_sortingLayerDiscovered && HudManager.Instance != null)
        {
            var renderers = HudManager.Instance.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Count; i++)
            {
                var sr = renderers[i];
                if (sr.sprite != null && sr.enabled)
                {
                    _hudSortingLayer = sr.sortingLayerName ?? "";
                    _hudSpriteMaterial = sr.material;
                    break;
                }
            }
            _sortingLayerDiscovered = true;
        }

        _panel = new GameObject("HG_Panel");
        _panel.transform.SetParent(_lobbyButton!.transform.parent);
        _panel.transform.localPosition = Vector3.zero;
        _panel.transform.localScale = Vector3.one;

        float screenW = _camRight - _camLeft;
        float screenH = _camTop - _camBot;
        _panelW = screenW * 0.30f;
        _panelH = screenH * 0.72f;
        float panelLeft = _camLeft + screenW * 0.02f;
        _panelLeft = panelLeft;
        _panelCX = panelLeft + _panelW / 2f;
        float panelTop = _panelH / 2f;
        _contentTop = panelTop - 0.95f;

        // Viewport: content visible between just below tabs and just above panel bottom
        _viewportTop = _contentTop + 0.05f;
        _viewportBot = -_panelH / 2f + 0.1f;

        // Border + Background
        MakeBg(_panel.transform, new Vector3(_panelCX, 0f, -99f), _panelW + 0.06f, _panelH + 0.06f, new Color(0.4f, 0.4f, 0.45f, 1f), 490);
        MakeBg(_panel.transform, new Vector3(_panelCX, 0f, -98.9f), _panelW, _panelH, PanelBg, 491);

        // Title
        MakeLabel(_panel.transform, "HostGuard Settings", new Vector3(_panelCX, panelTop - 0.22f, -100f),
            2.0f, Color.white, TextAlignmentOptions.Center, 500);

        // Close
        var closeObj = MakeLabel(_panel.transform, "X", new Vector3(_panelLeft + _panelW - 0.18f, panelTop - 0.22f, -100f),
            1.8f, new Color(1f, 0.3f, 0.3f), TextAlignmentOptions.Center, 500);
        closeObj.AddComponent<BoxCollider2D>().size = new Vector2(0.3f, 0.3f);
        AddButton(closeObj).OnClick.AddListener((Action)Close);

        // Tabs
        float tabY = panelTop - 0.55f;
        MakeTabBtn("Settings", _panelCX - _panelW * 0.2f, tabY, true, () => SwitchTab(true));
        MakeTabBtn("Presets", _panelCX + _panelW * 0.2f, tabY, false, () => SwitchTab(false));

        // Settings content (scrollable)
        _rowsContainer = new GameObject("Rows");
        _rowsContainer.transform.SetParent(_panel.transform);
        _rowsContainer.transform.localPosition = Vector3.zero;
        _rowsContainer.transform.localScale = Vector3.one;
        RenderAllSettings();
        ClipToViewport();

        // Presets content
        _presetsContainer = new GameObject("Presets");
        _presetsContainer.transform.SetParent(_panel.transform);
        _presetsContainer.transform.localPosition = Vector3.zero;
        _presetsContainer.transform.localScale = Vector3.one;
        BuildPresets();
        _presetsContainer.SetActive(false);

        _panel.SetActive(false);
    }

    private static void MakeTabBtn(string label, float x, float y, bool active, Action onClick)
    {
        var btn = new GameObject("Tab_" + label);
        btn.transform.SetParent(_panel!.transform);
        btn.transform.localPosition = new Vector3(x, y, -100f);
        btn.transform.localScale = Vector3.one;
        float tw = _panelW * 0.35f;
        MakeBg(btn.transform, Vector3.zero, tw, 0.25f, active ? TabActive : TabInactive, 500);
        MakeLabel(btn.transform, label, Vector3.zero, 1.4f, Color.white, TextAlignmentOptions.Center, 501);
        btn.AddComponent<BoxCollider2D>().size = new Vector2(tw, 0.25f);
        AddButton(btn).OnClick.AddListener((Action)onClick);
    }

    private static void SwitchTab(bool settings)
    {
        _settingsTab = settings;
        if (_rowsContainer != null) _rowsContainer.SetActive(settings);
        if (_presetsContainer != null) { _presetsContainer.SetActive(!settings); if (!settings) BuildPresets(); }
        UpdateTabVisual("Tab_Settings", settings);
        UpdateTabVisual("Tab_Presets", !settings);
    }

    /// <summary>
    /// Hides rows that are outside the viewport (above header or below panel bottom).
    /// Called after each scroll update.
    /// </summary>
    private static void ClipToViewport()
    {
        if (_rowsContainer == null) return;
        float containerY = _rowsContainer.transform.localPosition.y;

        for (int i = 0; i < _rowsContainer.transform.childCount; i++)
        {
            var child = _rowsContainer.transform.GetChild(i);
            float worldY = child.localPosition.y + containerY;
            bool visible = worldY <= _viewportTop && worldY >= _viewportBot;
            child.gameObject.SetActive(visible);
        }
    }

    private static void UpdateTabVisual(string name, bool active)
    {
        if (_panel == null) return;
        var t = _panel.transform.Find(name);
        if (t == null) return;
        var bg = t.Find("BG")?.GetComponent<SpriteRenderer>();
        if (bg != null) bg.color = active ? TabActive : TabInactive;
    }

    private static void RenderAllSettings()
    {
        if (_rowsContainer == null) return;
        float finalY = SettingsTab.Render(_rowsContainer, _panelLeft, _panelW, _panelCX, _contentTop, () => RebuildSettings());
        _totalContentHeight = _contentTop - finalY;
        float visibleH = _panelH - 1.2f;
        _scrollMax = Mathf.Max(0f, _totalContentHeight - visibleH);
    }

    private static void RebuildSettings(bool resetScroll = false)
    {
        if (_rowsContainer == null) return;
        if (resetScroll) _scrollY = 0f;
        for (int i = _rowsContainer.transform.childCount - 1; i >= 0; i--)
            Object.Destroy(_rowsContainer.transform.GetChild(i).gameObject);
        _rowsContainer.transform.localPosition = new Vector3(0f, _scrollY, 0f);
        RenderAllSettings();
        ClipToViewport();
    }

    private static void BuildPresets()
    {
        if (_presetsContainer == null) return;
        PresetsTab.Build(_presetsContainer, _panelCX, _panelLeft, _panelW, _contentTop, () => RebuildSettings());
    }

    // ==================== PRIMITIVES ====================

    internal static GameObject MakeBg(Transform parent, Vector3 pos, float w, float h, Color color, int order)
    {
        var o = new GameObject("BG");
        o.transform.SetParent(parent); o.transform.localPosition = pos; o.transform.localScale = new Vector3(w, h, 1f);
        var sr = o.AddComponent<SpriteRenderer>();
        sr.sprite = GetSquare(); sr.color = color; sr.sortingOrder = order;
        if (_hudSpriteMaterial != null) sr.material = _hudSpriteMaterial;
        if (!string.IsNullOrEmpty(_hudSortingLayer)) sr.sortingLayerName = _hudSortingLayer;
        return o;
    }

    internal static GameObject MakeLabel(Transform parent, string text, Vector3 pos,
        float size, Color color, TextAlignmentOptions align, int order)
    {
        var o = new GameObject("L");
        o.transform.SetParent(parent); o.transform.localPosition = pos; o.transform.localScale = Vector3.one;
        var tmp = o.AddComponent<TextMeshPro>();
        tmp.text = text; tmp.fontSize = size; tmp.alignment = align; tmp.color = color;
        tmp.sortingOrder = order; tmp.enableWordWrapping = false; tmp.fontStyle = FontStyles.Normal;
        tmp.overflowMode = TextOverflowModes.Overflow;
        var rt = tmp.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(3f, 0.3f);
        if (align == TextAlignmentOptions.Left) rt.pivot = new Vector2(0f, 0.5f);
        else if (align == TextAlignmentOptions.Right) rt.pivot = new Vector2(1f, 0.5f);
        else rt.pivot = new Vector2(0.5f, 0.5f);
        return o;
    }

    internal static PassiveButton AddButton(GameObject obj)
    {
        var p = obj.AddComponent<PassiveButton>();
        p.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        p.OnMouseOut = new UnityEvent(); p.OnMouseOver = new UnityEvent();
        return p;
    }

    private static Sprite? _sqSpr;
    private static Sprite GetSquare()
    {
        if (_sqSpr != null) return _sqSpr;
        var tex = new Texture2D(1, 1); tex.SetPixel(0, 0, Color.white); tex.Apply();
        _sqSpr = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _sqSpr;
    }

    public static void Cleanup()
    {
        if (_panel != null) Object.Destroy(_panel);
        if (_lobbyButton != null) Object.Destroy(_lobbyButton);
        _panel = null; _lobbyButton = null; _rowsContainer = null; _presetsContainer = null;
        _camReady = false;
        SettingsTab.Reset();
        PresetsTab.Reset();
    }
}
