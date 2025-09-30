using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardManager : MonoBehaviour
{
    public CardManager cardManager;
    
    public GameObject cardPrefab;
    public Transform CardUI1;
    public Transform CardUI2;
    public Transform CardUI3;
    public Transform CardUI4;

    public Button SumaButton;
    public Button RestaButton;

    private List<CardManager.Card> playerCards = new List<CardManager.Card>();
    private List<GameObject> spawnedCards = new List<GameObject>();


    void Awake()
    {
        if (cardPrefab == null) Debug.LogError("Card Prefab is not assigned in the inspector.");
        if (CardUI1 == null) Debug.LogError("Card UI Transform is not assigned in the inspector.");
        if (cardManager == null) Debug.LogError("CardManager is not assigned in the inspector.");
    }

    void Start()
    {
        List<CardManager.Card> randomCards = GetRandomCards(4);

        CreateCard(randomCards[0], CardUI1);
        CreateCard(randomCards[1], CardUI2);
        CreateCard(randomCards[2], CardUI3);
        CreateCard(randomCards[3], CardUI4);
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

    public void CreateCard(CardManager.Card cardData, Transform slotParent)
    {
        GameObject newCard = Instantiate(cardPrefab, slotParent);
        newCard.name = cardData.cardName;

        CardDisplay display = newCard.GetComponent<CardDisplay>();

        CardDrag drag = newCard.GetComponent<CardDrag>();
        if (drag != null)
        {
            drag.playerManager = this;
            drag.cardDisplay = display;
        }
        else
        {
            Debug.LogWarning("Prefab no contiene CardDrag");
        }

        spawnedCards.Add(newCard);
        cardData.ShowHimSelf();
    }

    public void SelectCard(GameObject card)
    {
        
    }

    public void PlayCard()
    {

    }

    public void CombineCards()
    {
        
    }

    public bool RequestGenerateCharacter(CardManager.Card cardData, Vector3 spawnPosition, GameObject cardUI)
    {
        // Llamamos al CardManager para intentar generar (CardManager gestiona intelecto si es necesario)
        bool ok = cardManager.GenerateCharacter(cardData, spawnPosition);
        if (ok)
        {
            // Eliminamos la carta del mazo del jugador:
            // 1) eliminar la instancia UI
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

    private void AddNextCard()
    {
        CardManager.Card next = cardManager.GetRandomCloneFromAvailable();
        if (next != null)
        {
            playerCards.Add(next);
            CreateCard(playerCards[0], CardUI1);
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
