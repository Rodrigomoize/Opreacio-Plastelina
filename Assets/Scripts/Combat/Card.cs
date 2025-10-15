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
    public GameObject selectionHighlight; // opcional: un objeto UI que marca selección

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

    // API que el manager usa para marcar/desmarcar visualmente
    public void SetSelectedVisual(bool on)
    {
        isSelected = on;
        if (selectionHighlight != null)
            selectionHighlight.SetActive(on);
        else
            Debug.Log($"[CardDisplay] Visual select {cardData?.cardName} = {on}");
    }

    public CardManager.Card GetCardData() => cardData;
}
