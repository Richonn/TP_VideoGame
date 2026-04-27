using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI panel for customizing avatars in the pause menu settings.
/// Allows each player to select their avatar from available options.
/// </summary>
public class AvatarCustomizationUI : MonoBehaviour
{
    [System.Serializable]
    public class AvatarOption
    {
        public AvatarSessionManager.AvatarType avatarType;
        public Sprite icon;
        public string displayName;
    }

    [SerializeField] private Transform player1Container;
    [SerializeField] private Transform player2Container;
    [SerializeField] private AvatarOption[] availableAvatars;
    [SerializeField] private GameObject avatarButtonPrefab;
    [SerializeField] private Color selectedColor = new Color(0.8f, 0.8f, 0f, 1f);
    [SerializeField] private Color deselectedColor = Color.white;

    private List<Button> _player1Buttons = new List<Button>();
    private List<Button> _player2Buttons = new List<Button>();
    private int _currentPlayer1Selection = 0;
    private int _currentPlayer2Selection = 1;

    void Start()
    {
        if (availableAvatars == null || availableAvatars.Length == 0)
        {
            Debug.LogWarning("[AvatarCustomizationUI] No avatars configured!");
            return;
        }

        BuildUI();
    }

    private void BuildUI()
    {
        // Clear existing buttons
        _player1Buttons.Clear();
        _player2Buttons.Clear();

        if (player1Container != null)
        {
            foreach (Transform child in player1Container)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < availableAvatars.Length; i++)
            {
                int idx = i;
                Button btn = CreateAvatarButton(player1Container, idx, 1);
                _player1Buttons.Add(btn);
            }
        }

        if (player2Container != null)
        {
            foreach (Transform child in player2Container)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < availableAvatars.Length; i++)
            {
                int idx = i;
                Button btn = CreateAvatarButton(player2Container, idx, 2);
                _player2Buttons.Add(btn);
            }
        }

        RefreshUI();
    }

    private Button CreateAvatarButton(Transform parent, int index, int playerNumber)
    {
        GameObject buttonGO = Instantiate(avatarButtonPrefab, parent);
        buttonGO.name = $"AvatarButton_{availableAvatars[index].displayName}";

        Button btn = buttonGO.GetComponent<Button>();
        Image btnImage = buttonGO.GetComponent<Image>();

        if (availableAvatars[index].icon != null && btnImage != null)
        {
            btnImage.sprite = availableAvatars[index].icon;
        }

        // Add tooltip or label if there's a text child
        TextMeshProUGUI text = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = availableAvatars[index].displayName;
        }

        btn.onClick.AddListener(() =>
        {
            if (playerNumber == 1)
            {
                _currentPlayer1Selection = index;
                AvatarSessionManager.Instance?.SetPlayerAvatar(1, availableAvatars[index].avatarType);
            }
            else
            {
                _currentPlayer2Selection = index;
                AvatarSessionManager.Instance?.SetPlayerAvatar(2, availableAvatars[index].avatarType);
            }

            RefreshUI();
            AudioManager.Instance?.PlaySFX(SFXType.UIClick);
        });

        return btn;
    }

    private void RefreshUI()
    {
        // Update Player 1 button colors
        for (int i = 0; i < _player1Buttons.Count; i++)
        {
            Image img = _player1Buttons[i].GetComponent<Image>();
            if (img != null)
            {
                img.color = (i == _currentPlayer1Selection) ? selectedColor : deselectedColor;
            }
        }

        // Update Player 2 button colors
        for (int i = 0; i < _player2Buttons.Count; i++)
        {
            Image img = _player2Buttons[i].GetComponent<Image>();
            if (img != null)
            {
                img.color = (i == _currentPlayer2Selection) ? selectedColor : deselectedColor;
            }
        }
    }
}
