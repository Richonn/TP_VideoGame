using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanelController : MonoBehaviour
{
    public void Populate(GameObject panel, System.Action onBack)
    {
        if (panel == null) return;

        AddSliderRow(panel, "Master",  VolumeKeys.Master,  0.8f, v => AudioManager.Instance?.SetMasterVolume(v));
        AddSliderRow(panel, "Music",   VolumeKeys.Music,   0.7f, v => AudioManager.Instance?.SetMusicVolume(v));
        AddSliderRow(panel, "SFX",     VolumeKeys.SFX,     0.8f, v => AudioManager.Instance?.SetSFXVolume(v));
        AddSliderRow(panel, "Ambient", VolumeKeys.Ambient, 0.6f, v => AudioManager.Instance?.SetAmbientVolume(v));

        AddBackButton(panel, "Back", onBack);
    }

    private void AddSliderRow(GameObject parent, string label, string key, float def, System.Action<float> apply)
    {
        GameObject row = new GameObject(label + "Row");
        row.transform.SetParent(parent.transform, false);
        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        LayoutElement rowLE = row.AddComponent<LayoutElement>();
        rowLE.preferredHeight = 50;
        rowLE.minHeight = 50;
        rowLE.flexibleWidth = 1;

        GameObject lbl = new GameObject("Label");
        lbl.transform.SetParent(row.transform, false);
        TextMeshProUGUI tmp = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        LayoutElement lblLE = lbl.AddComponent<LayoutElement>();
        lblLE.preferredWidth = 100;
        lblLE.flexibleWidth = 0;
        lblLE.preferredHeight = 32;

        GameObject sliderGO = new GameObject("Slider");
        sliderGO.transform.SetParent(row.transform, false);
        Slider slider = BuildSlider(sliderGO);
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = PlayerPrefs.GetFloat(key, def);
        LayoutElement sLE = sliderGO.AddComponent<LayoutElement>();
        sLE.flexibleWidth = 1;
        sLE.minWidth = 200;
        sLE.preferredHeight = 24;
        sLE.minHeight = 24;

        GameObject val = new GameObject("Value");
        val.transform.SetParent(row.transform, false);
        TextMeshProUGUI valTMP = val.AddComponent<TextMeshProUGUI>();
        valTMP.fontSize = 20;
        valTMP.color = Color.white;
        valTMP.alignment = TextAlignmentOptions.MidlineRight;
        valTMP.text = PercentText(slider.value);
        valTMP.textWrappingMode = TextWrappingModes.NoWrap;
        LayoutElement valLE = val.AddComponent<LayoutElement>();
        valLE.preferredWidth = 70;
        valLE.flexibleWidth = 0;
        valLE.preferredHeight = 32;

        apply(slider.value);
        slider.onValueChanged.AddListener(v =>
        {
            PlayerPrefs.SetFloat(key, v);
            apply(v);
            valTMP.text = PercentText(v);
        });
    }

    private static string PercentText(float v) => $"{Mathf.RoundToInt(v * 100)}%";

    private Slider BuildSlider(GameObject root)
    {
        Image bg = root.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.18f, 1f);

        Slider slider = root.AddComponent<Slider>();

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(root.transform, false);
        RectTransform faRT = fillArea.AddComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0, 0.25f);
        faRT.anchorMax = new Vector2(1, 0.75f);
        faRT.offsetMin = new Vector2(8, 0);
        faRT.offsetMax = new Vector2(-8, 0);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.4f, 0.7f, 0.95f, 1f);
        RectTransform fRT = fill.GetComponent<RectTransform>();
        fRT.anchorMin = Vector2.zero;
        fRT.anchorMax = Vector2.one;
        fRT.offsetMin = Vector2.zero;
        fRT.offsetMax = Vector2.zero;

        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(root.transform, false);
        RectTransform haRT = handleArea.AddComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero;
        haRT.anchorMax = Vector2.one;
        haRT.offsetMin = new Vector2(8, 0);
        haRT.offsetMax = new Vector2(-8, 0);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;
        RectTransform hRT = handle.GetComponent<RectTransform>();
        hRT.sizeDelta = new Vector2(20, 32);

        slider.targetGraphic = handleImage;
        slider.fillRect = fRT;
        slider.handleRect = hRT;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private void AddBackButton(GameObject parent, string label, System.Action onBack)
    {
        GameObject go = new GameObject(label);
        go.transform.SetParent(parent.transform, false);
        Color color = new Color(0.15f, 0.45f, 0.15f, 1f);
        go.AddComponent<Image>().color = color;
        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = color * 1.2f;
        cb.pressedColor = color * 0.8f;
        btn.colors = cb;
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlaySFX(SFXType.UIBack);
            PlayerPrefs.Save();
            onBack?.Invoke();
        });

        UIButtonFeedback fb = go.AddComponent<UIButtonFeedback>();
        fb.Init();

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 50;
        le.flexibleWidth = 1;

        GameObject text = new GameObject("Text");
        text.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = text.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        RectTransform rt = text.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
