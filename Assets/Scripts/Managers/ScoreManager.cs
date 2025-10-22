using System;
using UnityEngine;

/// <summary>
/// Gestiona la puntuación del jugador y el recuento de operaciones correctas.
/// - Suma X puntos por operación correcta.
/// - Añade un bonus por tiempo al finalizar la partida: cuanto más rápido, mayor el bonus.
/// - Persistente entre escenas (DontDestroyOnLoad).
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Puntuación por operación")]
    [Tooltip("Puntos que se suman por cada operación resuelta correctamente.")]
    [SerializeField] private int pointsPerCorrect = 100;

    [Header("Bonus por tiempo (al finalizar)")]
    [Tooltip("Bonus máximo si se termina en 0 segundos.")]
    [SerializeField] private int maxTimeBonus = 1000;

    [Tooltip("Penalización de bonus por cada segundo transcurrido.")]
    [SerializeField] private int timePenaltyPerSecond = 10;

    [Tooltip("Piso mínimo del bonus por tiempo (no baja de este valor). Puede ser 0.")]
    [SerializeField] private int minTimeBonus = 0;

    [Header("Umbrales de nota por puntuación final")]
    [Tooltip("Puntuación mínima para 'Excelente'.")]
    [SerializeField] private int excelenteThreshold = 750;
    [Tooltip("Puntuación mínima para 'Notable'.")]
    [SerializeField] private int notableThreshold = 500;
    [Tooltip("Puntuación mínima para 'Bien'.")]
    [SerializeField] private int bienThreshold = 250;

    public int CurrentScore { get; private set; }
    public int CorrectOperations { get; private set; }
    public bool IsFinalized { get; private set; }
    public float LastElapsedSeconds { get; private set; }
    public int LastTimeBonus { get; private set; }

    /// <summary>
    /// Notifica cambios de puntuación (score, correctOperations).
    /// </summary>
    public event Action<int, int> OnScoreChanged;

    /// <summary>
    /// Notifica final de partida (finalScore, timeBonusAplicado).
    /// </summary>
    public event Action<int, int> OnGameFinalized;

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
    /// Reinicia los contadores y puntuación.
    /// </summary>
    public void ResetScore()
    {
        CurrentScore = 0;
        CorrectOperations = 0;
        IsFinalized = false;
        LastElapsedSeconds = 0f;
        LastTimeBonus = 0;
        OnScoreChanged?.Invoke(CurrentScore, CorrectOperations);
    }

    /// <summary>
    /// Registra una o varias operaciones resueltas correctamente.
    /// </summary>
    public void RegisterCorrectOperation(int count = 1)
    {
        if (IsFinalized) return; // no modificar si ya se finalizó la partida
        if (count <= 0) return;

        CorrectOperations += count;
        CurrentScore += pointsPerCorrect * count;
        OnScoreChanged?.Invoke(CurrentScore, CorrectOperations);
    }

    /// <summary>
    /// Calcula el bonus por tiempo según los parámetros configurados.
    /// Bonus = clamp(maxTimeBonus - timePenaltyPerSecond * segundos, minTimeBonus, maxTimeBonus)
    /// </summary>
    public int ComputeTimeBonus(float elapsedSeconds)
    {
        if (elapsedSeconds < 0f) elapsedSeconds = 0f;
        // Penalización lineal por segundo
        float raw = maxTimeBonus - timePenaltyPerSecond * elapsedSeconds;
        int clamped = Mathf.Clamp(Mathf.RoundToInt(raw), minTimeBonus, maxTimeBonus);
        return clamped;
    }

    /// <summary>
    /// Aplica el bonus por tiempo y marca el fin de la partida.
    /// Devuelve el bonus aplicado en esta llamada.
    /// </summary>
    public int FinalizeScoreWithTime(float elapsedSeconds)
    {
        if (IsFinalized)
        {
            // Ya finalizado; no volver a aplicar bonus
            return 0;
        }

        int timeBonus = ComputeTimeBonus(elapsedSeconds);
        CurrentScore += timeBonus;
        IsFinalized = true;
        LastElapsedSeconds = Mathf.Max(0f, elapsedSeconds);
        LastTimeBonus = timeBonus;

        // Notificar a oyentes (UI, etc.)
        OnGameFinalized?.Invoke(CurrentScore, timeBonus);
        OnScoreChanged?.Invoke(CurrentScore, CorrectOperations);

        return timeBonus;
    }

    #region API pública para lectura/configuración

    public int GetPointsPerCorrect() => pointsPerCorrect;
    public void SetPointsPerCorrect(int value) => pointsPerCorrect = Mathf.Max(0, value);

    public (int max, int penaltyPerSecond, int min) GetTimeBonusParameters() => (maxTimeBonus, timePenaltyPerSecond, minTimeBonus);

    public void SetTimeBonusParameters(int maxBonus, int penaltyPerSec, int minBonus)
    {
        maxTimeBonus = Mathf.Max(0, maxBonus);
        timePenaltyPerSecond = Mathf.Max(0, penaltyPerSec);
        minTimeBonus = Mathf.Clamp(minBonus, 0, maxTimeBonus);
    }

    /// <summary>
    /// Devuelve la nota textual en base a la puntuación final actual y los umbrales configurados.
    /// Orden: Excelente > Notable > Bien > Suspendido.
    /// </summary>
    public string GetGrade()
    {
        int s = CurrentScore;
        if (s >= excelenteThreshold) return "Excel·lent";
        if (s >= notableThreshold) return "Notable";
        if (s >= bienThreshold) return "Bé";
        return "Aprovat";
    }

    #endregion
}
