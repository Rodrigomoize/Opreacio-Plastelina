using UnityEngine;
using TMPro;

/// <summary>
/// Componente UI para mostrar el tiempo restante de un powerup activo.
/// Se agrega como hijo del botón de powerup y muestra "X.Xs" mientras está activo.
/// </summary>
public class PowerUpDurationUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Texto que muestra el tiempo restante")]
    public TextMeshProUGUI timerText;
    
    [Header("Settings")]
    [Tooltip("Color del texto del temporizador")]
    public Color timerColor = Color.white;
    [Tooltip("Tamaño del texto")]
    public float fontSize = 24f;
    
    private void Awake()
    {
        // Si no se asignó el texto, buscarlo como hijo
        if (timerText == null)
        {
            timerText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // Configurar el texto si existe
        if (timerText != null)
        {
            timerText.color = timerColor;
            timerText.fontSize = fontSize;
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.raycastTarget = false; // No bloquear clics del botón
            timerText.gameObject.SetActive(false); // Empezar oculto
        }
        else
        {
            Debug.LogWarning($"[PowerUpDurationUI] No se encontró TextMeshProUGUI en {gameObject.name}. Asigna uno manualmente.");
        }
    }
    
    /// <summary>
    /// Actualiza el temporizador con el tiempo restante
    /// </summary>
    /// <param name="timeRemaining">Tiempo restante en segundos</param>
    public void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
        {
            if (timeRemaining > 0)
            {
                timerText.text = $"{timeRemaining:F1}s";
                timerText.gameObject.SetActive(true);
            }
            else
            {
                HideTimer();
            }
        }
    }
    
    /// <summary>
    /// Oculta el temporizador
    /// </summary>
    public void HideTimer()
    {
        if (timerText != null)
        {
            timerText.text = "";
            timerText.gameObject.SetActive(false);
        }
    }
}
