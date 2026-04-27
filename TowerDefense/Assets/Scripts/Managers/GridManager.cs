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
    [SerializeField] private string obstaclesSortingLayer = "Obstacles";

    public static event Action OnGridUpdated;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;
    public Vector2 GridOrigin => gridOrigin;
    public float GridWorldWidth => width * cellSize;
    public float GridWorldHeight => height * cellSize;
    public Vector2 GridWorldCenter => gridOrigin + new Vector2(GridWorldWidth * 0.5f, GridWorldHeight * 0.5f);

    private Node[,] _grid;
    private readonly List<Collider2D> _obstacleColliders = new List<Collider2D>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InitGrid();
    }

    public void RegisterObstacleColliders(List<Collider2D> colliders)
    {
        _obstacleColliders.AddRange(colliders);
    }

    public void ClearObstacleColliders()
    {
        _obstacleColliders.Clear();
    }

    private void InitGrid()
    {
        _grid = new Node[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _grid[x, y] = new Node(true, CellCenter(x, y), x, y);
    }

    private bool HasObstacleAt(Vector2 pos)
    {
        Bounds checkBounds = new Bounds(new Vector3(pos.x, pos.y, 0), Vector3.one * cellSize * 0.8f);
        foreach (Collider2D col in _obstacleColliders)
        {
            if (col != null && col.bounds.Intersects(checkBounds))
                return true;
        }
        return false;
    }

    public void UpdateGrid()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _grid[x, y].walkable = !HasObstacleAt(CellCenter(x, y));
        OnGridUpdated?.Invoke();
    }

    public void SetNodesWalkable(Node[] nodes, bool walkable)
    {
        foreach (Node n in nodes)
            if (n != null) n.walkable = walkable;
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
