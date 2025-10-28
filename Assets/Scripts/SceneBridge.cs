using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SceneBridge : MonoBehaviour
{
    [Header("Multi-Screen Configuration (Optional)")]
    [SerializeField] private Image screenImage;
    [SerializeField] private Button nextButton;
    [SerializeField] private Sprite[] screenSprites;

    [Header("Button Sprites (Optional)")]
    [SerializeField] private Sprite normalButtonSprite;
    [SerializeField] private Sprite finalButtonSprite;

    [Header("Button Audio Events (Optional)")]
    [Tooltip("Evento que se ejecuta cuando se presiona el bot�n en pantallas intermedias")]
    [SerializeField] private UnityEvent onNormalButtonClick;
    [Tooltip("Evento que se ejecuta cuando se presiona el bot�n en la �ltima pantalla")]
    [SerializeField] private UnityEvent onFinalButtonClick;

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

            // Limpia todos los listeners anteriores del bot�n
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButtonClicked);

            ShowCurrentScreen();
        }
    }

    private void ShowCurrentScreen()
    {
        if (screenImage != null && currentScreenIndex < screenSprites.Length)
        {
            screenImage.sprite = screenSprites[currentScreenIndex];
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

        // Reproducir el evento de audio apropiado seg�n si es la �ltima pantalla o no
        PlayAppropriateAudioEvent();

        if (currentScreenIndex < screenSprites.Length - 1)
        {
            // No estamos en la �ltima pantalla, avanzar
            currentScreenIndex++;
            ShowCurrentScreen();
        }
        else
        {
            // En la �ltima pantalla, cargar la siguiente escena
            LoadSceneByName(nextSceneAfterScreens);
        }
    }

    /// Invoca el UnityEvent apropiado seg�n si estamos en la �ltima pantalla o no
    private void PlayAppropriateAudioEvent()
    {
        if (currentScreenIndex == screenSprites.Length - 1)
        {
            // �ltima pantalla: invocar evento final
            if (onFinalButtonClick != null && onFinalButtonClick.GetPersistentEventCount() > 0)
            {
                onFinalButtonClick.Invoke();
            }
        }
        else
        {
            // Pantallas intermedias: invocar evento normal
            if (onNormalButtonClick != null && onNormalButtonClick.GetPersistentEventCount() > 0)
            {
                onNormalButtonClick.Invoke();
            }
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

    // M�todos est�ticos (SIN audio, se a�adir� desde Unity)
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