using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// Manages avatar selection for the current session.
/// Does not persist data - resets on game quit.
/// </summary>
public class AvatarSessionManager : MonoBehaviour
{
    public enum AvatarType { Red, Blue, Purple, Yellow, Black }

    public static AvatarSessionManager Instance { get; private set; }

    private AvatarType _player1Avatar = AvatarType.Blue;
    private AvatarType _player2Avatar = AvatarType.Purple;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Survive scene transitions
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Re-applies both players' avatars after a new scene finishes loading,
    /// because PlayerController objects are freshly instantiated each time.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Small delay via coroutine so all Start() methods have run first
        StartCoroutine(ApplyAvatarsNextFrame());
    }

    private System.Collections.IEnumerator ApplyAvatarsNextFrame()
    {
        yield return null; // wait one frame for PlayerControllers to Start()
        UpdatePlayerAvatar(1);
        UpdatePlayerAvatar(2);
    }

    public AvatarType GetPlayerAvatar(int playerNumber)
    {
        if (playerNumber == 1) return _player1Avatar;
        if (playerNumber == 2) return _player2Avatar;
        return AvatarType.Blue;
    }

    public void SetPlayerAvatar(int playerNumber, AvatarType avatarType)
    {
        if (playerNumber == 1)
        {
            _player1Avatar = avatarType;
            UpdatePlayerAvatar(1);
        }
        else if (playerNumber == 2)
        {
            _player2Avatar = avatarType;
            UpdatePlayerAvatar(2);
        }
    }

    public Sprite GetAvatarIcon(AvatarType avatarType)
    {
        // Try direct sprite load first
        Sprite s = Resources.Load<Sprite>($"AvatarIcons/{avatarType}");
        if (s != null) return s;

        // Fallback: load as Texture2D and convert
        Texture2D tex = Resources.Load<Texture2D>($"AvatarIcons/{avatarType}");
        if (tex != null)
        {
            Debug.LogWarning($"[Avatar] {avatarType} loaded as Texture2D — set Texture Type to 'Sprite' in import settings!");
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        Debug.LogError($"[Avatar] Could not load AvatarIcons/{avatarType} at all!");
        return null;
    }

    public RuntimeAnimatorController GetAnimatorController(AvatarType avatarType)
    {
        return Resources.Load<RuntimeAnimatorController>($"AvatarControllers/{avatarType}");
    }

    private void UpdatePlayerAvatar(int playerNumber)
    {
        PlayerController player = FindObjectsByType<PlayerController>(FindObjectsSortMode.None)
            .FirstOrDefault(p => p.playerNumber == playerNumber);

        if (player == null) return;

        AvatarType avatarType = playerNumber == 1 ? _player1Avatar : _player2Avatar;

        // Update animator controller on the player via the exposed method,
        // which also refreshes PlayerController's internal cached reference.
        RuntimeAnimatorController controller = GetAnimatorController(avatarType);
        if (controller != null)
        {
            player.ApplyAnimatorController(controller);
            Debug.Log($"[Avatar] P{playerNumber} animator set to: AvatarControllers/{avatarType}");
        }
        else
        {
            Debug.LogWarning($"[Avatar] No controller found at: AvatarControllers/{avatarType}");
        }

        // Update avatar icon in HUD
        HUDManager hudManager = FindFirstObjectByType<HUDManager>();
        if (hudManager != null)
        {
            Sprite avatarIcon = GetAvatarIcon(avatarType);
            hudManager.UpdatePlayerAvatarIcon(playerNumber, avatarIcon);
        }
    }
}
