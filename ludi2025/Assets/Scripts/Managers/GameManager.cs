using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] UIManager uiManager;
    [SerializeField] AudioManager audioManager;
    [SerializeField] CombatManager combatManager;
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
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("LevelScene");
    }

    public void StopGame()
    {
         SceneManager.LoadScene("PauseMenu");
    }

    public void MainMenuScene()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Settings()
    {
       SceneManager.LoadScene("SettingsMenu");
    }

    public void HelpButton()
    {
        SceneManager.LoadScene("HelpMenu");
    }


}
