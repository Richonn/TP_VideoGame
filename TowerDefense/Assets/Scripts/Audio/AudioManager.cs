using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public enum MusicTrack
{
    None,
    Menu,
    Preparation,
    DefenseLight,
    DefenseIntense,
    Victory,
    Defeat
}

public static class VolumeKeys
{
    public const string Master  = "vol_master";
    public const string Music   = "vol_music";
    public const string SFX     = "vol_sfx";
    public const string Ambient = "vol_ambient";
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup ambientGroup;

    [Header("Library")]
    [SerializeField] private SFXLibrary sfxLibrary;

    [System.Serializable]
    public class MusicEntry
    {
        public MusicTrack track;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.7f;
    }
    [SerializeField] private MusicEntry[] musicEntries;

    [Header("Mixer Parameters")]
    [SerializeField] private string masterParam = "MasterVolume";
    [SerializeField] private string musicParam = "MusicVolume";
    [SerializeField] private string sfxParam = "SFXVolume";
    [SerializeField] private string ambientParam = "AmbientVolume";

    [Header("Pool")]
    [SerializeField] private int sfxPrewarm = 12;

    [Header("Ambient")]
    [SerializeField] private AudioClip ambientClip;
    [SerializeField, Range(0f, 1f)] private float ambientVolume = 0.5f;
    [SerializeField] private float ambientFadeTime = 1.5f;
    [SerializeField] private string ambientSceneName = "Game";

    private AudioSourcePool _sfxPool;
    private AudioSource _musicA;
    private AudioSource _musicB;
    private AudioSource _ambient;
    private bool _usingA = true;
    private MusicTrack _currentTrack = MusicTrack.None;

    public MusicTrack CurrentMusic => _currentTrack;
    public AudioMixerGroup MusicGroup => musicGroup;

    private AudioListener _fallbackListener;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (ambientClip == null)
            ambientClip = Resources.Load<AudioClip>("Audio/ambient_wind");

        _sfxPool = new AudioSourcePool(transform, sfxGroup, sfxPrewarm);

        _musicA = CreateMusicSource("MusicA");
        _musicB = CreateMusicSource("MusicB");
        _ambient = CreateAmbientSource("AmbientLoop");

        _fallbackListener = GetComponent<AudioListener>();
        if (_fallbackListener == null)
            _fallbackListener = gameObject.AddComponent<AudioListener>();
        SyncListener();
    }

    void Start()
    {
        SetMasterVolume(PlayerPrefs.GetFloat(VolumeKeys.Master, 0.8f));
        SetMusicVolume(PlayerPrefs.GetFloat(VolumeKeys.Music, 0.7f));
        SetSFXVolume(PlayerPrefs.GetFloat(VolumeKeys.SFX, 0.8f));
        SetAmbientVolume(PlayerPrefs.GetFloat(VolumeKeys.Ambient, 0.6f));
    }

    void OnEnable()
    {
        GameManager.OnPhaseChanged += OnPhaseChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        GameManager.OnPhaseChanged -= OnPhaseChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SyncListener();
        UpdateAmbient(scene.name);
    }

    private void UpdateAmbient(string sceneName)
    {
        if (_ambient == null) return;
        bool shouldPlay = ambientClip != null && sceneName == ambientSceneName;

        if (shouldPlay)
        {
            if (_ambient.outputAudioMixerGroup != ambientGroup) _ambient.outputAudioMixerGroup = ambientGroup;
            if (_ambient.clip != ambientClip) _ambient.clip = ambientClip;
            _ambient.loop = true;
            _ambient.mute = false;
            if (!_ambient.isPlaying) _ambient.Play();
            StartCoroutine(FadeMusic(_ambient, ambientVolume, ambientFadeTime));
        }
        else
        {
            StartCoroutine(FadeMusic(_ambient, 0f, ambientFadeTime));
        }
    }

    private void SyncListener()
    {
        if (_fallbackListener == null) return;
        AudioListener[] all = FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        bool hasOther = false;
        foreach (AudioListener l in all)
        {
            if (l != null && l != _fallbackListener && l.enabled)
            {
                hasOther = true;
                break;
            }
        }
        _fallbackListener.enabled = !hasOther;
    }

    private void OnPhaseChanged(GameManager.GameState state)
    {
        if (DynamicMusicDirector.Instance != null) return;

        switch (state)
        {
            case GameManager.GameState.Menu:        PlayMusic(MusicTrack.Menu); break;
            case GameManager.GameState.Preparation: PlayMusic(MusicTrack.Preparation); break;
            case GameManager.GameState.Defense:     PlayMusic(MusicTrack.DefenseLight); break;
            case GameManager.GameState.GameOver:    break;
        }
    }

    private AudioSource CreateMusicSource(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.outputAudioMixerGroup = musicGroup;
        src.volume = 0f;
        return src;
    }

    private AudioSource CreateAmbientSource(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.outputAudioMixerGroup = ambientGroup;
        src.volume = 0f;
        return src;
    }

    void Update()
    {
        _sfxPool?.Tick();
    }

    public void PlaySFX(SFXType type, Vector3? worldPos = null, float volumeScale = 1f)
    {
        if (sfxLibrary == null) return;
        AudioClip clip = sfxLibrary.GetRandomClip(type);
        if (clip == null) return;

        SFXLibrary.Entry entry = sfxLibrary.GetEntry(type);
        AudioSource src = _sfxPool.Get();
        src.clip = clip;
        src.volume = (entry != null ? entry.volume : 1f) * volumeScale;
        src.pitch = 1f + Random.Range(-(entry?.pitchJitter ?? 0f), entry?.pitchJitter ?? 0f);
        src.outputAudioMixerGroup = sfxGroup;

        if (worldPos.HasValue)
        {
            src.spatialBlend = 1f;
            src.minDistance = 2f;
            src.maxDistance = 25f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.transform.position = worldPos.Value;
        }
        else
        {
            src.spatialBlend = 0f;
            src.transform.position = Vector3.zero;
        }

        src.Play();
    }

    public void PlayMusic(MusicTrack track, float fadeTime = 1.5f)
    {
        if (track == _currentTrack) return;
        _currentTrack = track;

        AudioClip clip = GetClip(track);
        float targetVol = GetVolume(track);

        AudioSource fadeIn = _usingA ? _musicB : _musicA;
        AudioSource fadeOut = _usingA ? _musicA : _musicB;
        _usingA = !_usingA;

        if (clip == null)
        {
            StartCoroutine(FadeMusic(fadeOut, 0f, fadeTime));
            return;
        }

        fadeIn.clip = clip;
        fadeIn.volume = 0f;
        fadeIn.Play();
        StartCoroutine(FadeMusic(fadeIn, targetVol, fadeTime));
        StartCoroutine(FadeMusic(fadeOut, 0f, fadeTime));
    }

    private IEnumerator FadeMusic(AudioSource src, float target, float duration)
    {
        if (src == null) yield break;
        float start = src.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(start, target, Easing.Evaluate(Easing.Ease.EaseInOutSine, t / duration));
            yield return null;
        }
        src.volume = target;
        if (target <= 0.001f) src.Stop();
    }

    private AudioClip GetClip(MusicTrack track)
    {
        if (musicEntries == null) return null;
        foreach (MusicEntry e in musicEntries)
            if (e != null && e.track == track) return e.clip;
        return null;
    }

    private float GetVolume(MusicTrack track)
    {
        if (musicEntries == null) return 0.7f;
        foreach (MusicEntry e in musicEntries)
            if (e != null && e.track == track) return e.volume;
        return 0.7f;
    }

    public void SetMasterVolume(float linear) => SetMixer(masterParam, linear);
    public void SetMusicVolume(float linear)  => SetMixer(musicParam, linear);
    public void SetSFXVolume(float linear)    => SetMixer(sfxParam, linear);
    public void SetAmbientVolume(float linear) => SetMixer(ambientParam, linear);

    private void SetMixer(string param, float linear)
    {
        if (mixer == null || string.IsNullOrEmpty(param)) return;
        float db = linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;
        mixer.SetFloat(param, db);
    }
}
