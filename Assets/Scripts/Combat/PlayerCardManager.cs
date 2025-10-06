using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardManager : MonoBehaviour
{
    public CardManager cardManager;
    public GameObject cardPrefab;
    public List<Transform> cardSlots = new List<Transform>();
    public Button SumaButton;
    public Button RestaButton;

    public Transform spawnPoint;

    private List<CardDisplay> selectedDisplays = new List<CardDisplay>(2);
    private List<CardManager.Card> playerCards = new List<CardManager.Card>();
    private List<GameObject> spawnedCards = new List<GameObject>();

    void Awake()
    {
        if (cardPrefab == null) Debug.LogError("Card Prefab is not assigned in the inspector.");
        if (cardSlots == null) Debug.LogError("Card UI Transform is not assigned in the inspector.");
        if (cardManager == null) Debug.LogError("CardManager is not assigned in the inspector.");
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

    public void CreateCard(CardManager.Card cardData)
    {
        playerCards.Add(cardData);
        Transform freeSlot = GetFirstFreeSlot();
        if (freeSlot == null) { Debug.LogWarning("No hay slots libres"); return; }

        GameObject newCard = Instantiate(cardPrefab, freeSlot, false);
        newCard.name = cardData.cardName;

        // Ajustar rect transform para que ocupe el slot
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
            display.SetCardData(cardData);
        }

        CardDrag drag = newCard.GetComponent<CardDrag>();
        if (drag != null)
        {
            drag.playerManager = this;
            drag.cardDisplay = display;
        }

        spawnedCards.Add(newCard);
        cardData.ShowHimSelf();
    }

    // Toggle select
    public void OnCardClicked(CardDisplay display)
    {
        if (display == null) return;

        if (selectedDisplays.Contains(display))
        {
            selectedDisplays.Remove(display);
            display.SetHighlight(false);
            return;
        }

        if (selectedDisplays.Count < 2)
        {
            selectedDisplays.Add(display);
            display.SetHighlight(true);
            return;
        }

        // replace oldest
        selectedDisplays[0].SetHighlight(false);
        selectedDisplays.RemoveAt(0);
        selectedDisplays.Add(display);
        display.SetHighlight(true);
    }

    // Exposed button handlers
    public void OnSumaButtonClicked() => OnCombineButton('+');
    public void OnRestaButtonClicked() => OnCombineButton('-');

    private void OnCombineButton(char op)
    {
        if (selectedDisplays.Count < 2)
        {
            Debug.Log("Selecciona 2 cartas antes de combinar.");
            return;
        }

        var a = selectedDisplays[0].GetCardData();
        var b = selectedDisplays[1].GetCardData();
        if (a == null || b == null) return;

        // Ordenar para que el número mayor vaya siempre como segundo
        int firstVal = Mathf.Min(a.cardValue, b.cardValue);
        int secondVal = Mathf.Max(a.cardValue, b.cardValue);

        string opText = $"{firstVal}{op}{secondVal}";
        Debug.Log($"[PlayerCardManager] Combinación seleccionada: {opText}");

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        bool played = cardManager.GenerateCombinedCharacter(a, b, spawnPos, op == '+' ? firstVal + secondVal : firstVal - secondVal, op);
        if (played)
        {
            foreach (var d in new List<CardDisplay>(selectedDisplays)) RemoveCardUI(d);
            selectedDisplays.Clear();
        }
        else
        {
            Debug.Log("[PlayerCardManager] No se pudo jugar la combinación (coste o fallo).");
        }
    }

    public bool IsSelected(CardDisplay d) => selectedDisplays.Contains(d);

    public void ForceSelect(CardDisplay d)
    {
        if (d == null) return;
        if (selectedDisplays.Contains(d)) return;
        if (selectedDisplays.Count >= 2)
        {
            selectedDisplays[0].SetHighlight(false);
            selectedDisplays.RemoveAt(0);
        }
        selectedDisplays.Add(d);
        d.SetHighlight(true);
    }

    private Vector3 GetDefaultSpawnPos()
    {
        return spawnPoint != null ? spawnPoint.position : Vector3.zero;
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
            Debug.Log($"[PlayerCardManager] Carta spawn creada: {cardData.cardName} en {spawnPosition}");
            if (spawnedCards.Contains(cardUI))
            {
                spawnedCards.Remove(cardUI);
                Destroy(cardUI);
            }
            if (playerCards.Contains(cardData))
            {
                playerCards.Remove(cardData);
            }
            AddNextCard();
            return true;
        }
        else
        {
            return false;
        }
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
