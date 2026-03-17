using UnityEngine;
using System.Collections.Generic;
using System;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Dimensions")]
    [SerializeField] private Vector2 gridOrigin = new Vector2(-20f, -10f);
    [SerializeField] private int width = 20;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 2f;

    [Header("Obstacles")]
    [SerializeField] private LayerMask obstacleLayer;

    public static event Action OnGridUpdated;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;
    public Vector2 GridOrigin => gridOrigin;
    public float GridWorldWidth => width * cellSize;
    public float GridWorldHeight => height * cellSize;
    public Vector2 GridWorldCenter => gridOrigin + new Vector2(GridWorldWidth * 0.5f, GridWorldHeight * 0.5f);

    private Node[,] _grid;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InitGrid();
    }

    private void InitGrid()
    {
        _grid = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = CellCenter(x, y);
                bool walkable = !Physics2D.OverlapCircle(pos, cellSize * 0.4f, obstacleLayer);
                _grid[x, y] = new Node(walkable, pos, x, y);
            }
        }
    }

    public void UpdateGrid()
    {
        InitGrid();
        OnGridUpdated?.Invoke();
    }

    public Node GetNode(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        return _grid[x, y];
    }

    public Node WorldToNode(Vector2 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize);
        int y = Mathf.FloorToInt((worldPos.y - gridOrigin.y) / cellSize);
        return GetNode(x, y);
    }

    public Vector2 CellCenter(int x, int y)
    {
        return gridOrigin + new Vector2(x * cellSize + cellSize * 0.5f, y * cellSize + cellSize * 0.5f);
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>(8);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                Node neighbor = GetNode(node.gridX + dx, node.gridY + dy);
                if (neighbor != null) neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    public void ResetCosts()
    {
        foreach (Node n in _grid)
            n.Reset();
    }

    void OnDrawGizmos()
    {
        if (_grid == null) return;

        foreach (Node n in _grid)
        {
            Gizmos.color = n.walkable ? new Color(0f, 1f, 0f, 0.15f) : new Color(1f, 0f, 0f, 0.4f);
            Gizmos.DrawCube(n.worldPosition, Vector3.one * (cellSize * 0.9f));
        }
    }
}
