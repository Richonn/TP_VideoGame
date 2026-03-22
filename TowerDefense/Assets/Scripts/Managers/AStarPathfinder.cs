using UnityEngine;
using System.Collections.Generic;

public class AStarPathfinder : MonoBehaviour
{
    public static AStarPathfinder Instance { get; private set; }

    private const int COST_ORTHO = 10;
    private const int COST_DIAGONAL = 14;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public List<Vector2> FindPath(Vector2 start, Vector2 end, int towerPenalty = 0)
    {
        GridManager grid = GridManager.Instance;
        if (grid == null) return null;

        Node startNode = grid.WorldToNode(start);
        Node endNode = grid.WorldToNode(end);

        if (startNode == null || endNode == null) return null;
        if (!endNode.walkable) return null;

        Node[] towerNodes = towerPenalty > 0 ? GetTowerNodes(grid) : null;

        grid.ResetCosts();

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        startNode.gCost = 0;
        startNode.hCost = Heuristic(startNode, endNode);
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node current = GetBestNode(openSet);

            if (current == endNode)
                return ReconstructPath(endNode);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Node neighbor in grid.GetNeighbors(current))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor)) continue;

                bool diagonal = neighbor.gridX != current.gridX && neighbor.gridY != current.gridY;
                int moveCost = diagonal ? COST_DIAGONAL : COST_ORTHO;
                int penalty = towerPenalty > 0 ? CalculateTowerPenalty(neighbor, towerNodes, towerPenalty) : 0;
                int newGCost = current.gCost + moveCost + penalty;
                if (newGCost < 0) newGCost = int.MaxValue;

                if (newGCost < neighbor.gCost)
                {
                    neighbor.gCost = newGCost;
                    neighbor.hCost = Heuristic(neighbor, endNode);
                    neighbor.parent = current;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    private Node[] GetTowerNodes(GridManager grid)
    {
        Tower[] towers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
        Node[] nodes = new Node[towers.Length];
        for (int i = 0; i < towers.Length; i++)
            nodes[i] = grid.WorldToNode(towers[i].transform.position);
        return nodes;
    }

    private int CalculateTowerPenalty(Node node, Node[] towerNodes, int penaltyBase)
    {
        int extra = 0;
        foreach (Node tower in towerNodes)
        {
            if (tower == null) continue;
            int dist = Mathf.Max(
                Mathf.Abs(node.gridX - tower.gridX),
                Mathf.Abs(node.gridY - tower.gridY)
            );
            if (dist <= 1) extra += penaltyBase * 3;
            else if (dist <= 2) extra += penaltyBase;
        }
        return extra;
    }

    private Node GetBestNode(List<Node> openSet)
    {
        Node best = openSet[0];
        for (int i = 1; i < openSet.Count; i++)
        {
            if (openSet[i].fCost < best.fCost ||
               (openSet[i].fCost == best.fCost && openSet[i].hCost < best.hCost))
            {
                best = openSet[i];
            }
        }
        return best;
    }

    private int Heuristic(Node a, Node b)
    {
        int dx = Mathf.Abs(a.gridX - b.gridX);
        int dy = Mathf.Abs(a.gridY - b.gridY);
        return COST_DIAGONAL * Mathf.Min(dx, dy) + COST_ORTHO * Mathf.Abs(dx - dy);
    }

    private List<Vector2> ReconstructPath(Node end)
    {
        List<Vector2> path = new List<Vector2>();
        Node current = end;

        while (current != null)
        {
            path.Add(current.worldPosition);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }
}
