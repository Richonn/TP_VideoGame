using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 14f;
    [SerializeField] private SFXType impactSFX = SFXType.TowerImpact;

    private Vector3 _start;
    private Vector3 _end;
    private float _totalDistance;
    private float _traveled;
    private bool _active;
    private Vector3 _baseScale;
    private bool _baseScaleCached;
    
    private System.Action _onArriveCallback;

    void Awake()
    {
        if (!_baseScaleCached)
        {
            _baseScale = transform.localScale;
            _baseScaleCached = true;
        }
    }

    public void Launch(Vector3 from, Vector3 to, System.Action onArrive = null)
    {
        _start = from;
        _end = to;
        _totalDistance = Mathf.Max(0.01f, Vector3.Distance(from, to));
        _traveled = 0f;
        transform.position = from;

        Vector3 dir = to - from;
        if (dir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        transform.localScale = _baseScale;

        _onArriveCallback = onArrive;
        _active = true;
    }

    void Update()
    {
        if (!_active) return;

        _traveled += speed * Time.deltaTime;
        float t = Mathf.Clamp01(_traveled / _totalDistance);
        transform.position = Vector3.Lerp(_start, _end, t);

        if (t >= 1f) OnArrive();
    }

    private void OnArrive()
    {
        _active = false;
        AudioManager.Instance?.PlaySFX(impactSFX, _end);
        _onArriveCallback?.Invoke();
        Destroy(gameObject);
    }
}
