using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Gestiona TODOS los efectos visuales del tutorial con prioridad sobre PlayerCardManager
/// - Resaltado con pulso de escala y color (cuando la carta está seleccionable)
/// - Tinte gris para cartas bloqueadas
/// - Se pausa cuando PlayerCardManager toma control (selección activa)
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class TutorialHighlight : MonoBehaviour
{
    [Header("Configuración de Escala")]
    [Range(1.0f, 1.5f)]
    public float pulseScaleMax = 1.15f;

    [Range(0.8f, 1.0f)]
    public float pulseScaleMin = 1.0f;

    [Range(0.5f, 5f)]
    public float pulseSpeed = 2f;

    [Header("Configuración de Color")]
    public bool enableColorPulse = true;
    public Color highlightColor = Color.yellow;

    [Range(0f, 1f)]
    public float colorIntensity = 0.5f;

    [Header("Tutorial Blocking")]
    public Color blockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    [Range(0f, 1f)]
    public float blockedTintStrength = 0.7f;

    [Header("Ocultación de Cartas")]
    [Tooltip("Alpha cuando está oculta")]
    [Range(0f, 0.3f)]
    public float hiddenAlpha = 0.15f;

    [Header("Configuración de Componentes")]
    public bool autoDetectImages = true; // Declarar la variable autoDetectImages

    // Estado
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHighlighting = false;
    private bool isBlocked = false;
    private bool isPausedByPlayerCardManager = false;
    private bool isHidden = false;
    private HideMode currentHideMode = HideMode.None;
    private Coroutine highlightCoroutine;

    // Componentes visuales
    private Image[] images;
    private Graphic[] graphics;
    private Color[] originalColors;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private CardDisplay cardDisplay;
    private bool includeAllGraphics = false; // Agregar esta línea para definir la variable

    public enum HideMode
    {
        None,           // Visible normal
        ClickToReveal,  // Oculta, click para mostrar (modo actual)
        Locked          // Oculta hasta que el tutorial lo permita
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        targetScale = originalScale;

        cardDisplay = GetComponent<CardDisplay>();

        // Crear CanvasGroup para control de alpha
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (autoDetectImages)
        {
            DetectComponents();
        }
    }

    void Update()
    {
        if (!isPausedByPlayerCardManager && rectTransform.localScale != targetScale)
        {
            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale,
                targetScale,
                Time.unscaledDeltaTime * 10f
            );
        }
    }

    private void DetectComponents()
    {
        images = GetComponentsInChildren<Image>(true);

        if ((images == null || images.Length == 0) &&   includeAllGraphics)
        {
            graphics = GetComponentsInChildren<Graphic>(true);
        }

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
        }
    }

    /// <summary>
    /// Oculta la carta con modo "Click para revelar" (modo actual)
    /// </summary>
    public void HideCard_ClickToReveal()
    {
        currentHideMode = HideMode.ClickToReveal;
        isHidden = true;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = hiddenAlpha;
        }

        Debug.Log($"[TutorialHighlight] 👁️ Carta OCULTA (Click para revelar): {gameObject.name}");
    }

    /// <summary>
    /// Oculta la carta en modo bloqueado (el tutorial debe revelarla)
    /// </summary>
    public void HideCard_Locked()
    {
        currentHideMode = HideMode.Locked;
        isHidden = true;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = hiddenAlpha;
        }

        // Deshabilitar interacción
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        Debug.Log($"[TutorialHighlight] 🔒 Carta BLOQUEADA (Tutorial debe revelar): {gameObject.name}");
    }

    /// <summary>
    /// Revela la carta (solo funciona si no está en modo Locked O si forzamos)
    /// </summary>
    public void RevealCard(bool force = false)
    {
        if (currentHideMode == HideMode.Locked && !force)
        {
            Debug.LogWarning($"[TutorialHighlight] ⚠️ Intento de revelar carta bloqueada sin force: {gameObject.name}");
            return;
        }

        currentHideMode = HideMode.None;
        isHidden = false;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        Debug.Log($"[TutorialHighlight] 👁️✅ Carta REVELADA: {gameObject.name}");
    }

    /// <summary>
    /// Maneja el click en carta oculta (solo si es modo ClickToReveal)
    /// </summary>
    public void OnCardClicked()
    {
        if (isHidden && currentHideMode == HideMode.ClickToReveal)
        {
            RevealCard();
        }
    }

    /// <summary>
    /// Pausa el efecto visual del tutorial para dar prioridad a PlayerCardManager
    /// </summary>
    public void PauseForPlayerCardManager(bool pause)
    {
        isPausedByPlayerCardManager = pause;

        if (pause)
        {
            // Detener highlight si estaba activo
            if (isHighlighting)
            {
                if (highlightCoroutine != null)
                {
                    StopCoroutine(highlightCoroutine);
                    highlightCoroutine = null;
                }
                targetScale = originalScale;
            }

            // Restaurar colores para que PlayerCardManager tome control
            if (isBlocked)
            {
                RestoreOriginalColors();
            }
        }
        else
        {
            // Reactivar efectos si correspondía
            if (isHighlighting)
            {
                highlightCoroutine = StartCoroutine(HighlightCoroutine());
            }
            if (isBlocked)
            {
                ApplyBlockedTint();
            }
        }
    }

    public void StartHighlight()
    {
        if (isHighlighting || isPausedByPlayerCardManager || isHidden) return;

        if (isBlocked)
        {
            SetBlocked(false);
        }

        isHighlighting = true;

        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
        }

        highlightCoroutine = StartCoroutine(HighlightCoroutine());
        Debug.Log($"[TutorialHighlight] 🌟 Resaltado iniciado en '{gameObject.name}'");
    }

    public void StopHighlight()
    {
        if (!isHighlighting) return;

        isHighlighting = false;

        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
            highlightCoroutine = null;
        }

        targetScale = originalScale;

        if (!isPausedByPlayerCardManager)
        {
            RestoreOriginalColors();
        }

        Debug.Log($"[TutorialHighlight] 🔵 Resaltado detenido en '{gameObject.name}'");
    }

    public void SetBlocked(bool blocked)
    {
        if (isBlocked == blocked || isPausedByPlayerCardManager) return;

        isBlocked = blocked;

        if (blocked && isHighlighting)
        {
            StopHighlight();
        }

        if (blocked)
        {
            ApplyBlockedTint();
            Debug.Log($"[TutorialHighlight] 🚫 Carta bloqueada: '{gameObject.name}'");
        }
        else
        {
            RestoreOriginalColors();
            Debug.Log($"[TutorialHighlight] ✅ Carta desbloqueada: '{gameObject.name}'");
        }
    }

    /// <summary>
    /// NUEVO: Fuerza el estado bloqueado AHORA (ignorando PlayerCardManager temporalmente)
    /// Útil para bloquear todas las cartas durante explicaciones
    /// </summary>
    public void ForceBlockedState(bool blocked)
    {
        bool wasPaused = isPausedByPlayerCardManager;
        isPausedByPlayerCardManager = false; // Temporal para aplicar el cambio

        isBlocked = blocked;

        if (blocked)
        {
            if (isHighlighting)
            {
                StopHighlight();
            }
            ApplyBlockedTint();
        }
        else
        {
            RestoreOriginalColors();
        }

        isPausedByPlayerCardManager = wasPaused;
        Debug.Log($"[TutorialHighlight] 💪 Estado forzado (bloqueado={blocked}) en '{gameObject.name}'");
    }

    private void ApplyBlockedTint()
    {
        if (isPausedByPlayerCardManager) return;

        int index = 0;

        if (images != null)
        {
            foreach (var img in images)
            {
                if (img != null && index < originalColors.Length)
                {
                    img.color = Color.Lerp(originalColors[index], blockedColor, blockedTintStrength);
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
                    graphic.color = Color.Lerp(originalColors[index], blockedColor, blockedTintStrength);
                    index++;
                }
            }
        }
    }

    private IEnumerator HighlightCoroutine()
    {
        while (isHighlighting && !isPausedByPlayerCardManager)
        {
            float pulse = Mathf.PingPong(Time.unscaledTime * pulseSpeed, 1f);
            pulse = Mathf.SmoothStep(0f, 1f, pulse);

            float currentScale = Mathf.Lerp(pulseScaleMin, pulseScaleMax, pulse);
            targetScale = originalScale * currentScale;

            if (enableColorPulse)
            {
                Color targetColor = Color.Lerp(Color.white, highlightColor, pulse * colorIntensity);
                ApplyColorPulse(targetColor);
            }

            yield return null;
        }
    }

    private void ApplyColorPulse(Color targetColor)
    {
        if (isPausedByPlayerCardManager) return;

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

    private void RestoreOriginalColors()
    {
        if (isPausedByPlayerCardManager) return;

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
}