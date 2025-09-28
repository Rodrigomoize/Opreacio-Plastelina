using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject cardPrefab;
    public Transform CardUI;

    public Button SumaButton;
    public Button RestaButton;

    [System.Serializable]
    public class Card
    {
        public string cardName;
        public int cardValue;
        public int cardLife;
        public int cardVelocity;
        public int intelectCost;
        public bool isSelected;
        public bool isCombined;
        bool isUsed;
        public GameObject cardImage;
        public GameObject fbxCharacter;

        public void ShowHimSelf()
        {
            Debug.Log($"{cardName} attacks with {cardValue} power at a cost of {intelectCost} intelect.");
        }
    }


    [Header("Cartas configurables en el inspector")]
    public List<Card> availableCards = new List<Card>(); 

    private List<GameObject> spawnedCards = new List<GameObject>();

    void Awake()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("Card Prefab is not assigned in the inspector.");
        }

        if (CardUI==null)
        {
            Debug.LogError("Card UI Transform is not assigned in the inspector.");
        }
    }

    void Start()
    {
        List<Card> randomCards = GetRandomCards(4);

        foreach (Card card in randomCards)
        {
            CreateCard(card);
        }
    }

    private List<Card> GetRandomCards(int n)
    {
        List<Card> tempList = new List<Card>(availableCards);
        List<Card> result = new List<Card>();

        for (int i = 0; i < n && tempList.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, tempList.Count);
            result.Add(tempList[randomIndex]);
            tempList.RemoveAt(randomIndex); // Para no repetir
        }

        return result;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateCard(Card cardData)
    {
        GameObject newCard = Instantiate(cardPrefab, CardUI);
        newCard.name = cardData.cardName;

        // 🔹 Aquí actualizas el UI de la carta con los datos
        // Ejemplo si tu prefab tiene un Text para el nombre y otro para el coste:
        Text[] texts = newCard.GetComponentsInChildren<Text>();
        foreach (Text t in texts)
        {
            if (t.name == "CardName") t.text = cardData.cardName;
            if (t.name == "CardCost") t.text = cardData.intelectCost.ToString();
        }

        spawnedCards.Add(newCard);
        cardData.ShowHimSelf();

    }

    public void SelectCard(GameObject card)
    {
        
    }
    public void GenerateCards()
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

    public void GenerateNextCards()
    {
        
    }
}
