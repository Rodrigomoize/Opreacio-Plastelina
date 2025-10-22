using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Feedback visual que muestra +1 Intelecto, se eleva y desvanece
/// </summary>
public class IntellectFeedback : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Velocidad de elevación en unidades por segundo")]
    public float riseSpeed = 2f;
    
    [Tooltip("Duración total del efecto antes de destruirse")]
    public float duration = 1f;
    
    [Tooltip("Escala inicial (pop-in effect)")]
    public float initialScale = 0.5f;
    
    [Tooltip("Escala final")]
    public float targetScale = 1f;
    
    [Tooltip("Duración del pop-in (escala)")]
    public float popDuration = 0.2f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Camera mainCamera;
    private float elapsedTime = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        
        // Obtener componentes
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        rectTransform = GetComponent<RectTransform>();
        
        // Configurar estado inicial
        canvasGroup.alpha = 1f;
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one * initialScale;
        }
        
        // Iniciar animación
        StartCoroutine(AnimateFeedback());
    }

    void LateUpdate()
    {
        // Hacer que el canvas mire a la cámara
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }

    private IEnumerator AnimateFeedback()
    {
        Vector3 startPosition = transform.position;
        
        // Fase 1: Pop-in (escala)
        float popTime = 0f;
        while (popTime < popDuration)
        {
            popTime += Time.deltaTime;
            float t = popTime / popDuration;
            
            if (rectTransform != null)
            {
                float scale = Mathf.Lerp(initialScale, targetScale, Mathf.SmoothStep(0, 1, t));
                rectTransform.localScale = Vector3.one * scale;
            }
            
            yield return null;
        }
        
        // Fase 2: Elevación y fade out
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            // Elevarse
            transform.position = startPosition + Vector3.up * (riseSpeed * elapsedTime);
            
            // Fade out (comienza después del pop-in)
            float fadeStartTime = popDuration;
            if (elapsedTime > fadeStartTime)
            {
                float fadeTime = elapsedTime - fadeStartTime;
                float fadeDuration = duration - fadeStartTime;
                float alpha = 1f - (fadeTime / fadeDuration);
                canvasGroup.alpha = Mathf.Clamp01(alpha);
            }
            
            yield return null;
        }
        
        // Destruir el objeto
        Destroy(gameObject);
    }

    /// <summary>
    /// Método estático para crear un feedback de intelecto en una posición
    /// </summary>
    public static void Create(GameObject prefab, Vector3 worldPosition)
    {
        if (prefab != null)
        {
            Instantiate(prefab, worldPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("[IntellectFeedback] Prefab es null!");
        }
    }
}
