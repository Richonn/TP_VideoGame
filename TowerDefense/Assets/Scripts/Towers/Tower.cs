using UnityEngine;

/// <summary>
/// Tour placée sur la grille.
/// Détecte les ennemis dans sa portée et leur inflige des dégâts
/// à intervalle régulier pendant la phase de défense.
/// </summary>
public class Tower : MonoBehaviour
{
    [Header("Stats")]
    public int  cout        = 50;
    public float portee     = 3f;
    public int   degats     = 2;
    public float cadence    = 1f;   // tirs par seconde

    [Header("Ciblage")]
    [SerializeField] private LayerMask layerEnnemis;

    private float _timer;

    void Update()
    {
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Defense) return;

        _timer += Time.deltaTime;
        if (_timer >= 1f / cadence)
        {
            _timer = 0f;
            TirerSurEnnemi();
        }
    }

    private void TirerSurEnnemi()
    {
        Collider2D[] ennemis = Physics2D.OverlapCircleAll(transform.position, portee, layerEnnemis);
        if (ennemis.Length == 0) return;

        // Cibler l'ennemi le plus avancé (index waypoint le plus élevé)
        EnemyAI cible = null;
        int maxWaypoint = -1;

        foreach (Collider2D col in ennemis)
        {
            EnemyAI ai = col.GetComponent<EnemyAI>();
            if (ai != null && ai.IndexWaypoint > maxWaypoint)
            {
                maxWaypoint = ai.IndexWaypoint;
                cible = ai;
            }
        }

        cible?.PrendreDegats(degats);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, portee);
    }
}
