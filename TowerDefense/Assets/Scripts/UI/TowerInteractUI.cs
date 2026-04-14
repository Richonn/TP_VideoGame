using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerInteractUI : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private int playerIndex = 1;

    [Header("World Space Prompt")]
    [SerializeField] private GameObject promptObject;
    [SerializeField] private TextMeshPro promptText;

    [Header("Screen Menu")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TextMeshProUGUI rangeText;
    [SerializeField] private TextMeshProUGUI damageText;

    [Header("Menu Buttons (in navigation order)")]
    [SerializeField] private Button[] menuButtons; // 0 = UpgradeRange, 1 = Close

    private Tower _currentTower;
    private Transform _playerTransform;
    private Vector3 _promptOffset = new Vector3(0f, 1.2f, 0f);

    private int _selectedIndex = 0;
    private float _navCooldown = 0f;
    private const float NAV_DELAY = 0.2f;

    private Vector3 _promptBaseScale = Vector3.one;
    private Vector3 _menuBaseScale = Vector3.one;

    public int PlayerIndex { get => playerIndex; set => playerIndex = value; }

    void Awake()
    {
        if (promptObject != null) _promptBaseScale = promptObject.transform.localScale;
        if (menuPanel != null) _menuBaseScale = menuPanel.transform.localScale;
        HideAll();
    }

    public void Init(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    void LateUpdate()
    {
        if (promptObject.activeSelf && _playerTransform != null)
            promptObject.transform.position = _playerTransform.position + _promptOffset;

        if (menuPanel.activeSelf)
            HandleMenuNavigation();
    }

    private void HandleMenuNavigation()
    {
        if (InputManager.Instance == null || menuButtons == null || menuButtons.Length == 0) return;

        var input = InputManager.Instance.GetInput(playerIndex);

        _navCooldown -= Time.deltaTime;

        if (_navCooldown <= 0f)
        {
            if (input.UINavigate.x < -0.5f)
            {
                _selectedIndex = 1; // Close button
                _navCooldown = NAV_DELAY;
                UpdateButtonVisuals();
            }
            else if (input.UINavigate.x > 0.5f)
            {
                _selectedIndex = 0; // Upgrade button
                _navCooldown = NAV_DELAY;
                UpdateButtonVisuals();
            }
        }

        if (input.UIConfirmPressed)
            menuButtons[_selectedIndex].onClick.Invoke();
    }

    private void UpdateButtonVisuals()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            var colors = menuButtons[i].colors;
            colors.normalColor = (i == _selectedIndex)
                ? new Color(1f, 0.85f, 0f)   // yellow
                : Color.white;
            menuButtons[i].colors = colors;
        }
    }

    private void ResetButtonVisuals()
    {
        if (menuButtons == null) return;
        foreach (var btn in menuButtons)
        {
            var colors = btn.colors;
            colors.normalColor = Color.white;
            btn.colors = colors;
        }
    }

    public void ShowPrompt(Tower tower)
    {
        bool wasActive = promptObject != null && promptObject.activeSelf;
        _currentTower = tower;
        if (promptObject != null)
        {
            promptObject.SetActive(true);
            if (!wasActive)
            {
                promptObject.transform.localScale = Vector3.zero;
                UITween.ScaleTo(promptObject.transform, _promptBaseScale, 0.25f, Easing.Ease.EaseOutBack);
            }
            else
            {
                promptObject.transform.localScale = _promptBaseScale;
            }
        }
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    public void OpenMenu(Tower tower)
    {
        _currentTower = tower;
        if (promptObject != null) promptObject.SetActive(false);
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            menuPanel.transform.localScale = _menuBaseScale * 0.7f;
            UITween.ScaleTo(menuPanel.transform, _menuBaseScale, 0.3f, Easing.Ease.EaseOutBack);
        }
        _selectedIndex = 0;
        UpdateButtonVisuals();
        RefreshText();
        AudioManager.Instance?.PlaySFX(SFXType.UIOpen);
    }

    public void HideAll()
    {
        _currentTower = null;
        if (promptObject != null) promptObject.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(false);
        ResetButtonVisuals();
    }

    public void OnUpgradeRangePressed()
    {
        if (_currentTower == null) return;
        if (_currentTower.TryUpgradeRange(playerIndex))
            RefreshText();
        else
            Debug.Log("[TowerUI] Not enough resources.");
    }

    public void OnClosePressed() => HideAll();

    private void RefreshText()
    {
        if (_currentTower != null)
        {
            rangeText.text = $"Range: {_currentTower.range:F1}";
            damageText.text = $"Damage: {_currentTower.damage:F1}";
        }
    }
}
