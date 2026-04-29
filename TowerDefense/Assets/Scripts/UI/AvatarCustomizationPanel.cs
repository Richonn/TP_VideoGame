using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AvatarCustomizationPanel : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private int playerIndex = 1;
    [SerializeField] private AvatarCustomizer previewCustomizer;

    [Header("Pawn Color (5 buttons: Black/Blue/Purple/Red/Yellow)")]
    [SerializeField] private Button[] colorButtons;

    [Header("Secondary tint sliders")]
    [SerializeField] private Slider tintR;
    [SerializeField] private Slider tintG;
    [SerializeField] private Slider tintB;

    [Header("Transform")]
    [SerializeField] private Slider scaleSlider;
    [SerializeField] private Toggle flipToggle;

    [Header("Label")]
    [SerializeField] private TMP_Text summaryText;

    private AvatarProfile _profile;

    void Start()
    {
        _profile = AvatarProfile.LoadForPlayer(playerIndex);
        BindUI();
        SyncSliders();
        Refresh();
    }

    private void BindUI()
    {
        for (int i = 0; i < (colorButtons != null ? colorButtons.Length : 0); i++)
        {
            int idx = i;
            if (colorButtons[idx] != null)
                colorButtons[idx].onClick.AddListener(() => OnColorSelected(idx));
        }

        if (tintR != null) tintR.onValueChanged.AddListener(v => { _profile.secondaryTint.r = v; Refresh(); });
        if (tintG != null) tintG.onValueChanged.AddListener(v => { _profile.secondaryTint.g = v; Refresh(); });
        if (tintB != null) tintB.onValueChanged.AddListener(v => { _profile.secondaryTint.b = v; Refresh(); });
        if (scaleSlider != null) scaleSlider.onValueChanged.AddListener(v => { _profile.scale = v; Refresh(); });
        if (flipToggle  != null) flipToggle.onValueChanged.AddListener(v => { _profile.flipHorizontal = v; Refresh(); });
    }

    private void SyncSliders()
    {
        if (tintR != null) tintR.value = _profile.secondaryTint.r;
        if (tintG != null) tintG.value = _profile.secondaryTint.g;
        if (tintB != null) tintB.value = _profile.secondaryTint.b;
        if (scaleSlider != null) scaleSlider.SetValueWithoutNotify(Mathf.Clamp(_profile.scale, scaleSlider.minValue, scaleSlider.maxValue));
        if (flipToggle  != null) flipToggle.SetIsOnWithoutNotify(_profile.flipHorizontal);
    }

    private void OnColorSelected(int idx)
    {
        _profile.pawnColorIndex = idx;
        Refresh();
        AudioManager.Instance?.PlaySFX(SFXType.UIClick);
    }

    private void Refresh()
    {
        previewCustomizer?.Apply(_profile);
        if (summaryText != null)
        {
            string colorNames = "Black/Blue/Purple/Red/Yellow";
            string[] names = colorNames.Split('/');
            string colorName = (_profile.pawnColorIndex < names.Length) ? names[_profile.pawnColorIndex] : _profile.pawnColorIndex.ToString();
            summaryText.text = $"{colorName} | tint ({_profile.secondaryTint.r:F1},{_profile.secondaryTint.g:F1},{_profile.secondaryTint.b:F1}) | scale {_profile.scale:F2}";
        }
    }

    public void Confirm()
    {
        _profile.SaveForPlayer(playerIndex);
        if (AvatarSessionManager.Instance != null)
        {
            AvatarSessionManager.AvatarType avatarType = previewCustomizer != null
                ? previewCustomizer.GetAvatarTypeForIndex(_profile.pawnColorIndex)
                : (AvatarSessionManager.AvatarType)_profile.pawnColorIndex;
            AvatarSessionManager.Instance.SetPlayerAvatar(playerIndex, avatarType, _profile);
        }
        AudioManager.Instance?.PlaySFX(SFXType.UIOpen);
        gameObject.SetActive(false);
    }
}
