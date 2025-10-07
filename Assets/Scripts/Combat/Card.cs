using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class CardDisplay : MonoBehaviour, IPointerClickHandler
{
    public CardManager.Card cardData;

    [Header("UI refs (assign in prefab)")]
    public Image artworkImage;         // child "Artwork"
    public Image highlightImage;       // child "Highlight" (active when selected)
    public Image desaturateOverlay;    // child "Desaturate" (active when NOT selected while others are selected)



    [HideInInspector] public PlayerCardManager ownerManager;

    [HideInInspector] public bool isSelected = false; // estado local

    private float lastClickTime = 0f;
    private float clickDebounce = 0.15f;

    private Color originalArtworkColor;
    private Sprite originalArtworkSprite;

    void Awake()
    {
        if (artworkImage != null)
        {
            originalArtworkColor = artworkImage.color;
            originalArtworkSprite = artworkImage.sprite;
        }

    }

    public void SetCardData(CardManager.Card data)
    {
        cardData = data;
        if (cardData == null) return;

        // Asignar sprite desde cardData.cardSprite (o fallback)
        if (artworkImage != null)
        {
            if (cardData.cardSprite != null)
            {
                artworkImage.sprite = cardData.cardSprite;
                artworkImage.color = Color.white;
                artworkImage.gameObject.SetActive(true);
            }
            else
            {
                artworkImage.sprite = originalArtworkSprite;
                artworkImage.color = originalArtworkColor;
                artworkImage.gameObject.SetActive(true);
                Debug.Log($"CardDisplay: fallback artwork para '{cardData.cardName}'");
            }
        }
    }

    // Compatibilidad: GetCardData (muchos sitios lo llaman)
    public CardManager.Card GetCardData()
    {
        return cardData;
    }

    // Toggle de selección local + notificar manager
    public void ToggleSelected()
    {
        isSelected = !isSelected;
        ApplyHighlight(isSelected);
        ownerManager?.OnCardClicked(this); // manager sincroniza lista y visuales globales
    }

    // Aplicar highlight visual (no desactiva el GameObject)
    public void ApplyHighlight(bool on)
    {
        if (highlightImage != null) highlightImage.gameObject.SetActive(on);
        else if (artworkImage != null) artworkImage.color = on ? new Color(1f, 0.95f, 0.8f) : originalArtworkColor;
    }

    // Compatibilidad: SetHighlight (tu PlayerCardManager lo llama así)
    public void SetHighlight(bool on)
    {
        ApplyHighlight(on);
    }

    // Desaturación local (por manager)
    public void SetDesaturate(bool on)
    {
        if (desaturateOverlay != null) desaturateOverlay.gameObject.SetActive(on);
        else if (artworkImage != null) artworkImage.color = on ? new Color(0.7f, 0.7f, 0.7f) : originalArtworkColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        float now = Time.unscaledTime;
        if (now - lastClickTime < clickDebounce)
        {
            Debug.Log("Click ignorado por debounce");
            return;
        }
        lastClickTime = now;

        ToggleSelected();
        Debug.Log($"CardDisplay: OnPointerClick -> {cardData?.cardName} (isSelected={isSelected})");
    }
}
