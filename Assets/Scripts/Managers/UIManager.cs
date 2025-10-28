using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Pause System")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseMenuGroup;  
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;

    [Header("Instructions System")]
    [SerializeField] private GameObject instructionPauseGroup; 

    [SerializeField] private Sprite[] instructionSprites;
    [SerializeField] private Image instructionsImage;
    [SerializeField] private Button nextInstructionButton;
    [SerializeField] private Button previousInstructionButton;
    [SerializeField] private Button closeInstructionsButton;
    [SerializeField] private Button showInstructionsButton;

    private bool isPaused = false;
    private int currentInstructionIndex = 0;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        // Resetear estado
        isPaused = false;
        currentInstructionIndex = 0;

        // Panel de pausa oculto al inicio
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (pauseMenuGroup != null)
        {
            pauseMenuGroup.SetActive(true);
        }

        if (instructionPauseGroup != null)
        {
            instructionPauseGroup.SetActive(false);
        }

        // Configurar botón de pausa
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(true);
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(PauseGame);
        }

        // Configurar botón de reanudar (oculto al inicio)
        if (resumeButton != null)
        {
            resumeButton.gameObject.SetActive(false);
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
        }

        // Configurar botones de instrucciones
        SetupInstructionButtons();
    }

    public void Reinitialize()
    {
        Initialize();
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

    public void PauseGame()
    {
        isPaused = true;

        // Mostrar panel de pausa
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (pauseMenuGroup != null)
        {
            pauseMenuGroup.SetActive(true);
        }

        if (instructionPauseGroup != null)
        {
            instructionPauseGroup.SetActive(false);
        }

        // Ocultar botón de pausa, mostrar botón de reanudar
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(false);
        }

        if (resumeButton != null)
        {
            resumeButton.gameObject.SetActive(true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseMusic();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    public void ResumeGame()
    {
        isPaused = false;

        // Ocultar panel de pausa
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Mostrar botón de pausa, ocultar botón de reanudar
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(true);
        }

        if (resumeButton != null)
        {
            resumeButton.gameObject.SetActive(false);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeMusic();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    // ===== SISTEMA DE INSTRUCCIONES =====

    public void ShowInstructions()
    {
        if (instructionPauseGroup == null || instructionSprites == null || instructionSprites.Length == 0)
        {
            return;
        }

        currentInstructionIndex = 0;

        if (pauseMenuGroup != null)
        {
            pauseMenuGroup.SetActive(false);
        }

        if (instructionPauseGroup != null)
        {
            instructionPauseGroup.SetActive(true);
        }

        // Ocultar botón de reanudar mientras se ven las instrucciones
        if (resumeButton != null)
        {
            resumeButton.gameObject.SetActive(false);
        }

        UpdateInstructionDisplay();
    }

    public void CloseInstructions()
    {
        if (instructionPauseGroup != null)
        {
            instructionPauseGroup.SetActive(false);
        }

        if (pauseMenuGroup != null)
        {
            pauseMenuGroup.SetActive(true);
        }

        // Volver a mostrar el botón de reanudar
        if (resumeButton != null)
        {
            resumeButton.gameObject.SetActive(true);
        }
    }

    private void NextInstruction()
    {
        if (instructionSprites == null || currentInstructionIndex >= instructionSprites.Length - 1)
        {
            return;
        }

        currentInstructionIndex++;
        UpdateInstructionDisplay();
    }

    private void PreviousInstruction()
    {
        if (currentInstructionIndex <= 0)
        {
            return;
        }

        currentInstructionIndex--;
        UpdateInstructionDisplay();
    }

    private void UpdateInstructionDisplay()
    {
        if (instructionsImage != null && instructionSprites != null && currentInstructionIndex < instructionSprites.Length)
        {
            instructionsImage.sprite = instructionSprites[currentInstructionIndex];
        }

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
            pauseButton.onClick.RemoveListener(PauseGame);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
        }

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