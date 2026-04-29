using UnityEngine;

public class AvatarCustomizer : MonoBehaviour
{
    [System.Serializable]
    public struct PawnColorEntry
    {
        public AvatarSessionManager.AvatarType avatarType;
        public RuntimeAnimatorController controller;
    }

    [Header("Renderers")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private Animator animator;

    [Header("Pawn Colors (one per AvatarType)")]
    [SerializeField] private PawnColorEntry[] colorEntries;

    [Header("Tint Mask")]
    [Tooltip("White silhouette texture defining which pixels receive the secondary tint. Leave empty to tint the whole sprite.")]
    [SerializeField] private Texture2D overlayMask;

    private MaterialPropertyBlock _mpb;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        if (bodyRenderer == null) bodyRenderer = GetComponentInChildren<SpriteRenderer>();
        if (animator    == null) animator     = GetComponentInChildren<Animator>();
    }

    public void Apply(AvatarProfile profile)
    {
        if (profile == null) return;

        ApplyController(profile.pawnColorIndex);
        ApplyTint(profile.secondaryTint);
        ApplyTransform(profile);
    }

    private void ApplyController(int colorIndex)
    {
        if (colorEntries == null || colorEntries.Length == 0) return;
        int idx = Mathf.Clamp(colorIndex, 0, colorEntries.Length - 1);
        RuntimeAnimatorController ctrl = colorEntries[idx].controller;
        if (animator != null && ctrl != null)
            animator.runtimeAnimatorController = ctrl;
    }

    private void ApplyTint(Color tint)
    {
        if (bodyRenderer == null) return;
        bodyRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor("_Color", tint);
        if (overlayMask != null)
            _mpb.SetTexture("_MaskTex", overlayMask);
        bodyRenderer.SetPropertyBlock(_mpb);
    }

    private void ApplyTransform(AvatarProfile profile)
    {
        float s = Mathf.Clamp(profile.scale, 0.6f, 1.4f);
        Vector3 base3 = transform.localScale;
        transform.localScale = new Vector3(
            Mathf.Abs(base3.x) * s * (profile.flipHorizontal ? -1f : 1f),
            Mathf.Abs(base3.y) * s,
            base3.z);
    }

    public AvatarSessionManager.AvatarType GetAvatarTypeForIndex(int colorIndex)
    {
        if (colorEntries == null || colorEntries.Length == 0)
            return AvatarSessionManager.AvatarType.Blue;
        int idx = Mathf.Clamp(colorIndex, 0, colorEntries.Length - 1);
        return colorEntries[idx].avatarType;
    }
}
