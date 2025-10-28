using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Temporizador simple para partidas.
/// - Cuenta el tiempo transcurrido.
/// - Muestra el tiempo en un Text/UIText opcional.
/// - Puede iniciar, pausar, reanudar y detener.
public class GameTimer : MonoBehaviour
{
    [Header("UI (opcional)")]
    [Tooltip("Texto donde se mostrará el tiempo. Puede ser un Text (UGUI) o TextMeshProUGUI via wrapper manual.")]
    [SerializeField] private Text timeText;
    [Tooltip("Alternativa con TextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI tmpTimeText;

    [Header("Formato")]
    [Tooltip("Usa formato MM:SS (true) o SS.dec (false)")]
    [SerializeField] private bool useMinutesSeconds = true;

    [Header("Cuenta atrás (opcional)")]
    [Tooltip("Si está activo, el temporizador funciona como cuenta atrás.")]
    [SerializeField] private bool countdownMode = false;
    [Tooltip("Duración para la cuenta atrás en segundos.")]
    [SerializeField] private float countdownDuration = 180f;

    [Header("Audio")]
    [Tooltip("Segundos restantes para reproducir el sonido de advertencia")]
    [SerializeField] private float warningThreshold = 4.5f;

    public bool IsRunning { get; private set; }
    public float ElapsedSeconds { get; private set; }
    public float RemainingSeconds => countdownMode ? Mathf.Max(0f, countdownDuration - ElapsedSeconds) : 0f;
    public bool IsCountdown => countdownMode;
    public float CountdownDuration => countdownDuration;

    public event Action<float> OnTimerTick; // envía segundos acumulados
    public event Action<float> OnTimerStopped; // envía segundos finales
    public event Action OnCountdownCompleted; // se dispara cuando la cuenta atrás llega a 0

    private bool hasPlayedWarningSound = false;

    private void OnEnable()
    {
        // Sólo refrescar el texto si está asignado
        UpdateText();
        StartCountdown(countdownDuration);
    }

    private void Update()
    {
        if (!IsRunning) return;
        ElapsedSeconds += Time.deltaTime;

        if (countdownMode && !hasPlayedWarningSound)
        {
            float remaining = countdownDuration - ElapsedSeconds;
            if (remaining <= warningThreshold && remaining > 0f)
            {
                hasPlayedWarningSound = true;
                
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayTimer5Seconds();
                }
                else
                {
                    Debug.LogWarning("[GameTimer] AudioManager.Instance es NULL, no se puede reproducir sonido de advertencia");
                }
            }
        }

        // Si es cuenta atrás y se llegó al final, detener
        if (countdownMode && ElapsedSeconds >= countdownDuration)
        {
            ElapsedSeconds = countdownDuration;
            UpdateText();
            OnTimerTick?.Invoke(ElapsedSeconds);
            StopTimer();
            OnCountdownCompleted?.Invoke();
            return;
        }

        UpdateText();
        OnTimerTick?.Invoke(ElapsedSeconds);
    }

    public void StartTimer(float startAt = 0f)
    {
        countdownMode = false;
        ElapsedSeconds = Mathf.Max(0f, startAt);
        IsRunning = true;
        hasPlayedWarningSound = false; // resetear flag
        UpdateText();
    }

    /// Inicia una cuenta atrás de la duración indicada (en segundos).
    public void StartCountdown(float durationSeconds)
    {
        countdownMode = true;
        countdownDuration = Mathf.Max(0f, durationSeconds);
        ElapsedSeconds = 0f;
        IsRunning = true;
        hasPlayedWarningSound = false; // resetear flag
        UpdateText();
    }

    public void PauseTimer()
    {
        IsRunning = false;
    }

    public void ResumeTimer()
    {
        IsRunning = true;
    }

    public void StopTimer()
    {
        if (!IsRunning && ElapsedSeconds <= 0f)
        {
            // nada que hacer si nunca arrancó
        }
        IsRunning = false;
        UpdateText();
        OnTimerStopped?.Invoke(ElapsedSeconds);
    }

    public void ResetTimer()
    {
        IsRunning = false;
        ElapsedSeconds = 0f;
        hasPlayedWarningSound = false; // resetear flag
        UpdateText();
    }

    private void UpdateText()
    {
        float displaySeconds = countdownMode ? Mathf.Max(0f, countdownDuration - ElapsedSeconds) : ElapsedSeconds;

        string value;
        if (useMinutesSeconds)
        {
            int total = Mathf.FloorToInt(displaySeconds);
            int minutes = total / 60;
            int seconds = total % 60;
            // Minutos sin cero a la izquierda, segundos con 2 dígitos
            value = $"{minutes}:{seconds:00}";
        }
        else
        {
            value = displaySeconds.ToString("0.0");
        }

        if (timeText != null) timeText.text = value;
        if (tmpTimeText != null) tmpTimeText.text = value;
    }
}
