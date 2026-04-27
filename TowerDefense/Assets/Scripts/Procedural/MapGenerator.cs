using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance { get; private set; }

    [SerializeField] private MapBlueprint blueprint;
    [SerializeField] private Transform    spawnRoot;
    [SerializeField] private bool         generateOnAwake = true;

    [Header("Seed Override")]
    [SerializeField] private bool useOverrideSeed = false;
    [SerializeField] private int  overrideSeed    = 0;

    [Header("Spawn Animation")]
    [SerializeField] private float animDuration = 0.35f;
    [SerializeField] private float animStagger  = 0.012f;
    [Tooltip("Skip animation on first load (Awake). Animation still plays on manual Generate() calls.")]
    [SerializeField] private bool skipAnimOnAwake = true;

    [Header("Connectivity Check")]
    [Tooltip("World-space positions that must reach the base after generation (enemy spawn points).")]
    [SerializeField] private Transform[] spawnPoints;
    [Tooltip("World-space position of the base the enemy path must reach.")]
    [SerializeField] private Transform   baseTransform;

    public int LastSeed { get; private set; }

    private readonly List<GameObject> _spawned = new List<GameObject>();
    private readonly Dictionary<Vector2Int, int> _obstacleMap = new Dictionary<Vector2Int, int>();
    private readonly List<Collider2D> _colliders = new List<Collider2D>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (generateOnAwake) Generate(skipAnimOnAwake);
    }

    public void Generate() => Generate(false);

    public void Generate(bool silent)
    {
        if (blueprint == null)
        {
            Debug.LogWarning("[MapGenerator] No blueprint assigned — skipping.");
            return;
        }

        GridManager gm = GridManager.Instance;
        if (gm == null)
        {
            Debug.LogError("[MapGenerator] GridManager not found.");
            return;
        }

        ClearSpawned();

        int seed;
        if (useOverrideSeed)               seed = overrideSeed;
        else if (blueprint.useRandomSeed)  seed = Random.Range(int.MinValue, int.MaxValue);
        else                               seed = blueprint.presetSeed;
        LastSeed = seed;

        System.Random rng = new System.Random(seed);

        float offTrees  = (float)(rng.NextDouble() * 1000.0);
        float offBushes = (float)(rng.NextDouble() * 1000.0);
        float offRocks  = (float)(rng.NextDouble() * 1000.0);

        int mapWidth  = gm.Width;
        int mapHeight = gm.Height;

        bool[,] occupied  = new bool[mapWidth, mapHeight];
        bool[,] reservedT = new bool[mapWidth, mapHeight];
        bool[,] reservedB = new bool[mapWidth, mapHeight];
        bool[,] reservedR = new bool[mapWidth, mapHeight];
        bool[][,] reserved = { reservedT, reservedB, reservedR };
        _obstacleMap.Clear();

        int midY      = mapHeight / 2;
        int corridorH = blueprint.safeCorridorHalfHeight;
        int pad       = blueprint.edgePadding;

        for (int x = blueprint.obstacleStartX; x < mapWidth; x++)
        {
            for (int y = pad; y < mapHeight - pad; y++)
            {
                if (Mathf.Abs(y - midY) <= corridorH) continue;

                int layerIdx = TryPlaceCell(x, y, occupied, reserved, rng, offTrees, offBushes, offRocks);
                if (layerIdx >= 0)
                {
                    occupied[x, y] = true;
                    _obstacleMap[new Vector2Int(x, y)] = layerIdx;

                    int spacing = GetLayer(layerIdx).minSpacing;
                    if (spacing > 0)
                        MarkReserved(reserved[layerIdx], x, y, spacing, mapWidth, mapHeight);
                }
            }
        }

        RepairConnectivity(occupied, gm);

        var sortedCells = new List<Vector2Int>(_obstacleMap.Keys);
        sortedCells.Sort((a, b) => (a.x + a.y).CompareTo(b.x + b.y));

        int spawnIndex = 0;
        foreach (Vector2Int cell in sortedCells)
        {
            int layerIdx = _obstacleMap[cell];
            MapBlueprint.ObstacleLayer layer = GetLayer(layerIdx);
            if (!layer.Enabled) continue;

            GameObject prefab = layer.variantSet.PickWeighted(rng);
            if (prefab == null) continue;

            Vector2 worldPos = gm.CellCenter(cell.x, cell.y);
            Transform parent = spawnRoot != null ? spawnRoot : transform;
            GameObject go    = Instantiate(prefab, worldPos, Quaternion.identity, parent);

            EnsureObstacleSortingLayer(go, layerIdx);
            layer.variantSet.ApplyRandomisation(go.transform, rng);
            foreach (Collider2D col in go.GetComponentsInChildren<Collider2D>(true))
                _colliders.Add(col);

            _spawned.Add(go);

            if (!silent)
            {
                Vector3 target = go.transform.localScale;
                go.transform.localScale = Vector3.zero;
                StartCoroutine(SpawnAnim(go.transform, target, spawnIndex * animStagger));
                spawnIndex++;
            }
        }

        StartCoroutine(RefreshGridNextFrame());
    }

    private int TryPlaceCell(int x, int y, bool[,] occupied, bool[][,] reserved,
                             System.Random rng,
                             float offT, float offB, float offR)
    {
        MapBlueprint.ObstacleLayer[] layers  = { blueprint.trees, blueprint.bushes, blueprint.rocks };
        float[]                      offsets = { offT, offB, offR };

        if (occupied[x, y]) return -1;

        for (int i = 0; i < layers.Length; i++)
        {
            MapBlueprint.ObstacleLayer layer = layers[i];
            if (!layer.Enabled) continue;
            if (reserved[i][x, y]) continue;

            float noise = Mathf.PerlinNoise(
                x * layer.noiseScale * 0.1f + offsets[i],
                y * layer.noiseScale * 0.1f + offsets[i] + 17.3f);

            float probability = layer.density * noise;

            if (layer.clusterBonus > 0f)
            {
                int w = occupied.GetLength(0);
                int h = occupied.GetLength(1);
                bool hasNeighbour =
                    (x > 0     && occupied[x - 1, y]) ||
                    (x < w - 1 && occupied[x + 1, y]) ||
                    (y > 0     && occupied[x, y - 1]) ||
                    (y < h - 1 && occupied[x, y + 1]);

                if (hasNeighbour) probability += layer.clusterBonus;
            }

            if (rng.NextDouble() < probability) return i;
        }
        return -1;
    }

    private void MarkReserved(bool[,] grid, int cx, int cy, int radius, int w, int h)
    {
        for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
            {
                int nx = cx + dx, ny = cy + dy;
                if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                    grid[nx, ny] = true;
            }
    }

    private MapBlueprint.ObstacleLayer GetLayer(int idx) =>
        idx switch { 0 => blueprint.trees, 1 => blueprint.bushes, _ => blueprint.rocks };

    private void RepairConnectivity(bool[,] occupied, GridManager gm)
    {
        if (spawnPoints == null || spawnPoints.Length == 0 || baseTransform == null) return;

        Node baseNode = gm.WorldToNode(baseTransform.position);
        if (baseNode == null) return;

        bool[,] walkable = BuildWalkableMap(occupied);

        const int maxPasses = 50;
        for (int pass = 0; pass < maxPasses; pass++)
        {
            HashSet<Vector2Int> reachable = FloodFill(walkable, baseNode.gridX, baseNode.gridY);

            bool        allReachable = true;
            Vector2Int  blocked      = default;

            foreach (Transform sp in spawnPoints)
            {
                if (sp == null) continue;
                Node spNode = gm.WorldToNode(sp.position);
                if (spNode == null) continue;
                var spCell = new Vector2Int(spNode.gridX, spNode.gridY);
                if (!reachable.Contains(spCell))
                {
                    allReachable = false;
                    blocked = FindNearestBlockingObstacle(reachable, spCell);
                    break;
                }
            }

            if (allReachable) break;

            if (_obstacleMap.ContainsKey(blocked))
            {
                _obstacleMap.Remove(blocked);
                occupied[blocked.x, blocked.y] = false;
                walkable[blocked.x, blocked.y]  = true;
            }
            else break;
        }
    }

    private bool[,] BuildWalkableMap(bool[,] occupied)
    {
        int w = occupied.GetLength(0);
        int h = occupied.GetLength(1);
        bool[,] result = new bool[w, h];
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result[x, y] = !occupied[x, y];
        return result;
    }

    private HashSet<Vector2Int> FloodFill(bool[,] walkable, int startX, int startY)
    {
        var visited = new HashSet<Vector2Int>();
        var queue   = new Queue<Vector2Int>();
        var start   = new Vector2Int(startX, startY);

        if (!InBounds(startX, startY) || !walkable[startX, startY]) return visited;

        queue.Enqueue(start);
        visited.Add(start);

        int[] dx = {  1, -1,  0,  0 };
        int[] dy = {  0,  0,  1, -1 };

        while (queue.Count > 0)
        {
            Vector2Int cur = queue.Dequeue();
            for (int d = 0; d < 4; d++)
            {
                int nx = cur.x + dx[d];
                int ny = cur.y + dy[d];
                var next = new Vector2Int(nx, ny);
                if (InBounds(nx, ny) && walkable[nx, ny] && !visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }
        return visited;
    }

    private Vector2Int FindNearestBlockingObstacle(HashSet<Vector2Int> reachable, Vector2Int target)
    {
        Vector2Int best     = target;
        int        bestDist = int.MaxValue;

        foreach (var cell in _obstacleMap.Keys)
        {
            if (reachable.Contains(cell)) continue;
            int dist = Mathf.Abs(cell.x - target.x) + Mathf.Abs(cell.y - target.y);
            if (dist < bestDist) { bestDist = dist; best = cell; }
        }
        return best;
    }

    private bool InBounds(int x, int y) =>
        x >= 0 && x < GridManager.Instance.Width && y >= 0 && y < GridManager.Instance.Height;

    private static void EnsureObstacleSortingLayer(GameObject go, int layerIdx)
    {
        int order = Mathf.RoundToInt(-go.transform.position.y * 100f)
                  + Mathf.RoundToInt(go.transform.position.x)
                  - layerIdx;
        foreach (SpriteRenderer sr in go.GetComponentsInChildren<SpriteRenderer>(true))
        {
            sr.sortingLayerName = "Obstacles";
            sr.sortingOrder     = order;
        }
    }

    private IEnumerator SpawnAnim(Transform t, Vector3 targetScale, float delay)
    {
        float elapsed = 0f;
        while (elapsed < delay)
        {
            if (t == null) yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < animDuration)
        {
            if (t == null) yield break;
            elapsed += Time.deltaTime;
            float n = Easing.Evaluate(Easing.Ease.EaseOutBack, elapsed / animDuration);
            t.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, n);
            yield return null;
        }
        if (t != null) t.localScale = targetScale;
    }

    private IEnumerator RefreshGridNextFrame()
    {
        yield return null;
        GridManager gm = GridManager.Instance;
        if (gm == null) yield break;
        gm.ClearObstacleColliders();
        gm.RegisterObstacleColliders(_colliders);
        gm.UpdateGrid();
    }

    private void ClearSpawned()
    {
        foreach (GameObject go in _spawned)
            if (go != null) Destroy(go);
        _spawned.Clear();
        _obstacleMap.Clear();
        _colliders.Clear();
    }
}
