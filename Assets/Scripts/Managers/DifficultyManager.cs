using UnityEngine;

/// <summary>
/// Gestiona centralmente TODOS los ajustes de dificultad del juego.
/// - Duración de partida
/// - Velocidad de tropas
/// - Configuración de IA (agresividad, intervalo de acción)
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [System.Serializable]
    public class DifficultySettings
    {
        [Header("Duración de Partida")]
        [Tooltip("Duración total de la partida en segundos")]
        public float duracionPartida = 180f;
    }

    [Header("Duraciones de Partida por Dificultad")]
    [Tooltip("Duración para dificultad FÁCIL (en segundos)")]
    [SerializeField] private float duracionFacil = 240f;    // 4 minutos
    
    [Tooltip("Duración para dificultad MEDIA (en segundos)")]
    [SerializeField] private float duracionMedia = 180f;    // 3 minutos
    
    [Tooltip("Duración para dificultad DIFÍCIL (en segundos)")]
    [SerializeField] private float duracionDificil = 120f;  // 2 minutos

    [Header("Estado Actual")]
    [SerializeField] private IAController.AIDificultad currentDifficulty = IAController.AIDificultad.Media;

    public IAController.AIDificultad CurrentDifficulty => currentDifficulty;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Establece la dificultad del juego y aplica todos los ajustes correspondientes
    /// </summary>
    public void SetDifficulty(IAController.AIDificultad difficulty)
    {
        currentDifficulty = difficulty;
        Debug.Log($"[DifficultyManager] Dificultad establecida: {difficulty}");
        
        ApplyDifficultySettings();
    }

    /// <summary>
    /// Aplica todos los ajustes de dificultad a los managers correspondientes
    /// </summary>
    public void ApplyDifficultySettings()
    {
        // Aplicar velocidad de juego
        if (GameSpeedManager.Instance != null)
        {
            // Convertir dificultad de IA a dificultad de GameSpeed
            GameSpeedManager.GameDifficulty speedDifficulty = currentDifficulty switch
            {
                IAController.AIDificultad.Facil => GameSpeedManager.GameDifficulty.Facil,
                IAController.AIDificultad.Media => GameSpeedManager.GameDifficulty.Media,
                IAController.AIDificultad.Dificil => GameSpeedManager.GameDifficulty.Dificil,
                _ => GameSpeedManager.GameDifficulty.Media
            };
            GameSpeedManager.Instance.SetSpeedByDifficulty(speedDifficulty);
        }

        // Aplicar configuración de IA (la IA ya tiene sus propios ajustes internos)
        IAController aiController = GameManager.Instance?.GetAICardManager();
        if (aiController != null)
        {
            aiController.SetDificultad(currentDifficulty);
        }

        Debug.Log($"[DifficultyManager] ✅ Todos los ajustes aplicados:\n{GetCurrentSettingsInfo()}");
    }

    /// <summary>
    /// Obtiene la duración de partida para la dificultad actual
    /// </summary>
    public float GetGameDuration()
    {
        return GetDurationForDifficulty(currentDifficulty);
    }

    /// <summary>
    /// Obtiene la duración para una dificultad específica
    /// </summary>
    public float GetDurationForDifficulty(IAController.AIDificultad difficulty)
    {
        return difficulty switch
        {
            IAController.AIDificultad.Facil => duracionFacil,
            IAController.AIDificultad.Media => duracionMedia,
            IAController.AIDificultad.Dificil => duracionDificil,
            _ => duracionMedia
        };
    }

    /// <summary>
    /// Obtiene información detallada de los ajustes actuales para debugging
    /// </summary>
    public string GetCurrentSettingsInfo()
    {
        float duracion = GetGameDuration();
        
        string info = $"=== CONFIGURACIÓN DE DIFICULTAD: {currentDifficulty} ===\n";
        info += $"Duración de Partida: {duracion}s ({duracion/60f:F1} minutos)\n";
        
        if (GameSpeedManager.Instance != null)
        {
            info += $"Velocidad de Juego: {GameSpeedManager.Instance.GameSpeedMultiplier}x\n";
        }
        
        IAController aiController = GameManager.Instance?.GetAICardManager();
        if (aiController != null)
        {
            info += aiController.GetConfigInfo();
        }
        
        return info;
    }
}
