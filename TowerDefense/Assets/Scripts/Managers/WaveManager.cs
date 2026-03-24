using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject prefabRush;
    [SerializeField] private GameObject prefabTank;
    [SerializeField] private GameObject prefabFlanker;

    [Header("Spawn")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Difficulty")]
    [SerializeField] private int enemiesWave1 = 5;
    [SerializeField] private int enemiesPerWave = 2;
    [SerializeField] private float spawnInterval = 1f;

    private int _enemiesRemaining;
    private bool _waveActive;

    void OnEnable()
    {
        GameManager.OnPhaseChanged += OnPhaseChanged;
        EnemyAI.OnEnemyDied += OnEnemyDied;
    }

    void OnDisable()
    {
        GameManager.OnPhaseChanged -= OnPhaseChanged;
        EnemyAI.OnEnemyDied -= OnEnemyDied;
    }

    private void OnPhaseChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Defense) LaunchWave();
        if (state == GameManager.GameState.Preparation) _waveActive = false;
    }

    private void LaunchWave()
    {
        if (_waveActive) return;

        int wave = GameManager.Instance != null ? GameManager.Instance.CurrentWave : 1;
        int count = enemiesWave1 + (wave - 1) * enemiesPerWave;

        if (GameManager.Instance?.Difficulty == GameManager.DifficultyLevel.Hard)
            count = Mathf.RoundToInt(count * 1.5f);

        StartCoroutine(SpawnWave(count, wave));
    }

    private IEnumerator SpawnWave(int count, int wave)
    {
        _waveActive = true;
        _enemiesRemaining = count;

        for (int i = 0; i < count; i++)
        {
            SpawnEnemy(PickPrefab(wave));
            yield return new WaitForSecondsRealtime(spawnInterval);
        }
    }

    private void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[WaveManager] Enemy prefab or spawn points not assigned!");
            return;
        }

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(prefab, point.position, Quaternion.identity);
    }

    private GameObject PickPrefab(int wave)
    {
        float r = Random.value;
        bool hard = GameManager.Instance?.Difficulty == GameManager.DifficultyLevel.Hard;

        if (hard)
        {
            if (wave <= 1)
                return r < 0.55f ? prefabRush : (prefabTank != null ? prefabTank : prefabRush);
            if (wave <= 3)
            {
                if (r < 0.35f) return prefabRush;
                if (r < 0.75f) return prefabTank != null ? prefabTank : prefabRush;
                return prefabFlanker != null ? prefabFlanker : prefabRush;
            }
            if (r < 0.25f) return prefabRush;
            if (r < 0.6f) return prefabTank != null ? prefabTank : prefabRush;
            return prefabFlanker != null ? prefabFlanker : prefabRush;
        }

        if (wave <= 2) return prefabRush;
        if (wave <= 4) return r < 0.7f ? prefabRush : (prefabTank != null ? prefabTank : prefabRush);
        if (r < 0.5f) return prefabRush;
        if (r < 0.8f) return prefabTank != null ? prefabTank : prefabRush;
        return prefabFlanker != null ? prefabFlanker : prefabRush;
    }

    private void OnEnemyDied()
    {
        _enemiesRemaining--;

        if (_enemiesRemaining <= 0 && _waveActive)
        {
            _waveActive = false;
            GameManager.Instance?.WaveCompleted();
        }
    }
}
