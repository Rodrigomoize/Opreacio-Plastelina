using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.InputSystem.XR;
using static IAController;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("References")]
    [SerializeField] UIManager uiManager;
    [SerializeField] AudioManager audioManager;
    [SerializeField] CardManager combatManager;
    [SerializeField] IAController aiCardManager;
    [SerializeField] PlayerCardManager playerCardManager;
    [SerializeField] GameTimer gameTimerManager;
    // ScoreManager se accede via singleton (ScoreManager.Instance), no necesita ser serializado

    [Header("Defaults")]
    [SerializeField] IAController.AIDificultad defaultAIDifficulty = IAController.AIDificultad.Media;

    [Header("Game State")]
    private bool isPaused = false;
    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    public static void GoToMainMenu()
    {
        if (Instance != null)
        {
            Instance.LoadMainMenu();
        }
    }

    public static void GoToHistoryScene()
    {
        if (Instance != null)
        {
            Instance.LoadHistoryScene();
        }
    }

    public static void GoToInstructionScene()
    {
        if (Instance != null)
        {
            Instance.LoadInstructionScene();
        }
    }

    public static void GoToLevelScene()
    {
        if (Instance != null)
        {
            Instance.LoadLevelScene();
        }
    }

    public static void GoToPlayScene()
    {
        if (Instance != null)
        {
            Instance.LoadPlayScene();
        }
    }

    public static void GoToWinScene()
    {
        if (Instance != null)
        {
            Instance.LoadWinScene();
        }
    }

    public static void GoToLoseScene()
    {
        if (Instance != null)
        {
            Instance.LoadLoseScene();
        }
    }   

    // METODOS PRIVADOS DE CARGA

    private void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void LoadHistoryScene()
    {
        SceneManager.LoadScene("HistoryScene");
    }

    private void LoadInstructionScene()
    {
        SceneManager.LoadScene("InstructionScene");
    }

    private void LoadLevelScene()
    {
        SceneManager.LoadScene("LevelScene");
    }

    private void LoadPlayScene()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene("PlayScene");
        // La inicialización específica de PlayScene se hace en HandleSceneLoaded
    }

    private void LoadWinScene()
    {
        // Detener el timer si está corriendo
        if (gameTimerManager != null && gameTimerManager.IsRunning)
        {
            gameTimerManager.StopTimer();
        }

        // Finalizar el score con bonus por tiempo
        if (gameTimerManager != null && ScoreManager.Instance != null)
        {
            float elapsedSeconds = gameTimerManager.ElapsedSeconds;
            int timeBonus = ScoreManager.Instance.FinalizeScoreWithTime(elapsedSeconds);
            Debug.Log($"[GameManager] Victoria! Tiempo: {elapsedSeconds:F1}s, Bonus: {timeBonus}, Score final: {ScoreManager.Instance.CurrentScore}");
        }

        SceneManager.LoadScene("WinScene");
    }

    private void LoadLoseScene()
    {
        // Detener el timer si está corriendo
        if (gameTimerManager != null && gameTimerManager.IsRunning)
        {
            gameTimerManager.StopTimer();
        }

        SceneManager.LoadScene("LoseScene");
    }

    // PAUSE MENU

    public void PauseGame()
    {
        if (!isPaused)
        {
            isPaused = true;
            Time.timeScale = 0f;
            Debug.Log("[GameManager] Juego pausado (Time.timeScale = 0)");
        }
    }

    public void ResumeGame()
    {
        if (isPaused)
        {
            isPaused = false;
            Time.timeScale = 1f;
            Debug.Log("[GameManager] Juego reanudado (Time.timeScale = 1)");
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void BackToMainMenu()
    {
        LoadMainMenu();
    }

    public void RestartCurrentLevel()
    {
        LoadPlayScene();
    }

    //GETTERS P�BLICOS

    public UIManager GetUIManager() => uiManager;
    public AudioManager GetAudioManager() => audioManager;
    public CardManager GetCombatManager() => combatManager;
    public IAController GetAICardManager() => aiCardManager;
    public PlayerCardManager GetPlayerCardManager() => playerCardManager;
    public GameTimer GetGameTimer() => gameTimerManager;

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;  
            UnsubscribeFromTimer();
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Asignar managers presentes en la escena cargada
        AssignSceneManagers(scene);

        if (scene.name == "PlayScene")
        {
            // Reiniciar puntuación
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetScore();
            }

            // Asegurar que la IA reciba la dificultad por defecto una vez esté presente
            if (aiCardManager != null)
            {
                aiCardManager.SetDificultad(defaultAIDifficulty);
            }
            else
            {
                Debug.LogWarning("[GameManager] IAController no encontrado en PlayScene para asignar dificultad");
            }
            SubscribeToTimer();
        }
        else
        {
            UnsubscribeFromTimer();
        }
    }

    private void SubscribeToTimer()
    {
        if (gameTimerManager != null)
        {
            gameTimerManager.OnCountdownCompleted += HandleTimeoutLose;
            Debug.Log("[GameManager] Suscrito al evento OnCountdownCompleted del timer");
        }
    }

    private void UnsubscribeFromTimer()
    {
        if (gameTimerManager != null)
        {
            gameTimerManager.OnCountdownCompleted -= HandleTimeoutLose;
        }
    }

    private void HandleTimeoutLose()
    {
        Debug.Log("[GameManager] Tiempo agotado. Cargando escena de derrota...");
        LoadLoseScene();
    }

    /// Busca e asigna los managers que existan en la escena especificada.
    /// Solo asigna si el campo aún es null (permite preconfiguración por Inspector).
    private void AssignSceneManagers(Scene scene)
    {
        uiManager = uiManager ?? FindInScene<UIManager>(scene);
        audioManager = audioManager ?? FindInScene<AudioManager>(scene);
        combatManager = combatManager ?? FindInScene<CardManager>(scene);
        aiCardManager = aiCardManager ?? FindInScene<IAController>(scene);
        playerCardManager = playerCardManager ?? FindInScene<PlayerCardManager>(scene);
        gameTimerManager = gameTimerManager ?? FindInScene<GameTimer>(scene);
        // ScoreManager se accede via singleton, no necesita FindInScene

        Debug.Log($"[GameManager] Managers asignados -> UI:{(uiManager != null)} Audio:{(audioManager != null)} Card:{(combatManager != null)} AI:{(aiCardManager != null)} PlayerCard:{(playerCardManager != null)} Timer:{(gameTimerManager != null)} Score:{(ScoreManager.Instance != null)}");
    }

    /// Busca un componente T dentro de la escena proporcionada (solo objetos activos).
    private T FindInScene<T>(Scene scene) where T : MonoBehaviour
    {
        T[] all = FindObjectsByType<T>(FindObjectsSortMode.None);
        foreach (var item in all)
        {
            if (item.gameObject.scene == scene) return item;
        }
        return null;
    }
}