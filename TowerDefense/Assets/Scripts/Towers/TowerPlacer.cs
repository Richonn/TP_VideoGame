using UnityEngine;

/// <summary>
/// Gère le placement de tours 2×2 cellules par un joueur.
///
/// - Affiche un curseur coloré sur le bloc 2×2 sous le joueur
///   (vert = placement valide, rouge = invalide)
/// - Appuyer sur E / buttonSouth pour poser la tour
/// - La tour est posée uniquement pendant la phase de préparation
/// - Le placement est limité à la demi-map du joueur (minX/maxX)
///   mais le mouvement est libre (géré dans PlayerController)
///
/// Attacher ce script sur le GameObject joueur (même que PlayerController).
/// </summary>
public class TowerPlacer : MonoBehaviour
{
    [Header("Joueur")]
    [SerializeField] private int playerIndex = 1;

    [Header("Tour à placer")]
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private int        coutTour = 50;

    [Header("Limites de placement (demi-map du joueur)")]
    [SerializeField] private float minX = -20f;
    [SerializeField] private float maxX =   0f;

    // ── Curseur ───────────────────────────────────────────────────────────────
    private GameObject     _cursor;
    private SpriteRenderer _cursorRenderer;

    private static readonly Color COULEUR_VALIDE   = new Color(0f, 1f, 0f, 0.45f);
    private static readonly Color COULEUR_INVALIDE = new Color(1f, 0f, 0f, 0.45f);

    // ── État ──────────────────────────────────────────────────────────────────
    private Node _noeudAncre;   // coin bas-gauche du bloc 2×2
    private bool _placementValide;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Start()
    {
        CreerCurseur();
    }

    void Update()
    {
        bool enPreparation = GameManager.Instance?.CurrentState == GameManager.GameState.Preparation;
        _cursor.SetActive(enPreparation);

        if (!enPreparation) return;

        MettreAJourCurseur();

        if (InputManager.Instance != null &&
            InputManager.Instance.GetInput(playerIndex).PlaceTowerPressed)
        {
            TenterPlacement();
        }
    }

    // ── Curseur ───────────────────────────────────────────────────────────────
    private void CreerCurseur()
    {
        _cursor = new GameObject($"Cursor_P{playerIndex}");

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);

        _cursorRenderer = _cursor.AddComponent<SpriteRenderer>();
        _cursorRenderer.sprite       = sprite;
        _cursorRenderer.sortingOrder = 5;

        // Curseur 2×2 cellules
        float taille = GridManager.Instance != null ? GridManager.Instance.TailleCellule : 1f;
        _cursor.transform.localScale = Vector3.one * taille * 2f * 0.95f;
    }

    private void MettreAJourCurseur()
    {
        if (GridManager.Instance == null) return;

        _noeudAncre = GridManager.Instance.MondeVersNoeud(transform.position);
        if (_noeudAncre == null) return;

        // Centrer le curseur sur le bloc 2×2 (ancre = coin bas-gauche)
        float tc = GridManager.Instance.TailleCellule;
        Vector2 centreBloc = _noeudAncre.worldPosition + new Vector2(tc * 0.5f, tc * 0.5f);
        _cursor.transform.position = centreBloc;

        _placementValide = VerifierValidite(_noeudAncre);
        _cursorRenderer.color = _placementValide ? COULEUR_VALIDE : COULEUR_INVALIDE;
    }

    // ── Validation ────────────────────────────────────────────────────────────
    private bool VerifierValidite(Node ancre)
    {
        if (ancre == null) return false;

        Node[] bloc = ObtenirBlocNodes(ancre);
        if (bloc == null) return false;

        foreach (Node n in bloc)
        {
            if (n == null || !n.walkable) return false;
            // Restriction de placement à la demi-map du joueur
            if (n.worldPosition.x < minX || n.worldPosition.x > maxX) return false;
        }

        if (ResourceManager.Instance != null &&
            !ResourceManager.Instance.AAssezDeRessources(playerIndex, coutTour)) return false;

        return true;
    }

    /// <summary>Retourne les 4 noeuds du bloc 2×2 ancré en bas-gauche, ou null si hors grille.</summary>
    private Node[] ObtenirBlocNodes(Node ancre)
    {
        GridManager gm = GridManager.Instance;
        Node n00 = gm.ObtenirNoeud(ancre.gridX,     ancre.gridY);
        Node n10 = gm.ObtenirNoeud(ancre.gridX + 1, ancre.gridY);
        Node n01 = gm.ObtenirNoeud(ancre.gridX,     ancre.gridY + 1);
        Node n11 = gm.ObtenirNoeud(ancre.gridX + 1, ancre.gridY + 1);

        if (n00 == null || n10 == null || n01 == null || n11 == null) return null;
        return new Node[] { n00, n10, n01, n11 };
    }

    // ── Placement ─────────────────────────────────────────────────────────────
    private void TenterPlacement()
    {
        if (!_placementValide || _noeudAncre == null || towerPrefab == null) return;

        if (ResourceManager.Instance != null &&
            !ResourceManager.Instance.Depenser(playerIndex, coutTour)) return;

        // Placer la tour au centre du bloc 2×2
        float tc = GridManager.Instance.TailleCellule;
        Vector2 centreBloc = _noeudAncre.worldPosition + new Vector2(tc * 0.5f, tc * 0.5f);
        Instantiate(towerPrefab, centreBloc, Quaternion.identity);

        GridManager.Instance?.MettreAJourGrille();
    }
}
