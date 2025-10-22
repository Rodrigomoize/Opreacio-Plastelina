using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Añade efectos visuales de hover y press a botones de operación
/// Adjuntar este script a los botones + y -
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonPressEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Visual Effects")]
    [Tooltip("Escala cuando el ratón está encima (hover)")]
    public float hoverScale = 1.1f;
    [Tooltip("Escala cuando se pulsa el botón (press)")]
    public float pressScale = 0.9f;
    [Tooltip("Velocidad de la animación de escala")]
    public float scaleAnimationSpeed = 12f;
    [Tooltip("Rotación al hacer hover (en grados)")]
    public float hoverRotation = 5f;
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private bool isHovering = false;
    private bool isPressing = false;
    private Button button;
    
    void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
        targetScale = originalScale;
        originalRotation = transform.localRotation;
        targetRotation = originalRotation;
    }
    
    void Update()
    {
        // Animar escala suavemente
        if (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleAnimationSpeed);
        }
        
        // Animar rotación suavemente
        if (transform.localRotation != targetRotation)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * scaleAnimationSpeed);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Solo animar si el botón está interactuable
        if (button != null && !button.interactable)
            return;
            
        isHovering = true;
        UpdateVisualState();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        isPressing = false;
        UpdateVisualState();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        // Solo animar si el botón está interactuable
        if (button != null && !button.interactable)
            return;
            
        isPressing = true;
        UpdateVisualState();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressing = false;
        UpdateVisualState();
    }
    
    private void UpdateVisualState()
    {
        if (isPressing)
        {
            // Estado presionado: escala reducida, sin rotación
            targetScale = originalScale * pressScale;
            targetRotation = originalRotation;
        }
        else if (isHovering)
        {
            // Estado hover: escala aumentada, ligera rotación
            targetScale = originalScale * hoverScale;
            targetRotation = originalRotation * Quaternion.Euler(0, 0, hoverRotation);
        }
        else
        {
            // Estado normal
            targetScale = originalScale;
            targetRotation = originalRotation;
        }
    }
    
    /// <summary>
    /// Resetea el estado visual (útil cuando el botón se deshabilita externamente)
    /// </summary>
    public void ResetVisualState()
    {
        isHovering = false;
        isPressing = false;
        transform.localScale = originalScale;
        transform.localRotation = originalRotation;
        targetScale = originalScale;
        targetRotation = originalRotation;
    }
}
