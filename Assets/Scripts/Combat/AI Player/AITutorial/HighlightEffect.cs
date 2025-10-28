using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Efecto de resaltado visual con parpadeo suave
/// Puede usarse en UI (Image) o en objetos 3D (Renderer)
/// </summary>
public class HighlightEffect : MonoBehaviour
{
    [Header("Configuración del Efecto")]
    [Tooltip("Velocidad del parpadeo (más alto = más rápido)")]
    [Range(0.5f, 5f)]
    public float pulseSpeed = 2f;

    [Tooltip("Intensidad del brillo (0 = sin brillo, 1 = brillo máximo)")]
    [Range(0f, 1f)]
    public float intensity = 0.7f;

    [Tooltip("Color del resaltado")]
    public Color highlightColor = Color.yellow;

    [Tooltip("Activar el efecto al iniciar")]
    public bool activeOnStart = false;

    [Header("Modo de Detección Automática")]
    [Tooltip("Si está activo, buscará Image o Renderer automáticamente")]
    public bool autoDetect = true;
    
    [Tooltip("Buscar componentes en hijos también (útil para Sliders)")]
    public bool includeChildren = true;

    // Componentes UI
    private Image[] images; // CAMBIADO: ahora es array para múltiples imágenes
    private Color[] originalColors;

    // Componentes 3D
    private Renderer[] renderers;
    private Color[] originalRendererColors;
    private MaterialPropertyBlock propertyBlock;

    // Estado
    private bool isHighlighting = false;
    private Coroutine highlightCoroutine;

    void Awake()
    {
        if (autoDetect)
        {
            DetectComponents();
        }

        propertyBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        if (activeOnStart)
        {
            StartHighlight();
        }
    }

    /// <summary>
    /// Detecta automáticamente si es UI o 3D
    /// </summary>
    private void DetectComponents()
    {
        Debug.Log($"[HighlightEffect] 🔍 Detectando componentes en '{gameObject.name}'...");

        // Intentar detectar componentes UI (Image)
        if (includeChildren)
        {
            images = GetComponentsInChildren<Image>(true); // Incluir inactivos
        }
        else
        {
            Image singleImage = GetComponent<Image>();
            if (singleImage != null)
            {
                images = new Image[] { singleImage };
            }
        }

        if (images != null && images.Length > 0)
        {
            originalColors = new Color[images.Length];
            for (int i = 0; i < images.Length; i++)
            {
                originalColors[i] = images[i].color;
            }
            Debug.Log($"[HighlightEffect] ✅ Detectadas {images.Length} imágenes en '{gameObject.name}'");
            
            // Mostrar detalles de cada imagen
            foreach (var img in images)
            {
                Debug.Log($"   - Image en '{img.gameObject.name}' (color original: {img.color})");
            }
            return;
        }

        // Intentar detectar Renderer 3D
        renderers = GetComponentsInChildren<Renderer>();
        if (renderers != null && renderers.Length > 0)
        {
            originalRendererColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].material.HasProperty("_Color"))
                {
                    originalRendererColors[i] = renderers[i].material.color;
                }
                else if (renderers[i].material.HasProperty("_BaseColor"))
                {
                    originalRendererColors[i] = renderers[i].material.GetColor("_BaseColor");
                }
                else
                {
                    originalRendererColors[i] = Color.white;
                }
            }
            Debug.Log($"[HighlightEffect] ✅ Detectados {renderers.Length} Renderers en '{gameObject.name}'");
            return;
        }

        // No se detectó nada
        Debug.LogWarning($"[HighlightEffect] ⚠️ NO se detectaron componentes Image ni Renderer en '{gameObject.name}'");
    }

    /// <summary>
    /// Inicia el efecto de resaltado
    /// </summary>
    public void StartHighlight()
    {
        if (isHighlighting)
        {
            Debug.Log($"[HighlightEffect] ⚠️ Ya está resaltado: {gameObject.name}");
            return;
        }

        // Verificar si hay componentes detectados
        bool hasComponents = (images != null && images.Length > 0) || (renderers != null && renderers.Length > 0);
        if (!hasComponents)
        {
            Debug.LogError($"[HighlightEffect] ❌ NO HAY COMPONENTES para resaltar en '{gameObject.name}'. Ejecuta DetectComponents() primero.");
            return;
        }

        isHighlighting = true;

        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
        }

        highlightCoroutine = StartCoroutine(HighlightCoroutine());
        Debug.Log($"[HighlightEffect] ✅ Resaltado iniciado en '{gameObject.name}' (Color: {highlightColor}, Intensidad: {intensity})");
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

        // Restaurar color original
        RestoreOriginalColor();
        Debug.Log($"[HighlightEffect] ⏹️ Resaltado detenido en '{gameObject.name}'");
    }

    /// <summary>
    /// Alterna el estado del resaltado
    /// </summary>
    public void ToggleHighlight()
    {
        if (isHighlighting)
        {
            StopHighlight();
        }
        else
        {
            StartHighlight();
        }
    }

    /// <summary>
    /// Corrutina de parpadeo suave
    /// </summary>
    private IEnumerator HighlightCoroutine()
    {
        Debug.Log($"[HighlightEffect] 🔄 Iniciando bucle de parpadeo en '{gameObject.name}'");
        int frameCount = 0;

        while (isHighlighting)
        {
            // Calcular factor de parpadeo (oscila entre 0 y 1)
            float pulse = Mathf.PingPong(Time.unscaledTime * pulseSpeed, 1f);

            // Interpolación suave con curva ease-in-out
            pulse = Mathf.SmoothStep(0f, 1f, pulse);

            // Calcular color interpolado
            Color targetColor = Color.Lerp(Color.white, highlightColor, pulse * intensity);

            // Log cada 30 frames para debugging
            if (frameCount % 30 == 0)
            {
                Debug.Log($"[HighlightEffect] Frame {frameCount}: pulse={pulse:F2}, targetColor={targetColor}");
            }

            // Aplicar color según el tipo de componente
            if (images != null && images.Length > 0)
            {
                ApplyColorToImages(targetColor);
            }
            else if (renderers != null && renderers.Length > 0)
            {
                ApplyColorToRenderers(targetColor);
            }

            frameCount++;
            yield return null;
        }

        Debug.Log($"[HighlightEffect] 🛑 Bucle de parpadeo terminado en '{gameObject.name}' (frames: {frameCount})");
    }

    /// <summary>
    /// Aplica el color a todas las imágenes (UI)
    /// </summary>
    private void ApplyColorToImages(Color targetColor)
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] == null) continue;

            // Interpolar entre el color original y el color objetivo
            images[i].color = Color.Lerp(originalColors[i], originalColors[i] * targetColor, intensity);
        }
    }

    /// <summary>
    /// Aplica el color a todos los renderers (3D)
    /// </summary>
    private void ApplyColorToRenderers(Color color)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            Color blendedColor = Color.Lerp(originalRendererColors[i], originalRendererColors[i] * color, intensity);

            // Usar MaterialPropertyBlock para evitar instanciar materiales
            renderers[i].GetPropertyBlock(propertyBlock);

            if (renderers[i].material.HasProperty("_Color"))
            {
                propertyBlock.SetColor("_Color", blendedColor);
            }
            else if (renderers[i].material.HasProperty("_BaseColor"))
            {
                propertyBlock.SetColor("_BaseColor", blendedColor);
            }

            renderers[i].SetPropertyBlock(propertyBlock);
        }
    }

    /// <summary>
    /// Restaura el color original
    /// </summary>
    private void RestoreOriginalColor()
    {
        if (images != null && images.Length > 0)
        {
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null)
                {
                    images[i].color = originalColors[i];
                }
            }
        }
        else if (renderers != null && renderers.Length > 0)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;

                renderers[i].GetPropertyBlock(propertyBlock);

                if (renderers[i].material.HasProperty("_Color"))
                {
                    propertyBlock.SetColor("_Color", originalRendererColors[i]);
                }
                else if (renderers[i].material.HasProperty("_BaseColor"))
                {
                    propertyBlock.SetColor("_BaseColor", originalRendererColors[i]);
                }

                renderers[i].SetPropertyBlock(propertyBlock);
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
    /// Fuerza redetección de componentes (útil para debugging)
    /// </summary>
    [ContextMenu("Redetectar Componentes")]
    public void ForceRedetect()
    {
        DetectComponents();
    }
}