using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Menu, Preparation, Defense, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Menu;

    public int CurrentWave { get; private set; }
    public float PrepTimeRemaining { get; private set; }
    public bool IsVictory { get; private set; }

    [Header("Phase de préparation")]
    [Tooltip("Durée du timer de préparation en secondes.")]
    [SerializeField] private float prepDuration = 30f;

    public static event Action<GameState> OnPhaseChanged;
    public static event Action<int> OnWaveChanged;
    public static event Action<float> OnPrepTimerUpdated;
    public static event Action<bool> OnGameEnded;

    private bool _prepRunning;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (_prepRunning)
            TickPrepTimer();
    }

    public void StartGame()
    {
        CurrentWave = 0;
        SceneManager.sceneLoaded += OnGameSceneLoaded;
        ChargerViaEcranChargement("Game");
    }

    private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Game") return;
        SceneManager.sceneLoaded -= OnGameSceneLoaded;
        
        Debug.Log("[GameManager] Scène Game chargée - initialisation...");
        
        // Ajuster le zoom des caméras
        AdjustCameraZoom();
        
        // Initialiser le menu de pause de la scène
        InitializePauseMenu();
        
        Debug.Log("[GameManager] Initialisation complète");
        EnterPreparationPhase();
    }

    private void AdjustCameraZoom()
    {
        // Dézoomez les caméras du jeu (Game scene)
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        Debug.Log($"[GameManager] {cameras.Length} caméra(s) trouvée(s)");
        
        foreach (Camera cam in cameras)
        {
            if (cam.orthographic && (cam.gameObject.name == "Camera_P1" || cam.gameObject.name == "Camera_P2"))
            {
                Debug.Log($"[GameManager] {cam.gameObject.name}: Size actuelle = {cam.orthographicSize}");
                cam.orthographicSize = 10f;  // Réduit de 6 à 5 pour dézoomzer
                Debug.Log($"[GameManager] {cam.gameObject.name}: Zoom ajusté à 10");
            }
        }
    }

    private void InitializePauseMenu()
    {
        // S'assurer que le PauseMenuController existe
        if (PauseMenuController.Instance == null)
        {
            Debug.Log("[GameManager] Création du PauseMenuController...");
            GameObject pauseManagerGO = new GameObject("PauseMenuManager");
            pauseManagerGO.AddComponent<PauseMenuController>();
            Debug.Log("[GameManager] PauseMenuController créé");
        }
        else
        {
            Debug.Log("[GameManager] PauseMenuController existe déjà");
        }
    }

    public void EnterPreparationPhase()
    {
        CurrentState = GameState.Preparation;
        PrepTimeRemaining = prepDuration;
        _prepRunning = true;

        InputManager.Instance?.SetPlayerInputEnabled(1, true);
        InputManager.Instance?.SetPlayerInputEnabled(2, true);

        OnPhaseChanged?.Invoke(CurrentState);
    }

    private void TickPrepTimer()
    {
        PrepTimeRemaining -= Time.deltaTime;

        if (PrepTimeRemaining <= 0f)
        {
            PrepTimeRemaining = 0f;
            _prepRunning = false;
        }

        OnPrepTimerUpdated?.Invoke(PrepTimeRemaining);
    }

    public void EnterDefensePhase()
    {
        _prepRunning = false;
        CurrentState = GameState.Defense;
        CurrentWave++;

        OnPhaseChanged?.Invoke(CurrentState);
        OnWaveChanged?.Invoke(CurrentWave);
    }

    public void WaveCompleted()
    {
        EnterPreparationPhase();
    }

    public void TriggerGameOver(bool victory)
    {
        IsVictory = victory;
        CurrentState = GameState.GameOver;
        _prepRunning = false;

        OnPhaseChanged?.Invoke(CurrentState);
        OnGameEnded?.Invoke(victory);

        ChargerViaEcranChargement("GameOver");
    }

    public void ReturnToMenu()
    {
        CurrentState = GameState.Menu;
        SceneManager.LoadScene("MainMenu");
    }

    private void ChargerViaEcranChargement(string scene)
    {
        LoadingScreenController.SceneCible = scene;
        SceneManager.LoadScene("Loading");
    }
}
