using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Pause System")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Image pauseButtonImage;

    [Header("Pause Icons")]
    [SerializeField] private Sprite pauseIcon;
    [SerializeField] private Sprite resumeIcon;

    private bool isPaused = false;

    private void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (pauseButtonImage != null && pauseIcon != null)
        {
            pauseButtonImage.sprite = pauseIcon;
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePause);
        }
    }

    /// Alterna entre pausar y reanudar el juego
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// Pausa el juego
    public void PauseGame()
    {
        isPaused = true;

        // Mostrar el panel de pausa
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        // Cambiar el icono a "resume" (play)
        if (pauseButtonImage != null && resumeIcon != null)
        {
            pauseButtonImage.sprite = resumeIcon;
        }

        // Pausar el tiempo del juego
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
        else
        {
            Time.timeScale = 0f;
        }

        Debug.Log("Juego pausado desde UIManager");
    }

    /// Reanuda el juego
    public void ResumeGame()
    {
        isPaused = false;

        // Ocultar el panel de pausa
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Cambiar el icono a "pause" (||)
        if (pauseButtonImage != null && pauseIcon != null)
        {
            pauseButtonImage.sprite = pauseIcon;
        }

        // Reanudar el tiempo del juego
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
        else
        {
            Time.timeScale = 1f;
        }

        Debug.Log("Juego reanudado desde UIManager");
    }

    /// Método auxiliar para botones del panel de pausa
    public void OnResumeButtonClick()
    {
        ResumeGame();
    }

    /// Volver al menú principal desde el panel de pausa
    public void OnMainMenuButtonClick()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.BackToMainMenu();
        }
    }

    /// Reiniciar el nivel actual
    public void OnRestartButtonClick()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartCurrentLevel();
        }
    }

    private void OnDestroy()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(TogglePause);
        }

        Time.timeScale = 1f;
    }
}