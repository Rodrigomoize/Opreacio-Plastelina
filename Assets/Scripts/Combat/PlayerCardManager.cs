using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardManager : MonoBehaviour
{
    public CardManager cardManager;
    public GameObject cardPrefab;            
    public List<Transform> cardSlots = new List<Transform>(4);
    public Button SumaButton;
    public Button RestaButton;

    [Header("Spawn figurativo en mundo")]
    public Transform spawnPoint;

    // Estado de selección / operación
    private List<CardDisplay> selectedDisplays = new List<CardDisplay>(2);
    private List<CardManager.Card> playerCards = new List<CardManager.Card>();
    private List<GameObject> spawnedCards = new List<GameObject>();
    private char currentOperator = '\0'; 

    void Awake()
    {
        if (cardPrefab == null) Debug.LogError("Card Prefab is not assigned in the inspector.");
        if (cardSlots == null) Debug.LogError("Card slots not assigned.");
        if (cardManager == null) Debug.LogError("CardManager not assigned.");
    }

    void OnEnable()
    {
        if (SumaButton != null) SumaButton.onClick.AddListener(OnSumaButtonClicked);
        if (RestaButton != null) RestaButton.onClick.AddListener(OnRestaButtonClicked);
    }

    void OnDisable()
    {
        if (SumaButton != null) SumaButton.onClick.RemoveListener(OnSumaButtonClicked);
        if (RestaButton != null) RestaButton.onClick.RemoveListener(OnRestaButtonClicked);
    }

    void Start()
    {
        // Inicializa mano
        List<CardManager.Card> randomCards = GetRandomCards(4);
        foreach (var card in randomCards) CreateCard(card);
    }

    private List<CardManager.Card> GetRandomCards(int n)
    {
        List<CardManager.Card> tempList = new List<CardManager.Card>(cardManager.availableCards);
        List<CardManager.Card> result = new List<CardManager.Card>();

        for (int i = 0; i < n && tempList.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, tempList.Count);
            CardManager.Card clone = cardManager.CloneCard(tempList[randomIndex]);
            result.Add(clone);
            tempList.RemoveAt(randomIndex);
        }
        return result;
    }


    public void CreateCard(CardManager.Card data)
    {
        if (data == null)
        {
            Debug.LogWarning("CreateCard recibió null");
            return;
        }

        playerCards.Add(data);
        Transform freeSlot = GetFirstFreeSlot();
        if (freeSlot == null) { Debug.LogWarning("No hay slots libres"); return; }

        GameObject newCard = Instantiate(cardPrefab, freeSlot, false);
        newCard.name = data.cardName;
        newCard.SetActive(true); // por si el prefab viene desactivado accidentalmente

        // Ajustar rectTransform para ocupar el slot
        RectTransform rt = newCard.GetComponent<RectTransform>();
        RectTransform slotRT = freeSlot.GetComponent<RectTransform>();
        if (rt != null && slotRT != null)
        {
            rt.localScale = Vector3.one;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = slotRT.pivot;
        }

        CardDisplay display = newCard.GetComponent<CardDisplay>();
        if (display != null)
        {
            display.ownerManager = this;
            display.SetCardData(data);
        }
        else
        {
            Debug.LogWarning("cardPrefab no contiene CardDisplay.");
        }

        CardDrag drag = newCard.GetComponent<CardDrag>();
        if (drag != null)
        {
            drag.playerManager = this;
            drag.cardDisplay = display;
        }

        spawnedCards.Add(newCard);
        data.ShowHimSelf();

        Debug.Log($"Se han instanciado Carta '{data.cardName}' en slot '{freeSlot.name}' - spawnedCards count: {spawnedCards.Count}");
    }


    public void OnCardClicked(CardDisplay display)
    {
        if (display == null) return;

        // Si la carta se acaba de seleccionar (display.isSelected == true) la añadimos
        if (display.isSelected)
        {
            // Si ya estaba en la lista, no duplicar
            if (!selectedDisplays.Contains(display))
            {
                if (selectedDisplays.Count >= 2)
                {
                    // eliminar la más antigua (y forzar su isSelected=false)
                    var old = selectedDisplays[0];
                    old.isSelected = false;
                    old.ApplyHighlight(false);
                    selectedDisplays.RemoveAt(0);
                }
                selectedDisplays.Add(display);
            }
        }
        else
        {
            // se ha deseleccionado -> quitar de la lista si existe
            if (selectedDisplays.Contains(display))
            {
                selectedDisplays.Remove(display);
            }
        }

        UpdateSelectionVisuals();
    }

    // Forzar seleccionar desde código (usado por CardDrag o llamadas externas)
    public void ForceSelect(CardDisplay d)
    {
        if (d == null) return;
        if (!d.isSelected)
        {
            d.isSelected = true;
            d.ApplyHighlight(true);
        }

        if (!selectedDisplays.Contains(d))
        {
            if (selectedDisplays.Count >= 2)
            {
                var old = selectedDisplays[0];
                old.isSelected = false;
                old.ApplyHighlight(false);
                selectedDisplays.RemoveAt(0);
            }
            selectedDisplays.Add(d);
        }

        UpdateSelectionVisuals();
    }

    // Actualiza la visibilidad/desaturación de todas las cartas
    private void UpdateSelectionVisuals()
    {
        // Si no hay selección -> restaurar todo
        if (selectedDisplays.Count == 0)
        {
            foreach (var go in spawnedCards)
            {
                if (go == null) continue;
                var cd = go.GetComponent<CardDisplay>();
                if (cd != null) cd.SetDesaturate(false);
            }
            return;
        }

        // Hay selección -> desaturar las no seleccionadas
        foreach (var go in spawnedCards)
        {
            if (go == null) continue;
            var cd = go.GetComponent<CardDisplay>();
            if (cd == null) continue;

            bool isSelectedLocal = cd.isSelected;
            cd.SetDesaturate(!isSelectedLocal);
        }
    }
    public void DeselectAll()
    {
        foreach (var d in selectedDisplays)
        {
            if (d != null) d.SetHighlight(false);
            Debug.Log(d);
        }
        selectedDisplays.Clear();
        currentOperator = '\0';
        UpdateSelectionVisuals();
    }


    public bool IsSelected(CardDisplay d) => selectedDisplays.Contains(d);

    private void OnSumaButtonClicked() => ToggleOperator('+');
    private void OnRestaButtonClicked() => ToggleOperator('-');

    private void ToggleOperator(char op)
    {
        if (currentOperator == op)
        {
            currentOperator = '\0';
            Debug.Log("[PlayerCardManager] Operación deshabilitada");
        }
        else
        {
            currentOperator = op;
            Debug.Log($"[PlayerCardManager] Operación seleccionada: {op}");
        }
    }

   
    public void HandlePlayAreaClick(Vector3 spawnPosition)
    {
        if (selectedDisplays.Count == 0)
        {
            
            Debug.Log("[PlayerCardManager] PlayArea clic: nada seleccionado.");
            return;
        }

        if (selectedDisplays.Count == 1)
        {
            // spawn single
            CardManager.Card c = selectedDisplays[0].GetCardData();
            if (c == null) { Debug.LogWarning("Carta null en HandlePlayAreaClick"); DeselectAll(); return; }

            bool ok = RequestGenerateCharacter(c, spawnPosition, selectedDisplays[0].gameObject);
            if (!ok) Debug.Log("[PlayerCardManager] No se pudo generar carta (coste u otro fallo).");

            DeselectAll();
            currentOperator = '\0';
            return;
        }

        if (selectedDisplays.Count == 2)
        {

            if (currentOperator == '\0')
            {
                Debug.Log("[PlayerCardManager] Dos cartas seleccionadas pero sin operación: se deseleccionan.");
                DeselectAll();
                return;
            }

            var a = selectedDisplays[0].GetCardData();
            var b = selectedDisplays[1].GetCardData();
            if (a == null || b == null) { Debug.LogWarning("Carta null en combinación"); DeselectAll(); return; }

            // ordenar valores: el mayor debe ir segundo
            int firstVal = Mathf.Min(a.cardValue, b.cardValue);
            int secondVal = Mathf.Max(a.cardValue, b.cardValue);
            int operationResult = currentOperator == '+' ? firstVal + secondVal : firstVal - secondVal;

            bool played = cardManager.GenerateCombinedCharacter(a, b, spawnPosition, operationResult, currentOperator);
            if (played)
            {
                // eliminar UI y reponer
                foreach (var d in new List<CardDisplay>(selectedDisplays)) RemoveCardUI(d);
                selectedDisplays.Clear();
            }
            else
            {
                Debug.Log("[PlayerCardManager] No se pudo jugar la combinación (coste u otro fallo).");
            }

            currentOperator = '\0';
            return;
        }
    }

    private void RemoveCardUI(CardDisplay display)
    {
        if (display == null) return;
        GameObject go = display.gameObject;
        if (spawnedCards.Contains(go)) spawnedCards.Remove(go);
        Destroy(go);
        var cd = display.GetCardData();
        if (playerCards.Contains(cd)) playerCards.Remove(cd);
        AddNextCard();
    }

    public bool RequestGenerateCharacter(CardManager.Card cardData, Vector3 spawnPosition, GameObject cardUI)
    {
        Debug.Log($"[PlayerCardManager] RequestGenerateCharacter: intentando jugar {cardData.cardName} coste {cardData.intelectCost}");
        bool ok = cardManager.GenerateCharacter(cardData, spawnPosition);
        if (ok)
        {
            Debug.Log($"[PlayerCardManager] Carta spawn creada (figurative): {cardData.cardName} en {spawnPosition}");
            if (cardUI != null && spawnedCards.Contains(cardUI))
            {
                spawnedCards.Remove(cardUI);
                Destroy(cardUI);
            }
            if (playerCards.Contains(cardData)) playerCards.Remove(cardData);
            AddNextCard();
            return true;
        }
        return false;
    }

    private Transform GetFirstFreeSlot()
    {
        foreach (Transform slot in cardSlots)
        {
            if (slot.childCount == 0) return slot;
        }
        return null;
    }

    private void AddNextCard()
    {
        List<CardManager.Card> randomCards = GetRandomCards(1);
        foreach (var card in randomCards) CreateCard(card);
    }

    public void DiscardCard(GameObject card)
    {
        if (spawnedCards.Contains(card))
        {
            spawnedCards.Remove(card);
            Destroy(card);
        }
    }
}
