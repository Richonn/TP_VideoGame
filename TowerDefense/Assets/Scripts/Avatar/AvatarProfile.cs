using UnityEngine;

[System.Serializable]
public class AvatarProfile
{
    public int pawnColorIndex = 0;
    public Color secondaryTint = Color.white;
    public float scale = 1f;
    public bool flipHorizontal = false;

    public static AvatarProfile LoadForPlayer(int playerIndex)
    {
        string p = Prefix(playerIndex);
        AvatarProfile profile = new AvatarProfile();
        profile.pawnColorIndex   = PlayerPrefs.GetInt(p + "color", 0);
        profile.secondaryTint    = new Color(
            PlayerPrefs.GetFloat(p + "tintR", 1f),
            PlayerPrefs.GetFloat(p + "tintG", 1f),
            PlayerPrefs.GetFloat(p + "tintB", 1f), 1f);
        profile.scale            = PlayerPrefs.GetFloat(p + "scale", 1f);
        profile.flipHorizontal   = PlayerPrefs.GetInt(p + "flip", 0) == 1;
        return profile;
    }

    public void SaveForPlayer(int playerIndex)
    {
        string p = Prefix(playerIndex);
        PlayerPrefs.SetInt(p + "color", pawnColorIndex);
        PlayerPrefs.SetFloat(p + "tintR", secondaryTint.r);
        PlayerPrefs.SetFloat(p + "tintG", secondaryTint.g);
        PlayerPrefs.SetFloat(p + "tintB", secondaryTint.b);
        PlayerPrefs.SetFloat(p + "scale", scale);
        PlayerPrefs.SetInt(p + "flip", flipHorizontal ? 1 : 0);
        PlayerPrefs.Save();
    }

    private static string Prefix(int playerIndex) => $"avatar_p{playerIndex}_";
}
