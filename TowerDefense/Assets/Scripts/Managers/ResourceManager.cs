using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Starting Resources")]
    [SerializeField] private int initialResources = 150;

    public static event Action<int, int> OnResourcesChanged;

    private int[] _resources = new int[2];

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _resources[0] = initialResources;
        _resources[1] = initialResources;
        OnResourcesChanged?.Invoke(1, _resources[0]);
        OnResourcesChanged?.Invoke(2, _resources[1]);
    }

    public int GetResources(int playerIndex) =>
        _resources[Mathf.Clamp(playerIndex - 1, 0, 1)];

    public bool HasEnoughResources(int playerIndex, int cost) =>
        GetResources(playerIndex) >= cost;

    public void Add(int playerIndex, int amount)
    {
        int idx = Mathf.Clamp(playerIndex - 1, 0, 1);
        _resources[idx] += amount;
        OnResourcesChanged?.Invoke(playerIndex, _resources[idx]);
        if (amount > 0) AudioManager.Instance?.PlaySFX(SFXType.ResourceGain);
    }

    public bool Spend(int playerIndex, int cost)
    {
        int idx = Mathf.Clamp(playerIndex - 1, 0, 1);
        if (_resources[idx] < cost) return false;
        _resources[idx] -= cost;
        OnResourcesChanged?.Invoke(playerIndex, _resources[idx]);
        return true;
    }
}
