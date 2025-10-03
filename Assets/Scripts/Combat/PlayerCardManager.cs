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

    private List<CardManager.Card> playerCards = new List<CardManager.Card>();
    private List<GameObject> spawnedCards = new List<GameObject>();


    void Awake()
    {
        if (cardPrefab == null) Debug.LogError("Card Prefab is not assigned in the inspector.");
        if (cardSlots == null) Debug.LogError("Card UI Transform is not assigned in the inspector.");
        if (cardManager == null) Debug.LogError("CardManager is not assigned in the inspector.");
    }

    void Start()
    {
        List<CardManager.Card> randomCards = GetRandomCards(4);
        foreach (var card in randomCards)
        {
            CreateCard(card);
        }
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
        Transform freeSlot = GetFirstFreeSlot();
        if (freeSlot == null)
        {
            Debug.LogWarning("No hay slots libres para crear más cartas");
            return;
        }

        // Instancia carta en el slot
        GameObject newCard = Instantiate(cardPrefab, freeSlot);
        newCard.name = cardData.cardName;

        RectTransform rt = newCard.GetComponent<RectTransform>();
        RectTransform slotRT = freeSlot.GetComponent<RectTransform>();

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        Vector2 slotSize = slotRT.rect.size;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slotSize.x);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slotSize.y);

        rt.anchoredPosition = Vector2.zero; // centra dentro del slot
        rt.pivot = slotRT.pivot;

        CardDisplay display = newCard.GetComponent<CardDisplay>();
        if (display != null) display.SetCardData(cardData);

        CardDrag drag = newCard.GetComponent<CardDrag>();
        if (drag != null)
        {
            drag.playerManager = this;
            drag.cardDisplay = display;
        }

        spawnedCards.Add(newCard);
        cardData.ShowHimSelf();
    }

    public void SelectCard(GameObject card)
    {
        
    }

    public void PlayCombinedCards(CardManager.Card a, CardManager.Card b, char op, Vector3 spawnPos)
    {
        int result = 0;
        if (op == '+') result = a.cardValue + b.cardValue;
        else if (op == '-') result = a.cardValue - b.cardValue;

        // Normaliza si quieres que esté en 1..5, o permite 0-... según reglas.
        // Ahora llama a CardManager para gestionar coste y spawn:
        bool played = cardManager.GenerateCombinedCharacter(a, b, spawnPos, Character.Team.Player, result, op);
        if (played)
        {
            // destruir UI, reponer cartas, etc.
        }
    }

    public bool RequestGenerateCharacter(CardManager.Card cardData, Vector3 spawnPosition, GameObject cardUI)
    {
        // Llamamos al CardManager para intentar generar (CardManager gestiona intelecto si es necesario)
        Debug.Log($"RequestGenerateCharacter: intentando jugar {cardData.cardName} coste {cardData.intelectCost}");
        bool ok = cardManager.GenerateCharacter(cardData, spawnPosition);
        if (ok)
        {
            if (spawnedCards.Contains(cardUI))
            {
                spawnedCards.Remove(cardUI);
                Destroy(cardUI);
            }
            // 2) eliminar el dato de la lista playerCards
            if (playerCards.Contains(cardData))
            {
                playerCards.Remove(cardData);
            }
            // 3) generar nueva carta para rellenar el hueco 
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
        foreach (var card in randomCards)
        {
            CreateCard(card);
        }
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
