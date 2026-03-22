using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector2 worldPosition;
    public int gridX, gridY;

    public int gCost;
    public int hCost;
    public int fCost => (gCost == int.MaxValue) ? int.MaxValue : gCost + hCost;
    // public int fCost => gCost + hCost;

    public Node parent;

    public Node(bool walkable, Vector2 worldPos, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPos;
        this.gridX = gridX;
        this.gridY = gridY;
    }

    public void Reset()
    {
        gCost = int.MaxValue;
        hCost = 0;
        parent = null;
    }
}
