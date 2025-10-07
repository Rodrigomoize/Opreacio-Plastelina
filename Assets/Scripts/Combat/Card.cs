using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CardDisplay : MonoBehaviour, IPointerClickHandler
{
    public CardManager.Card cardData;
    [HideInInspector] public PlayerCardManager ownerManager;

    [HideInInspector] public bool isSelected = false;

    public Image artworkImage; 

    public void SetCardData(CardManager.Card data)
    {
        cardData = data;
        if (artworkImage != null && cardData != null && cardData.cardSprite != null)
        {
            artworkImage.sprite = cardData.cardSprite;
            artworkImage.gameObject.SetActive(true); // asegúrate de que la imagen esté visible
        }
    }

    // Toggle lógico de selección + notificar al manager
    public void ToggleSelected()
    {
        isSelected = !isSelected;
        Debug.Log($"[CardDisplay] ToggleSelected -> {cardData?.cardName}  isSelected={isSelected}");
        ownerManager?.OnCardClicked(this);
    }

    // Cuando el usuario hace click en la carta
    public void OnPointerClick(PointerEventData eventData)
    {
        // simple debounce ligero (evita clicks duplicados muy rápidos)
        ToggleSelected();
    }

    // API pequeña de compatibilidad (si tu manager la usa)
    public CardManager.Card GetCardData() => cardData;
}
