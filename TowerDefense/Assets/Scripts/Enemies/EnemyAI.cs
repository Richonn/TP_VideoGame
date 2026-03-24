using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public enum EnemyType { Rush, Tank, Flanker }
    public enum EnemyState { WAITING, MOVING, ARRIVED, BLOCKED, DEAD}

    [Header("Type")]
    [SerializeField] public EnemyType type = EnemyType.Rush;

    [Header("Base Stats")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private int maxHP = 3;
    [SerializeField] private int baseDamage = 1;

    [Header("Navigation")]
    [SerializeField] private float waypointTolerance = 0.15f;
    [SerializeField] private float attackCooldown = 1f; // attack every 1 second

    private float _attackTimer = 0f;
    private BaseController _baseController; // cache it

    public static event Action OnEnemyDied; // enemy state dead

    private int _currentHP;
    private int _goldReward;
    private List<Vector2> _path;
    private int _waypointIndex;
    private Transform _baseTarget;
    private EnemyState _state = EnemyState.WAITING;
    private Animator _animator;

    public int WaypointIndex => _waypointIndex;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        ConfigureByType();
        ApplyDifficultyModifiers();
        _currentHP = maxHP;
    }

    void Start()
    {
        GameObject baseObj = GameObject.FindWithTag("Base");
        if (baseObj != null)
            _baseTarget = baseObj.transform;
        else
            Debug.LogWarning("[EnemyAI] No GameObject tagged 'Base' found!");

        RecalculatePath();
        GridManager.OnGridUpdated += RecalculatePath;
    }

    void OnDestroy()
    {
        GridManager.OnGridUpdated -= RecalculatePath;
    }

    void Update()
    {
        if (state == EnemyState.ARRIVED)
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _baseController?.TakeDamage(baseDamage);
                _attackTimer = attackCooldown;
            }
            return;
        }

        if (GameManager.Instance?.CurrentState != GameManager.GameState.Defense) {
            state = EnemyState.WAITING;
            return;
        }
        FollowPath();
    }

    private void ApplyDifficultyModifiers()
    {
        if (GameManager.Instance?.Difficulty != GameManager.DifficultyLevel.Hard) return;
        maxHP = Mathf.RoundToInt(maxHP * 1.75f);
        speed *= 1.25f;
    }

    private void ConfigureByType()
    {
        switch (type)
        {
            case EnemyType.Rush:
                speed = 3.5f;
                maxHP = 2;
                baseDamage = 1;
                _goldReward = 10;
                break;
            case EnemyType.Tank:
                speed = 1f;
                maxHP = 10;
                baseDamage = 3;
                _goldReward = 25;
                break;
            case EnemyType.Flanker:
                speed = 2.5f;
                maxHP = 4;
                baseDamage = 1;
                _goldReward = 15;
                break;
        }
    }

    private EnemyState state
    {
        get => _state;
        set
        {
            if (_state == value) return;
            _state = value;
            UpdateAnimator();
        }
    }

    private void UpdateAnimator()
    {
        int stateAnim = _state switch // idle = 0, run = 1, attack = 2
        {
            EnemyState.WAITING  => 0,
            EnemyState.MOVING   => 1,
            EnemyState.ARRIVED  => 2,
            EnemyState.BLOCKED  => 0,
            EnemyState.DEAD     => 0,
            _                   => 0
        };

        _animator?.SetInteger("stateAnim", stateAnim);
    }

    public void RecalculatePath()
    {
        if (_baseTarget == null || AStarPathfinder.Instance == null) return;

        int penalty = type == EnemyType.Flanker ? 50 : 0;

        List<Vector2> newPath = AStarPathfinder.Instance.FindPath(
            transform.position,
            _baseTarget.position,
            penalty
        );

        if (newPath != null)
        {
            _path = newPath;
            _waypointIndex = 0;
        }
        else
        {
            state = EnemyState.BLOCKED;
            Debug.LogWarning($"[EnemyAI] {gameObject.name}: {state}");
        }
    }

    private void FollowPath()
    {
        if (_path == null || _waypointIndex >= _path.Count) return;

        state = EnemyState.MOVING;
        Vector2 destination = _path[_waypointIndex];
        transform.position = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, destination) < waypointTolerance)
            _waypointIndex++;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Base"))
        {
            state = EnemyState.ARRIVED;
            _baseController = other.GetComponent<BaseController>();
            _baseController?.TakeDamage(baseDamage); // first hit
            _attackTimer = attackCooldown;
        }
    }

    public void TakeDamage(int damage)
    {
        _currentHP -= damage;
        if (_currentHP <= 0)
            Die();
    }

    private void Die()
    {
        if (ResourceManager.Instance != null)
        {
            state = EnemyState.DEAD;
            ResourceManager.Instance.Add(1, _goldReward);
            ResourceManager.Instance.Add(2, _goldReward);
        }

        OnEnemyDied?.Invoke();
        Destroy(gameObject);
    }
}
