using UnityEngine;

public class FogOfWarManager : MonoBehaviour
{
    public static FogOfWarManager Instance { get; private set; }

    [Header("Appearance")]
    [SerializeField] private Color fogColor = new Color(0.05f, 0.05f, 0.1f, 0.88f);
    [SerializeField] private float fadeSpeed = 4f;

    [Header("Vision")]
    [SerializeField] private float playerRadius = 5f;
    [SerializeField] private float baseRadius = 3f;

    private const int SORT_FOG = 10;
    private const int SORT_PLAYER = 15;

    private SpriteRenderer[,] _tiles;
    private float[,] _targetAlphas;
    private GridManager _grid;
    private Sprite _blockSprite;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _grid = GridManager.Instance;
        if (_grid == null) { enabled = false; return; }

        _blockSprite = CreateSquareSprite();
        CreateTiles();
        ElevatePlayersAboveFog();
    }

    void Update()
    {
        UpdateVision();
        AnimateTiles();
        CullEnemies();
    }

    private void CreateTiles()
    {
        int w = _grid.Width;
        int h = _grid.Height;
        float size = _grid.CellSize;

        _tiles = new SpriteRenderer[w, h];
        _targetAlphas = new float[w, h];

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Node node = _grid.GetNode(x, y);
                if (node == null) continue;

                GameObject go = new GameObject($"Fog_{x}_{y}");
                go.transform.SetParent(transform);
                go.transform.position = new Vector3(node.worldPosition.x, node.worldPosition.y, 0f);
                go.transform.localScale = new Vector3(size, size, 1f);

                int fogLayer = LayerMask.NameToLayer("Ignore Raycast");
                if (fogLayer >= 0) go.layer = fogLayer;

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _blockSprite;
                sr.color = fogColor;
                sr.sortingOrder = SORT_FOG;

                _tiles[x, y] = sr;
                _targetAlphas[x, y] = 1f;
            }
        }
    }

    private static Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private void UpdateVision()
    {
        int w = _grid.Width;
        int h = _grid.Height;

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                _targetAlphas[x, y] = 1f;

        foreach (PlayerController player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
            RevealAround(player.transform.position, playerRadius);

        foreach (Tower tower in FindObjectsByType<Tower>(FindObjectsSortMode.None))
            RevealAround(tower.transform.position, tower.range);

        GameObject baseObj = GameObject.FindWithTag("Base");
        if (baseObj != null) RevealAround(baseObj.transform.position, baseRadius);
    }

    private void RevealAround(Vector2 center, float radius)
    {
        float radiusSq = radius * radius;

        for (int x = 0; x < _grid.Width; x++)
        {
            for (int y = 0; y < _grid.Height; y++)
            {
                Node node = _grid.GetNode(x, y);
                if (node == null) continue;

                if ((node.worldPosition - center).sqrMagnitude <= radiusSq)
                    _targetAlphas[x, y] = 0f;
            }
        }
    }

    private void AnimateTiles()
    {
        float step = fadeSpeed * Time.deltaTime;
        float alphaMax = fogColor.a;

        for (int x = 0; x < _grid.Width; x++)
        {
            for (int y = 0; y < _grid.Height; y++)
            {
                SpriteRenderer sr = _tiles[x, y];
                if (sr == null) continue;

                Color c = sr.color;
                c.a = Mathf.MoveTowards(c.a, _targetAlphas[x, y] * alphaMax, step);
                sr.color = c;
            }
        }
    }

    private void CullEnemies()
    {
        foreach (EnemyAI enemy in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        {
            bool revealed = IsCellRevealed(enemy.transform.position);
            SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = revealed;
        }
    }

    public bool IsCellRevealed(Vector2 worldPos)
    {
        Node node = _grid.WorldToNode(worldPos);
        if (node == null) return false;
        return _targetAlphas[node.gridX, node.gridY] < 0.5f;
    }

    private void ElevatePlayersAboveFog()
    {
        foreach (PlayerController player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = SORT_PLAYER;
        }
    }
}
