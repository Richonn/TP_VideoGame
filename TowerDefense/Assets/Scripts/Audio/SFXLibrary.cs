using UnityEngine;

public enum SFXType
{
    TowerShoot,
    TowerImpact,
    TowerPlace,
    TowerUpgrade,
    EnemyDeath,
    EnemyFootstep,
    EnemyAttack,
    BaseHit,
    PlayerFootstep,
    ResourceGain,
    UIHover,
    UIClick,
    UIBack,
    UIOpen,
    UIWaveStart
}

[CreateAssetMenu(fileName = "SFXLibrary", menuName = "TowerDefense/Audio/SFX Library")]
public class SFXLibrary : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public SFXType type;
        public AudioClip[] variants;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 0.5f)] public float pitchJitter = 0.05f;
    }

    [SerializeField] private Entry[] entries;

    public Entry GetEntry(SFXType type)
    {
        if (entries == null) return null;
        foreach (Entry e in entries)
            if (e != null && e.type == type) return e;
        return null;
    }

    public AudioClip GetRandomClip(SFXType type)
    {
        Entry e = GetEntry(type);
        if (e == null || e.variants == null || e.variants.Length == 0) return null;
        return e.variants[Random.Range(0, e.variants.Length)];
    }
}
