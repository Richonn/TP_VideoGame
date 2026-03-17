using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public static MinimapController Instance { get; private set; }

    [SerializeField] private float displayHeight = 120f;
    [SerializeField] private int textureResolution = 256;

    private Camera _minimapCamera;
    private RenderTexture _renderTexture;
    private RectTransform _p1Dot;
    private RectTransform _p2Dot;
    private Transform _p1Transform;
    private Transform _p2Transform;

    private Vector2 _gridOrigin;
    private float _gridWorldWidth;
    private float _gridWorldHeight;
    private float _displayWidth;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        GridManager grid = GridManager.Instance;
        if (grid == null) { enabled = false; return; }

        _gridOrigin = grid.GridOrigin;
        _gridWorldWidth = grid.GridWorldWidth;
        _gridWorldHeight = grid.GridWorldHeight;

        float aspect = _gridWorldWidth / _gridWorldHeight;
        _displayWidth = displayHeight * aspect;

        int rtWidth = Mathf.RoundToInt(textureResolution * aspect);
        int rtHeight = textureResolution;

        SetupCamera(grid.GridWorldCenter, _gridWorldHeight * 0.5f, aspect, rtWidth, rtHeight);
        SetupUI(rtWidth, rtHeight);
        FindPlayers();
    }

    private void SetupCamera(Vector2 center, float orthoSize, float aspect, int rtWidth, int rtHeight)
    {
        GameObject camGO = new GameObject("MinimapCamera");
        camGO.transform.position = new Vector3(center.x, center.y, -10f);

        _minimapCamera = camGO.AddComponent<Camera>();
        _minimapCamera.orthographic = true;
        _minimapCamera.orthographicSize = orthoSize;
        _minimapCamera.aspect = aspect;
        _minimapCamera.nearClipPlane = 0.3f;
        _minimapCamera.farClipPlane = 20f;
        _minimapCamera.depth = -3;

        int uiLayer = LayerMask.NameToLayer("UI");
        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        _minimapCamera.cullingMask = ~((1 << uiLayer) | (1 << ignoreRaycastLayer));

        _renderTexture = new RenderTexture(rtWidth, rtHeight, 16);
        _minimapCamera.targetTexture = _renderTexture;
    }

    private void SetupUI(int rtWidth, int rtHeight)
    {
        GameObject canvasGO = new GameObject("MinimapCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        canvasGO.AddComponent<GraphicRaycaster>();
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        float border = 3f;
        float margin = 10f;

        GameObject bgGO = new GameObject("MinimapBG");
        bgGO.transform.SetParent(canvasGO.transform, false);
        Image bg = bgGO.AddComponent<Image>();
        bg.color = new Color(0.75f, 0.75f, 0.75f, 0.95f);
        bg.raycastTarget = false;
        SetBottomCenter(bgGO.GetComponent<RectTransform>(), _displayWidth + border * 2f, displayHeight + border * 2f, margin);

        GameObject mapGO = new GameObject("MinimapImage");
        mapGO.transform.SetParent(canvasGO.transform, false);
        RawImage rawImage = mapGO.AddComponent<RawImage>();
        rawImage.texture = _renderTexture;
        rawImage.raycastTarget = false;
        SetBottomCenter(mapGO.GetComponent<RectTransform>(), _displayWidth, displayHeight, margin + border);

        _p1Dot = CreateDot(mapGO.transform, new Color(0.2f, 0.55f, 1f, 1f));
        _p2Dot = CreateDot(mapGO.transform, new Color(1f, 0.3f, 0.3f, 1f));
    }

    private static void SetBottomCenter(RectTransform rt, float width, float height, float yOffset)
    {
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(0f, yOffset + height * 0.5f);
    }

    private static RectTransform CreateDot(Transform parent, Color color)
    {
        GameObject go = new GameObject("Dot");
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(8f, 8f);
        rt.anchoredPosition = Vector2.zero;
        return rt;
    }

    private void FindPlayers()
    {
        foreach (PlayerController p in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            if (p.playerNumber == 1) _p1Transform = p.transform;
            else if (p.playerNumber == 2) _p2Transform = p.transform;
        }
    }

    void Update()
    {
        if (_p1Transform == null || _p2Transform == null) FindPlayers();
        UpdateDot(_p1Dot, _p1Transform);
        UpdateDot(_p2Dot, _p2Transform);
    }

    private void UpdateDot(RectTransform dot, Transform target)
    {
        if (dot == null || target == null) return;
        float u = Mathf.Clamp01((target.position.x - _gridOrigin.x) / _gridWorldWidth);
        float v = Mathf.Clamp01((target.position.y - _gridOrigin.y) / _gridWorldHeight);
        dot.anchoredPosition = new Vector2((u - 0.5f) * _displayWidth, (v - 0.5f) * displayHeight);
    }
}
