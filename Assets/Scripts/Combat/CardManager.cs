using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    [System.Serializable]
    public class Card
    {
        public string cardName;
        public int cardValue;
        public int cardLife;
        public float cardVelocity;
        public int intelectCost;
        public bool isSelected;
        public bool isSingle;
        public bool isCombined;
        public bool isUsed;
        public Sprite cardSprite;
        public GameObject fbxCharacter;
    }

    [Header("Cartas configurables en el inspector")]
    public List<Card> availableCards = new List<Card>();

    [Header("Managers")]
    public IntelectManager intelectManagerPlayer;
    public CharacterManager characterManager;

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

    public Card GetRandomCloneFromAvailable()
    {
        if (availableCards == null || availableCards.Count == 0) return null;
        int idx = Random.Range(0, availableCards.Count);
        return CloneCard(availableCards[idx]);
    }

    public Card GetCardByIndex(int index)
    {
        if (availableCards == null || availableCards.Count == 0) return null;
        if (index < 0 || index >= availableCards.Count)
        {
            Debug.LogWarning($"[CardManager] Índice {index} fuera de rango. Debe estar entre 0 y {availableCards.Count - 1}");
            return null;
        }
        return availableCards[index];
    }

    public List<Card> GetOrderedCards(int count)
    {
        List<Card> result = new List<Card>();
        if (availableCards == null || availableCards.Count == 0 || count <= 0) return result;

        int cardsToGet = Mathf.Min(count, availableCards.Count);
        for (int i = 0; i < cardsToGet; i++)
        {
            result.Add(CloneCard(availableCards[i]));
        }
        return result;
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

    public bool GenerateCharacter(Card cardData, Vector3 spawnPosition, string teamTag, IntelectManager customIntelectManager = null)
    {
        if (cardData == null)
        {
            Debug.LogWarning("[CardManager] GenerateCharacter recibió cardData null.");
            return false;
        }

        if (characterManager == null)
        {
            Debug.LogError("[CardManager] CharacterManager no está asignado!");
            return false;
        }

        IntelectManager intelectToUse = customIntelectManager ?? intelectManagerPlayer;

        if (intelectToUse != null)
        {
            if (!intelectToUse.CanConsume(cardData.intelectCost))
            {
                Debug.Log($"[CardManager] No hay intelecto suficiente para {cardData.cardName} (coste {cardData.intelectCost})");
                return false;
            }
            bool consumed = intelectToUse.Consume(cardData.intelectCost);
            if (!consumed)
            {
                Debug.LogWarning("[CardManager] Consume falló aunque CanConsume devolvió true.");
                return false;
            }
        }

        // La velocidad cardVelocity se pasa automáticamente al CharacterManager
        GameObject spawnedCharacter = characterManager.InstantiateSingleCharacter(cardData, spawnPosition, teamTag);

        if (spawnedCharacter != null)
        {
            Debug.Log($"[CardManager] ✓ Personaje {cardData.cardName} instanciado en {spawnPosition} con tag {teamTag}");
            return true;
        }
        else
        {
            Debug.LogError($"[CardManager] Error al instanciar {cardData.cardName}");
            // Devolver el intelecto si falló
            if (intelectToUse != null)
            {
                intelectToUse.AddIntelect(cardData.intelectCost);
            }
            return false;
        }
    }

    public bool GenerateCombinedCharacter(Card partA, Card partB, Vector3 spawnPosition, int operationResult, char opSymbol, string teamTag, IntelectManager customIntelectManager = null)
    {
        if (partA == null || partB == null)
        {
            Debug.LogWarning("[CardManager] GenerateCombinedCharacter recibió cartas null");
            return false;
        }

        if (characterManager == null)
        {
            Debug.LogError("[CardManager] CharacterManager no está asignado!");
            return false;
        }

        int totalCost = Mathf.Max(0, operationResult);

        IntelectManager intelectToUse = customIntelectManager ?? intelectManagerPlayer;

        if (intelectToUse != null)
        {
            if (!intelectToUse.CanConsume(totalCost))
            {
                return false;
            }
            bool consumed = intelectToUse.Consume(totalCost);
            if (!consumed)
            {
                return false;
            }
        }

        // Las velocidades de partA y partB se promedian automáticamente en CharacterManager
        GameObject spawnedCombined = characterManager.InstantiateCombinedCharacter(
            partA, partB, spawnPosition, operationResult, opSymbol, teamTag
        );

        if (spawnedCombined != null)
        {
            return true;
        }
        else
        {
            if (intelectToUse != null)
            {
                intelectToUse.AddIntelect(totalCost);
            }
            return false;
        }
    }
}