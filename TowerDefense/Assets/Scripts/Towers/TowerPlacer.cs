using UnityEngine;

/// <summary>
/// Gère le placement de tours par un joueur.
///
/// - Affiche un curseur coloré sur la case sous le joueur
///   (vert = placement valide, rouge = invalide)
/// - Appuyer sur E / buttonSouth pour poser la tour
/// - La tour est posée uniquement pendant la phase de préparation
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

    [Header("Limites demi-map (doit correspondre à PlayerController)")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX =   0f;

    // ── Curseur ───────────────────────────────────────────────────────────────
    private GameObject     _cursor;
    private SpriteRenderer _cursorRenderer;

    private static readonly Color COULEUR_VALIDE   = new Color(0f, 1f, 0f, 0.45f);
    private static readonly Color COULEUR_INVALIDE = new Color(1f, 0f, 0f, 0.45f);

    // ── État ──────────────────────────────────────────────────────────────────
    private Node _noeudCible;
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

        // Sprite 1×1 blanc créé en mémoire
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);

        _cursorRenderer = _cursor.AddComponent<SpriteRenderer>();
        _cursorRenderer.sprite       = sprite;
        _cursorRenderer.sortingOrder = 5;

        float taille = GridManager.Instance != null ? GridManager.Instance.TailleCellule : 1f;
        _cursor.transform.localScale = Vector3.one * taille * 0.95f;
    }

    private void MettreAJourCurseur()
    {
        if (GridManager.Instance == null) return;

        _noeudCible = GridManager.Instance.MondeVersNoeud(transform.position);
        if (_noeudCible == null) return;

        _cursor.transform.position = _noeudCible.worldPosition;

        _placementValide = VerifierValidite(_noeudCible);
        _cursorRenderer.color = _placementValide ? COULEUR_VALIDE : COULEUR_INVALIDE;
    }

    // ── Validation ────────────────────────────────────────────────────────────
    private bool VerifierValidite(Node noeud)
    {
        if (noeud == null || !noeud.walkable) return false;

        // Case dans la demi-map du joueur
        if (noeud.worldPosition.x < minX || noeud.worldPosition.x > maxX) return false;

        // Ressources suffisantes
        if (ResourceManager.Instance != null &&
            !ResourceManager.Instance.AAssezDeRessources(playerIndex, coutTour)) return false;

        return true;
    }

    // ── Placement ─────────────────────────────────────────────────────────────
    private void TenterPlacement()
    {
        if (!_placementValide || _noeudCible == null || towerPrefab == null) return;

        // Déduire les ressources
        if (ResourceManager.Instance != null &&
            !ResourceManager.Instance.Depenser(playerIndex, coutTour)) return;

        // Instancier la tour
        Instantiate(towerPrefab, _noeudCible.worldPosition, Quaternion.identity);

        // Mettre à jour la grille → les ennemis recalculent leur chemin
        GridManager.Instance?.MettreAJourGrille();
    }
}
