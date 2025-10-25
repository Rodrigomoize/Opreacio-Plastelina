using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de Intelecto que escala la altura en lugar de usar fillAmount
/// Perfecto para sprites con formas redondeadas tipo "polo"
/// </summary>
public class IntelectBar : MonoBehaviour
{
    [Header("Referencias - Método de Escala")]
    public IntelectManager intelectManager;
    public RectTransform fillScaler;        // El GameObject que cambia de altura
    public RectTransform previewScaler;     // Opcional: para mostrar preview

    [Header("Referencias - Método Alternativo (Image Fill)")]
    public Image currentFillImage;          // Si prefieres usar fillAmount
    public Image previewFillImage;

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

        // Configurar preview si existe
        if (previewScaler != null)
        {
            previewScaler.gameObject.SetActive(false);
        }
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

        // Actualizar preview
        if (enablePreview && currentPreviewCost > 0)
        {
            UpdatePreview(currentValue);
        }
    }

    private void UpdatePreview(float currentValue)
    {
        float previewValue = Mathf.Max(0, currentValue - currentPreviewCost);
        float previewPercent = Mathf.Clamp01(previewValue / maxIntelect);
        float previewHeight = maxHeight * previewPercent;

        // Método de escala
        if (previewScaler != null)
        {
            previewScaler.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, previewHeight);
        }

        // Método fillAmount
        if (previewFillImage != null)
        {
            previewFillImage.fillAmount = previewPercent;
        }
    }

    /// <summary>
    /// Muestra preview de cuánto intelecto quedaría
    /// </summary>
    public void ShowPreview(int cost)
    {
        if (!enablePreview) return;

        currentPreviewCost = cost;

        if (previewScaler != null)
        {
            previewScaler.gameObject.SetActive(cost > 0);
        }

        if (previewFillImage != null)
        {
            previewFillImage.gameObject.SetActive(cost > 0);
        }
    }

    /// <summary>
    /// Oculta el preview
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