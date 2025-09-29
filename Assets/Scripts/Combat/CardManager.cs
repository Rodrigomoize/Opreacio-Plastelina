using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        public bool isUsed;
        public GameObject cardImage;
        public GameObject fbxCharacter;

        public void ShowHimSelf()
        {
            Debug.Log($"{cardName} attacks with {cardValue} power at a cost of {intelectCost} intelect.");
        }
    }


    [Header("Cartas configurables en el inspector")]
    public List<Card> availableCards = new List<Card>();

    [Header("Intelect Manager")]
    public IntelectManager intelectManager;
    public Card CloneCard(Card original)
    {
        Card clone = new Card();
        clone.cardName = original.cardName;
        clone.cardValue = original.cardValue;
        clone.cardLife = original.cardLife;
        clone.cardVelocity = original.cardVelocity;
        clone.intelectCost = original.intelectCost;
        clone.fbxCharacter = original.fbxCharacter;
        return clone;
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Card GetRandomCloneFromAvailable()
    {
        if (availableCards == null || availableCards.Count == 0) return null;
        int idx = Random.Range(0, availableCards.Count);
        return CloneCard(availableCards[idx]);
    }

    public List<Card> GetRandomClones(int n)
    {
        List<Card> result = new List<Card>();
        if (availableCards == null || availableCards.Count == 0 || n <= 0) return result;

        List<Card> temp = new List<Card>(availableCards);
        for (int i = 0; i < n && temp.Count > 0; i++)
        {
            int r = Random.Range(0, temp.Count);
            result.Add(CloneCard(temp[r]));
            temp.RemoveAt(r);
        }
        return result;
    }

    // Generar el character en world, comprobando intelecto. Devuelve true si ha sido posible.
    public bool GenerateCharacter(Card cardData, Vector3 spawnPosition)
    {
        if (cardData == null)
        {
            Debug.LogError("GenerateCharacter recibió cardData null.");
            return false;
        }

        if (intelectManager == null)
        {
            Debug.LogError("IntelectManager no asignado en CardManager!");
            return false;
        }

        if (!intelectManager.CanConsume(cardData.intelectCost))
        {
            Debug.Log($"No hay intelecto suficiente para {cardData.cardName} (coste {cardData.intelectCost})");
            return false;
        }

        // Consumir intelecto
        intelectManager.Consume(cardData.intelectCost);

        // Instanciar el fbxCharacter en el mundo (si existe)
        if (cardData.fbxCharacter != null)
        {
            GameObject go = Instantiate(cardData.fbxCharacter, spawnPosition, Quaternion.identity);
            Debug.Log($"Spawned {cardData.cardName} at {spawnPosition}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Carta {cardData.cardName} no tiene fbxCharacter asignado.");
            return false;
        }
    }
}
