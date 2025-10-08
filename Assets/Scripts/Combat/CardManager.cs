using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        public bool isSingle;
        public bool isCombined;
        public bool isUsed;
        public Sprite cardSprite;
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
        clone.isSelected = original.isSelected;
        clone.cardSprite = original.cardSprite;
        clone.isSingle = original.isSingle;
        clone.isCombined = original.isCombined;
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

    public bool GenerateCharacter(Card cardData, Vector3 spawnPosition)
    {
        if (cardData == null)
        {
            Debug.LogWarning("[CardManager] GenerateCharacter recibió cardData null.");
            return false;
        }

        // If an intelect manager exists, check/consume
        if (intelectManager != null)
        {
            if (!intelectManager.CanConsume(cardData.intelectCost))
            {
                Debug.Log($"[CardManager] No hay intelecto suficiente para {cardData.cardName} (coste {cardData.intelectCost})");
                return false;
            }
            bool consumed = intelectManager.Consume(cardData.intelectCost);
            if (!consumed)
            {
                Debug.LogWarning("[CardManager] Consume falló aunque CanConsume devolvió true (verifica IntelectManager).");
                return false;
            }
        }

        // Figurative spawn: no instanciamos modelos reales todavía
        Debug.Log($"[CardManager] (FIGURATIVE) Spawn pedido: {cardData.cardName} at {spawnPosition} (cost {cardData.intelectCost})");
        
        return true;
    }

    public bool GenerateCombinedCharacter(Card partA, Card partB, Vector3 spawnPosition, int operationResult, char opSymbol)
    {
        if (partA == null || partB == null)
        {
            return false;
        }

        int totalCost = partA.intelectCost + partB.intelectCost;

        if (intelectManager != null)
        {
            if (!intelectManager.CanConsume(totalCost))
            {
                return false;
            }
            bool consumed = intelectManager.Consume(totalCost);
            if (!consumed)
            {
                return false;
            }
        }

        return true;
    }
}


