using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardManager : MonoBehaviour
{
    public CardManager cardManager;
    
    public GameObject cardPrefab;
    public Transform CardUI;

    public Button SumaButton;
    public Button RestaButton;

    private List<CardManager.Card> playerCards = new List<CardManager.Card>();
    private List<GameObject> spawnedCards = new List<GameObject>();


    void Awake()
    {
        if (cardPrefab == null) Debug.LogError("Card Prefab is not assigned in the inspector.");
        if (CardUI == null) Debug.LogError("Card UI Transform is not assigned in the inspector.");
        if (cardManager == null) Debug.LogError("CardManager is not assigned in the inspector.");
    }

    void Start()
    {
        List<CardManager.Card> randomCards = GetRandomCards(4);

        foreach (CardManager.Card card in randomCards)
        {
            playerCards.Add(card);
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
        GameObject newCard = Instantiate(cardPrefab, CardUI);
        newCard.name = cardData.cardName;

        CardDisplay display = newCard.GetComponent<CardDisplay>();

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

    public void DiscardCard(GameObject card)
    {
        if (spawnedCards.Contains(card))
        {
            spawnedCards.Remove(card);
            Destroy(card);
        }
    }
    
    
}
