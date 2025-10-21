using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI para la pantalla de victoria. Muestra:
/// - Operaciones resueltas correctamente
/// - Tiempo total de juego
/// - Puntuación total (con bonus por tiempo)
/// - Nota final (Excelente/Notable/Bien/Suspendido)
/// Y dos botones: Volver a jugar y Salir al menú
/// </summary>
public class WinResultUI : MonoBehaviour
{
    [Header("Texts - Legacy UI")]
    [SerializeField] private Text operacionesText;
    [SerializeField] private Text tiempoText;
    [SerializeField] private Text puntuacionText;
    [SerializeField] private Text notaText;
    
    [Header("Texts - TextMeshPro (Alternativa)")]
    [SerializeField] private TextMeshProUGUI tmpOperacionesText;
    [SerializeField] private TextMeshProUGUI tmpTiempoText;
    [SerializeField] private TextMeshProUGUI tmpPuntuacionText;
    [SerializeField] private TextMeshProUGUI tmpNotaText;

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
        Debug.Log("[WinResultUI] Iniciando pantalla de victoria...");
        
        // Cargar datos del ScoreManager
        if (ScoreManager.Instance != null)
        {
            var sm = ScoreManager.Instance;
            Debug.Log($"[WinResultUI] ScoreManager encontrado - Score: {sm.CurrentScore}, Ops: {sm.CorrectOperations}, Tiempo: {sm.LastElapsedSeconds}s");
            
            // Operaciones resueltas correctamente
            string operacionesStr = $"Operacions resoltes: {sm.CorrectOperations}";
            SetText(operacionesText, tmpOperacionesText, operacionesStr);

            // Tiempo de juego
            float secs = sm.LastElapsedSeconds;
            int total = Mathf.FloorToInt(secs);
            int minutes = total / 60;
            int seconds = total % 60;
            string tiempoStr = $"Temps total de joc: {minutes:0}:{seconds:00}";
            SetText(tiempoText, tmpTiempoText, tiempoStr);

            // Puntuación total
            string puntuacionStr = $"Puntuació total: {sm.CurrentScore} pts";
            SetText(puntuacionText, tmpPuntuacionText, puntuacionStr);
            Debug.Log($"[WinResultUI] Puntuación establecida: {puntuacionStr}");
            
            // Nota final
            string grade = sm.GetGrade();
            string notaStr = $"Nota final: {grade}";
            SetText(notaText, tmpNotaText, notaStr);
            
            // Colorear la nota según el resultado
            Color gradeColor = GetGradeColor(grade);
            if (notaText != null) notaText.color = gradeColor;
            if (tmpNotaText != null) tmpNotaText.color = gradeColor;
        }
        else
        {
            Debug.LogError("[WinResultUI] ScoreManager.Instance es NULL! No se pueden mostrar los datos.");
            SetText(operacionesText, tmpOperacionesText, "Operaciones resueltas: -");
            SetText(tiempoText, tmpTiempoText, "Tiempo: -");
            SetText(puntuacionText, tmpPuntuacionText, "Puntuación total: -");
            SetText(notaText, tmpNotaText, "Nota: -");
        }
    }
    
    /// <summary>
    /// Helper para establecer texto en ambos tipos de componentes (Legacy UI y TMP)
    /// </summary>
    private void SetText(Text legacyText, TextMeshProUGUI tmpText, string text)
    {
        if (legacyText != null) legacyText.text = text;
        if (tmpText != null) tmpText.text = text;
    }
    
    /// <summary>
    /// Devuelve un color según la nota obtenida
    /// </summary>
    private Color GetGradeColor(string grade)
    {
        switch (grade)
        {
            case "Excelente":
                return new Color(0.2f, 1f, 0.2f); // Verde brillante
            case "Notable":
                return new Color(0.3f, 0.8f, 1f); // Azul claro
            case "Bien":
                return new Color(1f, 0.8f, 0.2f); // Amarillo/naranja
            default: // "Suspendido"
                return new Color(1f, 0.3f, 0.3f); // Rojo
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
