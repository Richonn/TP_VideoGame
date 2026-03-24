using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public int playerNumber = 1;
    public float moveSpeed = 5f;

    [Header("Movement Bounds")]
    public float minX = -20f;
    public float maxX = 20f;
    public float minY = -10f;
    public float maxY = 10f;

    [Header("UI")]
    [SerializeField] private TowerInteractUI interactUI;

    private Rigidbody2D _rb;
    private InputManager.PlayerInputData _input;
    private Animator _animator;
    private Tower _nearbyTower;
    private bool _upgradeMenuOpen = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        interactUI?.Init(transform);
    }

    void Update()
    {
        if (InputManager.Instance == null) return;

        _input = InputManager.Instance.GetInput(playerNumber);

        if (_input.PlaceTowerPressed) OnPlaceTower();
        if (_input.InteractPressed) OnInteract();
        HandleTowerInteraction();
    }

    void FixedUpdate()
    {
        Move();
    }

    private void HandleTowerInteraction()
    {
        Tower nearest = FindNearbyTower();

        if (_upgradeMenuOpen)
        {
            if (nearest == null || _input.InteractPressed)
            {
                _upgradeMenuOpen = false;
                interactUI?.HideAll();
            }
            return;
        }

        if (nearest != null)
        {
            _nearbyTower = nearest;
            interactUI?.ShowPrompt(nearest);

            if (_input.InteractPressed)
            {
                _upgradeMenuOpen = true;
                interactUI?.OpenMenu(nearest);
            }
        }
        else
        {
            _nearbyTower = null;
            interactUI?.HideAll();
        }
    }

    private Tower FindNearbyTower()
    {
        Tower[] towers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
        foreach (Tower t in towers)
        {
            if (t.IsPlayerInRange(transform.position))
                return t;
        }
        return null;
    }

    private void Move()
    {
        Vector2 newPos = _rb.position + _input.MoveDirection * moveSpeed * Time.fixedDeltaTime;
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
        
        // Check if the new position is on a walkable grid cell
        if (GridManager.Instance != null) {
            Node targetNode = GridManager.Instance.WorldToNode(newPos);
            if (targetNode == null || !targetNode.walkable) {
                return; // Don't move if the cell is not walkable
            }
        }
        // Separately block the player from walking onto the base
        if (IsBaseAtPosition(newPos))
            return;

        _rb.MovePosition(newPos);
    }

    private bool IsBaseAtPosition(Vector2 pos)
    {
        Collider2D hit = Physics2D.OverlapCircle(pos, 0.1f);
        return hit != null && hit.CompareTag("Base");
    }

    private void OnPlaceTower()
    {
        Debug.Log($"[Player {playerNumber}] Place tower at {transform.position}");
    }

    private void OnInteract()
    {
        Debug.Log($"[Player {playerNumber}] Interact");
    }
}
