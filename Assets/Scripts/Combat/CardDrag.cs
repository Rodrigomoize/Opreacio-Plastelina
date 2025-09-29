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
        // Busca el canvas padre (necesario para convertir posiciones)
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalAnchoredPos = rectTransform.anchoredPosition;
        transform.SetParent(rootCanvas.transform, true); // llevar al top del canvas
        canvasGroup.blocksRaycasts = false; // para que los raycasts no se queden en la UI mientras arrastramos
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 movePos;
        // Mueve el rectTransform con la posición del pointer
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rootCanvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out movePos);
        rectTransform.anchoredPosition = movePos;
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
                // Llama al manager para generar character
                bool spawned = playerManager.RequestGenerateCharacter(cardDisplay.GetCardData(), spawnPos, this.gameObject);
                if (spawned)
                {
                    return;
                }
                else
                {
                    // no se pudo (p.ej. intelecto insuficiente) -> volver a la posición original
                    ReturnToOriginal();
                    return;
                }
            }
        }

        // Si no golpea PlayableArea -> devolver al slot original
        ReturnToOriginal();
    }

    private void ReturnToOriginal()
    {
        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = originalAnchoredPos;
    }
}
