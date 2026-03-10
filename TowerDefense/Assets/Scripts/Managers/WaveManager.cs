using UnityEngine;
using System.Collections;

/// <summary>
/// Gère le spawn des vagues d'ennemis.
///
/// S'abonne aux événements du GameManager :
///   - GameState.Defense → lance la vague courante
///
/// S'abonne à EnemyAI.OnEnnemiMort pour détecter la fin de vague
/// et notifier GameManager.WaveCompleted().
///
/// La difficulté monte avec les vagues : +2 ennemis par vague.
/// </summary>
public class WaveManager : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Spawn")]
    [SerializeField] private GameObject prefabEnnemi;
    [SerializeField] private Transform[] pointsDeSpawn;

    [Header("Difficulté")]
    [SerializeField] private int ennemisVague1      = 5;
    [SerializeField] private int ennemisSupParVague = 2;
    [SerializeField] private float intervalleSpawn  = 1f;

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

        int vague = GameManager.Instance != null ? GameManager.Instance.CurrentWave : 1;
        int nbEnnemis = ennemisVague1 + (vague - 1) * ennemisSupParVague;

        StartCoroutine(SpawnVague(nbEnnemis));
    }

    private IEnumerator SpawnVague(int nbEnnemis)
    {
        _vagueActive      = true;
        _ennemisRestants  = nbEnnemis;

        for (int i = 0; i < nbEnnemis; i++)
        {
            SpawnEnnemi();
            yield return new WaitForSecondsRealtime(intervalleSpawn);
        }
    }

    private void SpawnEnnemi()
    {
        if (prefabEnnemi == null || pointsDeSpawn == null || pointsDeSpawn.Length == 0)
        {
            Debug.LogWarning("[WaveManager] Prefab ennemi ou points de spawn non assignés !");
            return;
        }

        Transform point = pointsDeSpawn[Random.Range(0, pointsDeSpawn.Length)];
        Instantiate(prefabEnnemi, point.position, Quaternion.identity);
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
