using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Orquesta las condiciones de victoria/derrota.
/// De momento: Derrota cuando el timer llega a 0:00 (cuenta atrás finalizada).
/// En el futuro: añade condiciones de victoria basadas en vida de bases, objetivos, etc.
/// </summary>
public class GameFlow : MonoBehaviour
{
    [SerializeField] private GameTimer timer;
    private TowerHealthBar enemyHealth;

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
            timer.OnCountdownCompleted += HandleTimeoutLose;
        }

        if (enemyHealth != null)
        {
            enemyHealth = FindFirstObjectByType<TowerHealthBar>();
        }
    }

    private void OnDisable()
    {
        if (timer != null)
        {
            timer.OnCountdownCompleted -= HandleTimeoutLose;
        }
    }

    private void HandleTimeoutLose()
    {
        // Asegurarse de que el tiempo esté detenido
        if (timer != null && timer.IsRunning)
        {
            timer.StopTimer();
        }

        // No aplicar bonus por tiempo en derrota por timeout
        // Cambiar a la escena de derrota
        LoadLoseScene();
    }

    public void LoadWinScene()
    {
        // Detener el timer si está corriendo
        if (timer != null && timer.IsRunning)
        {
            timer.StopTimer();
        }
        
        // Finalizar el score con bonus por tiempo
        if (timer != null && ScoreManager.Instance != null)
        {
            float elapsedSeconds = timer.ElapsedSeconds;
            int timeBonus = ScoreManager.Instance.FinalizeScoreWithTime(elapsedSeconds);
            Debug.Log($"[GameFlow] Victoria! Tiempo: {elapsedSeconds:F1}s, Bonus: {timeBonus}, Score final: {ScoreManager.Instance.CurrentScore}");
        }
        
        SceneManager.LoadScene("WinScene");
    }

    public void LoadLoseScene()
    {
        // Detener el timer si está corriendo
        if (timer != null && timer.IsRunning)
        {
            timer.StopTimer();
        }
        
        SceneManager.LoadScene("LoseScene");
    }
}
