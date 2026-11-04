using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public static class PresetsTab
{
    private static int _configPresetNum = 1;
    private static int _gamePresetNum = 1;
    private static bool _gamePresetsMode = false;
    private static string? _renamingPreset = null;
    private static string _renameInput = "";
    private static TextMeshPro? _renameInputTmp = null;
    private static Action? _renameConfirmAction = null;

    public static void Build(GameObject presetsContainer, float panelCX, float panelLeft, float panelW, float contentTop, Action rebuildSettingsCallback)
    {
        // Clear all children
        for (int i = presetsContainer.transform.childCount - 1; i >= 0; i--)
            Object.Destroy(presetsContainer.transform.GetChild(i).gameObject);

        float y = contentTop;

        // Sub-tab buttons: Config | Game
        float stW = panelW * 0.3f;

        var configTab = new GameObject("PSub_Config");
        configTab.transform.SetParent(presetsContainer.transform);
        configTab.transform.localPosition = new Vector3(panelCX - panelW * 0.17f, y, -100f);
        configTab.transform.localScale = Vector3.one;
        HostGuardUI.MakeBg(configTab.transform, Vector3.zero, stW, 0.22f, !_gamePresetsMode ? HostGuardUI.SubTabActive : HostGuardUI.SubTabInactive, 501);
        HostGuardUI.MakeLabel(configTab.transform, "Config", Vector3.zero, 1.2f, Color.white, TextAlignmentOptions.Center, 502);
        configTab.AddComponent<BoxCollider2D>().size = new Vector2(stW, 0.22f);
        HostGuardUI.AddButton(configTab).OnClick.AddListener((Action)(() =>
        {
            _gamePresetsMode = false;
            Build(presetsContainer, panelCX, panelLeft, panelW, contentTop, rebuildSettingsCallback);
        }));

        var gameTab = new GameObject("PSub_Game");
        gameTab.transform.SetParent(presetsContainer.transform);
        gameTab.transform.localPosition = new Vector3(panelCX + panelW * 0.17f, y, -100f);
        gameTab.transform.localScale = Vector3.one;
        HostGuardUI.MakeBg(gameTab.transform, Vector3.zero, stW, 0.22f, _gamePresetsMode ? HostGuardUI.SubTabActive : HostGuardUI.SubTabInactive, 501);
        HostGuardUI.MakeLabel(gameTab.transform, "Game", Vector3.zero, 1.2f, Color.white, TextAlignmentOptions.Center, 502);
        gameTab.AddComponent<BoxCollider2D>().size = new Vector2(stW, 0.22f);
        HostGuardUI.AddButton(gameTab).OnClick.AddListener((Action)(() =>
        {
            _gamePresetsMode = true;
            Build(presetsContainer, panelCX, panelLeft, panelW, contentTop, rebuildSettingsCallback);
        }));

        y -= 0.38f;

        // Save button
        string saveLabel = _gamePresetsMode ? "[ Save Game Settings ]" : "[ Save Config ]";
        var saveObj = HostGuardUI.MakeLabel(presetsContainer.transform, saveLabel,
            new Vector3(panelCX, y, -100f), 1.3f, Color.green, TextAlignmentOptions.Center, 501);
        saveObj.AddComponent<BoxCollider2D>().size = new Vector2(1.8f, 0.25f);
        HostGuardUI.AddButton(saveObj).OnClick.AddListener((Action)(() =>
        {
            if (_gamePresetsMode)
            {
                if (GamePresetManager.SavePreset("Game " + _gamePresetNum++))
                    ChatHelper.SendLocalMessage("Game settings saved.");
                else
                    ChatHelper.SendLocalMessage("Could not save — game options not available.");
            }
            else
            {
                PresetManager.SavePreset("Config " + _configPresetNum++);
                ChatHelper.SendLocalMessage("Config preset saved.");
            }
            Build(presetsContainer, panelCX, panelLeft, panelW, contentTop, rebuildSettingsCallback);
        }));

        y -= 0.38f;

        // Open Folder / Refresh buttons
        string folderPath = _gamePresetsMode ? GamePresetManager.GetPresetsDir() : PresetManager.GetPresetsDir();

        var openObj = HostGuardUI.MakeLabel(presetsContainer.transform, "Open Folder",
            new Vector3(panelCX - panelW * 0.17f, y, -100f), 1.0f, new Color(0.5f, 0.8f, 1f), TextAlignmentOptions.Center, 501);
        openObj.AddComponent<BoxCollider2D>().size = new Vector2(0.8f, 0.2f);
        HostGuardUI.AddButton(openObj).OnClick.AddListener((Action)(() =>
        {
            Process.Start("explorer.exe", folderPath);
            ChatHelper.SendLocalMessage("[HostGuard] Preset folder opened.");
        }));

        var refreshObj = HostGuardUI.MakeLabel(presetsContainer.transform, "Refresh",
            new Vector3(panelCX + panelW * 0.17f, y, -100f), 1.0f, new Color(1f, 0.8f, 0.3f), TextAlignmentOptions.Center, 501);
        refreshObj.AddComponent<BoxCollider2D>().size = new Vector2(0.6f, 0.2f);
        HostGuardUI.AddButton(refreshObj).OnClick.AddListener((Action)(() =>
        {
            _renamingPreset = null;
            Build(presetsContainer, panelCX, panelLeft, panelW, contentTop, rebuildSettingsCallback);
            ChatHelper.SendLocalMessage("[HostGuard] Preset list refreshed.");
        }));

        y -= 0.32f;

        // Preset list
        var presets = _gamePresetsMode ? GamePresetManager.GetPresetNames() : PresetManager.GetPresetNames();

        if (presets.Count == 0)
        {
            HostGuardUI.MakeLabel(presetsContainer.transform, "No presets yet.\nDrop .txt files into the folder, then press Refresh.",
                new Vector3(panelCX, y, -100f), 1.0f, HostGuardUI.Dim, TextAlignmentOptions.Center, 501);
        }
        else
        {
            for (int idx = 0; idx < presets.Count; idx++)
            {
                string n = presets[idx];

                // If renaming this preset, show input row instead
                if (_renamingPreset == n)
                {
                    _renameInput = "";
                    var inputLabel = HostGuardUI.MakeLabel(presetsContainer.transform, "New name: _",
                        new Vector3(panelLeft + 0.15f, y, -100f), 1.0f, Color.yellow, TextAlignmentOptions.Left, 501);
                    _renameInputTmp = inputLabel.GetComponent<TextMeshPro>();

                    string capturedName = n;
                    _renameConfirmAction = () =>
                    {
                        if (string.IsNullOrWhiteSpace(_renameInput))
                            ChatHelper.SendLocalMessage("[HostGuard] Name cannot be empty.");
                        else
                        {
                            bool ok = _gamePresetsMode
                                ? GamePresetManager.RenamePreset(capturedName, _renameInput)
                                : PresetManager.RenamePreset(capturedName, _renameInput);
                            if (ok)
                                ChatHelper.SendLocalMessage($"[HostGuard] Preset renamed to '{_renameInput}'.");
                        }
                        _renamingPreset = null;
                        _renameInputTmp = null;
                        _renameConfirmAction = null;
                        Build(presetsContainer, panelCX, panelLeft, panelW, contentTop, rebuildSettingsCallback);
                    };

                    var confirmObj = HostGuardUI.MakeLabel(presetsContainer.transform, "OK",
                        new Vector3(panelCX + panelW * 0.3f, y, -100f), 1.0f, Color.green, TextAlignmentOptions.Center, 501);
                    confirmObj.AddComponent<BoxCollider2D>().size = new Vector2(0.3f, 0.2f);
                    HostGuardUI.AddButton(confirmObj).OnClick.AddListener((Action)(() => _renameConfirmAction?.Invoke()));

                    var cancelObj = HostGuardUI.MakeLabel(presetsContainer.transform, "X",
                        new Vector3(panelCX + panelW * 0.42f, y, -100f), 1.0f, Palette.ImpostorRed, TextAlignmentOptions.Center, 501);
                    cancelObj.AddComponent<BoxCollider2D>().size = new Vector2(0.25f, 0.2f);
                    HostGuardUI.AddButton(cancelObj).OnClick.AddListener((Action)(() =>
                    {
                        _renamingPreset = null;
                        _renameInputTmp = null;
                        _renameConfirmAction = null;
                        Build(presetsContainer, panelCX, panelLeft, panelW, contentTop, rebuildSettingsCallback);
                    }));

                    y -= 0.28f;
                    continue;
                }

                HostGuardUI.MakeLabel(presetsContainer.transform, n, new Vector3(panelLeft + 0.15f, y, -100f),
                    1.1f, Color.white, TextAlignmentOptions.Left, 501);

                var loadObj = HostGuardUI.MakeLabel(presetsContainer.transform, "Load",
                    new Vector3(panelCX + panelW * 0.1f, y, -100f), 1.0f, Color.green, TextAlignmentOptions.Center, 501);
                loadObj.AddComponent<BoxCollider2D>().size = new Vector2(0.4f, 0.2f);
                HostGuardUI.AddButton(loadObj).OnClick.AddListener((Action)(() =>
                {
                    if (_gamePresetsMode)
                    {
                        if (GamePresetManager.LoadPreset(n))
                            ChatHelper.SendLocalMessage($"Game preset '{n}' loaded.");
                        else
                            ChatHelper.SendLocalMessage("Could not load — game options not available.");
                    }
                    else
                    {
                        if (PresetManager.LoadPreset(n))
                        {
                            rebuildSettingsCallback();
                            ChatHelper.SendLocalMessage($"Config preset '{n}' loaded.");
                        }
                    }
                }));

                string renameTarget = n;
                var renObj = HostGuardUI.MakeLabel(presetsContainer.transform, "Ren",
                    new Vector3(panelCX + panelW * 0.27f, y, -100f), 1.0f, new Color(1f, 0.8f, 0.3f), TextAlignmentOptions.Center, 501);
                renObj.AddComponent<BoxCollider2D>().size = new Vector2(0.35f, 0.2f);
                HostGuardUI.AddButton(renObj).OnClick.AddListener((Action)(() =>
                {
                    _renamingPreset = renameTarget;
                    Build(presetsContainer, panelCX, panelLeft, panelW, contentTop, rebuildSettingsCallback);
                }));

                var delObj = HostGuardUI.MakeLabel(presetsContainer.transform, "X",
                    new Vector3(panelCX + panelW * 0.42f, y, -100f), 1.0f, Palette.ImpostorRed, TextAlignmentOptions.Center, 501);
                delObj.AddComponent<BoxCollider2D>().size = new Vector2(0.25f, 0.2f);
                HostGuardUI.AddButton(delObj).OnClick.AddListener((Action)(() =>
                {
                    if (_gamePresetsMode)
                        GamePresetManager.DeletePreset(n);
                    else
                        PresetManager.DeletePreset(n);
                    Build(presetsContainer, panelCX, panelLeft, panelW, contentTop, rebuildSettingsCallback);
                }));

                y -= 0.28f;
            }
        }
    }

    public static void HandleKeyboardInput(Action rebuildPresetsCallback)
    {
        if (_renamingPreset != null && _renameInputTmp != null)
        {
            string typed = Input.inputString;
            for (int i = 0; i < typed.Length; i++)
            {
                char c = typed[i];
                if (c == '\b') // backspace
                {
                    if (_renameInput.Length > 0)
                        _renameInput = _renameInput.Substring(0, _renameInput.Length - 1);
                }
                else if (c == '\r' || c == '\n') // enter
                {
                    _renameConfirmAction?.Invoke();
                    return;
                }
                else if (c == 27) // escape
                {
                    _renamingPreset = null;
                    rebuildPresetsCallback();
                    return;
                }
                else if (!char.IsControl(c))
                {
                    _renameInput += c;
                }
            }
            _renameInputTmp.text = "New name: " + _renameInput + "_";
        }
    }

    public static void Reset()
    {
        _renamingPreset = null;
        _renameInput = "";
        _renameInputTmp = null;
        _renameConfirmAction = null;
    }
}
