using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI sencilla para mostrar el marcador de puntuación y operaciones correctas.
/// Escucha eventos del ScoreManager.
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Text correctOpsText;

    private void OnEnable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
            // Refrescar con estado actual
            HandleScoreChanged(ScoreManager.Instance.CurrentScore, ScoreManager.Instance.CorrectOperations);
        }
    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
        }
    }

    private void HandleScoreChanged(int score, int correct)
    {
        if (scoreText != null) scoreText.text = $"Puntos: {score}";
        if (correctOpsText != null) correctOpsText.text = $"Correctas: {correct}";
    }
}
