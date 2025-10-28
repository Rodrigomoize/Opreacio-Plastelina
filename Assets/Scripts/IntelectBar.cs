using UnityEngine;
using UnityEngine.UI;
using TMPro; // Añadido para TextMeshPro

/// Barra de Intelecto que escala la altura en lugar de usar fillAmount
/// Perfecto para sprites con formas redondeadas tipo "polo"
public class IntelectBar : MonoBehaviour
{
    [Header("Referencias - Método de Escala")]
    public IntelectManager intelectManager;
    public RectTransform fillScaler;        
    public RectTransform previewScaler;    

    [Header("Referencias - Método Alternativo (Image Fill)")]
    public Image currentFillImage;          
    public Image previewFillImage;

    [Header("Texto de Intelecto")]
    public TextMeshProUGUI intelectText;    
    public Text intelectTextLegacy;         
    public string textFormat = "{0}";   

    [Header("Configuración")]
    public float maxHeight = 856f;         
    public bool useSmoothTransition = true;
    public float smoothSpeed = 5f;

    [Header("Preview")]
    public bool enablePreview = true;

    [Header("Shake Effect")]
    [Tooltip("Duración del shake cuando no hay suficiente intelecto")]
    public float shakeDuration = 0.5f;
    
    [Tooltip("Magnitud/intensidad del shake (distancia del temblor)")]
    public float shakeMagnitude = 15f;

    private float maxIntelect = 10;
    private float currentHeight = 0f;
    private float targetHeight = 0f;
    private float currentPreviewCost = 0f;
    private float lastIntellectValue = -1f; // Cache para evitar updates innecesarios
    private Coroutine shakeCoroutine;

    void Start()
    {
        SetupBar();
        // UpdateBar() se llama en Update() de todas formas
    }

    void Update()
    {
        // Optimización: solo actualizar si el valor cambió
        if (intelectManager != null)
        {
            float currentValue = intelectManager.GetCurrentIntelectFloat();
            if (!Mathf.Approximately(lastIntellectValue, currentValue))
            {
                lastIntellectValue = currentValue;
                UpdateBar();
            }
        }
    }

    private void SetupBar()
    {
        if (intelectManager == null)
        {
            Debug.LogError("IntelectManager no asignado!");
            return;
        }

        maxIntelect = intelectManager.maxIntelect;

        // Detectar altura máxima automáticamente si no está configurada
        if (maxHeight <= 0 && fillScaler != null)
        {
            RectTransform parent = fillScaler.parent as RectTransform;
            if (parent != null)
            {
                maxHeight = parent.rect.height;
            }
        }

        // Configurar preview al inicio
        HidePreview();
    }

    public void UpdateBar()
    {
        if (intelectManager == null) return;

        float currentValue = intelectManager.GetCurrentIntelectFloat();
        float fillPercent = Mathf.Clamp01(currentValue / maxIntelect);
        targetHeight = maxHeight * fillPercent;

        
        if (fillScaler != null)
        {
            if (useSmoothTransition)
            {
                currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * smoothSpeed);
            }
            else
            {
                currentHeight = targetHeight;
            }

            fillScaler.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentHeight);
        }

        
        if (currentFillImage != null)
        {
            if (useSmoothTransition)
            {
                currentFillImage.fillAmount = Mathf.Lerp(
                    currentFillImage.fillAmount,
                    fillPercent,
                    Time.deltaTime * smoothSpeed
                );
            }
            else
            {
                currentFillImage.fillAmount = fillPercent;
            }
        }

        // Actualizar texto del intelecto
        UpdateIntelectText(currentValue);

        // Actualizar preview si hay costo activo
        if (enablePreview && currentPreviewCost > 0)
        {
            UpdatePreview(currentValue);
        }
    }

    /// Actualiza el texto que muestra el valor numérico del intelecto
    private void UpdateIntelectText(float currentValue)
    {
        int currentInt = Mathf.FloorToInt(currentValue);
        int maxInt = Mathf.RoundToInt(maxIntelect);
        string displayText = string.Format(textFormat, currentInt, maxInt);

        // TextMeshPro (recomendado)
        if (intelectText != null)
        {
            intelectText.text = displayText;
        }

        // UI Text tradicional (fallback)
        if (intelectTextLegacy != null)
        {
            intelectTextLegacy.text = displayText;
        }
    }

    private void UpdatePreview(float currentValue)
    {
        // Calcular cuánto intelecto quedaría después de gastar currentPreviewCost
        float previewValue = Mathf.Max(0, currentValue - currentPreviewCost);
        float previewPercent = Mathf.Clamp01(previewValue / maxIntelect);
        float previewHeight = maxHeight * previewPercent;

        // Método de escala
        if (previewScaler != null)
        {
            previewScaler.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, previewHeight);
        }

        // Método fillAmount - AQUÍ es donde se ve el preview del intelecto restante
        if (previewFillImage != null)
        {
            previewFillImage.fillAmount = previewPercent;
        }
    }

    /// Muestra preview de cuánto intelecto quedaría después de gastar el costo especificado
    /// <param name="cost">Costo de la carta u operación seleccionada (0 si no hay selección)</param>
    public void ShowPreview(int cost)
    {
        if (!enablePreview) return;

        currentPreviewCost = cost;

        // Activar/desactivar el preview según si hay costo
        bool showPreview = cost > 0;

        if (previewScaler != null)
        {
            previewScaler.gameObject.SetActive(showPreview);
        }

        if (previewFillImage != null)
        {
            previewFillImage.gameObject.SetActive(showPreview);
        }

        // Forzar actualización inmediata del preview
        if (showPreview && intelectManager != null)
        {
            UpdatePreview(intelectManager.GetCurrentIntelectFloat());
        }
    }

    /// Oculta el preview (cuando no hay carta seleccionada)
    public void HidePreview()
    {
        currentPreviewCost = 0;

        if (previewScaler != null)
        {
            previewScaler.gameObject.SetActive(false);
        }

        if (previewFillImage != null)
        {
            previewFillImage.gameObject.SetActive(false);
        }
    }

    // Método de compatibilidad
    public void SetIntelect()
    {
        UpdateBar();
    }

    /// Ajusta la altura máxima manualmente
    public void SetMaxHeight(float height)
    {
        maxHeight = height;
    }
    
    /// Shake visual de la barra con parámetros personalizados
    public void ShakeBar()
    {
        // Reproducir sonido de error
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayInsufficientIntellectSFX();
        }
        
        // Llamar al shake con parámetros configurados en el Inspector
        ShakeBar(shakeDuration, shakeMagnitude);
    }

    public void ShakeBar(float duration, float magnitude)
    {
        // Detener shake anterior si existe
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        
        // Iniciar nueva corrutina de shake
        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private System.Collections.IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        // Intentar obtener RectTransform primero, si no hay, usar Transform normal
        RectTransform rectTransform = GetComponent<RectTransform>();
        Transform normalTransform = transform;
        
        Vector3 originalPosition;
        bool useRectTransform = rectTransform != null;
        
        if (useRectTransform)
        {
            originalPosition = rectTransform.localPosition;
            Debug.LogWarning("[IntelectBar] Usando RectTransform para shake"); // DEBUG
        }
        else
        {
            originalPosition = normalTransform.localPosition;
            Debug.LogWarning("[IntelectBar] Usando Transform para shake"); // DEBUG
        }
        
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Vector3 shakeOffset = originalPosition + new Vector3(x, y, 0);
            
            if (useRectTransform)
            {
                rectTransform.localPosition = shakeOffset;
            }
            else
            {
                normalTransform.localPosition = shakeOffset;
            }

            elapsed += Time.unscaledDeltaTime; // Usar unscaled para que funcione en pausa
            yield return null;
        }

        // Restaurar posición original
        if (useRectTransform)
        {
            rectTransform.localPosition = originalPosition;
        }
        else
        {
            normalTransform.localPosition = originalPosition;
        }
        
        shakeCoroutine = null;
    }
}