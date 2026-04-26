using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public static class UIFactory
{
    private static ToggleButtonBehaviour _toggleTemplate;
    private static bool _initialized;

    public static void Init(OptionsMenuBehaviour optionsMenu)
    {
        if (_initialized && _toggleTemplate != null) return;

        _toggleTemplate = optionsMenu.CensorChatButton;
        if (_toggleTemplate != null)
        {
            _toggleTemplate = Object.Instantiate(_toggleTemplate);
            Object.DontDestroyOnLoad(_toggleTemplate);
            _toggleTemplate.gameObject.SetActive(false);
            _initialized = true;
            HostGuardPlugin.Logger.LogInfo("[HostGuard UI] UIFactory initialized.");
        }
    }

    public static ToggleButtonBehaviour CreateToggle(
        Transform parent, string name, string label,
        bool initialState, Action<bool> onToggle, Vector3 position)
    {
        var button = Object.Instantiate(_toggleTemplate, parent);
        button.name = name;
        button.transform.localPosition = position;
        button.transform.localScale = new Vector3(0.55f, 1f, 1f);
        button.gameObject.SetActive(true);

        button.onState = initialState;
        button.Background.color = initialState ? Color.green : Palette.ImpostorRed;
        button.Text.text = label;
        button.Text.fontSizeMin = button.Text.fontSizeMax = 1.8f;
        button.Text.GetComponent<RectTransform>().sizeDelta = new Vector2(2.5f, 2f);
        // Counter the X scale so text isn't stretched
        button.Text.transform.localScale = new Vector3(1f / 0.55f, 1f, 1f);

        var collider = button.GetComponent<BoxCollider2D>();
        if (collider != null)
            collider.size = new Vector2(2.5f, 0.7f);

        var passive = button.GetComponent<PassiveButton>();
        if (passive != null)
        {
            passive.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            passive.OnMouseOut = new UnityEvent();
            passive.OnMouseOver = new UnityEvent();

            passive.OnClick.AddListener((Action)(() =>
            {
                button.onState = !button.onState;
                button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;
                onToggle(button.onState);
            }));

            passive.OnMouseOver.AddListener((Action)(() =>
                button.Background.color = button.onState
                    ? new Color(0f, 0.7f, 0f, 1f)
                    : new Color(0.7f, 0.1f, 0.1f, 1f)));

            passive.OnMouseOut.AddListener((Action)(() =>
                button.Background.color = button.onState ? Color.green : Palette.ImpostorRed));
        }

        return button;
    }

    public static GameObject CreateNumberStepper(
        Transform parent, string name, string label,
        int initialValue, int min, int max, int step,
        Action<int> onChange, Vector3 position)
    {
        var container = new GameObject(name);
        container.transform.SetParent(parent);
        container.transform.localPosition = position;
        container.transform.localScale = Vector3.one;

        int currentValue = initialValue;

        // Label text
        var labelObj = CreateTextLabel(container.transform, label, new Vector3(-1.2f, 0f, 0f), 1.6f, TextAlignmentOptions.Right);

        // Value display
        var valueObj = CreateTextLabel(container.transform, currentValue.ToString(), new Vector3(0.3f, 0f, 0f), 2f, TextAlignmentOptions.Center);
        var valueTmp = valueObj.GetComponent<TextMeshPro>();

        // [-] button
        var minusBtn = Object.Instantiate(_toggleTemplate, container.transform);
        minusBtn.name = "Minus";
        minusBtn.transform.localPosition = new Vector3(-0.15f, 0f, 0f);
        minusBtn.transform.localScale = new Vector3(0.2f, 0.6f, 1f);
        minusBtn.Text.text = "-";
        minusBtn.Text.fontSizeMin = minusBtn.Text.fontSizeMax = 3f;
        minusBtn.Text.transform.localScale = new Vector3(1f / 0.2f, 1f / 0.6f, 1f);
        minusBtn.Background.color = Palette.ImpostorRed;
        minusBtn.gameObject.SetActive(true);
        var minusPassive = minusBtn.GetComponent<PassiveButton>();
        if (minusPassive != null)
        {
            minusPassive.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            minusPassive.OnClick.AddListener((Action)(() =>
            {
                currentValue = Math.Max(min, currentValue - step);
                valueTmp.text = currentValue.ToString();
                onChange(currentValue);
            }));
        }

        // [+] button
        var plusBtn = Object.Instantiate(_toggleTemplate, container.transform);
        plusBtn.name = "Plus";
        plusBtn.transform.localPosition = new Vector3(0.75f, 0f, 0f);
        plusBtn.transform.localScale = new Vector3(0.2f, 0.6f, 1f);
        plusBtn.Text.text = "+";
        plusBtn.Text.fontSizeMin = plusBtn.Text.fontSizeMax = 3f;
        plusBtn.Text.transform.localScale = new Vector3(1f / 0.2f, 1f / 0.6f, 1f);
        plusBtn.Background.color = Color.green;
        plusBtn.gameObject.SetActive(true);
        var plusPassive = plusBtn.GetComponent<PassiveButton>();
        if (plusPassive != null)
        {
            plusPassive.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            plusPassive.OnClick.AddListener((Action)(() =>
            {
                currentValue = Math.Min(max, currentValue + step);
                valueTmp.text = currentValue.ToString();
                onChange(currentValue);
            }));
        }

        return container;
    }

    public static GameObject CreateTextDisplay(
        Transform parent, string name, string label,
        string initialValue, Vector3 position)
    {
        var container = new GameObject(name);
        container.transform.SetParent(parent);
        container.transform.localPosition = position;
        container.transform.localScale = Vector3.one;

        // Label
        CreateTextLabel(container.transform, label, new Vector3(-1.5f, 0f, 0f), 1.5f, TextAlignmentOptions.Left);

        // Value (truncated)
        string display = initialValue.Length > 35 ? initialValue.Substring(0, 32) + "..." : initialValue;
        CreateTextLabel(container.transform, display, new Vector3(0.8f, 0f, 0f), 1.2f, TextAlignmentOptions.Left,
            new Color(0.7f, 0.7f, 0.7f, 1f));

        return container;
    }

    public static GameObject CreateHeader(Transform parent, string text, Vector3 position)
    {
        var obj = CreateTextLabel(parent, text, position, 2.2f, TextAlignmentOptions.Center, Color.white);
        var tmp = obj.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.fontStyle = FontStyles.Bold | FontStyles.Underline;
        }
        return obj;
    }

    public static ToggleButtonBehaviour CreateButton(
        Transform parent, string name, string label,
        Action onClick, Vector3 position, Color color)
    {
        var button = Object.Instantiate(_toggleTemplate, parent);
        button.name = name;
        button.transform.localPosition = position;
        button.transform.localScale = new Vector3(0.55f, 0.8f, 1f);
        button.gameObject.SetActive(true);

        button.Background.color = color;
        button.Text.text = label;
        button.Text.fontSizeMin = button.Text.fontSizeMax = 2f;
        button.Text.transform.localScale = new Vector3(1f / 0.55f, 1f / 0.8f, 1f);

        var passive = button.GetComponent<PassiveButton>();
        if (passive != null)
        {
            passive.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            passive.OnMouseOut = new UnityEvent();
            passive.OnMouseOver = new UnityEvent();
            passive.OnClick.AddListener((Action)onClick);
            passive.OnMouseOver.AddListener((Action)(() =>
                button.Background.color = new Color(color.r * 0.7f, color.g * 0.7f, color.b * 0.7f, 1f)));
            passive.OnMouseOut.AddListener((Action)(() =>
                button.Background.color = color));
        }

        return button;
    }

    private static GameObject CreateTextLabel(Transform parent, string text, Vector3 position,
        float fontSize, TextAlignmentOptions alignment, Color? color = null)
    {
        // Try to clone Text from template if available
        if (_toggleTemplate != null && _toggleTemplate.Text != null)
        {
            var label = Object.Instantiate(_toggleTemplate.Text, parent);
            label.name = "Label_" + text.Replace(" ", "");
            label.transform.localPosition = position;
            label.transform.localScale = Vector3.one;
            label.text = text;
            label.fontSizeMin = label.fontSizeMax = fontSize;
            label.alignment = alignment;
            label.color = color ?? Color.white;
            label.enableWordWrapping = false;
            label.GetComponent<RectTransform>().sizeDelta = new Vector2(3f, 0.5f);
            label.gameObject.SetActive(true);
            return label.gameObject;
        }

        // Fallback: create from scratch
        var obj = new GameObject("Label_" + text.Replace(" ", ""));
        obj.transform.SetParent(parent);
        obj.transform.localPosition = position;
        obj.transform.localScale = Vector3.one;
        return obj;
    }
}
