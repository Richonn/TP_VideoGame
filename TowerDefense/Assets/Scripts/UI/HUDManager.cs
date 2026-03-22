using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("Player 1 HUD")]
    [SerializeField] private TMP_Text p1ResourceText;
    [SerializeField] private Image p1ReadyBar;

    [Header("Player 2 HUD")]
    [SerializeField] private TMP_Text p2ResourceText;
    [SerializeField] private Image p2ReadyBar;

    [Header("Shared HUD")]
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text helpText;
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private Image phaseImage;
    [SerializeField] private Image baseHPBar;
    public Sprite prepSprite;
    public Sprite defenseSprite;

    [Header("References")]
    [SerializeField] private ReadySystem readySystem;

    void OnEnable()
    {
        GameManager.OnPhaseChanged += OnPhaseChanged;
        GameManager.OnWaveChanged += OnWaveChanged;
        BaseController.OnHPChanged += OnHPChanged;
        ResourceManager.OnResourcesChanged += OnResourcesChanged;
    }

    void Start()
    {
        if (ResourceManager.Instance != null)
        {
            OnResourcesChanged(1, ResourceManager.Instance.GetResources(1));
            OnResourcesChanged(2, ResourceManager.Instance.GetResources(2));
        }

        BaseController base_ = FindFirstObjectByType<BaseController>();
        if (base_ != null)
            OnHPChanged(base_.CurrentHP, base_.MaxHP);
    }

    void OnDisable()
    {
        GameManager.OnPhaseChanged -= OnPhaseChanged;
        GameManager.OnWaveChanged -= OnWaveChanged;
        BaseController.OnHPChanged -= OnHPChanged;
        ResourceManager.OnResourcesChanged -= OnResourcesChanged;
    }

    void Update()
    {
        if (readySystem == null) return;
        float prog = readySystem.Progression;
        if (p1ReadyBar != null) p1ReadyBar.fillAmount = prog;
        if (p2ReadyBar != null) p2ReadyBar.fillAmount = prog;
    }

    private void OnPhaseChanged(GameManager.GameState state)
    {
        if (phaseText != null)
            phaseText.text = state switch
            {
                GameManager.GameState.Preparation => "PREPARE THE BASE",
                GameManager.GameState.Defense => "DEFEND",
                _ => ""
            };

        if (phaseImage != null)
            phaseImage.sprite = state switch
            {
                GameManager.GameState.Preparation => prepSprite,
                GameManager.GameState.Defense => defenseSprite,
                _ => prepSprite
            };

        if (helpText != null)
            helpText.text = state switch
            {
                GameManager.GameState.Preparation => "Hold Tab or B to start wave",
                GameManager.GameState.Defense => "Wait the end of the wave",
                _ => ""
            };
    }

    private void OnWaveChanged(int wave)
    {
        if (waveText != null)
            waveText.text = $"Vague {wave}";
    }

    private void OnHPChanged(int currentHP, int maxHP)
    {
        if (baseHPBar != null)
            baseHPBar.fillAmount = (float)currentHP / maxHP;
    }

    private void OnResourcesChanged(int playerIndex, int amount)
    {
        if (playerIndex == 1 && p1ResourceText != null)
            p1ResourceText.text = $"{amount}";
        else if (playerIndex == 2 && p2ResourceText != null)
            p2ResourceText.text = $"{amount}";
    }
}
