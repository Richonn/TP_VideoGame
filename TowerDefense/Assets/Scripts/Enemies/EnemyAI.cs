using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Comportement d'un ennemi :
///   - Calcule son chemin vers la base via A*
///   - Suit le chemin waypoint par waypoint
///   - Recalcule son chemin quand une tour est posée (GridManager.OnGrilleModifiee)
///   - Inflige des dégâts à la base à l'arrivée, puis se détruit
///   - Peut prendre des dégâts des tours
///
/// Types disponibles (configurés sur le prefab) :
///   Rush    — rapide, fragile, fonce droit au but
///   Tank    — lent, très résistant, dégâts élevés à la base
///   Flanker — vitesse moyenne, évite les tours (A* pondéré, à venir)
///
/// Requis sur le prefab : Rigidbody2D (Kinematic), Collider2D (Trigger)
/// Le GameObject de la base doit avoir le tag "Base".
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    // ── Type d'ennemi ─────────────────────────────────────────────────────────
    public enum EnemyType { Rush, Tank, Flanker }

    [Header("Type")]
    [SerializeField] public EnemyType type = EnemyType.Rush;

    // ── Inspector (écrasé par ConfigurerSelonType) ────────────────────────────
    [Header("Stats de base")]
    [SerializeField] private float vitesse    = 2f;
    [SerializeField] private int   pvMax      = 3;
    [SerializeField] private int   degatsBase = 1;

    [Header("Navigation")]
    [Tooltip("Distance minimale pour valider l'atteinte d'un waypoint.")]
    [SerializeField] private float toleranceWaypoint = 0.15f;

    // ── Événement ─────────────────────────────────────────────────────────────
    /// <summary>Déclenché à la mort d'un ennemi (WaveManager l'écoute).</summary>
    public static event Action OnEnnemiMort;

    // ── État interne ──────────────────────────────────────────────────────────
    private int            _pvActuels;
    private int            _recompenseOr;
    private List<Vector2>  _chemin;
    private int            _indexWaypoint;
    private Transform      _cibleBase;
    private bool           _arrivee;

    public int IndexWaypoint => _indexWaypoint;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        ConfigurerSelonType();
        _pvActuels = pvMax;
    }

    void Start()
    {
        GameObject baseObj = GameObject.FindWithTag("Base");
        if (baseObj != null)
            _cibleBase = baseObj.transform;
        else
            Debug.LogWarning("[EnemyAI] Aucun GameObject taggé 'Base' trouvé !");

        CalculerChemin();
        GridManager.OnGrilleModifiee += CalculerChemin;
    }

    void OnDestroy()
    {
        GridManager.OnGrilleModifiee -= CalculerChemin;
    }

    void Update()
    {
        if (_arrivee) return;
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Defense) return;
        SuivreChemin();
    }

    // ── Configuration par type ────────────────────────────────────────────────
    private void ConfigurerSelonType()
    {
        switch (type)
        {
            case EnemyType.Rush:
                vitesse       = 3.5f;
                pvMax         = 2;
                degatsBase    = 1;
                _recompenseOr = 10;
                break;

            case EnemyType.Tank:
                vitesse       = 1f;
                pvMax         = 10;
                degatsBase    = 3;
                _recompenseOr = 25;
                break;

            case EnemyType.Flanker:
                vitesse       = 2.5f;
                pvMax         = 4;
                degatsBase    = 1;
                _recompenseOr = 15;
                break;
        }

        // Sprites différents par type — pas de teinte supplémentaire nécessaire
    }

    // ── Navigation ────────────────────────────────────────────────────────────
    public void CalculerChemin()
    {
        if (_cibleBase == null || AStarPathfinder.Instance == null) return;

        List<Vector2> nouveauChemin = AStarPathfinder.Instance.TrouverChemin(
            transform.position,
            _cibleBase.position
        );

        if (nouveauChemin != null)
        {
            _chemin        = nouveauChemin;
            _indexWaypoint = 0;
        }
        else
        {
            Debug.LogWarning($"[EnemyAI] {gameObject.name} : chemin bloqué !");
        }
    }

    private void SuivreChemin()
    {
        if (_chemin == null || _indexWaypoint >= _chemin.Count) return;

        Vector2 destination = _chemin[_indexWaypoint];
        transform.position  = Vector2.MoveTowards(
            transform.position,
            destination,
            vitesse * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, destination) < toleranceWaypoint)
            _indexWaypoint++;
    }

    // ── Collisions ────────────────────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Base"))
        {
            _arrivee = true;
            other.GetComponent<BaseController>()?.PrendreDegats(degatsBase);
            Mourir();
        }
    }

    // ── Dégâts ────────────────────────────────────────────────────────────────
    public void PrendreDegats(int degats)
    {
        _pvActuels -= degats;
        if (_pvActuels <= 0)
            Mourir();
    }

    private void Mourir()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.Ajouter(1, _recompenseOr);
            ResourceManager.Instance.Ajouter(2, _recompenseOr);
        }

        OnEnnemiMort?.Invoke();
        Destroy(gameObject);
    }
}
