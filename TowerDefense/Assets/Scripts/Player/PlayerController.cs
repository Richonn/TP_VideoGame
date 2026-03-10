using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Paramètres")]
    [Tooltip("1 = clavier (ZQSD), 2 = manette Switch.")]
    public int playerNumber = 1;
    public float moveSpeed = 5f;

    [Header("Limites de la demi-map")]
    [Tooltip("Bornes à configurer selon la moitié de map du joueur.")]
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
        if (InputManager.Instance == null)
        {
            Debug.LogWarning($"[Joueur {playerNumber}] InputManager.Instance est NULL !");
            return;
        }

        _input = InputManager.Instance.GetInput(playerNumber);

        if (_input.PlaceTowerPressed)
            OnPlaceTower();

        if (_input.InteractPressed)
            OnInteract();
    }

    void FixedUpdate()
    {
        Deplacer();
    }

    private void Deplacer()
    {
        // _animator.SetBool("isRunning", true); > FIX when player stop running
        Vector2 nouvellePos = _rb.position
            + _input.MoveDirection * moveSpeed * Time.fixedDeltaTime;

        nouvellePos.x = Mathf.Clamp(nouvellePos.x, minX, maxX);
        nouvellePos.y = Mathf.Clamp(nouvellePos.y, minY, maxY);

        _rb.MovePosition(nouvellePos);

    }

    private void OnPlaceTower()
    {
        // TODO: déclencher le placement de tour (TowerPlacementManager)
        Debug.Log($"[Joueur {playerNumber}] Placer une tour en {transform.position}");
    }

    private void OnInteract()
    {
        // TODO: améliorer / vendre la tour sous le curseur
        Debug.Log($"[Joueur {playerNumber}] Interagir");
    }
}
