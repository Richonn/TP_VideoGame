using System.Collections.Generic;
using UnityEngine;

public enum VFXType
{
    TowerMuzzle,
    TowerImpact,
    TowerPlace,
    TowerUpgrade,
    EnemyDeath,
    BaseHit,
    ResourceGain
}

[System.Serializable]
public class VFXEntry
{
    public VFXType type;
    public GameObject prefab;
    public int prewarm = 4;
}

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [SerializeField] private VFXEntry[] entries;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int projectilePrewarm = 8;

    private readonly Dictionary<VFXType, VFXPool> _pools = new Dictionary<VFXType, VFXPool>();
    private VFXPool _projectilePool;
    private Transform _poolRoot;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _poolRoot = new GameObject("VFX_Pool").transform;
        _poolRoot.SetParent(transform);

        if (entries != null)
        {
            foreach (VFXEntry e in entries)
            {
                if (e == null || e.prefab == null) continue;
                if (_pools.ContainsKey(e.type)) continue;
                _pools[e.type] = new VFXPool(e.prefab, _poolRoot, e.prewarm);
            }
        }

        if (projectilePrefab != null)
            _projectilePool = new VFXPool(projectilePrefab, _poolRoot, projectilePrewarm);
    }

    public GameObject LaunchProjectile(Vector3 from, Vector3 to)
    {
        if (_projectilePool == null) return null;
        GameObject go = _projectilePool.Get(from, Quaternion.identity);
        Projectile proj = go.GetComponent<Projectile>();
        if (proj != null) proj.Launch(from, to);
        return go;
    }

    public void ReleaseProjectile(GameObject go)
    {
        if (_projectilePool != null)
            _projectilePool.Release(go);
        else if (go != null)
            go.SetActive(false);
    }

    public void Play(VFXType type, Vector3 position, Quaternion rotation = default)
    {
        if (!_pools.TryGetValue(type, out VFXPool pool)) return;
        GameObject instance = pool.Get(position, rotation == default ? Quaternion.identity : rotation);

        AutoReleaseParticle auto = instance.GetComponent<AutoReleaseParticle>();
        if (auto == null) auto = instance.AddComponent<AutoReleaseParticle>();
        auto.Init(this, type);

        ParticleSystem ps = instance.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play(true);
    }

    public void Release(VFXType type, GameObject instance)
    {
        if (_pools.TryGetValue(type, out VFXPool pool))
            pool.Release(instance);
        else if (instance != null)
            Destroy(instance);
    }
}
