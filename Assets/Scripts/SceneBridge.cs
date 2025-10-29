using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SceneBridge : MonoBehaviour
{
    [Header("Multi-Screen Configuration")]
    [SerializeField] private Image screenImage;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Sprite[] screenSprites;

    [Header("Button Audio Events")]
    [SerializeField] private UnityEvent onNextButtonClick;
    [SerializeField] private UnityEvent onPreviousButtonClick;
    [SerializeField] private UnityEvent onFinalButtonClick;

    [Header("Next Scene After Screens")]
    [SerializeField] private string nextSceneAfterScreens;

    private int currentScreenIndex = 0;
    private bool isMultiScreenMode = false;

    private void Start()
    {
        if (screenSprites != null && screenSprites.Length > 0 && screenImage != null && nextButton != null && previousButton != null)
        {
            isMultiScreenMode = true;

            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButtonClicked);

            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(OnPreviousButtonClicked);

            ShowCurrentScreen();
        }
    }

    private void ShowCurrentScreen()
    {
        if (screenImage != null && currentScreenIndex < screenSprites.Length)
        {
            screenImage.sprite = screenSprites[currentScreenIndex];
        }

        UpdateButtonsState();
    }

    private void UpdateButtonsState()
    {
        if (previousButton != null)
        {
            previousButton.interactable = currentScreenIndex > 0;
        }

        if (nextButton != null)
        {
            nextButton.interactable = true;
        }
    }

    private void OnNextButtonClicked()
    {
        if (currentScreenIndex < screenSprites.Length - 1)
        {
            if (onNextButtonClick != null && onNextButtonClick.GetPersistentEventCount() > 0)
            {
                onNextButtonClick.Invoke();
            }

            currentScreenIndex++;
            ShowCurrentScreen();
        }
        else
        {
            if (onFinalButtonClick != null && onFinalButtonClick.GetPersistentEventCount() > 0)
            {
                onFinalButtonClick.Invoke();
            }

            LoadSceneByName(nextSceneAfterScreens);
        }
    }

    private void OnPreviousButtonClicked()
    {
        if (currentScreenIndex > 0)
        {
            if (onPreviousButtonClick != null && onPreviousButtonClick.GetPersistentEventCount() > 0)
            {
                onPreviousButtonClick.Invoke();
            }

            currentScreenIndex--;
            ShowCurrentScreen();
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
                break;
        }
    }

    public static void LoadMainMenu()
    {
        GameManager.GoToMainMenu();
    }

    public static void LoadHistoryScene()
    {
        GameManager.GoToHistoryScene();
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
        if (isMultiScreenMode)
        {
            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(OnNextButtonClicked);
            }

            if (previousButton != null)
            {
                previousButton.onClick.RemoveListener(OnPreviousButtonClicked);
            }
        }
    }
}