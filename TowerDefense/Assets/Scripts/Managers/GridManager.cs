using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Gère la grille de navigation 2D.
///
/// La grille couvre toute la map. Chaque cellule est soit walkable,
/// soit obstacle (tour placée ou décor bloquant).
///
/// Appeler MettreAJourGrille() après chaque placement de tour pour
/// déclencher le recalcul de chemin de tous les ennemis actifs.
/// </summary>
public class GridManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static GridManager Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Dimensions de la grille")]
    [Tooltip("Coin bas-gauche de la grille en coordonnées monde.")]
    [SerializeField] private Vector2 origineGrille = new Vector2(-20f, -10f);
    [SerializeField] private int largeur  = 20;
    [SerializeField] private int hauteur  = 10;
    [SerializeField] private float tailleCellule = 2f;

    [Header("Obstacles")]
    [Tooltip("Layer des tours et obstacles bloquants.")]
    [SerializeField] private LayerMask layerObstacles;

    // ── Événement ─────────────────────────────────────────────────────────────
    /// <summary>Déclenché quand la grille est mise à jour (tour posée, etc.).</summary>
    public static event Action OnGrilleModifiee;

    // ── Données ───────────────────────────────────────────────────────────────
    public int Largeur  => largeur;
    public int Hauteur  => hauteur;
    public float TailleCellule => tailleCellule;

    private Node[,] _grille;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InitialiserGrille();
    }

    // ── Initialisation ────────────────────────────────────────────────────────
    private void InitialiserGrille()
    {
        _grille = new Node[largeur, hauteur];

        for (int x = 0; x < largeur; x++)
        {
            for (int y = 0; y < hauteur; y++)
            {
                Vector2 positionMonde = CentreDeCase(x, y);
                bool walkable = !Physics2D.OverlapCircle(
                    positionMonde,
                    tailleCellule * 0.4f,
                    layerObstacles
                );
                _grille[x, y] = new Node(walkable, positionMonde, x, y);
            }
        }
    }

    /// <summary>
    /// Recalcule la walkability de toute la grille,
    /// puis notifie les ennemis de recalculer leur chemin.
    /// Appeler après chaque placement / destruction de tour.
    /// </summary>
    public void MettreAJourGrille()
    {
        InitialiserGrille();
        OnGrilleModifiee?.Invoke();
    }

    // ── Accès aux noeuds ──────────────────────────────────────────────────────
    public Node ObtenirNoeud(int x, int y)
    {
        if (x < 0 || x >= largeur || y < 0 || y >= hauteur) return null;
        return _grille[x, y];
    }

    /// <summary>Convertit une position monde en noeud de grille.</summary>
    public Node MondeVersNoeud(Vector2 positionMonde)
    {
        int x = Mathf.FloorToInt((positionMonde.x - origineGrille.x) / tailleCellule);
        int y = Mathf.FloorToInt((positionMonde.y - origineGrille.y) / tailleCellule);
        return ObtenirNoeud(x, y);
    }

    /// <summary>Retourne la position monde du centre d'une case.</summary>
    public Vector2 CentreDeCase(int x, int y)
    {
        return origineGrille + new Vector2(
            x * tailleCellule + tailleCellule * 0.5f,
            y * tailleCellule + tailleCellule * 0.5f
        );
    }

    /// <summary>Retourne les 8 voisins d'un noeud (orthogonaux + diagonaux).</summary>
    public List<Node> ObtenirVoisins(Node noeud)
    {
        List<Node> voisins = new List<Node>(8);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                Node voisin = ObtenirNoeud(noeud.gridX + dx, noeud.gridY + dy);
                if (voisin != null) voisins.Add(voisin);
            }
        }

        return voisins;
    }

    /// <summary>Remet à zéro les coûts de tous les noeuds (avant chaque recherche A*).</summary>
    public void ReinitialisationCouts()
    {
        foreach (Node n in _grille)
            n.Reinitialiser();
    }

    // ── Debug Gizmos ──────────────────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (_grille == null) return;

        foreach (Node n in _grille)
        {
            Gizmos.color = n.walkable
                ? new Color(0f, 1f, 0f, 0.15f)
                : new Color(1f, 0f, 0f, 0.4f);
            Gizmos.DrawCube(n.worldPosition, Vector3.one * (tailleCellule * 0.9f));
        }
    }
}
