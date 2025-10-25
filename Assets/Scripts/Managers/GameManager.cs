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
    [SerializeField] DifficultyManager difficultyManager;

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
        if (ScoreManager.Instance != null)
        {
            float elapsedSeconds = 0f;
            
            // Validar que el timer existe y es válido antes de obtener el tiempo
            if (gameTimerManager != null && gameTimerManager.gameObject != null)
            {
                elapsedSeconds = gameTimerManager.ElapsedSeconds;
            }
            else
            {
                Debug.LogWarning("[GameManager] GameTimer no válido al finalizar, usando tiempo 0");
            }
            
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
        // DETENER Y RESETEAR completamente la música de gameplay antes de volver al menú
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAndResetMusic();
        }

        LoadMainMenu();
    }

    public void RestartCurrentLevel()
    {
        // DETENER Y RESETEAR completamente la música de gameplay antes de reiniciar
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAndResetMusic();
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
        
        // Delegar al DifficultyManager
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.SetDifficulty(dificultad);
        }
        else
        {
            Debug.LogWarning("[GameManager] DifficultyManager no encontrado");
        }
    }

    /// Aplica la velocidad de juego correspondiente a la dificultad actual
    private void ApplyDifficultySpeed()
    {
        // Ahora lo maneja el DifficultyManager
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.ApplyDifficultySettings();
        }
    }

    /// Obtiene la duración de la partida según la dificultad actual
    private float GetDurationForDifficulty()
    {
        if (DifficultyManager.Instance != null)
        {
            return DifficultyManager.Instance.GetGameDuration();
        }
        return 180f; // Valor por defecto
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

            // Reiniciar y arrancar el timer con la duración según dificultad
            if (gameTimerManager != null)
            {
                float duracion = GetDurationForDifficulty();
                gameTimerManager.ResetTimer();
                gameTimerManager.StartCountdown(duracion);
                
                string dificultadActual = DifficultyManager.Instance != null 
                    ? DifficultyManager.Instance.CurrentDifficulty.ToString() 
                    : defaultAIDifficulty.ToString();
                    
                Debug.Log($"[GameManager] Timer reseteado y arrancado con duración de {duracion}s para dificultad {dificultadActual}");
            }

            // Iniciar la música de gameplay desde 0
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
            ApplyDifficultySpeed();

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
        
        // IMPORTANTE: Siempre reasignar el timer y score manager de la escena actual
        // porque estas referencias se destruyen al cambiar de escena
        gameTimerManager = FindInScene<GameTimer>(scene);
        scoreManager = FindInScene<ScoreManager>(scene);
        
        difficultyManager = difficultyManager ?? FindInScene<DifficultyManager>(scene);

        Debug.Log($"[GameManager] Managers asignados -> UI:{(uiManager != null)} Audio:{(audioManager != null)} Card:{(combatManager != null)} AI:{(aiCardManager != null)} PlayerCard:{(playerCardManager != null)} Timer:{(gameTimerManager != null)} Difficulty:{(difficultyManager != null)}");
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