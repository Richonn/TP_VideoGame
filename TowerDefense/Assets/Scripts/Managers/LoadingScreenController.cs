using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Écran de chargement affiché entre chaque transition de scène.
///
/// Usage :
///   LoadingScreenController.SceneCible = "Game";
///   SceneManager.LoadScene("Loading");
///
/// Le chargement est retardé artificiellement (DELAI_MINIMUM) pour
/// garantir une preuve visuelle de l'écran, même sur des scènes légères.
/// </summary>
public class LoadingScreenController : MonoBehaviour
{
    // ── API statique ──────────────────────────────────────────────────────────
    /// <summary>Scène à charger après l'écran de chargement.</summary>
    public static string SceneCible = "Game";

    // ── Config ────────────────────────────────────────────────────────────────
    private const float DELAI_MINIMUM = 2f;   // secondes — preuve visuelle pour le prof

    // ── Refs UI ───────────────────────────────────────────────────────────────
    private RectTransform   _rtRemplissage;
    private TextMeshProUGUI _texteProgression;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Start()
    {
        // Caméra minimale requise pour que Unity affiche le Canvas
        GameObject camGO = new GameObject("LoadingCamera");
        Camera cam = camGO.AddComponent<Camera>();
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.08f, 0.08f, 0.12f, 1f);
        cam.cullingMask      = 0; // ne rend rien du monde, juste le fond

        ConstruireUI();
        StartCoroutine(ChargerScene());
    }

    // ── Coroutine principale ──────────────────────────────────────────────────
    private IEnumerator ChargerScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(SceneCible);
        op.allowSceneActivation = false;    // garde l'écran visible jusqu'à la fin

        float tempsEcoule = 0f;

        while (true)
        {
            tempsEcoule += Time.unscaledDeltaTime;

            // Unity bloque op.progress à 0.9 quand allowSceneActivation = false
            float progressAssets = Mathf.Clamp01(op.progress / 0.9f);
            float progressTemps  = Mathf.Clamp01(tempsEcoule / DELAI_MINIMUM);

            // On avance au minimum des deux : on n'affiche pas 100% avant le délai ET le chargement
            float progressAffichee = Mathf.Min(progressAssets, progressTemps);

            _rtRemplissage.anchorMax = new Vector2(progressAffichee, 1f);
            _texteProgression.text   = $"{Mathf.RoundToInt(progressAffichee * 100)}%";

            if (tempsEcoule >= DELAI_MINIMUM && op.progress >= 0.9f)
            {
                _rtRemplissage.anchorMax = Vector2.one;
                _texteProgression.text   = "100%";
                yield return new WaitForSecondsRealtime(0.3f);
                op.allowSceneActivation = true;
                yield break;
            }

            yield return null;
        }
    }

    // ── Construction de l'UI ──────────────────────────────────────────────────
    private void ConstruireUI()
    {
        // Canvas
        GameObject canvasGO = new GameObject("LoadingCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Fond
        GameObject fond = new GameObject("Fond");
        fond.transform.SetParent(canvasGO.transform, false);
        fond.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 1f);
        Etirer(fond.GetComponent<RectTransform>());

        // Titre
        GameObject titre = new GameObject("Titre");
        titre.transform.SetParent(canvasGO.transform, false);
        TextMeshProUGUI tmpTitre = titre.AddComponent<TextMeshProUGUI>();
        tmpTitre.text      = "CHARGEMENT";
        tmpTitre.fontSize  = 72;
        tmpTitre.fontStyle = FontStyles.Bold;
        tmpTitre.color     = Color.white;
        tmpTitre.alignment = TextAlignmentOptions.Center;
        tmpTitre.textWrappingMode = TextWrappingModes.NoWrap;
        RectTransform rtTitre = titre.GetComponent<RectTransform>();
        rtTitre.anchorMin = new Vector2(0.2f, 0.55f);
        rtTitre.anchorMax = new Vector2(0.8f, 0.72f);
        rtTitre.offsetMin = Vector2.zero;
        rtTitre.offsetMax = Vector2.zero;

        // Fond de la barre
        GameObject barreFond = new GameObject("BarreFond");
        barreFond.transform.SetParent(canvasGO.transform, false);
        barreFond.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.28f, 1f);
        RectTransform rtBarreFond = barreFond.GetComponent<RectTransform>();
        rtBarreFond.anchorMin = new Vector2(0.15f, 0.44f);
        rtBarreFond.anchorMax = new Vector2(0.85f, 0.51f);
        rtBarreFond.offsetMin = Vector2.zero;
        rtBarreFond.offsetMax = Vector2.zero;

        // Remplissage de la barre (anchorMax.x animé de 0 à 1)
        GameObject remplissageGO = new GameObject("Remplissage");
        remplissageGO.transform.SetParent(barreFond.transform, false);
        remplissageGO.AddComponent<Image>().color = new Color(0.25f, 0.55f, 0.9f, 1f);
        _rtRemplissage          = remplissageGO.GetComponent<RectTransform>();
        _rtRemplissage.anchorMin = Vector2.zero;
        _rtRemplissage.anchorMax = Vector2.zero;   // commence à 0 de large
        _rtRemplissage.offsetMin = Vector2.zero;
        _rtRemplissage.offsetMax = Vector2.zero;

        // Texte pourcentage
        GameObject texteGO = new GameObject("Pourcentage");
        texteGO.transform.SetParent(canvasGO.transform, false);
        _texteProgression           = texteGO.AddComponent<TextMeshProUGUI>();
        _texteProgression.text      = "0%";
        _texteProgression.fontSize  = 36;
        _texteProgression.color     = new Color(0.75f, 0.75f, 0.85f, 1f);
        _texteProgression.alignment = TextAlignmentOptions.Center;
        _texteProgression.textWrappingMode = TextWrappingModes.NoWrap;
        RectTransform rtTexte = texteGO.GetComponent<RectTransform>();
        rtTexte.anchorMin = new Vector2(0.15f, 0.37f);
        rtTexte.anchorMax = new Vector2(0.85f, 0.44f);
        rtTexte.offsetMin = Vector2.zero;
        rtTexte.offsetMax = Vector2.zero;
    }

    private static void Etirer(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
