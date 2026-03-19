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

    private Rigidbody2D _rb;
    private InputManager.PlayerInputData _input;
    private Animator _animator;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (InputManager.Instance == null) return;

        _input = InputManager.Instance.GetInput(playerNumber);

        if (_input.PlaceTowerPressed) OnPlaceTower();
        if (_input.InteractPressed) OnInteract();
    }

    void FixedUpdate()
    {
        Move();
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
        
        _rb.MovePosition(newPos);
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
