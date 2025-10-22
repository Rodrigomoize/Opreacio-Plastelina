using UnityEngine;
using UnityEngine.UI;

public class SceneBridge : MonoBehaviour
{
    [Header("Multi-Screen Configuration (Optional)")]
    [SerializeField] private Image screenImage;
    [SerializeField] private Button nextButton;
    [SerializeField] private Sprite[] screenSprites;

    [Header("Button Sprites (Optional)")]
    [SerializeField] private Sprite normalButtonSprite;
    [SerializeField] private Sprite finalButtonSprite;

    [Header("Next Scene After Screens")]
    [SerializeField] private string nextSceneAfterScreens;

    private int currentScreenIndex = 0;
    private bool isMultiScreenMode = false;

    private void Start()
    {
        // Solo activar modo multipantalla si hay sprites configurados
        if (screenSprites != null && screenSprites.Length > 0 && screenImage != null && nextButton != null)
        {
            isMultiScreenMode = true;

            // Limpia todos los listeners anteriores del botón
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButtonClicked);

            Debug.Log($"[SceneBridge] Modo multipantalla activado. Total pantallas: {screenSprites.Length}");
            ShowCurrentScreen();
        }
    }

    private void ShowCurrentScreen()
    {
        if (screenImage != null && currentScreenIndex < screenSprites.Length)
        {
            screenImage.sprite = screenSprites[currentScreenIndex];
            Debug.Log($"[SceneBridge] Mostrando pantalla {currentScreenIndex + 1}/{screenSprites.Length}");
        }

        UpdateButtonSprite();
    }

    private void UpdateButtonSprite()
    {
        if (nextButton != null)
        {
            Image buttonImage = nextButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (currentScreenIndex == screenSprites.Length - 1 && finalButtonSprite != null)
                {
                    buttonImage.sprite = finalButtonSprite;
                    Debug.Log("[SceneBridge] Botón cambiado a sprite final");
                }
                else if (normalButtonSprite != null)
                {
                    buttonImage.sprite = normalButtonSprite;
                }
            }
        }
    }

    private void OnNextButtonClicked()
    {
        Debug.Log($"[SceneBridge] Botón clickeado. Pantalla actual: {currentScreenIndex + 1}/{screenSprites.Length}");

        if (currentScreenIndex < screenSprites.Length - 1)
        {
            // No estamos en la última pantalla, avanzar
            currentScreenIndex++;
            ShowCurrentScreen();
        }
        else
        {
            // En la última pantalla, cargar la siguiente escena
            Debug.Log($"[SceneBridge] Última pantalla alcanzada. Cargando escena: {nextSceneAfterScreens}");
            LoadSceneByName(nextSceneAfterScreens);
        }
    }

    private void LoadSceneByName(string sceneName)
    {
        switch (sceneName)
        {
            case "MainMenu":
                LoadMainMenu();
                break;
            case "HistoryScene":
                LoadHistoryScene();
                break;
            case "InstructionScene":
                LoadInstructionScene();
                break;
            case "LevelScene":
                LoadLevelScene();
                break;
            case "PlayScene":
                LoadPlayScene();
                break;
            case "WinScene":
                LoadWinScene();
                break;
            case "LoseScene":
                LoadLoseScene();
                break;
            default:
                Debug.LogWarning($"[SceneBridge] Escena '{sceneName}' no reconocida");
                break;
        }
    }

    // Métodos estáticos (SIN audio, se añadirá desde Unity)
    public static void LoadMainMenu()
    {
        GameManager.GoToMainMenu();
    }

    public static void LoadHistoryScene()
    {
        GameManager.GoToHistoryScene();
    }

    public static void LoadInstructionScene()
    {
        GameManager.GoToInstructionScene();
    }

    public static void LoadLevelScene()
    {
        GameManager.GoToLevelScene();
    }

    public static void LoadPlayScene()
    {
        GameManager.GoToPlayScene();
    }

    public static void LoadWinScene()
    {
        GameManager.GoToWinScene();
    }

    public static void LoadLoseScene()
    {
        GameManager.GoToLoseScene();
    }

    private void OnDestroy()
    {
        if (nextButton != null && isMultiScreenMode)
        {
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
        }
    }
}