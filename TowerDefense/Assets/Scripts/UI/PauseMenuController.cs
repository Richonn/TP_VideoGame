using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Menu de pause déclenché par Escape.
/// Panneaux : Principal → Controls (rebind) / Settings (stub)
/// Créé entièrement par code si non configuré dans l'Inspector.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    // ── Références (auto-créées si null) ─────────────────────────────────────
    [SerializeField] private Canvas pauseMenuCanvas;

    private GameObject _panelPrincipal;
    private GameObject _panelControls;

    private bool _isPaused;
    public bool IsPaused => _isPaused;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (pauseMenuCanvas == null)
            BuildUI();

        pauseMenuCanvas.enabled = false;
    }

    // ── Pause / Resume ────────────────────────────────────────────────────────
    public void TogglePause() { if (_isPaused) Resume(); else Pause(); }

    public void Pause()
    {
        if (_isPaused) return;
        _isPaused        = true;
        Time.timeScale   = 0f;
        MontrerPrincipal();
        pauseMenuCanvas.enabled = true;
        InputManager.Instance?.SetPlayerInputEnabled(1, false);
        InputManager.Instance?.SetPlayerInputEnabled(2, false);
    }

    public void Resume()
    {
        if (!_isPaused) return;
        _isPaused        = false;
        Time.timeScale   = 1f;
        pauseMenuCanvas.enabled = false;
        InputManager.Instance?.SetPlayerInputEnabled(1, true);
        InputManager.Instance?.SetPlayerInputEnabled(2, true);
    }

    // ── Navigation entre panneaux ─────────────────────────────────────────────
    private void MontrerPrincipal()
    {
        _panelPrincipal?.SetActive(true);
        _panelControls?.SetActive(false);
    }

    private void MontrerControls()
    {
        _panelPrincipal?.SetActive(false);
        _panelControls?.SetActive(true);
    }

    // ── Actions boutons ───────────────────────────────────────────────────────
    private void OnSettings()   { /* stub */ }

    private void OnControls()   { MontrerControls(); }

    private void OnRestart()
    {
        Time.timeScale = 1f;
        _isPaused = false;
        if (GameManager.Instance != null) GameManager.Instance.StartGame();
        else SceneManager.LoadScene("Game");
    }

    private void OnBackToMenu()
    {
        Time.timeScale = 1f;
        _isPaused = false;
        if (GameManager.Instance != null) GameManager.Instance.ReturnToMenu();
        else SceneManager.LoadScene("MainMenu");
    }

    private void OnQuit()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ── Construction de l'UI ──────────────────────────────────────────────────
    private void BuildUI()
    {
        // Canvas racine
        GameObject canvasGO = new GameObject("PauseCanvas");
        pauseMenuCanvas = canvasGO.AddComponent<Canvas>();
        pauseMenuCanvas.renderMode      = RenderMode.ScreenSpaceOverlay;
        pauseMenuCanvas.sortingOrder    = 100;
        canvasGO.AddComponent<GraphicRaycaster>();
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        // Fond semi-transparent (ne bloque pas les raycasts)
        GameObject fond = new GameObject("Fond");
        fond.transform.SetParent(canvasGO.transform, false);
        Image fondImg = fond.AddComponent<Image>();
        fondImg.color = new Color(0f, 0f, 0f, 0.55f);
        fondImg.raycastTarget = false;      // ← le fond ne capte pas les clics
        Etirer(fond.GetComponent<RectTransform>());

        // Panel principal
        _panelPrincipal = BuildPanelPrincipal(canvasGO);

        // Panel Controls (caché au départ)
        _panelControls  = BuildPanelControls(canvasGO);
        _panelControls.SetActive(false);
    }

    // ── Panel principal ───────────────────────────────────────────────────────
    private GameObject BuildPanelPrincipal(GameObject parent)
    {
        GameObject panel = CréerPanel(parent, 500, 620);

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing              = 12;
        vlg.padding              = new RectOffset(30, 30, 30, 30);
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment         = TextAnchor.UpperCenter;

        AjouterTitre(panel, "PAUSE");
        CréerBouton(panel, "Resume",       Resume);
        CréerBouton(panel, "Settings",     OnSettings);
        CréerBouton(panel, "Controls",     OnControls);
        CréerBouton(panel, "Restart",      OnRestart);
        CréerBouton(panel, "Back to Menu", OnBackToMenu);
        CréerBouton(panel, "Quit",         OnQuit);

        return panel;
    }

    // ── Panel Controls ────────────────────────────────────────────────────────
    private GameObject BuildPanelControls(GameObject parent)
    {
        GameObject panel = CréerPanel(parent, 700, 620);

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing              = 8;
        vlg.padding              = new RectOffset(30, 30, 20, 20);
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment         = TextAnchor.UpperCenter;

        AjouterTitre(panel, "CONTROLS");

        // Zone de rebinding (prend tout l'espace disponible)
        GameObject zone = new GameObject("ZoneRebind");
        zone.transform.SetParent(panel.transform, false);
        LayoutElement zoneLE = zone.AddComponent<LayoutElement>();
        zoneLE.flexibleHeight  = 1;
        zoneLE.flexibleWidth   = 1;

        // VLG dans la zone
        VerticalLayoutGroup zoneVLG = zone.AddComponent<VerticalLayoutGroup>();
        zoneVLG.spacing               = 6;
        zoneVLG.childForceExpandWidth  = true;
        zoneVLG.childForceExpandHeight = false;

        // Ajouter le composant de rebinding sur la zone
        ControlsRebindingUI rebindUI = zone.AddComponent<ControlsRebindingUI>();
        rebindUI.SetRebindingContainer(zone.transform);

        // Boutons bas
        GameObject bas = new GameObject("Bas");
        bas.transform.SetParent(panel.transform, false);
        HorizontalLayoutGroup basHLG = bas.AddComponent<HorizontalLayoutGroup>();
        basHLG.spacing               = 20;
        basHLG.childForceExpandWidth  = false;
        basHLG.childForceExpandHeight = false;
        basHLG.childAlignment         = TextAnchor.MiddleCenter;
        LayoutElement basLE = bas.AddComponent<LayoutElement>();
        basLE.preferredHeight = 60;
        basLE.flexibleWidth   = 1;

        CréerBoutonFixe(bas, "Reset Defaults", new Color(0.6f, 0.15f, 0.15f, 1f), 220, () =>
        {
            KeyBindingManager.Instance?.ResetAllBindings();
            rebindUI.RefreshUI();
        });
        CréerBoutonFixe(bas, "Back", new Color(0.15f, 0.45f, 0.15f, 1f), 160, MontrerPrincipal);

        return panel;
    }

    // ── Helpers de construction ───────────────────────────────────────────────
    private GameObject CréerPanel(GameObject parent, float largeur, float hauteur)
    {
        GameObject go = new GameObject("Panel");
        go.transform.SetParent(parent.transform, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.12f, 0.12f, 0.18f, 1f);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin  = new Vector2(0.5f, 0.5f);
        rt.anchorMax  = new Vector2(0.5f, 0.5f);
        rt.pivot      = new Vector2(0.5f, 0.5f);
        rt.sizeDelta  = new Vector2(largeur, hauteur);
        rt.anchoredPosition = Vector2.zero;
        return go;
    }

    private void AjouterTitre(GameObject parent, string texte)
    {
        GameObject go = new GameObject("Titre");
        go.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = texte;
        tmp.fontSize  = 46;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 70;
        le.flexibleWidth   = 1;
    }

    private void CréerBouton(GameObject parent, string texte, UnityEngine.Events.UnityAction callback)
    {
        GameObject go = new GameObject(texte);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = new Color(0.2f, 0.38f, 0.65f, 1f);
        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.3f, 0.52f, 0.85f, 1f);
        cb.pressedColor     = new Color(0.12f, 0.28f, 0.5f, 1f);
        btn.colors = cb;
        btn.onClick.AddListener(callback);

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 58;
        le.flexibleWidth   = 1;

        AjouterTexteBouton(go, texte, 28);
    }

    private void CréerBoutonFixe(GameObject parent, string texte, Color couleur, float largeur, UnityEngine.Events.UnityAction callback)
    {
        GameObject go = new GameObject(texte);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = couleur;
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(callback);

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredWidth  = largeur;
        le.preferredHeight = 56;

        AjouterTexteBouton(go, texte, 22);
    }

    private void AjouterTexteBouton(GameObject parent, string texte, float taille)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = texte;
        tmp.fontSize  = taille;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        // Ancres étirées → le texte remplit le bouton
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;
    }

    private static void Etirer(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
