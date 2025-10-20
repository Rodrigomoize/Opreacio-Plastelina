using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI para la pantalla de victoria. Muestra:
/// - Operaciones resueltas
/// - Tiempo de juego
/// - Puntuación total
/// - Nota (Excelente/Notable/Bien/Suspendido)
/// Y dos botones: Volver a jugar y Salir al menú
/// </summary>
public class WinResultUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private Text operacionesText;
    [SerializeField] private Text tiempoText;
    [SerializeField] private Text puntuacionText;
    [SerializeField] private Text notaText;

    [Header("Buttons")]
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

    private void Start()
    {
        // Cargar datos del ScoreManager
        if (ScoreManager.Instance != null)
        {
            var sm = ScoreManager.Instance;
            if (operacionesText != null) operacionesText.text = $"Operaciones resueltas: {sm.CorrectOperations}";

            float secs = sm.LastElapsedSeconds;
            int total = Mathf.FloorToInt(secs);
            int minutes = total / 60;
            int seconds = total % 60;
            if (tiempoText != null) tiempoText.text = $"Tiempo: {minutes}:{seconds:00}";

            if (puntuacionText != null) puntuacionText.text = $"Puntuación total: {sm.CurrentScore}";
            if (notaText != null) notaText.text = $"Nota: {sm.GetGrade()}";
        }
        else
        {
            if (operacionesText != null) operacionesText.text = "Operaciones resueltas: -";
            if (tiempoText != null) tiempoText.text = "Tiempo: -";
            if (puntuacionText != null) puntuacionText.text = "Puntuación total: -";
            if (notaText != null) notaText.text = "Nota: -";
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BackToMainMenu();
        }
    }
}
