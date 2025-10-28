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
    
    [Tooltip("Imagen para la zona bloqueada del enemigo")]
    public Image blockedEnemyZoneImage;
    
    [Header("Colores de Feedback")]
    [Tooltip("Color base cuando ambas zonas están disponibles (sin hover)")]
    public Color availableColor = new Color(0.2f, 1f, 0.2f, 0.3f); // Verde semi-transparente
    
    [Tooltip("Color cuando el cursor está sobre la zona izquierda")]
    public Color leftHighlightColor = new Color(0.2f, 0.5f, 1f, 0.5f); // Azul semi-transparente
    
    [Tooltip("Color cuando el cursor está sobre la zona derecha")]
    public Color rightHighlightColor = new Color(1f, 0.5f, 0.2f, 0.5f); // Naranja semi-transparente
    
    [Tooltip("Color de la zona bloqueada del enemigo")]
    public Color blockedColor = new Color(70f/255f, 70f/255f, 70f/255f, 130f/255f); // Gris oscuro semi-transparente
    
    [Header("Configuración de Animación")]
    [Tooltip("Duración del fade in/out al mostrar/ocultar zonas")]
    public float fadeDuration = 0.2f;
    
    [Tooltip("Activar parpadeo continuo para llamar la atención")]
    public bool enablePulse = true;
    
    [Tooltip("Velocidad del parpadeo (ciclos por segundo)")]
    public float pulseSpeed = 2f; // Aumentado de 1.5 a 2
    
    [Tooltip("Intensidad del parpadeo (0-1, donde 1 es máximo contraste)")]
    public float pulseIntensity = 0.7f; // Aumentado de 0.3 a 0.7 para que sea MUY visible
    
    private bool isVisible = false;
    private Coroutine fadeCoroutine = null;
    private Coroutine pulseCoroutine = null;
    private bool isHovering = false;
    private bool isHoveringLeft = false; // true si hover en izquierda, false si en derecha
    
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
        
        if (blockedEnemyZoneImage != null)
        {
            blockedEnemyZoneImage.enabled = false;
        }
    }
    
    /// <summary>
    /// Muestra las zonas de despliegue con parpadeo inmediato
    /// </summary>
    public void ShowZones()
    {
        if (isVisible) return;
        
        Debug.LogWarning($"[DeploymentZoneFeedback] ShowZones llamado - enablePulse={enablePulse}"); // DEBUG
        
        isVisible = true;
        
        if (leftZoneImage != null)
        {
            leftZoneImage.enabled = true;
            leftZoneImage.color = availableColor;
            Debug.LogWarning($"[DeploymentZoneFeedback] Left zone enabled, color={availableColor}"); // DEBUG
        }
        
        if (rightZoneImage != null)
        {
            rightZoneImage.enabled = true;
            rightZoneImage.color = availableColor;
            Debug.LogWarning($"[DeploymentZoneFeedback] Right zone enabled, color={availableColor}"); // DEBUG
        }
        
        // Mostrar zona bloqueada del enemigo
        if (blockedEnemyZoneImage != null)
        {
            blockedEnemyZoneImage.enabled = true;
            blockedEnemyZoneImage.color = blockedColor;
            Debug.LogWarning($"[DeploymentZoneFeedback] Blocked enemy zone enabled, color={blockedColor}"); // DEBUG
        }
        
        // Detener parpadeo anterior si existe
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        
        // Iniciar parpadeo continuo INMEDIATAMENTE
        if (enablePulse)
        {
            pulseCoroutine = StartCoroutine(PulseZones());
        }
    }
    
    /// <summary>
    /// Oculta las zonas de despliegue inmediatamente
    /// </summary>
    public void HideZones()
    {
        if (!isVisible) return;
        
        isVisible = false;
        isHovering = false;
        
        // Detener parpadeo
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        
        // Ocultar inmediatamente
        if (leftZoneImage != null)
        {
            leftZoneImage.enabled = false;
        }
        
        if (rightZoneImage != null)
        {
            rightZoneImage.enabled = false;
        }
        
        if (blockedEnemyZoneImage != null)
        {
            blockedEnemyZoneImage.enabled = false;
        }
    }
    
    /// <summary>
    /// Actualiza el feedback visual según la posición del cursor.
    /// La zona con hover se mantiene resaltada, la otra sigue parpadeando.
    /// </summary>
    /// <param name="normalizedX">Posición X normalizada del cursor (0 = izquierda, 1 = derecha)</param>
    public void UpdateHoverFeedback(float normalizedX)
    {
        if (!isVisible) return;
        
        isHovering = true;
        isHoveringLeft = normalizedX < 0.5f;
        
        Debug.LogWarning($"[DeploymentZoneFeedback] UpdateHoverFeedback - normalizedX={normalizedX}, isHoveringLeft={isHoveringLeft}"); // DEBUG
        
        // La zona con hover se mantiene resaltada (sin parpadeo)
        if (isHoveringLeft && leftZoneImage != null)
        {
            leftZoneImage.color = leftHighlightColor;
        }
        else if (!isHoveringLeft && rightZoneImage != null)
        {
            rightZoneImage.color = rightHighlightColor;
        }
    }
    
    /// <summary>
    /// Resetea el hover feedback (ambas zonas vuelven a parpadear)
    /// </summary>
    public void ResetHoverFeedback()
    {
        if (!isVisible) return;
        
        Debug.LogWarning("[DeploymentZoneFeedback] ResetHoverFeedback llamado - volviendo a parpadeo"); // DEBUG
        
        isHovering = false;
        // El parpadeo se reanudará automáticamente en PulseZones()
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
    
    /// <summary>
    /// Parpadeo continuo de las zonas para llamar la atención.
    /// Solo parpadea la zona que NO tiene hover.
    /// </summary>
    private System.Collections.IEnumerator PulseZones()
    {
        Debug.LogWarning("[DeploymentZoneFeedback] PulseZones iniciado"); // DEBUG
        Debug.LogWarning($"[DeploymentZoneFeedback] availableColor={availableColor}, pulseSpeed={pulseSpeed}, pulseIntensity={pulseIntensity}"); // DEBUG
        
        int frameCount = 0;
        
        while (isVisible)
        {
            float time = Time.time * pulseSpeed;
            float pulse = Mathf.Sin(time * Mathf.PI * 2f) * 0.5f + 0.5f; // 0 a 1
            
            // Modificar el alpha del color base: va de (availableColor.a * (1-intensity)) a availableColor.a
            float minAlpha = availableColor.a * (1f - pulseIntensity);
            float maxAlpha = availableColor.a;
            float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, pulse);
            
            // Log cada 60 frames para debug
            if (frameCount % 60 == 0)
            {
                Debug.LogWarning($"[DeploymentZoneFeedback] Frame {frameCount}: pulse={pulse:F2}, alpha={currentAlpha:F2} (min={minAlpha:F2}, max={maxAlpha:F2})"); // DEBUG
            }
            frameCount++;
            
            // Solo parpadear la zona SIN hover
            if (leftZoneImage != null)
            {
                // Si hay hover y es sobre la izquierda, NO parpadear
                if (!isHovering || !isHoveringLeft)
                {
                    Color c = availableColor;
                    c.a = currentAlpha;
                    leftZoneImage.color = c;
                }
                // Si hay hover sobre izquierda, mantener color highlight (ya se setea en UpdateHoverFeedback)
            }
            
            if (rightZoneImage != null)
            {
                // Si hay hover y es sobre la derecha, NO parpadear
                if (!isHovering || isHoveringLeft)
                {
                    Color c = availableColor;
                    c.a = currentAlpha;
                    rightZoneImage.color = c;
                }
                // Si hay hover sobre derecha, mantener color highlight (ya se setea en UpdateHoverFeedback)
            }
            
            yield return null;
        }
        
        Debug.LogWarning("[DeploymentZoneFeedback] PulseZones terminado"); // DEBUG
    }
}

