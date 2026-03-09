using UnityEngine;
using System;

/// <summary>
/// Gère les ressources (or) de chaque joueur.
/// Les ressources servent à poser et améliorer des tours.
///
/// Abonnement HUD : ResourceManager.OnRessourcesChangees
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Ressources de départ")]
    [SerializeField] private int ressourcesInitiales = 150;

    /// <summary>Déclenché quand les ressources d'un joueur changent.</summary>
    /// <param name="playerIndex">1 ou 2</param>
    /// <param name="nouvellValeur">Nouveau montant</param>
    public static event Action<int, int> OnRessourcesChangees;

    private int[] _ressources = new int[2];

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _ressources[0] = ressourcesInitiales;
        _ressources[1] = ressourcesInitiales;
        OnRessourcesChangees?.Invoke(1, _ressources[0]);
        OnRessourcesChangees?.Invoke(2, _ressources[1]);
    }

    public int GetRessources(int playerIndex) =>
        _ressources[Mathf.Clamp(playerIndex - 1, 0, 1)];

    public bool AAssezDeRessources(int playerIndex, int cout) =>
        GetRessources(playerIndex) >= cout;

    /// <summary>Ajoute des ressources au joueur (récompense après vague, etc.).</summary>
    public void Ajouter(int playerIndex, int montant)
    {
        int idx = Mathf.Clamp(playerIndex - 1, 0, 1);
        _ressources[idx] += montant;
        OnRessourcesChangees?.Invoke(playerIndex, _ressources[idx]);
    }

    /// <summary>
    /// Retire des ressources. Retourne false si fonds insuffisants.
    /// </summary>
    public bool Depenser(int playerIndex, int cout)
    {
        int idx = Mathf.Clamp(playerIndex - 1, 0, 1);
        if (_ressources[idx] < cout) return false;
        _ressources[idx] -= cout;
        OnRessourcesChangees?.Invoke(playerIndex, _ressources[idx]);
        return true;
    }
}
