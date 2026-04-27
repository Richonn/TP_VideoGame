using UnityEngine;

public class TowerPlacer : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private int playerIndex = 1;

    [Header("Tower")]
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private int towerCost = 50;

    [Header("Placement Bounds")]
    [SerializeField] private float minX = -20f;
    [SerializeField] private float maxX = 0f;

    private GameObject _cursor;
    private SpriteRenderer _cursorRenderer;

    private static readonly Color COLOR_VALID = new Color(0f, 1f, 0f, 0.45f);
    private static readonly Color COLOR_INVALID = new Color(1f, 0f, 0f, 0.45f);

    private Node _anchorNode;
    private bool _placementValid;

    void Start()
    {
        CreateCursor();
    }

    void Update()
    {
        bool inPrep = GameManager.Instance?.CurrentState == GameManager.GameState.Preparation;
        _cursor.SetActive(inPrep);

        if (!inPrep) return;

        UpdateCursor();

        if (InputManager.Instance != null && InputManager.Instance.GetInput(playerIndex).PlaceTowerPressed)
            TryPlace();
    }

    private void CreateCursor()
    {
        _cursor = new GameObject($"Cursor_P{playerIndex}");

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);

        _cursorRenderer = _cursor.AddComponent<SpriteRenderer>();
        _cursorRenderer.sprite = sprite;
        _cursorRenderer.sortingOrder = 5;

        float size = GridManager.Instance != null ? GridManager.Instance.CellSize : 1f;
        _cursor.transform.localScale = Vector3.one * size * 2f * 0.95f;
    }

    private void UpdateCursor()
    {
        if (GridManager.Instance == null) return;

        _anchorNode = GridManager.Instance.WorldToNode(transform.position);
        if (_anchorNode == null) return;

        float cs = GridManager.Instance.CellSize;
        Vector2 blockCenter = _anchorNode.worldPosition + new Vector2(cs * 0.5f, cs * 0.5f);
        _cursor.transform.position = blockCenter;

        _placementValid = IsPlacementValid(_anchorNode);
        _cursorRenderer.color = _placementValid ? COLOR_VALID : COLOR_INVALID;
    }

    private bool IsPlacementValid(Node anchor)
    {
        if (anchor == null) return false;

        Node[] block = GetBlockNodes(anchor);
        if (block == null) return false;

        foreach (Node n in block)
        {
            if (n == null || !n.walkable) return false;
            if (n.worldPosition.x < minX || n.worldPosition.x > maxX) return false;
        }

        if (ResourceManager.Instance != null &&
            !ResourceManager.Instance.HasEnoughResources(playerIndex, towerCost)) return false;

        return true;
    }

    private Node[] GetBlockNodes(Node anchor)
    {
        GridManager gm = GridManager.Instance;
        Node n00 = gm.GetNode(anchor.gridX, anchor.gridY);
        Node n10 = gm.GetNode(anchor.gridX + 1, anchor.gridY);
        Node n01 = gm.GetNode(anchor.gridX, anchor.gridY + 1);
        Node n11 = gm.GetNode(anchor.gridX + 1, anchor.gridY + 1);

        if (n00 == null || n10 == null || n01 == null || n11 == null) return null;
        return new Node[] { n00, n10, n01, n11 };
    }

    private void TryPlace()
    {
        if (!_placementValid || _anchorNode == null || towerPrefab == null) return;

        if (ResourceManager.Instance != null &&
            !ResourceManager.Instance.Spend(playerIndex, towerCost)) return;

        float cs = GridManager.Instance.CellSize;
        Vector2 blockCenter = _anchorNode.worldPosition + new Vector2(cs * 0.5f, cs * 0.5f);
        Instantiate(towerPrefab, blockCenter, Quaternion.identity);

        Node[] nodes = GetBlockNodes(_anchorNode);
        if (nodes != null)
            GridManager.Instance?.SetNodesWalkable(nodes, false);
        else
            GridManager.Instance?.UpdateGrid();

        AudioManager.Instance?.PlaySFX(SFXType.TowerPlace, blockCenter);
    }
}
