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

    [Header("Preparation Phase")]
    [SerializeField] private float prepDuration = 30f;

    public static event Action<GameState> OnPhaseChanged;
    public static event Action<int> OnWaveChanged;
    public static event Action<bool> OnGameEnded;

    private bool _prepRunning;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update() {}

    public void StartGame()
    {
        CurrentWave = 0;
        SceneManager.sceneLoaded += OnGameSceneLoaded;
        LoadWithLoadingScreen("Game");
    }

    private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Game") return;
        SceneManager.sceneLoaded -= OnGameSceneLoaded;

        AdjustCameraZoom();
        InitializePauseMenu();
        InitializeMinimap();
        EnterPrepPhase();
    }

    private void AdjustCameraZoom()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

        foreach (Camera cam in cameras)
        {
            if (cam.orthographic && (cam.gameObject.name == "Camera_P1" || cam.gameObject.name == "Camera_P2"))
                cam.orthographicSize = 10f;
        }
    }

    private void InitializePauseMenu()
    {
        if (PauseMenuController.Instance == null)
        {
            GameObject go = new GameObject("PauseMenuManager");
            go.AddComponent<PauseMenuController>();
        }
    }

    private void InitializeMinimap()
    {
        if (MinimapController.Instance == null)
        {
            GameObject go = new GameObject("MinimapController");
            go.AddComponent<MinimapController>();
        }
    }

    public void EnterPrepPhase()
    {
        CurrentState = GameState.Preparation;
        PrepTimeRemaining = prepDuration;
        _prepRunning = true;

        InputManager.Instance?.SetPlayerInputEnabled(1, true);
        InputManager.Instance?.SetPlayerInputEnabled(2, true);

        OnPhaseChanged?.Invoke(CurrentState);
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
        EnterPrepPhase();
    }

    public void TriggerGameOver(bool victory)
    {
        IsVictory = victory;
        CurrentState = GameState.GameOver;
        _prepRunning = false;

        OnPhaseChanged?.Invoke(CurrentState);
        OnGameEnded?.Invoke(victory);

        SceneManager.LoadScene("GameOver");
    }

    public void ReturnToMenu()
    {
        CurrentState = GameState.Menu;
        SceneManager.LoadScene("MainMenu");
    }

    private void LoadWithLoadingScreen(string scene)
    {
        LoadingScreenController.TargetScene = scene;
        SceneManager.LoadScene("Loading");
    }
}
