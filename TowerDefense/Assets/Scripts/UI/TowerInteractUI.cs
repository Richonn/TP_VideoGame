using UnityEngine;
using TMPro;

public class TowerInteractUI : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private int playerIndex = 1;

    [Header("World Space Prompt")]
    [SerializeField] private GameObject promptObject;      // "Press F to interact" floating above player
    [SerializeField] private TextMeshPro promptText;       // TextMeshPro (NOT TextMeshProUGUI) — world space

    [Header("Screen Menu")]
    [SerializeField] private GameObject menuPanel;         // upgrade panel on this player's canvas
    [SerializeField] private TextMeshProUGUI rangeText;
    [SerializeField] private TextMeshProUGUI damageText;

    private Tower _currentTower;
    private Transform _playerTransform;
    private Vector3 _promptOffset = new Vector3(0f, 1.2f, 0f); // above player's head

    void Awake()
    {
        HideAll();
    }

    public void Init(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    void LateUpdate()
    {
        // Keep prompt floating above the player
        if (promptObject.activeSelf && _playerTransform != null)
            promptObject.transform.position = _playerTransform.position + _promptOffset;
    }

    public void ShowPrompt(Tower tower)
    {
        _currentTower = tower;
        promptObject.SetActive(true);
        menuPanel.SetActive(false);
    }

    public void OpenMenu(Tower tower)
    {
        _currentTower = tower;
        promptObject.SetActive(false);
        menuPanel.SetActive(true);
        RefreshText();
    }

    public void HideAll()
    {
        _currentTower = null;
        if (promptObject != null) promptObject.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(false);
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
        if (_currentTower != null) {
            rangeText.text = $"Range: {_currentTower.range:F1}";
            damageText.text = $"Damage: {_currentTower.damage:F1}";
        }
    }
}