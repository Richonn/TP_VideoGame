using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Stats")]
    public int cost = 50;
    public float range = 3f;
    public int damage = 2;
    public float fireRate = 1f;

    [Header("Interaction")]
    public float interactRadius = 1.5f;
    [SerializeField] private int rangeUpgradeCost = 100;
    [SerializeField] private float rangeUpgradeAmount = 0.5f;
    [SerializeField] private int damageUpgradeAmount = 1;
    [SerializeField] private ParticleSystem upgradeParticles;


    [Header("Targeting")]
    [SerializeField] private LayerMask enemyLayer;

    private float _timer;
    private Animator _animator;

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Defense) return;

        _timer += Time.deltaTime;
        if (_timer >= 1f / fireRate)
        {
            _timer = 0f;
            ShootAtEnemy();
        }
    }

    public bool IsPlayerInRange(Vector2 playerPos)
    {
        return Vector2.Distance(transform.position, playerPos) <= interactRadius;
    }

    public bool TryUpgradeRange(int playerIndex)
    {
        if (ResourceManager.Instance == null) return false;
        if (!ResourceManager.Instance.HasEnoughResources(playerIndex, rangeUpgradeCost)) return false;

        ResourceManager.Instance.Spend(playerIndex, rangeUpgradeCost);
        range += rangeUpgradeAmount;
        damage += damageUpgradeAmount;

        GetComponent<TowerRangeDisplay>()?.RefreshDisplay();

        AudioManager.Instance?.PlaySFX(SFXType.TowerUpgrade, transform.position);
        upgradeParticles.Play();

        return true;
    }

    private void ShootAtEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        if (enemies.Length == 0) return;

        EnemyAI target = null;
        int maxWaypoint = -1;

        foreach (Collider2D col in enemies)
        {
            EnemyAI ai = col.GetComponent<EnemyAI>();
            if (ai != null && ai.WaypointIndex > maxWaypoint)
            {
                maxWaypoint = ai.WaypointIndex;
                target = ai;
            }
        }

        if (target == null) return;

        _animator?.SetTrigger("fire");
        AudioManager.Instance?.PlaySFX(SFXType.TowerShoot, transform.position);

        Vector3 enemyPos = target.transform.position;
        target.TakeDamage(damage);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
