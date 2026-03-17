using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public enum ControllerType { Keyboard, Gamepad }

    [SerializeField, Range(0f, 0.5f)] private float deadZone = 0.15f;
    [SerializeField, Range(0.1f, 3f)] private float sensitivity = 1f;

    public struct PlayerInputData
    {
        public Vector2 MoveDirection;
        public bool PlaceTowerPressed;
        public bool InteractPressed;
        public bool PlaceTowerHeld;
        public bool InteractHeld;
        public bool LaunchWaveHeld;
    }

    private PlayerInputData[] _inputData = new PlayerInputData[2];
    private bool _playerInputEnabled1 = true;
    private bool _playerInputEnabled2 = true;

    private ControllerType _p1Controller = ControllerType.Keyboard;
    private ControllerType _p2Controller = ControllerType.Gamepad;

    public float DeadZone => deadZone;
    public float Sensitivity => sensitivity;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureKeyBindingManager();
        LoadSettings();
    }

    private void LoadSettings()
    {
        deadZone = PlayerPrefs.GetFloat("Gamepad_DeadZone", deadZone);
        sensitivity = PlayerPrefs.GetFloat("Gamepad_Sensitivity", sensitivity);
        _p1Controller = (ControllerType)PlayerPrefs.GetInt("P1_ControllerType", (int)ControllerType.Keyboard);
        _p2Controller = (ControllerType)PlayerPrefs.GetInt("P2_ControllerType", (int)ControllerType.Gamepad);
    }

    private void EnsureKeyBindingManager()
    {
        if (KeyBindingManager.Instance == null)
        {
            GameObject go = new GameObject("KeyBindingManager");
            go.AddComponent<KeyBindingManager>();
            DontDestroyOnLoad(go);
        }
    }

    void Update()
    {
        _inputData[0] = _playerInputEnabled1 ? ProcessInput(_p1Controller) : new PlayerInputData();
        _inputData[1] = _playerInputEnabled2 ? ProcessInput(_p2Controller) : new PlayerInputData();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenuController pauseController = PauseMenuController.Instance
                ?? FindFirstObjectByType<PauseMenuController>();

            if (pauseController == null)
            {
                GameObject go = new GameObject("PauseMenuManager");
                pauseController = go.AddComponent<PauseMenuController>();
            }

            pauseController?.TogglePause();
        }
    }

    private PlayerInputData ProcessInput(ControllerType type)
    {
        return type == ControllerType.Keyboard ? ProcessKeyboardInput() : ProcessGamepadInput();
    }

    private PlayerInputData ProcessKeyboardInput()
    {
        KeyBindingManager.KeyBinding up = KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.Move_Up);
        KeyBindingManager.KeyBinding down = KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.Move_Down);
        KeyBindingManager.KeyBinding left = KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.Move_Left);
        KeyBindingManager.KeyBinding right = KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.Move_Right);
        KeyBindingManager.KeyBinding placeTower = KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.PlaceTower);
        KeyBindingManager.KeyBinding interact = KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.Interact);
        KeyBindingManager.KeyBinding launchWave = KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.LaunchWave);

        Vector2 direction = Vector2.zero;
        if (Input.GetKey(up.KeyboardKey)) direction.y += 1;
        if (Input.GetKey(down.KeyboardKey)) direction.y -= 1;
        if (Input.GetKey(left.KeyboardKey)) direction.x -= 1;
        if (Input.GetKey(right.KeyboardKey)) direction.x += 1;
        if (direction.magnitude > 0) direction = direction.normalized;

        return new PlayerInputData
        {
            MoveDirection = direction,
            PlaceTowerPressed = Input.GetKeyDown(placeTower.KeyboardKey),
            InteractPressed = Input.GetKeyDown(interact.KeyboardKey),
            PlaceTowerHeld = Input.GetKey(placeTower.KeyboardKey),
            InteractHeld = Input.GetKey(interact.KeyboardKey),
            LaunchWaveHeld = Input.GetKey(launchWave.KeyboardKey),
        };
    }

    private PlayerInputData ProcessGamepadInput()
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return new PlayerInputData();

        Vector2 stick = gamepad.leftStick.ReadValue();
        if (stick.magnitude > deadZone)
            stick = stick.normalized * Mathf.Clamp01(Mathf.InverseLerp(deadZone, 1f, stick.magnitude) * sensitivity);
        else
            stick = Vector2.zero;

        ButtonControl placeTowerBtn = GetGamepadButton(gamepad, KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.PlaceTower).GamepadButton);
        ButtonControl interactBtn = GetGamepadButton(gamepad, KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.Interact).GamepadButton);
        ButtonControl launchWaveBtn = GetGamepadButton(gamepad, KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.LaunchWave).GamepadButton);

        return new PlayerInputData
        {
            MoveDirection = stick,
            PlaceTowerPressed = placeTowerBtn.wasPressedThisFrame,
            InteractPressed = interactBtn.wasPressedThisFrame,
            PlaceTowerHeld = placeTowerBtn.isPressed,
            InteractHeld = interactBtn.isPressed,
            LaunchWaveHeld = launchWaveBtn.isPressed,
        };
    }

    public static ButtonControl GetGamepadButton(Gamepad gp, KeyBindingManager.GamepadButtonType type)
    {
        switch (type)
        {
            case KeyBindingManager.GamepadButtonType.South: return gp.buttonSouth;
            case KeyBindingManager.GamepadButtonType.North: return gp.buttonNorth;
            case KeyBindingManager.GamepadButtonType.East: return gp.buttonEast;
            case KeyBindingManager.GamepadButtonType.West: return gp.buttonWest;
            case KeyBindingManager.GamepadButtonType.LeftShoulder: return gp.leftShoulder;
            case KeyBindingManager.GamepadButtonType.RightShoulder: return gp.rightShoulder;
            case KeyBindingManager.GamepadButtonType.Start: return gp.startButton;
            case KeyBindingManager.GamepadButtonType.Select: return gp.selectButton;
            default: return gp.buttonSouth;
        }
    }

    public ControllerType GetControllerType(int playerIndex) => playerIndex == 1 ? _p1Controller : _p2Controller;

    public void SetControllerType(int playerIndex, ControllerType type)
    {
        if (playerIndex == 1) _p1Controller = type;
        else if (playerIndex == 2) _p2Controller = type;
        PlayerPrefs.SetInt($"P{playerIndex}_ControllerType", (int)type);
        PlayerPrefs.Save();
    }

    public void SetDeadZone(float value)
    {
        deadZone = value;
        PlayerPrefs.SetFloat("Gamepad_DeadZone", value);
        PlayerPrefs.Save();
    }

    public void SetSensitivity(float value)
    {
        sensitivity = value;
        PlayerPrefs.SetFloat("Gamepad_Sensitivity", value);
        PlayerPrefs.Save();
    }

    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey("Gamepad_DeadZone");
        PlayerPrefs.DeleteKey("Gamepad_Sensitivity");
        PlayerPrefs.DeleteKey("P1_ControllerType");
        PlayerPrefs.DeleteKey("P2_ControllerType");
        PlayerPrefs.Save();
        deadZone = 0.15f;
        sensitivity = 1f;
        _p1Controller = ControllerType.Keyboard;
        _p2Controller = ControllerType.Gamepad;
    }

    public PlayerInputData GetInput(int playerIndex)
    {
        int idx = Mathf.Clamp(playerIndex - 1, 0, 1);
        return _inputData[idx];
    }

    public void SetPlayerInputEnabled(int playerIndex, bool enabled)
    {
        if (playerIndex == 1) _playerInputEnabled1 = enabled;
        else if (playerIndex == 2) _playerInputEnabled2 = enabled;
    }
}
