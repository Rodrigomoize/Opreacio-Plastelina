using UnityEngine;
using UnityEngine.UI;
using TMPro; // Añadido para TextMeshPro

/// <summary>
/// Barra de Intelecto que escala la altura en lugar de usar fillAmount
/// Perfecto para sprites con formas redondeadas tipo "polo"
/// </summary>
public class IntelectBar : MonoBehaviour
{
    [Header("Referencias - Método de Escala")]
    public IntelectManager intelectManager;
    public RectTransform fillScaler;        
    public RectTransform previewScaler;    

    [Header("Referencias - Método Alternativo (Image Fill)")]
    public Image currentFillImage;          // Si prefieres usar fillAmount
    public Image previewFillImage;

    [Header("Texto de Intelecto")]
    public TextMeshProUGUI intelectText;    // Para TextMeshPro
    public Text intelectTextLegacy;         // Para UI Text tradicional
    public string textFormat = "{0}/{1}";   // Formato: "7/10"

    [Header("Configuración")]
    public float maxHeight = 856f;          // Altura máxima de la barra
    public bool useSmoothTransition = true;
    public float smoothSpeed = 5f;

    [Header("Preview")]
    public bool enablePreview = true;

    private float maxIntelect = 10;
    private float currentHeight = 0f;
    private float targetHeight = 0f;
    private float currentPreviewCost = 0f;

    void Start()
    {
        SetupBar();
        UpdateBar();
    }

    void Update()
    {
        UpdateBar();
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

        // MÉTODO 1: Escalar altura (RECOMENDADO para sprites redondeados)
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

        // MÉTODO 2: Fill Amount (alternativo, pero corta las esquinas)
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

    /// <summary>
    /// Actualiza el texto que muestra el valor numérico del intelecto
    /// </summary>
    private void UpdateIntelectText(float currentValue)
    {
        int currentInt = Mathf.RoundToInt(currentValue);
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

    /// <summary>
    /// Muestra preview de cuánto intelecto quedaría después de gastar el costo especificado
    /// </summary>
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

    /// <summary>
    /// Oculta el preview (cuando no hay carta seleccionada)
    /// </summary>
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

    /// <summary>
    /// Ajusta la altura máxima manualmente
    /// </summary>
    public void SetMaxHeight(float height)
    {
        maxHeight = height;
    }
}