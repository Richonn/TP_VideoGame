using UnityEngine;

[CreateAssetMenu(fileName = "MapBlueprint", menuName = "TowerDefense/Procedural/Map Blueprint")]
public class MapBlueprint : ScriptableObject
{
    [System.Serializable]
    public struct ObstacleLayer
    {
        public string label;
        public BlockVariantSet variantSet;

        [Header("Noise Distribution")]
        [Tooltip("Perlin noise threshold: higher = denser. 0 = no obstacles.")]
        [Range(0f, 1f)] public float density;
        [Tooltip("Perlin frequency. Higher = smaller, tighter patches.")]
        [Range(0.5f, 8f)] public float noiseScale;

        [Header("Clustering")]
        [Tooltip("Extra spawn probability when a neighbour already has an obstacle of this type.")]
        [Range(0f, 0.6f)] public float clusterBonus;

        [Header("Spacing")]
        [Tooltip("Minimum cell distance between two obstacles of this type. Use 1-2 for trees to avoid overlap.")]
        [Min(0)] public int minSpacing;

        public bool Enabled => variantSet != null && density > 0f;
    }

    [Header("Identity")]
    public string displayName = "Default Map";
    public int presetSeed = 42;
    public bool useRandomSeed = false;

    [Header("Grid Dimensions (match GridManager)")]
    public int width  = 20;
    public int height = 10;

    [Header("Obstacle Layers")]
    public ObstacleLayer trees;
    public ObstacleLayer bushes;
    public ObstacleLayer rocks;

    [Header("Zones")]
    [Tooltip("Columns 0 … obstacleStartX-1 stay fully clear (tower placement zone).")]
    public int obstacleStartX = 8;

    [Tooltip("Half-height (in cells) of the central corridor that is always walkable for the enemy path.")]
    [Min(0)] public int safeCorridorHalfHeight = 1;

    [Tooltip("Rows kept empty on top and bottom map edges.")]
    [Min(0)] public int edgePadding = 1;
}
