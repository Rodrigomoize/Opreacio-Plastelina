using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la UI de la pantalla de resultado (win/lose) con dos botones:
/// - Volver a jugar (reinicia la PlayScene)
/// - Salir al menú (MainMenu)
/// </summary>
public class GameResultUI : MonoBehaviour
{
    [SerializeField] private Button replayButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        if (replayButton != null)
        {
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(OnReplayClicked);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    private void OnDestroy()
    {
        if (replayButton != null) replayButton.onClick.RemoveListener(OnReplayClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
    }

    private void OnReplayClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartCurrentLevel();
        }
    }

    private void OnMainMenuClicked()
    {
        GameManager.GoToMainMenu();
    }
}
