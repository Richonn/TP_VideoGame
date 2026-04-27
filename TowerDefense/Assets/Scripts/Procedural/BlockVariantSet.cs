using UnityEngine;

[CreateAssetMenu(fileName = "BlockVariantSet", menuName = "TowerDefense/Procedural/Block Variant Set")]
public class BlockVariantSet : ScriptableObject
{
    [System.Serializable]
    public struct WeightedVariant
    {
        public GameObject prefab;
        [Min(0.01f)] public float weight;
    }

    [Header("Prefab Variants")]
    public WeightedVariant[] variants;

    [Header("Spawn Randomisation")]
    [Range(0f, 0.5f)] public float scaleVariation = 0.15f;
    public bool randomFlipX = false;

    public GameObject PickWeighted(System.Random rng)
    {
        if (variants == null || variants.Length == 0) return null;

        float total = 0f;
        foreach (var v in variants) total += v.weight;

        double roll = rng.NextDouble() * total;
        float cumul = 0f;
        foreach (var v in variants)
        {
            cumul += v.weight;
            if (roll <= cumul) return v.prefab;
        }
        return variants[variants.Length - 1].prefab;
    }

    public void ApplyRandomisation(Transform t, System.Random rng)
    {
        if (scaleVariation > 0f)
        {
            float delta = (float)(rng.NextDouble() * 2.0 - 1.0) * scaleVariation;
            t.localScale *= (1f + delta);
        }
        if (randomFlipX && rng.NextDouble() < 0.5)
        {
            Vector3 s = t.localScale;
            s.x = -Mathf.Abs(s.x);
            t.localScale = s;
        }
    }
}
