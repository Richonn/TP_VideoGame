using UnityEngine;

/// <summary>
/// Affiche un cercle en pointillés représentant la portée de la tour.
/// Génère N arcs courts (LineRenderer) disposés en cercle.
///
/// Attacher sur le même prefab que Tower.
/// </summary>
[RequireComponent(typeof(Tower))]
public class TowerRangeDisplay : MonoBehaviour
{
    [Header("Apparence")]
    [SerializeField] private Color couleur      = new Color(1f, 1f, 0.2f, 0.5f);
    [SerializeField] private float largeurLigne = 0.06f;

    [Header("Pointillés")]
    [SerializeField] private int   nombreTirets   = 20;
    [SerializeField] [Range(0.1f, 0.9f)]
                     private float ratioTiret     = 0.55f;  // part du tiret vs espace
    [SerializeField] private int   pointsParTiret = 8;      // résolution de chaque arc

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Start()
    {
        CreerCerclePointille();
    }

    // ── Génération ────────────────────────────────────────────────────────────
    private void CreerCerclePointille()
    {
        float rayon        = GetComponent<Tower>().portee;
        float anglePeriode = 360f / nombreTirets;
        float angleTiret   = anglePeriode * ratioTiret;

        // Un seul matériau partagé entre tous les tirets
        Material mat = new Material(Shader.Find("Sprites/Default"));

        for (int i = 0; i < nombreTirets; i++)
        {
            GameObject go = new GameObject($"RangeDash_{i}");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            LineRenderer lr = go.AddComponent<LineRenderer>();
            ConfigurerLR(lr, mat);

            // Calculer les positions de l'arc
            float angleDebut = i * anglePeriode;
            float angleFin   = angleDebut + angleTiret;

            lr.positionCount = pointsParTiret;
            for (int j = 0; j < pointsParTiret; j++)
            {
                float t = j / (float)(pointsParTiret - 1);
                float a = Mathf.Lerp(angleDebut, angleFin, t) * Mathf.Deg2Rad;
                lr.SetPosition(j, new Vector3(Mathf.Cos(a) * rayon, Mathf.Sin(a) * rayon, 0f));
            }
        }
    }

    private void ConfigurerLR(LineRenderer lr, Material mat)
    {
        lr.useWorldSpace = false;
        lr.loop          = false;
        lr.startWidth    = largeurLigne;
        lr.endWidth      = largeurLigne;
        lr.material      = mat;
        lr.startColor    = couleur;
        lr.endColor      = couleur;
        lr.sortingOrder  = 4;   // sous le curseur (5), au-dessus du fond
    }
}
