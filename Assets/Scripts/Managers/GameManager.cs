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
    [SerializeField] ScoreManager scoreManager;

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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMainMenuMusic();
        }
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

        // Mantener la música del MainMenu (no cambiar)
    }

    private void LoadPlayScene()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene("PlayScene");
        // La música se iniciará en HandleSceneLoaded para asegurar que empiece desde 0
    }

    private void LoadWinScene()
    {
        // DETENER Y RESETEAR completamente la música de gameplay antes de cambiar de escena
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAndResetMusic();
        }

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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVictoryMusic();
        }
    }

    private void LoadLoseScene()
    {
        // DETENER completamente la música de gameplay antes de cambiar de escena
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(fadeOut: false);
        }

        // Detener el timer si está corriendo
        if (gameTimerManager != null && gameTimerManager.IsRunning)
        {
            gameTimerManager.StopTimer();
        }

        SceneManager.LoadScene("LoseScene");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDefeatMusic();
        }
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
        // DETENER completamente la música de gameplay antes de volver al menú
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(fadeOut: false);
        }

        LoadMainMenu();
    }

    public void RestartCurrentLevel()
    {
        // DETENER completamente la música de gameplay antes de reiniciar
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(fadeOut: false);
        }

        LoadPlayScene();
    }

    //GETTERS PBLICOS

    public UIManager GetUIManager() => uiManager;
    public AudioManager GetAudioManager() => audioManager;
    public CardManager GetCombatManager() => combatManager;
    public IAController GetAICardManager() => aiCardManager;
    public PlayerCardManager GetPlayerCardManager() => playerCardManager;
    public GameTimer GetGameTimer() => gameTimerManager;

    // ===== MÉTODO PARA CAMBIAR DIFICULTAD =====
    public void SetDificultad(IAController.AIDificultad dificultad)
    {
        defaultAIDifficulty = dificultad;
        Debug.Log($"[GameManager] Dificultad cambiada a: {dificultad}");

        // Actualizar la velocidad del juego según la dificultad
        if (GameSpeedManager.Instance != null)
        {
            GameSpeedManager.GameDifficulty speedDifficulty = dificultad switch
            {
                IAController.AIDificultad.Facil => GameSpeedManager.GameDifficulty.Facil,
                IAController.AIDificultad.Media => GameSpeedManager.GameDifficulty.Media,
                IAController.AIDificultad.Dificil => GameSpeedManager.GameDifficulty.Dificil,
                _ => GameSpeedManager.GameDifficulty.Media
            };
            GameSpeedManager.Instance.SetSpeedByDifficulty(speedDifficulty);
        }
    }

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
                Debug.Log("[GameManager] Score reseteado para nueva partida");
            }

            // Reiniciar y arrancar el timer
            if (gameTimerManager != null)
            {
                gameTimerManager.ResetTimer();
                gameTimerManager.StartCountdown(gameTimerManager.CountdownDuration);
                Debug.Log("[GameManager] Timer reseteado y arrancado para nueva partida");
            }

            // IMPORTANTE: Iniciar la música de gameplay desde 0
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameplayMusic();
                Debug.Log("[GameManager] Música de gameplay iniciada desde el principio");
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

            // Aplicar la velocidad de juego según la dificultad seleccionada
            if (GameSpeedManager.Instance != null)
            {
                GameSpeedManager.GameDifficulty speedDifficulty = defaultAIDifficulty switch
                {
                    IAController.AIDificultad.Facil => GameSpeedManager.GameDifficulty.Facil,
                    IAController.AIDificultad.Media => GameSpeedManager.GameDifficulty.Media,
                    IAController.AIDificultad.Dificil => GameSpeedManager.GameDifficulty.Dificil,
                    _ => GameSpeedManager.GameDifficulty.Media
                };
                GameSpeedManager.Instance.SetSpeedByDifficulty(speedDifficulty);
                Debug.Log($"[GameManager] Velocidad de juego aplicada para dificultad: {defaultAIDifficulty}");
            }
            else
            {
                Debug.LogWarning("[GameManager] GameSpeedManager no encontrado en PlayScene");
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
        scoreManager = scoreManager ?? FindInScene<ScoreManager>(scene);

        Debug.Log($"[GameManager] Managers asignados -> UI:{(uiManager != null)} Audio:{(audioManager != null)} Card:{(combatManager != null)} AI:{(aiCardManager != null)} PlayerCard:{(playerCardManager != null)} Timer:{(gameTimerManager != null)}");
    }

    /// Busca un componente T dentro de la escena proporcionada (solo objetos activos).
    private T FindInScene<T>(Scene scene) where T : MonoBehaviour
    {
        T[] all = FindObjectsOfType<T>();
        foreach (var item in all)
        {
            if (item.gameObject.scene == scene) return item;
        }
        return null;
    }
}