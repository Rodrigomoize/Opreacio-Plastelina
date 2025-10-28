using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CardDisplay : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public CardManager.Card cardData;
    [HideInInspector] public PlayerCardManager ownerManager;

    [HideInInspector] public bool isSelected = false;

    public Image artworkImage;
    public GameObject selectionHighlight; // opcional: un objeto UI que marca selección
    
    [Header("Visual Effects")]
    [Tooltip("Escala cuando el ratón está encima (hover)")]
    public float hoverScale = 1.1f;
    [Tooltip("Escala cuando se pulsa la carta (press)")]
    public float pressScale = 0.95f;
    [Tooltip("Velocidad de la animación de escala")]
    public float scaleAnimationSpeed = 10f;
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovering = false;
    private bool isPressing = false;

    void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }
    
    void Update()
    {
        // Animar escala suavemente
        if (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleAnimationSpeed);
        }
    }

    public void SetCardData(CardManager.Card data)
    {
        cardData = data;
        if (artworkImage != null && cardData != null && cardData.cardSprite != null)
        {
            artworkImage.sprite = cardData.cardSprite;
            artworkImage.gameObject.SetActive(true);
        }
    }

    // NO TOGGLE aquí: delegamos al manager
    public void OnPointerClick(PointerEventData eventData)
    {
        ownerManager?.OnCardClickedRequest(this);
    }
    
    // Cuando el ratón entra en la carta
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        UpdateVisualState();
    }
    
    // Cuando el ratón sale de la carta
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        isPressing = false; // Reset press si salimos
        UpdateVisualState();
    }
    
    // Cuando se presiona el botón del ratón
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressing = true;
        UpdateVisualState();
    }
    
    // Cuando se suelta el botón del ratón
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressing = false;
        UpdateVisualState();
    }
    
    /// Actualiza el estado visual basado en hover y press
    private void UpdateVisualState()
    {
        // Prioridad: Press > Hover > Normal
        if (isPressing)
        {
            // Estado presionado: escala reducida
            targetScale = originalScale * pressScale;
        }
        else if (isHovering)
        {
            // Estado hover: escala aumentada
            targetScale = originalScale * hoverScale;
        }
        else
        {
            // Estado normal
            targetScale = originalScale;
        }
    }

    // API que el manager usa para marcar/desmarcar visualmente
    public void SetSelectedVisual(bool on)
    {
        isSelected = on;
        if (selectionHighlight != null)
            selectionHighlight.SetActive(on);
    }

    public CardManager.Card GetCardData() => cardData;
}