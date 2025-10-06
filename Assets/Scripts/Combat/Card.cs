using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class CardDisplay : MonoBehaviour, IPointerClickHandler
{
    public CardManager.Card cardData;
    public Image highlightImage; 
    [HideInInspector] public PlayerCardManager ownerManager;

    void Awake()
    {
        if (highlightImage != null) highlightImage.gameObject.SetActive(false);
    }

    public void SetCardData(CardManager.Card data)
    {
        cardData = data;

        // Si la carta tiene un "Artwork" Image child, intenta asignar un Sprite
        if (cardData != null && cardData.cardImage != null)
        {
            UnityEngine.UI.Image artwork = transform.Find("Artwork")?.GetComponent<UnityEngine.UI.Image>();
            if (artwork != null)
            {
                // si cardImage es un prefab con SpriteRenderer o Image, intenta extraer el sprite
                var sr = cardData.cardImage.GetComponent<SpriteRenderer>();
                if (sr != null) artwork.sprite = sr.sprite;
                else
                {
                    var img = cardData.cardImage.GetComponent<UnityEngine.UI.Image>();
                    if (img != null) artwork.sprite = img.sprite;
                    else
                    {
                        // fallback: si cardImage tiene una textura en un material, no manejamos aquí
                    }
                }
            }
        }
    }


    public CardManager.Card GetCardData() => cardData;

    public void OnPointerClick(PointerEventData eventData)
    {
        ownerManager?.OnCardClicked(this);
    }

    public void SetHighlight(bool on)
    {
        if (highlightImage != null) highlightImage.gameObject.SetActive(on);
        else
        {
            var img = GetComponent<Image>();
            if (img != null) img.color = on ? new Color(1f, 1f, 0.85f) : Color.white;
        }
    }
}
