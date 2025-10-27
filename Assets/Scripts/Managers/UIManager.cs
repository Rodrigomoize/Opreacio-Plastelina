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
        Initialize();
    }

    // ⭐ NUEVO: Método de inicialización separado
    private void Initialize()
    {
        Debug.Log("[UIManager] ===== INICIALIZANDO UIManager =====");
        
        // Resetear estado
        isPaused = false;
        currentInstructionIndex = 0;

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
            Debug.Log("[UIManager] Botón de pausa configurado");
        }
        else
        {
            Debug.LogWarning("[UIManager] ⚠️ pauseButton es NULL");
        }

        // Conecta los botones de instrucciones
        SetupInstructionButtons();

        Debug.Log("[UIManager] Inicializado correctamente");
    }

    // ⭐ NUEVO: Método público para reinicializar desde GameManager
    public void Reinitialize()
    {
        Debug.Log("[UIManager] 🔄 Reinicializando UIManager...");
        Initialize();
    }

    private void SetupInstructionButtons()
    {
        Debug.Log("[UIManager] Configurando botones de instrucciones...");
        
        if (showInstructionsButton != null)
        {
            showInstructionsButton.onClick.RemoveAllListeners();
            showInstructionsButton.onClick.AddListener(ShowInstructions);
            Debug.Log("[UIManager] ✅ Botón 'Mostrar Instrucciones' configurado");
        }
        else
        {
            Debug.LogWarning("[UIManager] ⚠️ showInstructionsButton es NULL");
        }

        if (closeInstructionsButton != null)
        {
            closeInstructionsButton.onClick.RemoveAllListeners();
            closeInstructionsButton.onClick.AddListener(CloseInstructions);
            Debug.Log("[UIManager] ✅ Botón 'Cerrar Instrucciones' configurado");
        }
        else
        {
            Debug.LogWarning("[UIManager] ⚠️ closeInstructionsButton es NULL");
        }

        if (nextInstructionButton != null)
        {
            nextInstructionButton.onClick.RemoveAllListeners();
            nextInstructionButton.onClick.AddListener(NextInstruction);
            Debug.Log("[UIManager] ✅ Botón 'Siguiente' configurado");
        }
        else
        {
            Debug.LogWarning("[UIManager] ⚠️ nextInstructionButton es NULL");
        }

        if (previousInstructionButton != null)
        {
            previousInstructionButton.onClick.RemoveAllListeners();
            previousInstructionButton.onClick.AddListener(PreviousInstruction);
            Debug.Log("[UIManager] ✅ Botón 'Anterior' configurado");
        }
        else
        {
            Debug.LogWarning("[UIManager] ⚠️ previousInstructionButton es NULL");
        }
    }

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
            Debug.LogWarning("[UIManager] GameManager no encontrado, pausando directamente");
        }

        Debug.Log("[UIManager] Juego pausado - Panel visible");
    }

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
            Debug.LogWarning("[UIManager] GameManager no encontrado, reanudando directamente");
        }

        Debug.Log("[UIManager] Juego reanudado - Panel oculto");
    }

    // ===== SISTEMA DE INSTRUCCIONES =====

    public void ShowInstructions()
    {
        Debug.Log("[UIManager] ShowInstructions() llamado");
        
        if (instructionsPanel == null)
        {
            Debug.LogError("[UIManager] ❌ instructionsPanel es NULL");
            return;
        }
        
        if (instructionSprites == null || instructionSprites.Length == 0)
        {
            Debug.LogError("[UIManager] ❌ instructionSprites vacío o NULL");
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

        Debug.Log("[UIManager] ✅ Instrucciones mostradas correctamente");
    }

    public void CloseInstructions()
    {
        Debug.Log("[UIManager] CloseInstructions() llamado");
        
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
        Debug.Log("[UIManager] OnDestroy llamado - Limpiando listeners");
        
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