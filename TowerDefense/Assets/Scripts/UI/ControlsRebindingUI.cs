using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Interface de rebinding personnalisée sans dépendre de l'InputActionAsset
/// Utilise KeyBindingManager pour gérer les touches
/// </summary>
public class ControlsRebindingUI : MonoBehaviour
{
    [SerializeField] private Transform rebindingContainer;  // Container pour les boutons de rebinding

    private class RebindingDisplay
    {
        public KeyBindingManager.ActionType Action;
        public TextMeshProUGUI displayText;
        public Button rebindButton;
    }

    private List<RebindingDisplay> rebindingDisplays = new List<RebindingDisplay>();
    private RebindingDisplay currentRebinding;
    private float rebindTimeout = 0f;
    private const float REBIND_TIMEOUT = 5f;  // 5 secondes pour rebind

    void Start()
    {
        // Si le container n'est pas assigné, attendre qu'il le soit via SetRebindingContainer
        if (rebindingContainer == null)
        {
            Debug.LogWarning("[ControlsRebinding] rebindingContainer not assigned yet, waiting for SetRebindingContainer()");
            return;
        }

        // Container est assigné, créer l'UI
        CreateRebindingUI();
    }

    public void SetRebindingContainer(Transform container)
    {
        rebindingContainer = container;
        Debug.Log("[ControlsRebinding] Container assigné: " + container.name);
        // Recréer l'UI avec le bon container
        CreateRebindingUI();
    }

    void Update()
    {
        // Gestion du rebinding en cours
        if (currentRebinding != null)
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(key))
                    {
                        // Ignorer certaines touches
                        if (key == KeyCode.Escape || key == KeyCode.Return)
                            continue;

                        CompleteRebinding(key);
                        return;
                    }
                }
            }

            // Vérifier le timeout
            rebindTimeout -= Time.deltaTime;
            if (rebindTimeout <= 0f)
            {
                CancelRebinding();
            }
        }
    }

    private void CreateRebindingUI()
    {
        // Nettoyer les anciens éléments
        foreach (Transform child in rebindingContainer)
        {
            Destroy(child.gameObject);
        }
        rebindingDisplays.Clear();

        // Titre
        CreateTitle("KEYBOARD CONTROLS");

        // Créer une ligne pour chaque action
        KeyBindingManager.ActionType[] actions = new KeyBindingManager.ActionType[]
        {
            KeyBindingManager.ActionType.Move_Up,
            KeyBindingManager.ActionType.Move_Down,
            KeyBindingManager.ActionType.Move_Left,
            KeyBindingManager.ActionType.Move_Right,
            KeyBindingManager.ActionType.PlaceTower,
            KeyBindingManager.ActionType.Interact,
            KeyBindingManager.ActionType.LancerVague
        };

        string[] displayNames = new string[]
        {
            "Move Up",
            "Move Down",
            "Move Left",
            "Move Right",
            "Place Tower",
            "Interact",
            "Launch Wave"
        };

        for (int i = 0; i < actions.Length; i++)
        {
            CreateRebindingRow(actions[i], displayNames[i]);
        }

        // Forcer Unity à recalculer les layouts
        LayoutRebuilder.ForceRebuildLayoutImmediate(rebindingContainer as RectTransform);
        
        Debug.Log("[ControlsRebinding] UI créée avec succès");
    }

    private void CreateTitle(string text)
    {
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(rebindingContainer, false);
        
        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = text;
        titleText.fontSize = 28;
        titleText.alignment = TextAlignmentOptions.Left;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;  // Assurer que le texte est blanc
        
        LayoutElement layoutElement = titleGO.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 40;
        layoutElement.preferredWidth = -1;  // -1 = flexible width

        RectTransform rt = titleGO.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(0, 0);
    }

    private void CreateRebindingRow(KeyBindingManager.ActionType action, string displayName) {
        // Container pour la ligne
        GameObject rowGO = new GameObject($"{action}_Row");
        rowGO.transform.SetParent(rebindingContainer, false);
        
        HorizontalLayoutGroup hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        LayoutElement rowLE = rowGO.AddComponent<LayoutElement>();
        rowLE.preferredHeight = 60;
        rowLE.preferredWidth = -1;

        RectTransform rowRT = rowGO.GetComponent<RectTransform>();
        rowRT.sizeDelta = new Vector2(0, 60);

        // Label (nom de l'action)
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(rowGO.transform, false);
        
        TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
        labelText.text = displayName;
        labelText.fontSize = 24;
        labelText.alignment = TextAlignmentOptions.Left;
        labelText.color = Color.white;
        labelText.textWrappingMode = TextWrappingModes.NoWrap;
        
        LayoutElement labelLE = labelGO.AddComponent<LayoutElement>();
        labelLE.preferredWidth = 200;
        labelLE.preferredHeight = 60;

        RectTransform labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.sizeDelta = new Vector2(200, 60);

        // Affichage de la touche actuelle
        GameObject displayGO = new GameObject("Display");
        displayGO.transform.SetParent(rowGO.transform, false);
        
        Image displayBG = displayGO.AddComponent<Image>();
        displayBG.color = new Color(0.2f, 0.2f, 0.3f, 1f);
        
        LayoutElement displayLE = displayGO.AddComponent<LayoutElement>();
        displayLE.preferredWidth = 150;
        displayLE.preferredHeight = 60;

        RectTransform displayRT = displayGO.GetComponent<RectTransform>();
        displayRT.sizeDelta = new Vector2(150, 60);

        // Texte de la touche
        GameObject displayTextGO = new GameObject("Text");
        displayTextGO.transform.SetParent(displayGO.transform, false);
        
        TextMeshProUGUI displayText = displayTextGO.AddComponent<TextMeshProUGUI>();
        KeyBindingManager.KeyBinding binding = KeyBindingManager.Instance.GetBinding(action);
        displayText.text = KeyBindingManager.GetKeyDisplayName(binding.KeyboardKey);
        displayText.fontSize = 22;
        displayText.alignment = TextAlignmentOptions.Center;
        displayText.color = Color.white;
        displayText.textWrappingMode = TextWrappingModes.NoWrap;
        
        RectTransform displayTextRT = displayTextGO.GetComponent<RectTransform>();
        displayTextRT.offsetMin = Vector2.zero;
        displayTextRT.offsetMax = Vector2.zero;

        // Bouton Rebind
        GameObject rebindBtnGO = new GameObject("RebindButton");
        rebindBtnGO.transform.SetParent(rowGO.transform, false);
        
        Image rebindBtnImage = rebindBtnGO.AddComponent<Image>();
        rebindBtnImage.color = new Color(0.3f, 0.6f, 0.8f, 1f);
        
        Button rebindBtn = rebindBtnGO.AddComponent<Button>();
        ColorBlock colors = rebindBtn.colors;
        colors.normalColor = new Color(0.3f, 0.6f, 0.8f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.7f, 0.9f, 1f);
        colors.pressedColor = new Color(0.2f, 0.5f, 0.7f, 1f);
        rebindBtn.colors = colors;
        
        LayoutElement rebindBtnLE = rebindBtnGO.AddComponent<LayoutElement>();
        rebindBtnLE.preferredWidth = 120;
        rebindBtnLE.preferredHeight = 60;

        RectTransform rebindBtnRT = rebindBtnGO.GetComponent<RectTransform>();
        rebindBtnRT.sizeDelta = new Vector2(120, 60);

        // Texte du bouton
        GameObject rebindBtnTextGO = new GameObject("Text");
        rebindBtnTextGO.transform.SetParent(rebindBtnGO.transform, false);
        TextMeshProUGUI rebindBtnText = rebindBtnTextGO.AddComponent<TextMeshProUGUI>();
        rebindBtnText.text = "REBIND";
        rebindBtnText.fontSize = 18;
        rebindBtnText.alignment = TextAlignmentOptions.Center;
        rebindBtnText.color = Color.white;
        rebindBtnText.textWrappingMode = TextWrappingModes.NoWrap;
        
        RectTransform rebindBtnTextRT = rebindBtnTextGO.GetComponent<RectTransform>();
        rebindBtnTextRT.offsetMin = Vector2.zero;
        rebindBtnTextRT.offsetMax = Vector2.zero;

        // Enregistrer pour tracking
        RebindingDisplay display = new RebindingDisplay
        {
            Action = action,
            displayText = displayText,
            rebindButton = rebindBtn
        };

        rebindingDisplays.Add(display);

        // Listener
        rebindBtn.onClick.AddListener(() => StartRebinding(display));
    }

    private void StartRebinding(RebindingDisplay display)
    {
        Debug.Log($"[ControlsRebinding] Début du rebinding pour {display.Action}");

        // Désactiver les boutons
        foreach (var d in rebindingDisplays)
        {
            d.rebindButton.interactable = false;
        }

        currentRebinding = display;
        rebindTimeout = REBIND_TIMEOUT;
        display.displayText.text = "PRESS KEY...";
    }

    private void CompleteRebinding(KeyCode key)
    {
        if (currentRebinding == null) return;

        Debug.Log($"[ControlsRebinding] Rebinding complété pour {currentRebinding.Action}: {key}");

        // Appliquer le binding
        KeyBindingManager.Instance.SetBinding(currentRebinding.Action, key);

        // Mettre à jour l'affichage
        currentRebinding.displayText.text = KeyBindingManager.GetKeyDisplayName(key);

        // Réactiver les boutons
        foreach (var d in rebindingDisplays)
        {
            d.rebindButton.interactable = true;
        }

        currentRebinding = null;
    }

    private void CancelRebinding()
    {
        if (currentRebinding == null) return;

        Debug.Log("[ControlsRebinding] Rebinding annulé");

        // Restaurer l'affichage
        KeyBindingManager.KeyBinding binding = KeyBindingManager.Instance.GetBinding(currentRebinding.Action);
        currentRebinding.displayText.text = KeyBindingManager.GetKeyDisplayName(binding.KeyboardKey);

        // Réactiver les boutons
        foreach (var d in rebindingDisplays)
        {
            d.rebindButton.interactable = true;
        }

        currentRebinding = null;
    }

    public void ReloadDisplay()
    {
        CreateRebindingUI();
    }

    public void RefreshUI()
    {
        CreateRebindingUI();
    }
}
