using UnityEngine;

/// <summary>
/// Conecta el GameTimer con el ScoreManager para aplicar el bonus por tiempo
/// automáticamente cuando el temporizador se detenga (fin de partida).
/// </summary>
public class ScoreFinalizer : MonoBehaviour
{
    [Tooltip("Referencia al GameTimer de la escena. Si se deja vacío intentará encontrarlo.")]
    [SerializeField] private GameTimer timer;
    [Tooltip("Si es true, no aplica bonus por tiempo cuando la derrota es por timeout.")]
    [SerializeField] private bool skipFinalizeOnTimeoutLose = true;

    private bool timeoutTriggered = false;

    private void Awake()
    {
        if (timer == null)
        {
            timer = FindFirstObjectByType<GameTimer>();
        }
    }

    private void OnEnable()
    {
        if (timer != null)
        {
            timer.OnTimerStopped += HandleTimerStopped;
            timer.OnCountdownCompleted += HandleCountdownCompleted;
        }
    }

    private void OnDisable()
    {
        if (timer != null)
        {
            timer.OnTimerStopped -= HandleTimerStopped;
            timer.OnCountdownCompleted -= HandleCountdownCompleted;
        }
    }

    private void HandleTimerStopped(float elapsedSeconds)
    {
        if (timeoutTriggered && skipFinalizeOnTimeoutLose)
        {
            // Derrota por timeout: no aplicar bonus
            timeoutTriggered = false; // reset
            return;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.FinalizeScoreWithTime(elapsedSeconds);
        }
        else
        {
            Debug.LogWarning("[ScoreFinalizer] ScoreManager no encontrado en escena.");
        }
    }

    private void HandleCountdownCompleted()
    {
        timeoutTriggered = true;
    }

    /// <summary>
    /// Llamar manualmente cuando el jugador gana (no por timeout) para aplicar el bonus por tiempo.
    /// </summary>
    public void FinalizeOnWin()
    {
        if (timer == null)
        {
            timer = FindFirstObjectByType<GameTimer>();
        }
        if (timer != null)
        {
            HandleTimerStopped(timer.ElapsedSeconds);
        }
    }
}
