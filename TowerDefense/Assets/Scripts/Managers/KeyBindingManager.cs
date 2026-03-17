using UnityEngine;
using System.Collections.Generic;

public class KeyBindingManager : MonoBehaviour
{
    public static KeyBindingManager Instance { get; private set; }

    public enum GamepadButtonType
    {
        South, North, East, West,
        LeftShoulder, RightShoulder,
        Start, Select
    }

    [System.Serializable]
    public struct KeyBinding
    {
        public string ActionName;
        public KeyCode KeyboardKey;
        public GamepadButtonType GamepadButton;
    }

    private Dictionary<string, KeyBinding> keyBindings = new Dictionary<string, KeyBinding>();

    public enum ActionType
    {
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        PlaceTower,
        Interact,
        LaunchWave
    }

    private static readonly Dictionary<ActionType, KeyBinding> DefaultBindings = new Dictionary<ActionType, KeyBinding>()
    {
        { ActionType.Move_Up, new KeyBinding { ActionName = "Move_Up", KeyboardKey = KeyCode.Z, GamepadButton = GamepadButtonType.South } },
        { ActionType.Move_Down, new KeyBinding { ActionName = "Move_Down", KeyboardKey = KeyCode.S, GamepadButton = GamepadButtonType.South } },
        { ActionType.Move_Left, new KeyBinding { ActionName = "Move_Left", KeyboardKey = KeyCode.Q, GamepadButton = GamepadButtonType.South } },
        { ActionType.Move_Right, new KeyBinding { ActionName = "Move_Right", KeyboardKey = KeyCode.D, GamepadButton = GamepadButtonType.South } },
        { ActionType.PlaceTower, new KeyBinding { ActionName = "PlaceTower", KeyboardKey = KeyCode.E, GamepadButton = GamepadButtonType.South } },
        { ActionType.Interact, new KeyBinding { ActionName = "Interact", KeyboardKey = KeyCode.F, GamepadButton = GamepadButtonType.North } },
        { ActionType.LaunchWave, new KeyBinding { ActionName = "LaunchWave", KeyboardKey = KeyCode.Tab, GamepadButton = GamepadButtonType.East } }
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAllBindings();
    }

    private void LoadAllBindings()
    {
        keyBindings.Clear();

        foreach (var pair in DefaultBindings)
        {
            KeyBinding binding = pair.Value;

            string kbKey = $"KeyBinding_{pair.Value.ActionName}";
            if (PlayerPrefs.HasKey(kbKey))
            {
                if (System.Enum.TryParse(PlayerPrefs.GetString(kbKey), out KeyCode parsedKey))
                    binding.KeyboardKey = parsedKey;
            }

            string gpKey = $"GamepadBinding_{pair.Value.ActionName}";
            if (PlayerPrefs.HasKey(gpKey))
            {
                if (System.Enum.TryParse(PlayerPrefs.GetString(gpKey), out GamepadButtonType parsedBtn))
                    binding.GamepadButton = parsedBtn;
            }

            keyBindings[pair.Value.ActionName] = binding;
        }
    }

    public KeyBinding GetBinding(ActionType action)
    {
        string actionName = action.ToString();
        if (keyBindings.ContainsKey(actionName))
            return keyBindings[actionName];
        return DefaultBindings[action];
    }

    public void SetBinding(ActionType action, KeyCode newKey)
    {
        string actionName = action.ToString();
        KeyBinding binding = keyBindings.ContainsKey(actionName) ? keyBindings[actionName] : DefaultBindings[action];
        binding.KeyboardKey = newKey;
        keyBindings[actionName] = binding;
        PlayerPrefs.SetString($"KeyBinding_{actionName}", newKey.ToString());
        PlayerPrefs.Save();
    }

    public void SetGamepadBinding(ActionType action, GamepadButtonType button)
    {
        string actionName = action.ToString();
        KeyBinding binding = keyBindings.ContainsKey(actionName) ? keyBindings[actionName] : DefaultBindings[action];
        binding.GamepadButton = button;
        keyBindings[actionName] = binding;
        PlayerPrefs.SetString($"GamepadBinding_{actionName}", button.ToString());
        PlayerPrefs.Save();
    }

    public void ResetAllBindings()
    {
        foreach (var pair in DefaultBindings)
        {
            PlayerPrefs.DeleteKey($"KeyBinding_{pair.Value.ActionName}");
            PlayerPrefs.DeleteKey($"GamepadBinding_{pair.Value.ActionName}");
        }
        PlayerPrefs.Save();
        LoadAllBindings();
    }

    public static string GetKeyDisplayName(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Space: return "SPACE";
            case KeyCode.Return: return "ENTER";
            case KeyCode.Tab: return "TAB";
            case KeyCode.Escape: return "ESCAPE";
            case KeyCode.LeftShift: return "LSHIFT";
            case KeyCode.RightShift: return "RSHIFT";
            case KeyCode.LeftControl: return "LCTRL";
            case KeyCode.RightControl: return "RCTRL";
            default:
                string name = key.ToString().ToUpper();
                if (name.StartsWith("KEYPAD"))
                    name = "KP_" + name.Substring(6);
                return name;
        }
    }

    public static string GetGamepadButtonDisplayName(GamepadButtonType btn)
    {
        switch (btn)
        {
            case GamepadButtonType.South: return "A";
            case GamepadButtonType.North: return "Y";
            case GamepadButtonType.East: return "B";
            case GamepadButtonType.West: return "X";
            case GamepadButtonType.LeftShoulder: return "LB";
            case GamepadButtonType.RightShoulder: return "RB";
            case GamepadButtonType.Start: return "START";
            case GamepadButtonType.Select: return "SELECT";
            default: return btn.ToString().ToUpper();
        }
    }

    public List<KeyBinding> GetAllBindings()
    {
        List<KeyBinding> bindings = new List<KeyBinding>();
        foreach (var action in DefaultBindings.Keys)
            bindings.Add(GetBinding(action));
        return bindings;
    }
}
