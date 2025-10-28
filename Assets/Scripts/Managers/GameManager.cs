using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.InputSystem.XR;
using static IAController;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] UIManager uiManager;
    [SerializeField] AudioManager audioManager;
    [SerializeField] CardManager combatManager;
    [SerializeField] IAController aiCardManager;
    [SerializeField] PlayerCardManager playerCardManager;
    [SerializeField] GameTimer gameTimerManager;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] DifficultyManager difficultyManager;

    [Header("Difficulty Settings")]
    [Tooltip("Dificultad actual seleccionada por el jugador")]
    [SerializeField] private IAController.AIDificultad currentSelectedDifficulty = IAController.AIDificultad.Media;
    
    [Header("Game State")]
    private bool isPaused = false;
    public bool IsPaused => isPaused;
    
    private bool gameplayDisabled = false;
    public bool IsGameplayDisabled => gameplayDisabled;
    
    public IAController.AIDificultad CurrentDifficulty => currentSelectedDifficulty;

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

    public static void GoToTutorialScene()
    {
        if (Instance != null)
        {
            Instance.LoadTutorialScene();
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
    }

    private void LoadTutorialScene()
    {
        SceneManager.LoadScene("TutorialScene");
    }

    private void LoadWinScene()
    {
        // DETENER Y RESETEAR completamente la música de gameplay
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
            
            if (gameTimerManager != null && gameTimerManager.gameObject != null)
            {
                elapsedSeconds = gameTimerManager.ElapsedSeconds;
            }
            else
            {
                Debug.LogWarning("[GameManager] GameTimer no válido al finalizar, usando tiempo 0");
            }
            
            int timeBonus = ScoreManager.Instance.FinalizeScoreWithTime(elapsedSeconds);
        }

        SceneManager.LoadScene("WinScene");
    }

    private void LoadLoseScene()
    {
        // DETENER Y RESETEAR completamente la música de gameplay
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
    }

    // PAUSE MENU

    public void PauseGame()
    {
        if (!isPaused)
        {
            isPaused = true;
            Time.timeScale = 0f;
        }
    }

    public void ResumeGame()
    {
        if (isPaused)
        {
            isPaused = false;
            Time.timeScale = 1f;
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    // DISABLE GAMEPLAY (sin afectar Time.timeScale - para secuencias cinemáticas)

    public void DisableGameplay()
    {
        gameplayDisabled = true;
    }

    public void EnableGameplay()
    {
        gameplayDisabled = false;
    }

    /// Detiene todas las tropas en el campo durante la secuencia de victoria.
    /// Congela su movimiento y desactiva su capacidad de combate/interacción.
    public void FreezeAllTroops()
    {
        
        // Buscar todos los Characters (tropas individuales)
        Character[] characters = FindObjectsByType<Character>(FindObjectsSortMode.None);
        foreach (Character character in characters)
        {
            if (character != null && character.gameObject != null)
            {
                // Detener el NavMeshAgent
                UnityEngine.AI.NavMeshAgent agent = character.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null && agent.enabled)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                }
                
                // Marcar como en combate para evitar nuevas interacciones
                character.isInCombat = true;
                
            }
        }
        
        // Buscar todos los CharacterCombined (camiones/operaciones)
        CharacterCombined[] combinedCharacters = FindObjectsByType<CharacterCombined>(FindObjectsSortMode.None);
        foreach (CharacterCombined combined in combinedCharacters)
        {
            if (combined != null && combined.gameObject != null)
            {
                // Detener el NavMeshAgent
                UnityEngine.AI.NavMeshAgent agent = combined.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null && agent.enabled)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                }
                
                // Marcar como en combate para evitar nuevas interacciones
                combined.isInCombat = true;
                
            }
        }
        
    }

    public void BackToMainMenu()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAndResetMusic();
        }

        // Resetear todos los estados antes de volver al menú
        ResumeGame();
        EnableGameplay();
        
        LoadMainMenu();
    }

    public void RestartCurrentLevel()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAndResetMusic();
        }
        
        // Resetear todos los estados antes de cargar la escena
        ResumeGame();
        EnableGameplay();
        
        LoadPlayScene();
    }

    // GETTERS PUBLICOS

    public UIManager GetUIManager() => uiManager;
    public AudioManager GetAudioManager() => audioManager;
    public CardManager GetCombatManager() => combatManager;
    public IAController GetAICardManager() => aiCardManager;
    public PlayerCardManager GetPlayerCardManager() => playerCardManager;
    public GameTimer GetGameTimer() => gameTimerManager;

    public void SetDificultad(IAController.AIDificultad dificultad)
    {
        currentSelectedDifficulty = dificultad;
        
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.SetDifficulty(dificultad);
        }
        else
        {
            Debug.LogWarning("[GameManager] DifficultyManager no encontrado");
        }
    }

    private void ApplyDifficultySpeed()
    {
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.ApplyDifficultySettings();
        }
    }

    private float GetDurationForDifficulty()
    {
        if (DifficultyManager.Instance != null)
        {
            return DifficultyManager.Instance.GetGameDuration();
        }
        return 180f;
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
        // Resetear estado de gameplay al cambiar de escena
        gameplayDisabled = false;

        AssignSceneManagers(scene);

        string[] menuScenes = { "MainMenu", "HistoryScene", "InstructionScene", "LevelScene" };
        bool isMenuScene = System.Array.Exists(menuScenes, s => s == scene.name);

        if (isMenuScene)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMainMenuMusic();
            }

            UnsubscribeFromTimer();
        }
        else if (scene.name == "PlayScene")
        {
            // DETENER música de menú antes de iniciar gameplay
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopAndResetMusic();
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetScore();
            }

            if (gameTimerManager != null)
            {
                float duracion = GetDurationForDifficulty();
                gameTimerManager.ResetTimer();
                gameTimerManager.StartCountdown(duracion);

                string dificultadActual = DifficultyManager.Instance != null
                    ? DifficultyManager.Instance.CurrentDifficulty.ToString()
                    : currentSelectedDifficulty.ToString();

            }

            // Reproducir música de gameplay
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameplayMusic();
            }

            if (aiCardManager != null)
            {
                aiCardManager.SetDificultad(currentSelectedDifficulty);
            }
            else
            {
                Debug.LogWarning("[GameManager] IAController no encontrado en PlayScene para asignar dificultad");
            }

            ApplyDifficultySpeed();
            SubscribeToTimer();
        }
        else if (scene.name == "WinScene")
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopAndResetMusic();
                AudioManager.Instance.PlayVictorySFX();
            }
            UnsubscribeFromTimer();
        }
        else if (scene.name == "LoseScene")
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopAndResetMusic();
                AudioManager.Instance.PlayDefeatSFX();
            }
            UnsubscribeFromTimer();
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
        }
        else
        {
            Debug.LogWarning("[GameManager] No se pudo suscribir al timer: gameTimerManager es null");
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
        LoadLoseScene();
    }

    private void AssignSceneManagers(Scene scene)
    {
        if (scene.name == "PlayScene")
        {
            uiManager = FindInScene<UIManager>(scene);
            
            if (uiManager != null)
            {
                uiManager.Reinitialize();
            }
            else
            {
                Debug.LogWarning("[GameManager] ⚠️ No se encontró UIManager en PlayScene");
            }
        }
        else
        {
            uiManager = uiManager ?? FindInScene<UIManager>(scene);
        }
        
        audioManager = audioManager ?? FindInScene<AudioManager>(scene);
        combatManager = combatManager ?? FindInScene<CardManager>(scene);
        
        if (scene.name == "PlayScene")
        {
            aiCardManager = FindInScene<IAController>(scene);
            playerCardManager = FindInScene<PlayerCardManager>(scene);
        }
        else
        {
            aiCardManager = aiCardManager ?? FindInScene<IAController>(scene);
            playerCardManager = playerCardManager ?? FindInScene<PlayerCardManager>(scene);
        }
        
        gameTimerManager = FindInScene<GameTimer>(scene);
        scoreManager = FindInScene<ScoreManager>(scene);
        
        difficultyManager = difficultyManager ?? FindInScene<DifficultyManager>(scene);

    }

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