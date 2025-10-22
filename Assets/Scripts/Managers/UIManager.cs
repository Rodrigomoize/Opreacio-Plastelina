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

    [Header("Instructions System")]
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private Image instructionsImage;
    [SerializeField] private Sprite[] instructionSprites;
    [SerializeField] private Button nextInstructionButton;
    [SerializeField] private Button previousInstructionButton;
    [SerializeField] private Button closeInstructionsButton;
    [SerializeField] private Button showInstructionsButton;

    private bool isPaused = false;
    private int currentInstructionIndex = 0;

    private void Start()
    {
        // Panel de pausa oculto al inicio
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Panel de instrucciones oculto al inicio
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
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

        // Conecta los botones de instrucciones
        SetupInstructionButtons();

        Debug.Log("[UIManager] Inicializado correctamente");
    }

    private void SetupInstructionButtons()
    {
        if (showInstructionsButton != null)
        {
            showInstructionsButton.onClick.RemoveAllListeners();
            showInstructionsButton.onClick.AddListener(ShowInstructions);
        }

        if (closeInstructionsButton != null)
        {
            closeInstructionsButton.onClick.RemoveAllListeners();
            closeInstructionsButton.onClick.AddListener(CloseInstructions);
        }

        if (nextInstructionButton != null)
        {
            nextInstructionButton.onClick.RemoveAllListeners();
            nextInstructionButton.onClick.AddListener(NextInstruction);
        }

        if (previousInstructionButton != null)
        {
            previousInstructionButton.onClick.RemoveAllListeners();
            previousInstructionButton.onClick.AddListener(PreviousInstruction);
        }
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

    // ===== SISTEMA DE INSTRUCCIONES =====

    public void ShowInstructions()
    {
        if (instructionsPanel == null || instructionSprites == null || instructionSprites.Length == 0)
        {
            Debug.LogWarning("[UIManager] No se pueden mostrar instrucciones: panel o sprites no configurados");
            return;
        }

        // Resetear al inicio
        currentInstructionIndex = 0;

        // Ocultar menú de pausa y mostrar panel de instrucciones
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        instructionsPanel.SetActive(true);
        UpdateInstructionDisplay();

        Debug.Log("[UIManager] Mostrando instrucciones");
    }

    public void CloseInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }

        // Volver a mostrar el menú de pausa
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Debug.Log("[UIManager] Instrucciones cerradas");
    }

    private void NextInstruction()
    {
        if (instructionSprites == null || currentInstructionIndex >= instructionSprites.Length - 1)
        {
            return;
        }

        currentInstructionIndex++;
        UpdateInstructionDisplay();
        Debug.Log($"[UIManager] Siguiente instrucción: {currentInstructionIndex + 1}/{instructionSprites.Length}");
    }

    private void PreviousInstruction()
    {
        if (currentInstructionIndex <= 0)
        {
            return;
        }

        currentInstructionIndex--;
        UpdateInstructionDisplay();
        Debug.Log($"[UIManager] Instrucción anterior: {currentInstructionIndex + 1}/{instructionSprites.Length}");
    }

    private void UpdateInstructionDisplay()
    {
        if (instructionsImage != null && instructionSprites != null && currentInstructionIndex < instructionSprites.Length)
        {
            instructionsImage.sprite = instructionSprites[currentInstructionIndex];
        }

        // Activar/desactivar botones según la posición
        if (previousInstructionButton != null)
        {
            previousInstructionButton.interactable = currentInstructionIndex > 0;
        }

        if (nextInstructionButton != null)
        {
            nextInstructionButton.interactable = instructionSprites != null && currentInstructionIndex < instructionSprites.Length - 1;
        }
    }

    private void OnDestroy()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(TogglePause);
        }

        // Limpiar listeners de instrucciones
        if (showInstructionsButton != null)
        {
            showInstructionsButton.onClick.RemoveListener(ShowInstructions);
        }
        if (closeInstructionsButton != null)
        {
            closeInstructionsButton.onClick.RemoveListener(CloseInstructions);
        }
        if (nextInstructionButton != null)
        {
            nextInstructionButton.onClick.RemoveListener(NextInstruction);
        }
        if (previousInstructionButton != null)
        {
            previousInstructionButton.onClick.RemoveListener(PreviousInstruction);
        }

        Time.timeScale = 1f;
    }
}