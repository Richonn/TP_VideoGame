using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère le menu de pause qui s'affiche quand un joueur appuie sur Escape.
/// Propose les options: Settings, Controls, Restart, Back to Menu, Quit
/// Crée automatiquement son UI si elle n'existe pas.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    [SerializeField] private Canvas pauseMenuCanvas;  // Le Canvas du menu de pause (auto-créé si nul)
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private Canvas settingsPanel;    // Panneaux de sous-menus (optionnel)
    [SerializeField] private GameObject controlsPanel;  // Panel de contrôles (pas un Canvas)
    
    private GameObject menuPanelGO;  // Référence au panel de menu principal

    private bool _isPaused = false;
    public bool IsPaused => _isPaused;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("[PauseMenuController] Instance initialisée dans Awake()");
    }

    void Start()
    {
        Debug.Log("[PauseMenuController] Start() appelé");
        
        // Créer automatiquement le UI si non configuré
        if (pauseMenuCanvas == null)
        {
            Debug.Log("[PauseMenu] Canvas non trouvé, création automatique...");
            CreateAutoUI();
        }
        else
        {
            Debug.Log("[PauseMenu] Canvas trouvé dans l'inspecteur");
        }

        // S'assurer que le menu est masqué au démarrage
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.enabled = false;

        if (settingsPanel != null)
            settingsPanel.enabled = false;

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        // Assigner les listeners aux boutons
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsPressed);

        if (controlsButton != null)
            controlsButton.onClick.AddListener(OnControlsPressed);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartPressed);

        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(OnBackToMenuPressed);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitPressed);
            
        Debug.Log("[PauseMenuController] Initialisation terminée");
    }

    /// <summary>
    /// Crée automatiquement le UI du menu de pause
    /// </summary>
    private void CreateAutoUI()
    {
        // Créer le Canvas principal
        GameObject canvasGO = new GameObject("PauseMenuCanvas");
        pauseMenuCanvas = canvasGO.AddComponent<Canvas>();
        pauseMenuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
        canvasRT.offsetMin = Vector2.zero;
        canvasRT.offsetMax = Vector2.zero;

        // Créer le fond semi-transparent
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.5f);
        RectTransform bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // Créer le panel du menu
        GameObject panelGO = new GameObject("MenuPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.15f, 0.15f, 0.2f, 1f);
        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(600, 550);
        
        // Garder la référence au panel du menu principal
        menuPanelGO = panelGO;

        // Ajouter une VerticalLayoutGroup
        VerticalLayoutGroup vlg = panelGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 15;
        vlg.padding = new RectOffset(30, 30, 40, 40);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Créer le titre
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(panelGO.transform, false);
        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "PAUSED";
        titleText.fontSize = 50;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.textWrappingMode = TextWrappingModes.NoWrap;
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.sizeDelta = new Vector2(540, 80);

        // LayoutElement pour le titre
        LayoutElement titleLE = titleGO.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 80;

        // Créer les boutons
        settingsButton = CreateButton(panelGO, "Settings", "Settings");
        controlsButton = CreateButton(panelGO, "Controls", "Controls");
        restartButton = CreateButton(panelGO, "Restart", "Restart");
        backToMenuButton = CreateButton(panelGO, "BackToMenu", "Back to Menu");
        quitButton = CreateButton(panelGO, "Quit", "Quit");

        Debug.Log("[PauseMenu] UI créée automatiquement avec succès.");
    }

    /// <summary>
    /// Crée un bouton avec styling
    /// </summary>
    private Button CreateButton(GameObject parent, string name, string text)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent.transform, false);
        
        Image btnImage = btnGO.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.4f, 0.7f, 1f);
        
        Button btn = btnGO.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.4f, 0.7f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.9f, 1f);
        colors.pressedColor = new Color(0.1f, 0.3f, 0.6f, 1f);
        btn.colors = colors;
        
        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.sizeDelta = new Vector2(540, 70);
        
        LayoutElement btnLE = btnGO.AddComponent<LayoutElement>();
        btnLE.preferredHeight = 70;

        // Ajouter le texte
        GameObject txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        TextMeshProUGUI txtTMP = txtGO.AddComponent<TextMeshProUGUI>();
        txtTMP.text = text;
        txtTMP.fontSize = 32;
        txtTMP.alignment = TextAlignmentOptions.Center;
        txtTMP.textWrappingMode = TextWrappingModes.NoWrap;
        RectTransform txtRT = txtGO.GetComponent<RectTransform>();
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        return btn;
    }

    /// <summary>
    /// Bascule l'état du pause (appelé par InputManager lors de la pression d'Escape)
    /// </summary>
    public void TogglePause()
    {
        if (_isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (_isPaused) return;

        _isPaused = true;
        Time.timeScale = 0f;  // Gèle le jeu

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.enabled = true;

        // Désactiver les inputs des joueurs
        if (InputManager.Instance != null)
        {
            InputManager.Instance.SetPlayerInputEnabled(1, false);
            InputManager.Instance.SetPlayerInputEnabled(2, false);
        }

        Debug.Log("[PauseMenu] Jeu en pause.");
    }

    public void Resume()
    {
        if (!_isPaused) return;

        _isPaused = false;
        Time.timeScale = 1f;  // Redémarre le jeu

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.enabled = false;

        // Fermer les sous-panneaux aussi
        if (settingsPanel != null)
            settingsPanel.enabled = false;

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        // Réactiver les inputs des joueurs
        if (InputManager.Instance != null)
        {
            InputManager.Instance.SetPlayerInputEnabled(1, true);
            InputManager.Instance.SetPlayerInputEnabled(2, true);
        }

        Debug.Log("[PauseMenu] Jeu repris.");
    }

    // ── Callbacks des boutons ─────────────────────────────────────────────────

    private void OnSettingsPressed()
    {
        Debug.Log("[PauseMenu] Bouton Settings pressé.");
        if (settingsPanel != null)
        {
            settingsPanel.enabled = !settingsPanel.enabled;
        }
        // TODO: Implémenter la logique des paramètres (volume, graphiques, etc.)
    }

    private void OnControlsPressed()
    {
        Debug.Log("[PauseMenu] Bouton Controls pressé.");
        
        // Si le panneau n'existe pas, le créer
        if (controlsPanel == null)
        {
            CreateControlsPanel();
        }
        
        // Masquer le menu principal
        if (menuPanelGO != null)
        {
            menuPanelGO.SetActive(false);
        }
        
        // Afficher le panneau de contrôles
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true);
        }
    }

    private void CreateControlsPanel()
    {
        // Créer un GameObject pour le panneau de contrôles (PAS un Canvas !)
        GameObject controlsPanelGO = new GameObject("ControlsPanel");
        controlsPanelGO.transform.SetParent(pauseMenuCanvas.transform, false);
        
        // Image de fond
        Image panelBG = controlsPanelGO.AddComponent<Image>();
        panelBG.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        RectTransform panelRT = controlsPanelGO.GetComponent<RectTransform>();
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // Container vertical principal
        GameObject mainContainerGO = new GameObject("MainContainer");
        mainContainerGO.transform.SetParent(controlsPanelGO.transform, false);
        
        VerticalLayoutGroup mainVLG = mainContainerGO.AddComponent<VerticalLayoutGroup>();
        mainVLG.spacing = 10;
        mainVLG.padding = new RectOffset(30, 30, 20, 20);
        mainVLG.childForceExpandWidth = true;
        mainVLG.childForceExpandHeight = true;

        RectTransform mainRT = mainContainerGO.GetComponent<RectTransform>();
        mainRT.offsetMin = Vector2.zero;
        mainRT.offsetMax = new Vector2(0, -80);  // Réserver l'espace pour les boutons

        // Content pour les contrôles (grandi pour occuper l'espace)
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(mainContainerGO.transform, false);
        
        VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.padding = new RectOffset(0, 0, 0, 0);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        LayoutElement contentLE = contentGO.AddComponent<LayoutElement>();
        contentLE.preferredWidth = -1;  // -1 = flexible
        contentLE.flexibleHeight = 1;   // Prend tout l'espace disponible

        RectTransform contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;

        // Créer un container pour les rebindings
        GameObject rebindContainerGO = new GameObject("RebindContainer");
        rebindContainerGO.transform.SetParent(contentGO.transform, false);
        
        VerticalLayoutGroup rebindVLG = rebindContainerGO.AddComponent<VerticalLayoutGroup>();
        rebindVLG.spacing = 10;
        rebindVLG.padding = new RectOffset(10, 10, 10, 10);
        rebindVLG.childForceExpandWidth = true;
        rebindVLG.childForceExpandHeight = false;

        RectTransform rebindRT = rebindContainerGO.GetComponent<RectTransform>();
        rebindRT.offsetMin = Vector2.zero;
        rebindRT.offsetMax = Vector2.zero;

        LayoutElement rebindLE = rebindContainerGO.AddComponent<LayoutElement>();
        rebindLE.preferredWidth = -1;  // -1 = flexible
        rebindLE.flexibleHeight = 1;   // Prend tout l'espace disponible

        // Désactiver le container pour que Start() ne soit pas appelé immédiatement
        rebindContainerGO.SetActive(false);

        // Ajouter le ControlsRebindingUI component au content (pas au container)
        ControlsRebindingUI rebindingUI = contentGO.AddComponent<ControlsRebindingUI>();
        rebindingUI.SetRebindingContainer(rebindContainerGO.transform);

        // Réactiver le container
        rebindContainerGO.SetActive(true);
        
        Debug.Log("[PauseMenu] ControlsRebindingUI assigné avec container: " + rebindContainerGO.name);

        // Panel pour les boutons (Bottom)
        GameObject bottomPanelGO = new GameObject("BottomPanel");
        bottomPanelGO.transform.SetParent(controlsPanelGO.transform, false);
        
        HorizontalLayoutGroup hlg = bottomPanelGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        LayoutElement bottomLE = bottomPanelGO.AddComponent<LayoutElement>();
        bottomLE.preferredHeight = 80;
        bottomLE.preferredWidth = -1;  // -1 = flexible

        RectTransform bottomRT = bottomPanelGO.GetComponent<RectTransform>();
        bottomRT.offsetMin = Vector2.zero;
        bottomRT.offsetMax = Vector2.zero;

        // Bouton Reset
        CreateControlsButton(bottomPanelGO, "Reset", "RESET DEFAULTS", () => ResetControlBindings(rebindingUI));

        // Bouton Back
        CreateControlsButton(bottomPanelGO, "Back", "BACK", () => CloseControlsPanel());

        // Forcer la reconstruction des layouts
        LayoutRebuilder.ForceRebuildLayoutImmediate(mainRT);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rebindRT);

        // Stocker la référence du panel
        controlsPanel = controlsPanelGO;

        Debug.Log("[PauseMenu] Panneau de contrôles créé");
    }

    private void CreateControlsButton(GameObject parent, string name, string text, UnityEngine.Events.UnityAction callback)
    {
        GameObject btnGO = new GameObject(name + "Button");
        btnGO.transform.SetParent(parent.transform, false);
        
        Image btnImage = btnGO.AddComponent<Image>();
        if (name == "Reset")
            btnImage.color = new Color(0.7f, 0.2f, 0.2f, 1f);  // Rouge
        else
            btnImage.color = new Color(0.2f, 0.5f, 0.2f, 1f);  // Vert
        
        Button btn = btnGO.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = btnImage.color;
        colors.highlightedColor = btnImage.color + new Color(0.1f, 0.1f, 0.1f, 0);
        colors.pressedColor = btnImage.color - new Color(0.1f, 0.1f, 0.1f, 0);
        btn.colors = colors;
        
        TextMeshProUGUI btnText = new GameObject("Text").AddComponent<TextMeshProUGUI>();
        btnText.transform.SetParent(btnGO.transform, false);
        btnText.text = text;
        btnText.fontSize = 20;
        btnText.alignment = TextAlignmentOptions.Center;
        RectTransform btnTextRT = btnText.GetComponent<RectTransform>();
        btnTextRT.offsetMin = Vector2.zero;
        btnTextRT.offsetMax = Vector2.zero;

        LayoutElement btnLE = btnGO.AddComponent<LayoutElement>();
        btnLE.preferredWidth = 250;
        btnLE.preferredHeight = 60;

        btn.onClick.AddListener(callback);
    }

    private void ResetControlBindings(ControlsRebindingUI rebindingUI)
    {
        Debug.Log("[PauseMenu] Réinitialisation des contrôles");
        if (KeyBindingManager.Instance != null)
        {
            KeyBindingManager.Instance.ResetAllBindings();
            Debug.Log("[PauseMenu] Contrôles réinitialisés par défaut");
            
            if (rebindingUI != null)
            {
                rebindingUI.RefreshUI();
            }
        }
    }

    private void CloseControlsPanel()
    {
        Debug.Log("[PauseMenu] Fermeture du panneau de contrôles");
        
        // Fermer le panneau de contrôles
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }
        
        // Réouvrir le menu principal
        if (menuPanelGO != null)
        {
            menuPanelGO.SetActive(true);
        }
    }

    private void OnRestartPressed()
    {
        Debug.Log("[PauseMenu] Bouton Restart pressé.");
        Time.timeScale = 1f;  // Redémarrer le temps avant de charger
        _isPaused = false;

        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
        else
            SceneManager.LoadScene("Game");
    }

    private void OnBackToMenuPressed()
    {
        Debug.Log("[PauseMenu] Bouton Back to Menu pressé.");
        Time.timeScale = 1f;  // Redémarrer le temps avant de charger
        _isPaused = false;

        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMenu();
        else
            SceneManager.LoadScene("MainMenu");
    }

    private void OnQuitPressed()
    {
        Debug.Log("[PauseMenu] Bouton Quit pressé.");
        Time.timeScale = 1f;  // Redémarrer le temps avant de quitter
        _isPaused = false;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
