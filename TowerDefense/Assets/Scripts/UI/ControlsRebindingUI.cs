using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Interface de rebinding des touches clavier.
/// Affiche une ligne par action : [Nom de l'action] [Touche actuelle] [Bouton REBIND]
/// Utilise KeyBindingManager pour persister les bindings via PlayerPrefs.
/// </summary>
public class ControlsRebindingUI : MonoBehaviour
{
    private Transform _container;

    private class RebindEntry
    {
        public KeyBindingManager.ActionType Action;
        public TextMeshProUGUI KeyDisplay;
        public Button          RebindButton;
    }

    private readonly List<RebindEntry> _entries = new List<RebindEntry>();
    private RebindEntry _enCours;
    private float       _timeout;
    private const float TIMEOUT = 5f;

    // ── API publique ──────────────────────────────────────────────────────────

    public void SetRebindingContainer(Transform container)
    {
        _container = container;
        // Étirer le container sur tout son parent
        RectTransform rt = container as RectTransform;
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        BuildUI();
    }

    public void RefreshUI() => BuildUI();

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Start()
    {
        if (_container == null) return;
        BuildUI();
    }

    void Update()
    {
        if (_enCours == null) return;

        _timeout -= Time.unscaledDeltaTime;
        if (_timeout <= 0f) { AnnulerRebind(); return; }

        if (!Input.anyKeyDown) return;

        foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (!Input.GetKeyDown(k)) continue;
            if (k == KeyCode.Escape || k == KeyCode.Return) { AnnulerRebind(); return; }
            TerminerRebind(k);
            return;
        }
    }

    // ── Construction de l'UI ──────────────────────────────────────────────────
    private void BuildUI()
    {
        if (_container == null || KeyBindingManager.Instance == null) return;

        // Vider l'ancien contenu
        foreach (Transform child in _container) Destroy(child.gameObject);
        _entries.Clear();

        // Titre
        AjouterTitre("TOUCHES CLAVIER — JOUEUR 1");

        // Une ligne par action
        (KeyBindingManager.ActionType, string)[] actions =
        {
            (KeyBindingManager.ActionType.Move_Up,    "Haut"),
            (KeyBindingManager.ActionType.Move_Down,  "Bas"),
            (KeyBindingManager.ActionType.Move_Left,  "Gauche"),
            (KeyBindingManager.ActionType.Move_Right, "Droite"),
            (KeyBindingManager.ActionType.PlaceTower, "Poser tour"),
            (KeyBindingManager.ActionType.Interact,   "Interagir"),
            (KeyBindingManager.ActionType.LancerVague,"Lancer vague"),
        };

        foreach (var (action, label) in actions)
            AjouterLigne(action, label);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_container as RectTransform);
    }

    private void AjouterTitre(string texte)
    {
        GameObject go = new GameObject("Titre");
        go.transform.SetParent(_container, false);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = texte;
        tmp.fontSize  = 22;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 40;
        le.flexibleWidth   = 1;

        SetAnchorStretchH(go.GetComponent<RectTransform>());
    }

    private void AjouterLigne(KeyBindingManager.ActionType action, string label)
    {
        // Ligne conteneur
        GameObject rowGO = new GameObject($"{action}_Row");
        rowGO.transform.SetParent(_container, false);

        HorizontalLayoutGroup hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing              = 10;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.padding              = new RectOffset(0, 0, 4, 4);

        LayoutElement rowLE = rowGO.AddComponent<LayoutElement>();
        rowLE.preferredHeight = 52;
        rowLE.flexibleWidth   = 1;   // ← s'étire sur toute la largeur du container

        SetAnchorStretchH(rowGO.GetComponent<RectTransform>());

        // ── Nom de l'action ──────────────────────────────────────────────────
        TextMeshProUGUI labelTMP = CreerTexte(rowGO, label, 20, TextAlignmentOptions.Left);
        LayoutElement labelLE = labelTMP.gameObject.AddComponent<LayoutElement>();
        labelLE.flexibleWidth  = 1;   // prend l'espace restant
        labelLE.minWidth       = 120;

        // ── Touche actuelle ──────────────────────────────────────────────────
        GameObject displayGO = new GameObject("Display");
        displayGO.transform.SetParent(rowGO.transform, false);
        displayGO.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.25f, 1f);

        LayoutElement displayLE = displayGO.AddComponent<LayoutElement>();
        displayLE.minWidth      = 110;
        displayLE.preferredWidth = 110;

        KeyBindingManager.KeyBinding binding = KeyBindingManager.Instance.GetBinding(action);
        TextMeshProUGUI keyTMP = CreerTexte(displayGO, KeyBindingManager.GetKeyDisplayName(binding.KeyboardKey), 18, TextAlignmentOptions.Center);
        SetAnchorStretch(keyTMP.GetComponent<RectTransform>());

        // ── Bouton Rebind ─────────────────────────────────────────────────────
        GameObject btnGO = new GameObject("RebindBtn");
        btnGO.transform.SetParent(rowGO.transform, false);
        btnGO.AddComponent<Image>().color = new Color(0.25f, 0.5f, 0.75f, 1f);

        Button btn = btnGO.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.35f, 0.65f, 0.9f, 1f);
        cb.pressedColor     = new Color(0.15f, 0.35f, 0.6f, 1f);
        btn.colors = cb;

        LayoutElement btnLE = btnGO.AddComponent<LayoutElement>();
        btnLE.minWidth      = 90;
        btnLE.preferredWidth = 90;

        CreerTexte(btnGO, "REBIND", 16, TextAlignmentOptions.Center);

        // Enregistrement
        RebindEntry entry = new RebindEntry
        {
            Action       = action,
            KeyDisplay   = keyTMP,
            RebindButton = btn,
        };
        _entries.Add(entry);
        btn.onClick.AddListener(() => DemarrerRebind(entry));
    }

    // ── Helpers UI ────────────────────────────────────────────────────────────
    private TextMeshProUGUI CreerTexte(GameObject parent, string texte, float size, TextAlignmentOptions align)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent.transform, false);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = texte;
        tmp.fontSize  = size;
        tmp.color     = Color.white;
        tmp.alignment = align;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;

        SetAnchorStretch(go.GetComponent<RectTransform>());
        return tmp;
    }

    private static void SetAnchorStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetAnchorStretchH(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
    }

    // ── Logique de rebinding ──────────────────────────────────────────────────
    private void DemarrerRebind(RebindEntry entry)
    {
        foreach (var e in _entries) e.RebindButton.interactable = false;
        _enCours = entry;
        _timeout = TIMEOUT;
        entry.KeyDisplay.text = "...";
    }

    private void TerminerRebind(KeyCode key)
    {
        if (_enCours == null) return;
        KeyBindingManager.Instance.SetBinding(_enCours.Action, key);
        _enCours.KeyDisplay.text = KeyBindingManager.GetKeyDisplayName(key);
        FinRebind();
    }

    private void AnnulerRebind()
    {
        if (_enCours == null) return;
        KeyBindingManager.KeyBinding b = KeyBindingManager.Instance.GetBinding(_enCours.Action);
        _enCours.KeyDisplay.text = KeyBindingManager.GetKeyDisplayName(b.KeyboardKey);
        FinRebind();
    }

    private void FinRebind()
    {
        _enCours = null;
        foreach (var e in _entries) e.RebindButton.interactable = true;
    }
}
