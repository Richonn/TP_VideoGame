using UnityEngine;
using System.Collections;

/// <summary>
/// Gère le spawn des vagues d'ennemis.
///
/// Distribution des types par vague :
///   Vagues 1-2  → Rush uniquement
///   Vagues 3-4  → Rush (70 %) + Tank (30 %)
///   Vagues 5+   → Rush (50 %) + Tank (30 %) + Flanker (20 %)
///
/// S'abonne aux événements du GameManager :
///   - GameState.Defense     → lance la vague courante
///   - GameState.Preparation → réinitialise l'état de vague
/// </summary>
public class WaveManager : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Prefabs ennemis")]
    [SerializeField] private GameObject prefabRush;
    [SerializeField] private GameObject prefabTank;
    [SerializeField] private GameObject prefabFlanker;

    [Header("Spawn")]
    [SerializeField] private Transform[] pointsDeSpawn;

    [Header("Difficulté")]
    [SerializeField] private int   ennemisVague1      = 5;
    [SerializeField] private int   ennemisSupParVague = 2;
    [SerializeField] private float intervalleSpawn    = 1f;

    // ── État ──────────────────────────────────────────────────────────────────
    private int  _ennemisRestants;
    private bool _vagueActive;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void OnEnable()
    {
        GameManager.OnPhaseChanged += OnPhaseChanged;
        EnemyAI.OnEnnemiMort       += OnEnnemiMort;
    }

    void OnDisable()
    {
        GameManager.OnPhaseChanged -= OnPhaseChanged;
        EnemyAI.OnEnnemiMort       -= OnEnnemiMort;
    }

    // ── Écoute des phases ─────────────────────────────────────────────────────
    private void OnPhaseChanged(GameManager.GameState etat)
    {
        if (etat == GameManager.GameState.Defense)
            LancerVague();

        if (etat == GameManager.GameState.Preparation)
            _vagueActive = false;
    }

    // ── Spawn ─────────────────────────────────────────────────────────────────
    private void LancerVague()
    {
        if (_vagueActive) return;

        int vague     = GameManager.Instance != null ? GameManager.Instance.CurrentWave : 1;
        int nbEnnemis = ennemisVague1 + (vague - 1) * ennemisSupParVague;

        StartCoroutine(SpawnVague(nbEnnemis, vague));
    }

    private IEnumerator SpawnVague(int nbEnnemis, int vague)
    {
        _vagueActive     = true;
        _ennemisRestants = nbEnnemis;

        for (int i = 0; i < nbEnnemis; i++)
        {
            SpawnEnnemi(ChoisirPrefab(vague));
            yield return new WaitForSecondsRealtime(intervalleSpawn);
        }
    }

    private void SpawnEnnemi(GameObject prefab)
    {
        if (prefab == null || pointsDeSpawn == null || pointsDeSpawn.Length == 0)
        {
            Debug.LogWarning("[WaveManager] Prefab ennemi ou points de spawn non assignés !");
            return;
        }

        Transform point = pointsDeSpawn[Random.Range(0, pointsDeSpawn.Length)];
        Instantiate(prefab, point.position, Quaternion.identity);
    }

    /// <summary>
    /// Choisit un prefab selon la vague et une distribution aléatoire pondérée.
    ///   Vagues 1-2 : Rush uniquement
    ///   Vagues 3-4 : Rush 70 % / Tank 30 %
    ///   Vagues 5+  : Rush 50 % / Tank 30 % / Flanker 20 %
    /// Retourne prefabRush si un prefab attendu n'est pas assigné.
    /// </summary>
    private GameObject ChoisirPrefab(int vague)
    {
        float r = Random.value;

        if (vague <= 2)
            return prefabRush;

        if (vague <= 4)
            return r < 0.7f ? prefabRush : (prefabTank != null ? prefabTank : prefabRush);

        // Vague 5+
        if (r < 0.5f) return prefabRush;
        if (r < 0.8f) return prefabTank    != null ? prefabTank    : prefabRush;
        return             prefabFlanker   != null ? prefabFlanker : prefabRush;
    }

    // ── Fin de vague ──────────────────────────────────────────────────────────
    private void OnEnnemiMort()
    {
        _ennemisRestants--;

        if (_ennemisRestants <= 0 && _vagueActive)
        {
            _vagueActive = false;
            GameManager.Instance?.WaveCompleted();
        }
    }
}
