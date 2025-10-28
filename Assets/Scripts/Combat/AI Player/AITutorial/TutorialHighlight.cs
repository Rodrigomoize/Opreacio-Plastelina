using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Efecto de resaltado para el tutorial inspirado en CardDisplay
/// Usa escalado suave + parpadeo de color para llamar la atención
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class TutorialHighlight : MonoBehaviour
{
    [Header("Configuración de Escala")]
    [Tooltip("Escala máxima del pulso (1.2 = 120% del tamaño original)")]
    [Range(1.0f, 1.5f)]
    public float pulseScaleMax = 1.15f;
    
    [Tooltip("Escala mínima del pulso (0.95 = 95% del tamaño original)")]
    [Range(0.8f, 1.0f)]
    public float pulseScaleMin = 1.0f;
    
    [Tooltip("Velocidad del pulso de escala")]
    [Range(0.5f, 5f)]
    public float pulseSpeed = 2f;
    
    [Header("Configuración de Color")]
    [Tooltip("Activar parpadeo de color")]
    public bool enableColorPulse = true;
    
    [Tooltip("Color del resaltado")]
    public Color highlightColor = Color.yellow;
    
    [Tooltip("Intensidad del color (0 = sin efecto, 1 = color completo)")]
    [Range(0f, 1f)]
    public float colorIntensity = 0.5f;
    
    [Header("Detección Automática")]
    [Tooltip("Buscar Image en el objeto y sus hijos")]
    public bool autoDetectImages = true;
    
    [Tooltip("Buscar Graphic (Text, Image, etc.) si no encuentra Image")]
    public bool includeAllGraphics = true;

    // Estado
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHighlighting = false;
    private Coroutine highlightCoroutine;
    
    // Componentes visuales
    private Image[] images;
    private Graphic[] graphics;
    private Color[] originalColors;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        targetScale = originalScale;
        
        if (autoDetectImages)
        {
            DetectComponents();
        }
    }

    void Update()
    {
        // Animar escala suavemente (igual que CardDisplay)
        if (rectTransform.localScale != targetScale)
        {
            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale, 
                targetScale, 
                Time.unscaledDeltaTime * 10f // Usar unscaledDeltaTime para que funcione con Time.timeScale = 0
            );
        }
    }

    /// <summary>
    /// Detecta componentes Image y Graphic automáticamente
    /// </summary>
    private void DetectComponents()
    {
        // Detectar Images
        images = GetComponentsInChildren<Image>(true);
        
        // Si no hay Images, buscar todos los Graphic (Text, RawImage, etc.)
        if ((images == null || images.Length == 0) && includeAllGraphics)
        {
            graphics = GetComponentsInChildren<Graphic>(true);
        }

        // Guardar colores originales
        int totalComponents = (images?.Length ?? 0) + (graphics?.Length ?? 0);
        if (totalComponents > 0)
        {
            originalColors = new Color[totalComponents];
            int index = 0;
            
            if (images != null)
            {
                foreach (var img in images)
                {
                    originalColors[index++] = img.color;
                }
            }
            
            if (graphics != null)
            {
                foreach (var graphic in graphics)
                {
                    originalColors[index++] = graphic.color;
                }
            }
            
            Debug.Log($"[TutorialHighlight] ? Detectados {totalComponents} componentes visuales en '{gameObject.name}'");
        }
        else
        {
            Debug.LogWarning($"[TutorialHighlight] ?? No se encontraron componentes visuales en '{gameObject.name}'");
        }
    }

    /// <summary>
    /// Inicia el efecto de resaltado
    /// </summary>
    public void StartHighlight()
    {
        if (isHighlighting)
        {
            Debug.Log($"[TutorialHighlight] Ya está resaltando: {gameObject.name}");
            return;
        }

        isHighlighting = true;

        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
        }

        highlightCoroutine = StartCoroutine(HighlightCoroutine());
        Debug.Log($"[TutorialHighlight] ? Resaltado iniciado en '{gameObject.name}'");
    }

    /// <summary>
    /// Detiene el efecto de resaltado
    /// </summary>
    public void StopHighlight()
    {
        if (!isHighlighting) return;

        isHighlighting = false;

        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
            highlightCoroutine = null;
        }

        // Restaurar estado original
        targetScale = originalScale;
        RestoreOriginalColors();
        
        Debug.Log($"[TutorialHighlight] ?? Resaltado detenido en '{gameObject.name}'");
    }

    /// <summary>
    /// Corrutina de efecto de pulso (escala + color)
    /// </summary>
    private IEnumerator HighlightCoroutine()
    {
        while (isHighlighting)
        {
            // Calcular pulso (oscila entre 0 y 1)
            float pulse = Mathf.PingPong(Time.unscaledTime * pulseSpeed, 1f);
            
            // Aplicar curva suave
            pulse = Mathf.SmoothStep(0f, 1f, pulse);

            // EFECTO 1: ESCALA (como CardDisplay hover)
            float currentScale = Mathf.Lerp(pulseScaleMin, pulseScaleMax, pulse);
            targetScale = originalScale * currentScale;

            // EFECTO 2: COLOR (parpadeo)
            if (enableColorPulse)
            {
                Color targetColor = Color.Lerp(Color.white, highlightColor, pulse * colorIntensity);
                ApplyColorPulse(targetColor);
            }

            yield return null;
        }
    }

    /// <summary>
    /// Aplica el color de pulso a todos los componentes
    /// </summary>
    private void ApplyColorPulse(Color targetColor)
    {
        int index = 0;

        if (images != null)
        {
            foreach (var img in images)
            {
                if (img != null && index < originalColors.Length)
                {
                    img.color = originalColors[index] * targetColor;
                    index++;
                }
            }
        }

        if (graphics != null)
        {
            foreach (var graphic in graphics)
            {
                if (graphic != null && index < originalColors.Length)
                {
                    graphic.color = originalColors[index] * targetColor;
                    index++;
                }
            }
        }
    }

    /// <summary>
    /// Restaura los colores originales
    /// </summary>
    private void RestoreOriginalColors()
    {
        int index = 0;

        if (images != null)
        {
            foreach (var img in images)
            {
                if (img != null && index < originalColors.Length)
                {
                    img.color = originalColors[index++];
                }
            }
        }

        if (graphics != null)
        {
            foreach (var graphic in graphics)
            {
                if (graphic != null && index < originalColors.Length)
                {
                    graphic.color = originalColors[index++];
                }
            }
        }
    }

    void OnDisable()
    {
        StopHighlight();
    }

    void OnDestroy()
    {
        StopHighlight();
    }

    /// <summary>
    /// Método para debugging - fuerza redetección
    /// </summary>
    [ContextMenu("Redetectar Componentes")]
    public void ForceRedetect()
    {
        DetectComponents();
    }

    /// <summary>
    /// Test rápido desde el Inspector
    /// </summary>
    [ContextMenu("Test Highlight 3s")]
    public void TestHighlight()
    {
        StartHighlight();
        StartCoroutine(StopAfterDelay(3f));
    }

    private IEnumerator StopAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        StopHighlight();
    }
}