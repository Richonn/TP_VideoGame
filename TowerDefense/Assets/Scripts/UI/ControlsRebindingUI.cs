using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using TMPro;
using System.Collections.Generic;

public class ControlsRebindingUI : MonoBehaviour
{
    private Transform _container;

    private class RebindEntry
    {
        public KeyBindingManager.ActionType Action;
        public TextMeshProUGUI KeyDisplay;
        public Button RebindButton;
        public bool IsGamepad;
    }

    private readonly List<RebindEntry> _entries = new List<RebindEntry>();
    private RebindEntry _currentEntry;

    private enum RebindMode { None, Keyboard, Gamepad }
    private RebindMode _rebindMode;

    private float _timeout;
    private const float TIMEOUT = 5f;

    private Button _p1KeyboardBtn, _p1GamepadBtn;
    private Button _p2KeyboardBtn, _p2GamepadBtn;
    private bool _built;

    public void SetRebindingContainer(Transform container)
    {
        _container = container;
        BuildUI();
    }

    public void RefreshUI() => BuildUI();

    void Start()
    {
        if (_container == null || _built) return;
        BuildUI();
    }

    void Update()
    {
        if (_currentEntry == null) return;

        _timeout -= Time.unscaledDeltaTime;
        if (_timeout <= 0f) { CancelRebind(); return; }

        if (_rebindMode == RebindMode.Keyboard)
        {
            if (!Input.anyKeyDown) return;
            foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (!Input.GetKeyDown(k)) continue;
                if (k == KeyCode.Escape || k == KeyCode.Return) { CancelRebind(); return; }
                FinishKeyboardRebind(k);
                return;
            }
        }
        else if (_rebindMode == RebindMode.Gamepad)
        {
            Gamepad gamepad = Gamepad.current;
            if (gamepad == null) return;
            foreach (KeyBindingManager.GamepadButtonType btn in System.Enum.GetValues(typeof(KeyBindingManager.GamepadButtonType)))
            {
                ButtonControl bc = InputManager.GetGamepadButton(gamepad, btn);
                if (bc.wasPressedThisFrame)
                {
                    FinishGamepadRebind(btn);
                    return;
                }
            }
        }
    }

    private void BuildUI()
    {
        if (_container == null || KeyBindingManager.Instance == null) return;
        _built = true;

        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in _container) toDestroy.Add(child.gameObject);
        foreach (GameObject go in toDestroy) { go.transform.SetParent(null); Destroy(go); }
        _entries.Clear();
        _p1KeyboardBtn = _p1GamepadBtn = _p2KeyboardBtn = _p2GamepadBtn = null;

        AddSectionTitle("CONTROLLER ASSIGNMENT");
        AddControllerChoiceRow(1);
        AddControllerChoiceRow(2);

        AddSectionTitle("GAMEPAD SETTINGS");
        float deadZone = InputManager.Instance != null ? InputManager.Instance.DeadZone : 0.15f;
        float sensitivity = InputManager.Instance != null ? InputManager.Instance.Sensitivity : 1f;
        AddAdjusterRow("Dead Zone", 0f, 0.5f, 0.05f, deadZone,
            v => InputManager.Instance?.SetDeadZone(v), "F2");
        AddAdjusterRow("Sensitivity", 0.1f, 3f, 0.1f, sensitivity,
            v => InputManager.Instance?.SetSensitivity(v), "F1");

        AddSectionTitle("KEYBOARD — PLAYER 1");
        (KeyBindingManager.ActionType, string)[] kbActions =
        {
            (KeyBindingManager.ActionType.Move_Up,    "Up"),
            (KeyBindingManager.ActionType.Move_Down,  "Down"),
            (KeyBindingManager.ActionType.Move_Left,  "Left"),
            (KeyBindingManager.ActionType.Move_Right, "Right"),
            (KeyBindingManager.ActionType.PlaceTower, "Place Tower"),
            (KeyBindingManager.ActionType.Interact,   "Interact"),
            (KeyBindingManager.ActionType.LaunchWave, "Launch Wave"),
        };
        foreach (var (action, label) in kbActions)
            AddRebindRow(action, label, false);

        AddSectionTitle("GAMEPAD — PLAYER 2");
        AddFixedRow("Move", "L-STICK");
        (KeyBindingManager.ActionType, string)[] gpActions =
        {
            (KeyBindingManager.ActionType.PlaceTower, "Place Tower"),
            (KeyBindingManager.ActionType.Interact,   "Interact"),
            (KeyBindingManager.ActionType.LaunchWave, "Launch Wave"),
        };
        foreach (var (action, label) in gpActions)
            AddRebindRow(action, label, true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_container as RectTransform);
    }

    private void AddSectionTitle(string text)
    {
        GameObject go = new GameObject("SectionTitle");
        go.transform.SetParent(_container, false);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(0.7f, 0.85f, 1f, 1f);
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 36;
        le.flexibleWidth = 1;

        SetAnchorStretchH(go.GetComponent<RectTransform>());
    }

    private void AddControllerChoiceRow(int player)
    {
        GameObject rowGO = new GameObject($"P{player}_ControllerRow");
        rowGO.transform.SetParent(_container, false);

        HorizontalLayoutGroup hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(0, 0, 2, 2);

        LayoutElement rowLE = rowGO.AddComponent<LayoutElement>();
        rowLE.preferredHeight = 48;
        rowLE.flexibleWidth = 1;
        SetAnchorStretchH(rowGO.GetComponent<RectTransform>());

        TextMeshProUGUI labelTMP = CreateText(rowGO, $"Player {player}", 18, TextAlignmentOptions.Left);
        LayoutElement labelLE = labelTMP.gameObject.AddComponent<LayoutElement>();
        labelLE.flexibleWidth = 1;
        labelLE.minWidth = 80;

        Button kbBtn = CreateChoiceButton(rowGO, "Keyboard", 110);
        Button gpBtn = CreateChoiceButton(rowGO, "Gamepad", 110);

        if (player == 1) { _p1KeyboardBtn = kbBtn; _p1GamepadBtn = gpBtn; }
        else { _p2KeyboardBtn = kbBtn; _p2GamepadBtn = gpBtn; }

        int p = player;
        kbBtn.onClick.AddListener(() =>
        {
            InputManager.Instance?.SetControllerType(p, InputManager.ControllerType.Keyboard);
            UpdateControllerButtonColors();
        });
        gpBtn.onClick.AddListener(() =>
        {
            InputManager.Instance?.SetControllerType(p, InputManager.ControllerType.Gamepad);
            UpdateControllerButtonColors();
        });

        UpdateControllerButtonColors();
    }

    private Button CreateChoiceButton(GameObject parent, string label, float width)
    {
        GameObject go = new GameObject(label + "Btn");
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = new Color(0.2f, 0.38f, 0.65f, 1f);
        Button btn = go.AddComponent<Button>();

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.preferredHeight = 40;

        CreateText(go, label, 15, TextAlignmentOptions.Center);
        return btn;
    }

    private void UpdateControllerButtonColors()
    {
        if (InputManager.Instance == null) return;

        Color active = new Color(0.25f, 0.65f, 0.25f, 1f);
        Color inactive = new Color(0.2f, 0.38f, 0.65f, 1f);

        bool p1Kb = InputManager.Instance.GetControllerType(1) == InputManager.ControllerType.Keyboard;
        if (_p1KeyboardBtn != null) _p1KeyboardBtn.GetComponent<Image>().color = p1Kb ? active : inactive;
        if (_p1GamepadBtn != null) _p1GamepadBtn.GetComponent<Image>().color = p1Kb ? inactive : active;

        bool p2Kb = InputManager.Instance.GetControllerType(2) == InputManager.ControllerType.Keyboard;
        if (_p2KeyboardBtn != null) _p2KeyboardBtn.GetComponent<Image>().color = p2Kb ? active : inactive;
        if (_p2GamepadBtn != null) _p2GamepadBtn.GetComponent<Image>().color = p2Kb ? inactive : active;
    }

    private void AddAdjusterRow(string label, float min, float max, float step, float initialValue, System.Action<float> setter, string format)
    {
        float[] current = { Mathf.Round(initialValue / step) * step };

        GameObject rowGO = new GameObject($"{label}_Row");
        rowGO.transform.SetParent(_container, false);

        HorizontalLayoutGroup hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(0, 0, 4, 4);

        LayoutElement rowLE = rowGO.AddComponent<LayoutElement>();
        rowLE.preferredHeight = 48;
        rowLE.flexibleWidth = 1;
        SetAnchorStretchH(rowGO.GetComponent<RectTransform>());

        TextMeshProUGUI labelTMP = CreateText(rowGO, label, 18, TextAlignmentOptions.Left);
        LayoutElement labelLE = labelTMP.gameObject.AddComponent<LayoutElement>();
        labelLE.flexibleWidth = 1;
        labelLE.minWidth = 100;

        Button minusBtn = CreateSmallButton(rowGO, "−", 36);
        TextMeshProUGUI valueTMP = CreateValueDisplay(rowGO, current[0].ToString(format), 70);
        Button plusBtn = CreateSmallButton(rowGO, "+", 36);

        minusBtn.onClick.AddListener(() =>
        {
            float next = Mathf.Round((current[0] - step) / step) * step;
            if (next < min) return;
            current[0] = next;
            valueTMP.text = current[0].ToString(format);
            setter(current[0]);
        });

        plusBtn.onClick.AddListener(() =>
        {
            float next = Mathf.Round((current[0] + step) / step) * step;
            if (next > max) return;
            current[0] = next;
            valueTMP.text = current[0].ToString(format);
            setter(current[0]);
        });
    }

    private Button CreateSmallButton(GameObject parent, string label, float width)
    {
        GameObject go = new GameObject(label + "Btn");
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.45f, 1f);
        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.45f, 0.45f, 0.65f, 1f);
        cb.pressedColor = new Color(0.15f, 0.15f, 0.25f, 1f);
        btn.colors = cb;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.preferredHeight = 40;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return btn;
    }

    private TextMeshProUGUI CreateValueDisplay(GameObject parent, string text, float width)
    {
        GameObject go = new GameObject("ValueDisplay");
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.2f, 1f);

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.preferredHeight = 40;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 18;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        SetAnchorStretch(textRT);

        return tmp;
    }

    private void AddFixedRow(string label, string displayText)
    {
        GameObject rowGO = new GameObject($"{label}_Fixed");
        rowGO.transform.SetParent(_container, false);

        HorizontalLayoutGroup hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(0, 0, 4, 4);

        LayoutElement rowLE = rowGO.AddComponent<LayoutElement>();
        rowLE.preferredHeight = 48;
        rowLE.flexibleWidth = 1;
        SetAnchorStretchH(rowGO.GetComponent<RectTransform>());

        TextMeshProUGUI labelTMP = CreateText(rowGO, label, 18, TextAlignmentOptions.Left);
        LayoutElement labelLE = labelTMP.gameObject.AddComponent<LayoutElement>();
        labelLE.flexibleWidth = 1;
        labelLE.minWidth = 120;

        GameObject displayGO = new GameObject("Display");
        displayGO.transform.SetParent(rowGO.transform, false);
        displayGO.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.2f, 1f);
        LayoutElement displayLE = displayGO.AddComponent<LayoutElement>();
        displayLE.minWidth = 110;
        displayLE.preferredWidth = 110;

        TextMeshProUGUI displayTMP = CreateText(displayGO, displayText, 16, TextAlignmentOptions.Center);
        displayTMP.color = new Color(0.55f, 0.55f, 0.65f, 1f);
        SetAnchorStretch(displayTMP.GetComponent<RectTransform>());

        GameObject btnGO = new GameObject("RebindBtn");
        btnGO.transform.SetParent(rowGO.transform, false);
        btnGO.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);
        Button btn = btnGO.AddComponent<Button>();
        btn.interactable = false;
        LayoutElement btnLE = btnGO.AddComponent<LayoutElement>();
        btnLE.minWidth = 90;
        btnLE.preferredWidth = 90;
        TextMeshProUGUI btnTMP = CreateText(btnGO, "—", 16, TextAlignmentOptions.Center);
        btnTMP.color = new Color(0.35f, 0.35f, 0.45f, 1f);
    }

    private void AddRebindRow(KeyBindingManager.ActionType action, string label, bool isGamepad)
    {
        GameObject rowGO = new GameObject($"{action}_Row");
        rowGO.transform.SetParent(_container, false);

        HorizontalLayoutGroup hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(0, 0, 4, 4);

        LayoutElement rowLE = rowGO.AddComponent<LayoutElement>();
        rowLE.preferredHeight = 48;
        rowLE.flexibleWidth = 1;
        SetAnchorStretchH(rowGO.GetComponent<RectTransform>());

        TextMeshProUGUI labelTMP = CreateText(rowGO, label, 18, TextAlignmentOptions.Left);
        LayoutElement labelLE = labelTMP.gameObject.AddComponent<LayoutElement>();
        labelLE.flexibleWidth = 1;
        labelLE.minWidth = 120;

        GameObject displayGO = new GameObject("Display");
        displayGO.transform.SetParent(rowGO.transform, false);
        displayGO.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.25f, 1f);
        LayoutElement displayLE = displayGO.AddComponent<LayoutElement>();
        displayLE.minWidth = 110;
        displayLE.preferredWidth = 110;

        KeyBindingManager.KeyBinding binding = KeyBindingManager.Instance.GetBinding(action);
        string displayValue = isGamepad
            ? KeyBindingManager.GetGamepadButtonDisplayName(binding.GamepadButton)
            : KeyBindingManager.GetKeyDisplayName(binding.KeyboardKey);
        TextMeshProUGUI keyTMP = CreateText(displayGO, displayValue, 16, TextAlignmentOptions.Center);
        SetAnchorStretch(keyTMP.GetComponent<RectTransform>());

        GameObject btnGO = new GameObject("RebindBtn");
        btnGO.transform.SetParent(rowGO.transform, false);
        btnGO.AddComponent<Image>().color = new Color(0.25f, 0.5f, 0.75f, 1f);
        Button btn = btnGO.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.35f, 0.65f, 0.9f, 1f);
        cb.pressedColor = new Color(0.15f, 0.35f, 0.6f, 1f);
        btn.colors = cb;
        LayoutElement btnLE = btnGO.AddComponent<LayoutElement>();
        btnLE.minWidth = 90;
        btnLE.preferredWidth = 90;
        CreateText(btnGO, "REBIND", 16, TextAlignmentOptions.Center);

        RebindEntry entry = new RebindEntry
        {
            Action = action,
            KeyDisplay = keyTMP,
            RebindButton = btn,
            IsGamepad = isGamepad,
        };
        _entries.Add(entry);
        btn.onClick.AddListener(() => StartRebind(entry));
    }

    private TextMeshProUGUI CreateText(GameObject parent, string text, float size, TextAlignmentOptions align)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent.transform, false);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = Color.white;
        tmp.alignment = align;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;

        SetAnchorStretch(go.GetComponent<RectTransform>());
        return tmp;
    }

    private static void SetAnchorStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetAnchorStretchH(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    private void StartRebind(RebindEntry entry)
    {
        foreach (var e in _entries) e.RebindButton.interactable = false;
        _currentEntry = entry;
        _rebindMode = entry.IsGamepad ? RebindMode.Gamepad : RebindMode.Keyboard;
        _timeout = TIMEOUT;
        entry.KeyDisplay.text = "...";
    }

    private void FinishKeyboardRebind(KeyCode key)
    {
        if (_currentEntry == null) return;
        KeyBindingManager.Instance.SetBinding(_currentEntry.Action, key);
        _currentEntry.KeyDisplay.text = KeyBindingManager.GetKeyDisplayName(key);
        EndRebind();
    }

    private void FinishGamepadRebind(KeyBindingManager.GamepadButtonType btn)
    {
        if (_currentEntry == null) return;
        KeyBindingManager.Instance.SetGamepadBinding(_currentEntry.Action, btn);
        _currentEntry.KeyDisplay.text = KeyBindingManager.GetGamepadButtonDisplayName(btn);
        EndRebind();
    }

    private void CancelRebind()
    {
        if (_currentEntry == null) return;
        KeyBindingManager.KeyBinding b = KeyBindingManager.Instance.GetBinding(_currentEntry.Action);
        _currentEntry.KeyDisplay.text = _currentEntry.IsGamepad
            ? KeyBindingManager.GetGamepadButtonDisplayName(b.GamepadButton)
            : KeyBindingManager.GetKeyDisplayName(b.KeyboardKey);
        EndRebind();
    }

    private void EndRebind()
    {
        _currentEntry = null;
        _rebindMode = RebindMode.None;
        foreach (var e in _entries) e.RebindButton.interactable = true;
    }
}
