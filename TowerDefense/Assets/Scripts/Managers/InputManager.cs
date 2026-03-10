using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gestionnaire d'entrées personnalisé sans dépendre de l'InputActionAsset.
/// Utilise la classe KeyBindingManager pour gérer les touches directement.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Tooltip("Rayon du seuil de dead zone (0–1) pour le joystick")]
    [SerializeField, Range(0f, 0.5f)] private float deadZone = 0.15f;

    public struct PlayerInputData
    {
        public Vector2 MoveDirection;
        public bool PlaceTowerPressed;
        public bool InteractPressed;
        public bool PlaceTowerHeld;
        public bool InteractHeld;
        public bool LancerVagueHeld;
    }

    private PlayerInputData[] _inputData = new PlayerInputData[2];
    private bool _playerInputEnabled1 = true;
    private bool _playerInputEnabled2 = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // S'assurer que KeyBindingManager existe
        EnsureKeyBindingManager();
    }

    private void EnsureKeyBindingManager()
    {
        if (KeyBindingManager.Instance == null)
        {
            Debug.Log("[InputManager] KeyBindingManager n'existe pas, création automatique...");
            GameObject kbmGO = new GameObject("KeyBindingManager");
            kbmGO.AddComponent<KeyBindingManager>();
            DontDestroyOnLoad(kbmGO);
        }
    }

    void Update()
    {
        // Joueur 1 : clavier (KeyBindingManager)
        // Joueur 2 : manette (joystick Unity)
        _inputData[0] = _playerInputEnabled1 ? TraiterInputsCustom()   : new PlayerInputData();
        _inputData[1] = _playerInputEnabled2 ? TraiterInputsGamepad()  : new PlayerInputData();

        // Détection de la touche Escape pour afficher le menu de pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[InputManager] Escape pressé détecté");
            
            // Chercher le PauseMenuController s'il n'a pas d'Instance
            PauseMenuController pauseController = PauseMenuController.Instance;
            if (pauseController == null)
            {
                Debug.Log("[InputManager] PauseMenuController.Instance est NULL, recherche dans la scène...");
                pauseController = FindFirstObjectByType<PauseMenuController>();
            }

            if (pauseController == null)
            {
                Debug.LogError("[InputManager] PauseMenuController introuvable dans la scène! Création automatique...");
                GameObject pauseManagerGO = new GameObject("PauseMenuManager");
                pauseController = pauseManagerGO.AddComponent<PauseMenuController>();
            }

            if (pauseController != null)
            {
                Debug.Log("[InputManager] Appel TogglePause()");
                pauseController.TogglePause();
            }
        }
    }
    /// <summary>
    /// Traite les entrées personnalisées à partir de KeyBindingManager.
    /// Utilise Input.GetKeyDown() et Input.GetKey() pour détecter les touches.
    /// </summary>
    private PlayerInputData TraiterInputsCustom()
    {
        // KeyBindingManager.Instance est garanti d'exister grâce à EnsureKeyBindingManager()
        KeyBindingManager.KeyBinding moveUpBinding = KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.Move_Up);
        KeyBindingManager.KeyBinding moveDownBinding = KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.Move_Down);
        KeyBindingManager.KeyBinding moveLeftBinding = KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.Move_Left);
        KeyBindingManager.KeyBinding moveRightBinding = KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.Move_Right);

        KeyBindingManager.KeyBinding placeTowerBinding = KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.PlaceTower);
        KeyBindingManager.KeyBinding interactBinding = KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.Interact);
        KeyBindingManager.KeyBinding lancerVagueBinding = KeyBindingManager.Instance.GetBinding(KeyBindingManager.ActionType.LancerVague);

        // Calculer le direction de mouvement
        Vector2 direction = Vector2.zero;
        
        if (Input.GetKey(moveUpBinding.KeyboardKey))
            direction.y += 1;
        if (Input.GetKey(moveDownBinding.KeyboardKey))
            direction.y -= 1;
        if (Input.GetKey(moveLeftBinding.KeyboardKey))
            direction.x -= 1;
        if (Input.GetKey(moveRightBinding.KeyboardKey))
            direction.x += 1;

        // Normaliser pour les mouvements diagonaux (+0.7 au lieu de +1)
        if (direction.magnitude > 0)
            direction = direction.normalized;

        // Retourner les données d'entrée
        return new PlayerInputData
        {
            MoveDirection = direction,
            PlaceTowerPressed = Input.GetKeyDown(placeTowerBinding.KeyboardKey),
            InteractPressed = Input.GetKeyDown(interactBinding.KeyboardKey),
            PlaceTowerHeld = Input.GetKey(placeTowerBinding.KeyboardKey),
            InteractHeld = Input.GetKey(interactBinding.KeyboardKey),
            LancerVagueHeld = Input.GetKey(lancerVagueBinding.KeyboardKey),
        };
    }

    /// <summary>
    /// Entrées Joueur 2 via manette (New Input System — Gamepad.current).
    /// Complètement isolé du clavier.
    /// </summary>
    private PlayerInputData TraiterInputsGamepad()
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return new PlayerInputData();

        Vector2 stick = gamepad.leftStick.ReadValue();
        if (stick.magnitude > deadZone)
            stick = stick.normalized * Mathf.InverseLerp(deadZone, 1f, stick.magnitude);
        else
            stick = Vector2.zero;

        return new PlayerInputData
        {
            MoveDirection     = stick,
            PlaceTowerPressed = gamepad.buttonSouth.wasPressedThisFrame, // B Switch
            InteractPressed   = gamepad.buttonNorth.wasPressedThisFrame, // X Switch
            PlaceTowerHeld    = gamepad.buttonSouth.isPressed,
            InteractHeld      = gamepad.buttonNorth.isPressed,
            LancerVagueHeld   = gamepad.buttonEast.isPressed,            // A Switch
        };
    }

    public PlayerInputData GetInput(int playerIndex)
    {
        int idx = Mathf.Clamp(playerIndex - 1, 0, 1);
        return _inputData[idx];
    }

    public void SetPlayerInputEnabled(int playerIndex, bool enabled)
    {
        if (playerIndex == 1)
            _playerInputEnabled1 = enabled;
        else if (playerIndex == 2)
            _playerInputEnabled2 = enabled;

        Debug.Log($"[InputManager] Joueur {playerIndex} input: {(enabled ? "ENABLED" : "DISABLED")}");
    }
}
