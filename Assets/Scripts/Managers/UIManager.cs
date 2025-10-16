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
        // Panel de pausa oculto al inicio
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (pauseButtonImage != null && pauseIcon != null)
        {
            pauseButtonImage.sprite = pauseIcon;
        }

        // Conecta el botón de pausa automáticamente
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(TogglePause);
        }

        Debug.Log("[UIManager] Inicializado correctamente");
    }

    /// Este método se llama AUTOMÁTICAMENTE desde el botón (sin necesidad de asignarlo)
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

    /// Pausa el juego: muestra el panel y cambia el icono
    private void PauseGame()
    {
        isPaused = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (pauseButtonImage != null && resumeIcon != null)
        {
            pauseButtonImage.sprite = resumeIcon;
        }

        // Llamar al GameManager para congelar el tiempo
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
        else
        {
            Time.timeScale = 0f;
            Debug.LogWarning("[UIManager] GameManager no encontrado, pausando directamente");
        }

        Debug.Log("[UIManager] Juego pausado - Panel visible");
    }

    /// Reanuda el juego: oculta el panel y cambia el icono
    private void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (pauseButtonImage != null && pauseIcon != null)
        {
            pauseButtonImage.sprite = pauseIcon;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
        else
        {
            Time.timeScale = 1f;
            Debug.LogWarning("[UIManager] GameManager no encontrado, reanudando directamente");
        }

        Debug.Log("[UIManager] Juego reanudado - Panel oculto");
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