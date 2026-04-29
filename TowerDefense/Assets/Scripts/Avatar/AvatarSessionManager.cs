using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class AvatarSessionManager : MonoBehaviour
{
    public enum AvatarType { Black, Blue, Purple, Red, Yellow }

    public static AvatarSessionManager Instance { get; private set; }

    private AvatarType    _player1Avatar  = AvatarType.Blue;
    private AvatarType    _player2Avatar  = AvatarType.Purple;
    private AvatarProfile _player1Profile = null;
    private AvatarProfile _player2Profile = null;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _player1Profile = AvatarProfile.LoadForPlayer(1);
        _player2Profile = AvatarProfile.LoadForPlayer(2);
        _player1Avatar  = (AvatarType)Mathf.Clamp(_player1Profile.pawnColorIndex, 0, 4);
        _player2Avatar  = (AvatarType)Mathf.Clamp(_player2Profile.pawnColorIndex, 0, 4);
    }

    void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ApplyAvatarsNextFrame());
    }

    private System.Collections.IEnumerator ApplyAvatarsNextFrame()
    {
        yield return null;
        ApplyToPlayer(1);
        ApplyToPlayer(2);
    }

    public AvatarType GetPlayerAvatar(int playerNumber) =>
        playerNumber == 1 ? _player1Avatar : _player2Avatar;

    public void SetPlayerAvatar(int playerNumber, AvatarType avatarType, AvatarProfile profile = null)
    {
        if (playerNumber == 1) { _player1Avatar = avatarType; if (profile != null) _player1Profile = profile; }
        else                   { _player2Avatar = avatarType; if (profile != null) _player2Profile = profile; }
        ApplyToPlayer(playerNumber);
    }

    private void ApplyToPlayer(int playerNumber)
    {
        PlayerController player = FindObjectsByType<PlayerController>(FindObjectsSortMode.None)
            .FirstOrDefault(p => p.playerNumber == playerNumber);
        if (player == null) return;

        AvatarProfile profile = (playerNumber == 1 ? _player1Profile : _player2Profile)
                                ?? AvatarProfile.LoadForPlayer(playerNumber);

        AvatarCustomizer customizer = player.GetComponentInChildren<AvatarCustomizer>();
        if (customizer != null)
        {
            customizer.Apply(profile);
        }
        else
        {
            RuntimeAnimatorController ctrl = GetAnimatorController(playerNumber == 1 ? _player1Avatar : _player2Avatar);
            if (ctrl != null) player.ApplyAnimatorController(ctrl);
        }

        HUDManager hud = FindFirstObjectByType<HUDManager>();
        hud?.UpdatePlayerAvatarIcon(playerNumber, GetAvatarIcon(playerNumber == 1 ? _player1Avatar : _player2Avatar));
    }

    public Sprite GetAvatarIcon(AvatarType avatarType)
    {
        Sprite s = Resources.Load<Sprite>($"AvatarIcons/{avatarType}");
        if (s != null) return s;
        Texture2D tex = Resources.Load<Texture2D>($"AvatarIcons/{avatarType}");
        if (tex != null) return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return null;
    }

    public RuntimeAnimatorController GetAnimatorController(AvatarType avatarType) =>
        Resources.Load<RuntimeAnimatorController>($"AvatarControllers/{avatarType}");
}
