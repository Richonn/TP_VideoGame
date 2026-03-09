using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Met à jour tous les éléments du HUD en réponse aux événements du jeu.
/// Aucune logique de jeu ici — uniquement de l'affichage.
///
/// Assigner les références dans l'Inspector.
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("HUD Joueur 1 (gauche)")]
    [SerializeField] private TMP_Text texteRessourcesP1;
    [SerializeField] private Image    barreReadyP1;       // Image type=Filled

    [Header("HUD Joueur 2 (droite)")]
    [SerializeField] private TMP_Text texteRessourcesP2;
    [SerializeField] private Image    barreReadyP2;       // Image type=Filled

    [Header("HUD Partagé (centre)")]
    [SerializeField] private TMP_Text texteVague;
    [SerializeField] private TMP_Text texteTimer;
    [SerializeField] private TMP_Text textePhase;
    [SerializeField] private Slider   sliderBaseHP;

    [Header("Références")]
    [SerializeField] private ReadySystem readySystem;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void OnEnable()
    {
        GameManager.OnPhaseChanged      += OnPhaseChanged;
        GameManager.OnWaveChanged       += OnWaveChanged;
        GameManager.OnPrepTimerUpdated  += OnTimerUpdated;
        BaseController.OnPVChanges      += OnPVChanges;
        ResourceManager.OnRessourcesChangees += OnRessourcesChangees;
    }

    void Start()
    {
        // Initialiser le HUD avec les valeurs actuelles des managers
        if (ResourceManager.Instance != null)
        {
            OnRessourcesChangees(1, ResourceManager.Instance.GetRessources(1));
            OnRessourcesChangees(2, ResourceManager.Instance.GetRessources(2));
        }

        BaseController base_ = FindFirstObjectByType<BaseController>();
        if (base_ != null)
            OnPVChanges(base_.PVActuels, base_.PVMax);
    }

    void OnDisable()
    {
        GameManager.OnPhaseChanged      -= OnPhaseChanged;
        GameManager.OnWaveChanged       -= OnWaveChanged;
        GameManager.OnPrepTimerUpdated  -= OnTimerUpdated;
        BaseController.OnPVChanges      -= OnPVChanges;
        ResourceManager.OnRessourcesChangees -= OnRessourcesChangees;
    }

    void Update()
    {
        // Barre de chargement ReadySystem (mise à jour chaque frame)
        if (readySystem != null)
        {
            float prog = readySystem.Progression;
            if (barreReadyP1 != null) barreReadyP1.fillAmount = prog;
            if (barreReadyP2 != null) barreReadyP2.fillAmount = prog;
        }
    }

    // ── Callbacks événements ──────────────────────────────────────────────────
    private void OnPhaseChanged(GameManager.GameState etat)
    {
        if (textePhase == null) return;
        textePhase.text = etat switch
        {
            GameManager.GameState.Preparation => "PRÉPARATION",
            GameManager.GameState.Defense     => "DÉFENSE",
            _                                 => ""
        };
    }

    private void OnWaveChanged(int vague)
    {
        if (texteVague != null)
            texteVague.text = $"Vague {vague}";
    }

    private void OnTimerUpdated(float temps)
    {
        if (texteTimer == null) return;

        if (temps > 0f)
            texteTimer.text = $"{Mathf.CeilToInt(temps)}s";
        else
            texteTimer.text = "Maintien Tab / B pour lancer !";
    }

    private void OnPVChanges(int pvActuels, int pvMax)
    {
        if (sliderBaseHP != null)
        {
            sliderBaseHP.maxValue = pvMax;
            sliderBaseHP.value    = pvActuels;
        }
    }

    private void OnRessourcesChangees(int playerIndex, int montant)
    {
        if (playerIndex == 1 && texteRessourcesP1 != null)
            texteRessourcesP1.text = $"Or : {montant}";
        else if (playerIndex == 2 && texteRessourcesP2 != null)
            texteRessourcesP2.text = $"Or : {montant}";
    }
}
