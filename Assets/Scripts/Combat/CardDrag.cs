using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class CardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public PlayerCardManager playerManager; 
    public CardDisplay cardDisplay; // Referencia al CardDisplay del prefab
    private Canvas rootCanvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalAnchoredPos;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (cardDisplay == null) cardDisplay = GetComponent<CardDisplay>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalAnchoredPos = rectTransform.anchoredPosition;
        transform.SetParent(rootCanvas.transform, true); 
        canvasGroup.blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 movePos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out movePos))
        {
            rectTransform.anchoredPosition = movePos;
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        // Raycast al mundo para ver si hemos soltado sobre una PlayableArea
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider != null && hit.collider.CompareTag("PlayableArea"))
            {
                Vector3 spawnPos = hit.point;
                Debug.Log("CardDrag: soltada en PlayableArea.");

                // Recuperar los datos desde el CardDisplay
                if (cardDisplay == null)
                {
                    Debug.LogError("CardDrag: cardDisplay es null. ¿Se asignó en CreateCard?");
                    ReturnToOriginal();
                    return;
                }

                CardManager.Card data = cardDisplay.GetCardData();
                if (data == null)
                {
                    Debug.LogError("CardDrag: GetCardData devolvió null. ¿SetCardData fue llamado al instanciar la carta?");
                    ReturnToOriginal();
                    return;
                }

                // Llamamos al PlayerCardManager para intentar generar el personaje
                bool spawned = playerManager.RequestGenerateCharacter(data, spawnPos, this.gameObject);
                if (spawned)
                {
                    // La UI será destruida por PlayerCardManager (no hacemos ReturnToOriginal)
                    return;
                }
                else
                {
                    // Ej: intelecto insuficiente -> volver al slot original
                    ReturnToOriginal();
                    return;
                }
            }
        }
        ReturnToOriginal();
    }

    private void ReturnToOriginal()
    {
        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = originalAnchoredPos;
    }
}
