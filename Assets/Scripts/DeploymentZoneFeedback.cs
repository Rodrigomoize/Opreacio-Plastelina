using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona el feedback visual de las zonas de despliegue de tropas.
/// Muestra áreas destacadas (izquierda y derecha) donde el jugador puede soltar tropas.
/// El color cambia instantáneamente según la posición del cursor para mejor claridad.
/// </summary>
public class DeploymentZoneFeedback : MonoBehaviour
{
    [Header("Referencias de UI")]
    [Tooltip("Imagen para la zona de despliegue izquierda")]
    public Image leftZoneImage;
    
    [Tooltip("Imagen para la zona de despliegue derecha")]
    public Image rightZoneImage;
    
    [Header("Colores de Feedback")]
    [Tooltip("Color base cuando ambas zonas están disponibles (sin hover)")]
    public Color availableColor = new Color(0.2f, 1f, 0.2f, 0.3f); // Verde semi-transparente
    
    [Tooltip("Color cuando el cursor está sobre la zona izquierda")]
    public Color leftHighlightColor = new Color(0.2f, 0.5f, 1f, 0.5f); // Azul semi-transparente
    
    [Tooltip("Color cuando el cursor está sobre la zona derecha")]
    public Color rightHighlightColor = new Color(1f, 0.5f, 0.2f, 0.5f); // Naranja semi-transparente
    
    [Header("Configuración de Animación")]
    [Tooltip("Duración del fade in/out al mostrar/ocultar zonas")]
    public float fadeDuration = 0.2f;
    
    private bool isVisible = false;
    private Coroutine fadeCoroutine = null;
    
    void Awake()
    {
        // Inicializar en estado invisible
        if (leftZoneImage != null)
        {
            leftZoneImage.enabled = false;
        }
        
        if (rightZoneImage != null)
        {
            rightZoneImage.enabled = false;
        }
    }
    
    /// <summary>
    /// Muestra las zonas de despliegue con animación de fade-in
    /// </summary>
    public void ShowZones()
    {
        if (isVisible) return;
        
        isVisible = true;
        
        if (leftZoneImage != null)
        {
            leftZoneImage.enabled = true;
            leftZoneImage.color = availableColor;
        }
        
        if (rightZoneImage != null)
        {
            rightZoneImage.enabled = true;
            rightZoneImage.color = availableColor;
        }
        
        // Animar fade-in
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeIn());
    }
    
    /// <summary>
    /// Oculta las zonas de despliegue con animación de fade-out
    /// </summary>
    public void HideZones()
    {
        if (!isVisible) return;
        
        isVisible = false;
        
        // Animar fade-out
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOut());
    }
    
    /// <summary>
    /// Actualiza el feedback visual según la posición del cursor.
    /// CORRECCIÓN: Cambio instantáneo de color en lugar de lerp gradual para mejor claridad.
    /// </summary>
    /// <param name="normalizedX">Posición X normalizada del cursor (0 = izquierda, 1 = derecha)</param>
    public void UpdateHoverFeedback(float normalizedX)
    {
        if (!isVisible) return;
        
        bool isLeftSide = normalizedX < 0.5f;
        
        // CAMBIO INSTANTÁNEO de colores para claridad visual
        if (leftZoneImage != null)
        {
            leftZoneImage.color = isLeftSide ? leftHighlightColor : availableColor;
        }
        
        if (rightZoneImage != null)
        {
            rightZoneImage.color = isLeftSide ? availableColor : rightHighlightColor;
        }
    }
    
    /// <summary>
    /// Resetea el hover feedback (vuelve ambas zonas al color base)
    /// </summary>
    public void ResetHoverFeedback()
    {
        if (!isVisible) return;
        
        if (leftZoneImage != null)
        {
            leftZoneImage.color = availableColor;
        }
        
        if (rightZoneImage != null)
        {
            rightZoneImage.color = availableColor;
        }
    }
    
    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color leftColor = leftZoneImage != null ? leftZoneImage.color : availableColor;
        Color rightColor = rightZoneImage != null ? rightZoneImage.color : availableColor;
        
        // Guardar alpha objetivo
        float leftTargetAlpha = leftColor.a;
        float rightTargetAlpha = rightColor.a;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            
            // Curva ease-out para suavidad
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            if (leftZoneImage != null)
            {
                Color c = leftZoneImage.color;
                c.a = Mathf.Lerp(0f, leftTargetAlpha, t);
                leftZoneImage.color = c;
            }
            
            if (rightZoneImage != null)
            {
                Color c = rightZoneImage.color;
                c.a = Mathf.Lerp(0f, rightTargetAlpha, t);
                rightZoneImage.color = c;
            }
            
            yield return null;
        }
        
        // Asegurar alpha final
        if (leftZoneImage != null)
        {
            Color c = leftZoneImage.color;
            c.a = leftTargetAlpha;
            leftZoneImage.color = c;
        }
        
        if (rightZoneImage != null)
        {
            Color c = rightZoneImage.color;
            c.a = rightTargetAlpha;
            rightZoneImage.color = c;
        }
    }
    
    private System.Collections.IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color leftStartColor = leftZoneImage != null ? leftZoneImage.color : Color.clear;
        Color rightStartColor = rightZoneImage != null ? rightZoneImage.color : Color.clear;
        
        float leftStartAlpha = leftStartColor.a;
        float rightStartAlpha = rightStartColor.a;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            
            // Curva ease-in para suavidad
            t = Mathf.Pow(t, 3f);
            
            if (leftZoneImage != null)
            {
                Color c = leftStartColor;
                c.a = Mathf.Lerp(leftStartAlpha, 0f, t);
                leftZoneImage.color = c;
            }
            
            if (rightZoneImage != null)
            {
                Color c = rightStartColor;
                c.a = Mathf.Lerp(rightStartAlpha, 0f, t);
                rightZoneImage.color = c;
            }
            
            yield return null;
        }
        
        // Ocultar completamente
        if (leftZoneImage != null)
        {
            leftZoneImage.enabled = false;
        }
        
        if (rightZoneImage != null)
        {
            rightZoneImage.enabled = false;
        }
    }
}
