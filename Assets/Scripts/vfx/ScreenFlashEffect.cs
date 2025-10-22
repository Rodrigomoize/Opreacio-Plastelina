using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Sistema de flash de pantalla para feedback visual negativo
/// Usar ScreenFlashEffect.Instance.Flash() para activar desde cualquier script
/// </summary>
public class ScreenFlashEffect : MonoBehaviour
{
    public static ScreenFlashEffect Instance { get; private set; }
    
    [Header("Flash Settings")]
    [Tooltip("Color del flash (rojo para error)")]
    public Color flashColor = new Color(1f, 0f, 0f, 0.5f);
    
    [Tooltip("Duración del flash en segundos")]
    public float flashDuration = 0.3f;
    
    [Header("UI References")]
    [Tooltip("Image UI que cubre toda la pantalla para el efecto")]
    public Image flashImage;
    
    private bool isFlashing = false;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Asegurarse de que la imagen empiece invisible
        if (flashImage != null)
        {
            Color c = flashImage.color;
            c.a = 0;
            flashImage.color = c;
            flashImage.enabled = true;
        }
    }
    
    /// <summary>
    /// Inicia el efecto de flash de pantalla con el color configurado
    /// </summary>
    public void Flash()
    {
        Flash(flashColor, flashDuration);
    }
    
    /// <summary>
    /// Inicia el efecto de flash de pantalla con parámetros personalizados
    /// </summary>
    /// <param name="color">Color del flash</param>
    /// <param name="duration">Duración del flash</param>
    public void Flash(Color color, float duration)
    {
        if (!isFlashing && flashImage != null)
        {
            StartCoroutine(DoFlash(color, duration));
        }
    }
    
    private IEnumerator DoFlash(Color color, float duration)
    {
        isFlashing = true;
        
        if (flashImage == null)
        {
            Debug.LogWarning("[ScreenFlashEffect] flashImage no está asignada!");
            isFlashing = false;
            yield break;
        }
        
        float elapsed = 0f;
        
        // Fade in rápido (25% del tiempo)
        float fadeInDuration = duration * 0.25f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0, color.a, elapsed / fadeInDuration);
            Color c = color;
            c.a = alpha;
            flashImage.color = c;
            yield return null;
        }
        
        // Fade out más lento (75% del tiempo)
        elapsed = 0f;
        float fadeOutDuration = duration * 0.75f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(color.a, 0, elapsed / fadeOutDuration);
            Color c = color;
            c.a = alpha;
            flashImage.color = c;
            yield return null;
        }
        
        // Asegurarse de que quede completamente invisible
        Color finalColor = color;
        finalColor.a = 0;
        flashImage.color = finalColor;
        
        isFlashing = false;
    }
}
