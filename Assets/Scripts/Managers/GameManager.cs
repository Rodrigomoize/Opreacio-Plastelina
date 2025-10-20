using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    // M�TODOS PRIVADOS DE CARGA

    private void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
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
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PlayScene")
        {
            // Reiniciar puntuación
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetScore();
            }
        }
    }

    // Navegación a escenas de resultado
    public void GoToWinScene()
    {
        SceneManager.LoadScene("WinScene");
    }

    public void GoToLoseScene()
    {
        SceneManager.LoadScene("LoseScene");
    }
}
