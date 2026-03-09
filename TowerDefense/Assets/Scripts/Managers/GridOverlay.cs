using UnityEngine;

/// <summary>
/// Affiche un quadrillage léger sur la map en traçant les lignes de la grille.
/// Génère un LineRenderer par ligne (horizontale + verticale).
///
/// Attacher sur n'importe quel GameObject de la scène Game.
/// Dépend de GridManager (doit être présent dans la scène).
/// </summary>
public class GridOverlay : MonoBehaviour
{
    [Header("Apparence")]
    [SerializeField] private Color couleur      = new Color(1f, 1f, 1f, 0.12f);
    [SerializeField] private float largeurLigne = 0.03f;
    [SerializeField] private int   sortingOrder = 1;   // au-dessus du fond, sous les entités

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Start()
    {
        if (GridManager.Instance == null)
        {
            Debug.LogWarning("[GridOverlay] GridManager introuvable.");
            return;
        }

        GenererGrille();
    }

    // ── Génération ────────────────────────────────────────────────────────────
    private void GenererGrille()
    {
        GridManager gm = GridManager.Instance;
        float tc = gm.TailleCellule;

        // Coin bas-gauche de la grille calculé depuis le centre de la cellule (0,0)
        Node n00 = gm.ObtenirNoeud(0, 0);
        if (n00 == null) return;
        Vector2 origine = n00.worldPosition - new Vector2(tc * 0.5f, tc * 0.5f);

        float largeurTotale = gm.Largeur * tc;
        float hauteurTotale = gm.Hauteur * tc;

        // Matériau partagé
        Material mat = new Material(Shader.Find("Sprites/Default"));

        // Lignes verticales
        for (int x = 0; x <= gm.Largeur; x++)
        {
            float posX = origine.x + x * tc;
            CreerLigne(
                new Vector3(posX, origine.y,                0f),
                new Vector3(posX, origine.y + hauteurTotale, 0f),
                mat
            );
        }

        // Lignes horizontales
        for (int y = 0; y <= gm.Hauteur; y++)
        {
            float posY = origine.y + y * tc;
            CreerLigne(
                new Vector3(origine.x,               posY, 0f),
                new Vector3(origine.x + largeurTotale, posY, 0f),
                mat
            );
        }
    }

    private void CreerLigne(Vector3 debut, Vector3 fin, Material mat)
    {
        GameObject go = new GameObject("GridLine");
        go.transform.SetParent(transform);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace  = true;
        lr.positionCount  = 2;
        lr.startWidth     = largeurLigne;
        lr.endWidth       = largeurLigne;
        lr.material       = mat;
        lr.startColor     = couleur;
        lr.endColor       = couleur;
        lr.sortingOrder   = sortingOrder;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.SetPosition(0, debut);
        lr.SetPosition(1, fin);
    }
}
