using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    [SerializeField] private Canvas pauseMenuCanvas;

    private GameObject _mainPanel;
    private GameObject _controlsPanel;
    private GameObject _settingsPanel;

    private bool _isPaused;
    public bool IsPaused => _isPaused;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
        pauseMenuCanvas.enabled = false;
    }

    public void TogglePause() { if (_isPaused) Resume(); else Pause(); }

    public void Pause()
    {
        if (_isPaused) return;
        _isPaused = true;
        Time.timeScale = 0f;
        ShowMainPanel();
        pauseMenuCanvas.enabled = true;
        AnimatePanelIn(_mainPanel);
        AudioManager.Instance?.PlaySFX(SFXType.UIOpen);
        InputManager.Instance?.SetPlayerInputEnabled(1, false);
        InputManager.Instance?.SetPlayerInputEnabled(2, false);
    }

    public void Resume()
    {
        if (!_isPaused) return;
        _isPaused = false;
        Time.timeScale = 1f;
        pauseMenuCanvas.enabled = false;
        AudioManager.Instance?.PlaySFX(SFXType.UIBack);
        InputManager.Instance?.SetPlayerInputEnabled(1, true);
        InputManager.Instance?.SetPlayerInputEnabled(2, true);
    }

    private void AnimatePanelIn(GameObject panel)
    {
        if (panel == null) return;
        panel.transform.localScale = Vector3.one * 0.7f;
        UITween.ScaleTo(panel.transform, Vector3.one, 0.35f, Easing.Ease.EaseOutBack);
    }

    private void ShowMainPanel()
    {
        _mainPanel?.SetActive(true);
        _controlsPanel?.SetActive(false);
        _settingsPanel?.SetActive(false);
    }

    private void ShowControlsPanel()
    {
        _mainPanel?.SetActive(false);
        _controlsPanel?.SetActive(true);
        _settingsPanel?.SetActive(false);
        AnimatePanelIn(_controlsPanel);
        if (_controlsPanel != null)
        {
            ControlsRebindingUI rebindUI = _controlsPanel.GetComponentInChildren<ControlsRebindingUI>();
            rebindUI?.RefreshUI();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_controlsPanel.GetComponent<RectTransform>());
            ScrollRect sr = _controlsPanel.GetComponentInChildren<ScrollRect>();
            if (sr != null) sr.verticalNormalizedPosition = 1f;
        }
    }

    private void ShowSettingsPanel()
    {
        _mainPanel?.SetActive(false);
        _controlsPanel?.SetActive(false);
        _settingsPanel?.SetActive(true);
        AnimatePanelIn(_settingsPanel);
    }

    private void OnSettings() { ShowSettingsPanel(); }

    private void OnControls() { ShowControlsPanel(); }

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

    private void BuildUI()
    {
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.enabled = false;
            Destroy(pauseMenuCanvas.gameObject);
        }

        GameObject canvasGO = new GameObject("PauseCanvas");
        pauseMenuCanvas = canvasGO.AddComponent<Canvas>();
        pauseMenuCanvas.enabled = false;
        pauseMenuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        pauseMenuCanvas.sortingOrder = 100;
        canvasGO.AddComponent<GraphicRaycaster>();
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject overlay = new GameObject("Overlay");
        overlay.transform.SetParent(canvasGO.transform, false);
        Image overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.55f);
        overlayImg.raycastTarget = false;
        Stretch(overlay.GetComponent<RectTransform>());

        _mainPanel = BuildMainPanel(canvasGO);
        _controlsPanel = BuildControlsPanel(canvasGO);
        _controlsPanel?.SetActive(false);
        _settingsPanel = BuildSettingsPanel(canvasGO);
        _settingsPanel?.SetActive(false);
    }

    private GameObject BuildMainPanel(GameObject parent)
    {
        GameObject panel = CreatePanel(parent, 500, 700);

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12;
        vlg.padding = new RectOffset(30, 30, 30, 30);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        AddTitle(panel, "PAUSE");
        CreateButton(panel, "Resume", Resume);
        CreateButton(panel, "Controls", OnControls);
        CreateButton(panel, "Settings", OnSettings);
        CreateButton(panel, "Restart", OnRestart);
        CreateButton(panel, "Back to Menu", OnBackToMenu);
        CreateButton(panel, "Quit", OnQuit);

        return panel;
    }

    private GameObject BuildSettingsPanel(GameObject parent)
    {
        GameObject panel = CreatePanel(parent, 800, 700);

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12;
        vlg.padding = new RectOffset(30, 30, 30, 30);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        AddTitle(panel, "SETTINGS");

        // Create scroll view for settings content
        GameObject scrollViewGO = new GameObject("ScrollView");
        scrollViewGO.transform.SetParent(panel.transform, false);
        LayoutElement scrollLE = scrollViewGO.AddComponent<LayoutElement>();
        scrollLE.flexibleHeight = 1;
        scrollLE.flexibleWidth = 1;
        scrollViewGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        ScrollRect scrollRect = scrollViewGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 30f;

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollViewGO.transform, false);
        viewport.AddComponent<Image>().color = Color.white;
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        RectTransform vpRT = viewport.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero;
        vpRT.offsetMax = Vector2.zero;
        scrollRect.viewport = vpRT;

        // Content container
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        VerticalLayoutGroup contentVLG = content.AddComponent<VerticalLayoutGroup>();
        contentVLG.spacing = 20;
        contentVLG.childForceExpandWidth = true;
        contentVLG.childForceExpandHeight = false;
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;
        scrollRect.content = contentRT;

        // Audio Settings Section
        AddSettingsSection(content, "AUDIO SETTINGS");
        SettingsPanelController audioController = content.AddComponent<SettingsPanelController>();
        audioController.Populate(content, () => { });

        // Avatar Customization Section
        AddSettingsSection(content, "CUSTOMISATION OF AVATAR");
        GameObject avatarCustomizationGO = new GameObject("AvatarCustomization");
        avatarCustomizationGO.transform.SetParent(content.transform, false);
        LayoutElement avatarLayoutElement = avatarCustomizationGO.AddComponent<LayoutElement>();
        avatarLayoutElement.flexibleWidth = 1;
        avatarLayoutElement.preferredHeight = 280;

        // Player 1 Avatar Section
        BuildPlayerAvatarSection(avatarCustomizationGO, 1);

        // Player 2 Avatar Section
        BuildPlayerAvatarSection(avatarCustomizationGO, 2);

        // Bottom buttons
        GameObject bottom = new GameObject("Bottom");
        bottom.transform.SetParent(panel.transform, false);
        HorizontalLayoutGroup bottomHLG = bottom.AddComponent<HorizontalLayoutGroup>();
        bottomHLG.spacing = 20;
        bottomHLG.childForceExpandWidth = false;
        bottomHLG.childForceExpandHeight = false;
        bottomHLG.childAlignment = TextAnchor.MiddleCenter;
        LayoutElement bottomLE = bottom.AddComponent<LayoutElement>();
        bottomLE.preferredHeight = 60;
        bottomLE.flexibleWidth = 1;

        CreateFixedButton(bottom, "Back", new Color(0.15f, 0.45f, 0.15f, 1f), 160, ShowMainPanel);

        return panel;
    }

    private void AddSettingsSection(GameObject parent, string sectionTitle)
    {
        GameObject section = new GameObject("Section_" + sectionTitle);
        section.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = section.AddComponent<TextMeshProUGUI>();
        tmp.text = sectionTitle;
        tmp.fontSize = 28;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.BottomLeft;
        tmp.color = new Color(1f, 0.85f, 0.2f, 1f);
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        LayoutElement le = section.AddComponent<LayoutElement>();
        le.preferredHeight = 40;
        le.flexibleWidth = 1;
    }

    private void BuildPlayerAvatarSection(GameObject parent, int playerNumber)
    {
        GameObject sectionGO = new GameObject($"Player{playerNumber}_AvatarSection");
        sectionGO.transform.SetParent(parent.transform, false);

        VerticalLayoutGroup vlg = sectionGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.padding = new RectOffset(0, 0, 0, 0);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        LayoutElement le = sectionGO.AddComponent<LayoutElement>();
        le.flexibleWidth = 1;
        le.preferredHeight = 120;

        // Player label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(sectionGO.transform, false);
        TextMeshProUGUI labelTmp = labelGO.AddComponent<TextMeshProUGUI>();
        labelTmp.text = $"Player {playerNumber}";
        labelTmp.fontSize = 20;
        labelTmp.alignment = TextAlignmentOptions.BottomLeft;
        labelTmp.color = Color.white;
        LayoutElement labelLE = labelGO.AddComponent<LayoutElement>();
        labelLE.preferredHeight = 30;

        // Avatar grid container
        GameObject gridContainerGO = new GameObject("GridContainer");
        gridContainerGO.transform.SetParent(sectionGO.transform, false);
        HorizontalLayoutGroup hlg = gridContainerGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        LayoutElement gridLE = gridContainerGO.AddComponent<LayoutElement>();
        gridLE.flexibleWidth = 1;
        gridLE.flexibleHeight = 1;

        // Create avatar buttons using hardcoded avatars
        CreateAvatarButtonForPlayer(gridContainerGO, AvatarSessionManager.AvatarType.Red, playerNumber);
        CreateAvatarButtonForPlayer(gridContainerGO, AvatarSessionManager.AvatarType.Blue, playerNumber);
        CreateAvatarButtonForPlayer(gridContainerGO, AvatarSessionManager.AvatarType.Purple, playerNumber);
        CreateAvatarButtonForPlayer(gridContainerGO, AvatarSessionManager.AvatarType.Yellow, playerNumber);
        CreateAvatarButtonForPlayer(gridContainerGO, AvatarSessionManager.AvatarType.Black, playerNumber);
    }

    private void CreateAvatarButtonForPlayer(GameObject parent, AvatarSessionManager.AvatarType avatarType, int playerNumber)
    {
        GameObject buttonGO = new GameObject(avatarType.ToString());
        buttonGO.transform.SetParent(parent.transform, false);

        Image btnImage = buttonGO.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        Button btn = buttonGO.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        cb.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        cb.pressedColor = new Color(0.8f, 0.8f, 0f, 1f);
        btn.colors = cb;

        btn.onClick.AddListener(() =>
        {
            AvatarSessionManager.Instance?.SetPlayerAvatar(playerNumber, avatarType);
            AudioManager.Instance?.PlaySFX(SFXType.UIClick);
        });

        LayoutElement le = buttonGO.AddComponent<LayoutElement>();
        le.preferredWidth = 60;
        le.preferredHeight = 60;

        // Optional: Add avatar type text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = avatarType.ToString().Substring(0, 1);
        tmp.fontSize = 12;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        RectTransform rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private GameObject BuildControlsPanel(GameObject parent)
    {
        GameObject panel = CreatePanel(parent, 760, 700);
        try
        {

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.padding = new RectOffset(24, 24, 20, 20);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        AddTitle(panel, "CONTROLS");

        GameObject scrollViewGO = new GameObject("ScrollView");
        scrollViewGO.transform.SetParent(panel.transform, false);
        LayoutElement scrollLE = scrollViewGO.AddComponent<LayoutElement>();
        scrollLE.flexibleHeight = 1;
        scrollLE.flexibleWidth = 1;
        scrollViewGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        ScrollRect scrollRect = scrollViewGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 30f;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollViewGO.transform, false);
        viewport.AddComponent<Image>().color = Color.white;
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        RectTransform vpRT = viewport.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero;
        vpRT.offsetMax = Vector2.zero;
        scrollRect.viewport = vpRT;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        VerticalLayoutGroup contentVLG = content.AddComponent<VerticalLayoutGroup>();
        contentVLG.spacing = 4;
        contentVLG.childForceExpandWidth = true;
        contentVLG.childForceExpandHeight = false;
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;
        scrollRect.content = contentRT;

        ControlsRebindingUI rebindUI = content.AddComponent<ControlsRebindingUI>();
        rebindUI.SetRebindingContainer(content.transform);

        GameObject bottom = new GameObject("Bottom");
        bottom.transform.SetParent(panel.transform, false);
        HorizontalLayoutGroup bottomHLG = bottom.AddComponent<HorizontalLayoutGroup>();
        bottomHLG.spacing = 20;
        bottomHLG.childForceExpandWidth = false;
        bottomHLG.childForceExpandHeight = false;
        bottomHLG.childAlignment = TextAnchor.MiddleCenter;
        LayoutElement bottomLE = bottom.AddComponent<LayoutElement>();
        bottomLE.preferredHeight = 60;
        bottomLE.flexibleWidth = 1;

        CreateFixedButton(bottom, "Reset Defaults", new Color(0.6f, 0.15f, 0.15f, 1f), 220, () =>
        {
            InputManager.Instance?.ResetToDefaults();
            KeyBindingManager.Instance?.ResetAllBindings();
            rebindUI.RefreshUI();
        });
        CreateFixedButton(bottom, "Back", new Color(0.15f, 0.45f, 0.15f, 1f), 160, ShowMainPanel);

        return panel;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PauseMenu] BuildControlsPanel FAILED: {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
            return panel;
        }
    }

    private GameObject CreatePanel(GameObject parent, float width, float height)
    {
        GameObject go = new GameObject("Panel");
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.18f, 1f);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = Vector2.zero;
        return go;
    }

    private void AddTitle(GameObject parent, string text)
    {
        GameObject go = new GameObject("Title");
        go.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 46;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 70;
        le.flexibleWidth = 1;
    }

    private void CreateButton(GameObject parent, string label, UnityEngine.Events.UnityAction callback)
    {
        GameObject go = new GameObject(label);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = new Color(0.2f, 0.38f, 0.65f, 1f);
        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.3f, 0.52f, 0.85f, 1f);
        cb.pressedColor = new Color(0.12f, 0.28f, 0.5f, 1f);
        btn.colors = cb;
        btn.onClick.AddListener(callback);
        go.AddComponent<UIButtonFeedback>();

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 58;
        le.flexibleWidth = 1;

        AddButtonText(go, label, 28);
    }

    private void CreateFixedButton(GameObject parent, string label, Color color, float width, UnityEngine.Events.UnityAction callback)
    {
        GameObject go = new GameObject(label);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = color;
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(callback);
        go.AddComponent<UIButtonFeedback>();

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredWidth = width;
        le.preferredHeight = 56;

        AddButtonText(go, label, 22);
    }

    private void AddButtonText(GameObject parent, string text, float size)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
