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
    [SerializeField] AICardManager aiCardManager;
    [SerializeField] PlayerCardManager playerCardManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
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
        else
        {
            Debug.LogError("GameManager.Instance es null.");
        }
    }

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
        SceneManager.LoadScene("PlayScene");
    }

    public UIManager GetUIManager() => uiManager;
    public AudioManager GetAudioManager() => audioManager;
    public CardManager GetCombatManager() => combatManager;
    public AICardManager GetAICardManager() => aiCardManager;
    public PlayerCardManager GetPlayerCardManager() => playerCardManager;
}
