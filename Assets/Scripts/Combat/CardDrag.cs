using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
[RequireComponent(typeof(CanvasGroup))]
public class CardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public PlayerCardManager playerManager;
    public CardDisplay cardDisplay;
    private Canvas rootCanvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalAnchoredPos;
    private bool isDragging = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (cardDisplay == null) cardDisplay = GetComponent<CardDisplay>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardDisplay != null && playerManager != null)
        {
            if (!playerManager.IsSelected(cardDisplay))
            {
                playerManager.ForceSelect(cardDisplay);
            }
        }

        originalParent = transform.parent;
        originalAnchoredPos = rectTransform.anchoredPosition;
        transform.SetParent(rootCanvas.transform, true);
        canvasGroup.blocksRaycasts = false;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 movePos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rootCanvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out movePos))
        {
            rectTransform.anchoredPosition = movePos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        isDragging = false;

        // 1) Primero comprobar drop UI con GraphicRaycaster (si tienes una PlayableArea UI)
        if (TryDropOnUI(eventData)) return;

        // 2) Sino, raycast al mundo para ver si hemos soltado sobre una PlayableArea 3D
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider != null && hit.collider.CompareTag("PlayableArea"))
            {
                Vector3 spawnPos = hit.point;
                CardManager.Card data = cardDisplay?.GetCardData();
                if (data != null)
                {
                    bool spawned = playerManager.RequestGenerateCharacter(data, spawnPos, this.gameObject);
                    if (spawned) return;
                }
            }
        }

        // fallback: volver al slot original
        ReturnToOriginal();
    }

    private bool TryDropOnUI(PointerEventData eventData)
    {
        GraphicRaycaster gr = rootCanvas.GetComponent<GraphicRaycaster>();
        if (gr == null) return false;
        PointerEventData ped = new PointerEventData(EventSystem.current) { position = eventData.position };
        var results = new System.Collections.Generic.List<RaycastResult>();
        gr.Raycast(ped, results);
        foreach (var r in results)
        {
            if (r.gameObject.CompareTag("PlayableArea")) // <-- usar PlayableAreaUI para UI panel
            {
                Vector3 spawnPos = Vector3.zero;
                if (playerManager != null && playerManager.spawnPoint != null)
                    spawnPos = playerManager.spawnPoint.position;

                CardManager.Card data = cardDisplay?.GetCardData();
                if (data != null)
                {
                    bool spawned = playerManager.RequestGenerateCharacter(data, spawnPos, this.gameObject);
                    if (spawned) return true;
                }
            }
        }
        return false;
    }


    private void ReturnToOriginal()
    {
        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = originalAnchoredPos;
    }
}
