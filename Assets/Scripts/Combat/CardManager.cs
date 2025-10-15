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

    }


    [Header("Cartas configurables en el inspector")]
    public List<Card> availableCards = new List<Card>();

    [Header("Intelect Manager")]
    public IntelectManager intelectManager;
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

    public bool GenerateCharacter(Card cardData, Vector3 spawnPosition, string teamTag)
    {
        if (cardData == null)
        {
            Debug.LogWarning("[CardManager] GenerateCharacter recibió cardData null.");
            return false;
        }

        // Verificar que tengamos CharacterManager
        if (characterManager == null)
        {
            Debug.LogError("[CardManager] CharacterManager no está asignado!");
            return false;
        }

        // Verificar intelecto
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
                Debug.LogWarning("[CardManager] Consume falló aunque CanConsume devolvió true.");
                return false;
            }
        }

        // NUEVA PARTE: Instanciar el personaje real usando CharacterManager
        GameObject spawnedCharacter = characterManager.InstantiateSingleCharacter(cardData, spawnPosition, teamTag);

        if (spawnedCharacter != null)
        {
            Debug.Log($"[CardManager] ✓ Personaje {cardData.cardName} instanciado en {spawnPosition} con tag {teamTag}");
            return true;
        }
        else
        {
            Debug.LogError($"[CardManager] Error al instanciar {cardData.cardName}");
            // Devolver el intelecto si falló la instanciación
            if (intelectManager != null)
            {
                intelectManager.AddIntelect(cardData.intelectCost);
            }
            return false;
        }
    }

    public bool GenerateCombinedCharacter(Card partA, Card partB, Vector3 spawnPosition, int operationResult, char opSymbol, string teamTag)
    {
        if (partA == null || partB == null)
        {
            Debug.LogWarning("[CardManager] GenerateCombinedCharacter recibió cartas null");
            return false;
        }

        // Verificar que tengamos CharacterManager
        if (characterManager == null)
        {
            Debug.LogError("[CardManager] CharacterManager no está asignado!");
            return false;
        }

        // NUEVO: el coste de intelecto será el resultado de la operación (operationResult),
        // no la suma de los costes de las cartas.
        int totalCost = Mathf.Max(0, operationResult); // asegurar no negativo

        // Verificar intelecto
        if (intelectManager != null)
        {
            if (!intelectManager.CanConsume(totalCost))
            {
                Debug.Log($"[CardManager] No hay intelecto suficiente para combinación (coste {totalCost})");
                return false;
            }
            bool consumed = intelectManager.Consume(totalCost);
            if (!consumed)
            {
                Debug.LogWarning("[CardManager] Consume falló en combinación.");
                return false;
            }
        }

        GameObject spawnedCombined = characterManager.InstantiateCombinedCharacter(
            partA, partB, spawnPosition, operationResult, opSymbol, teamTag
        );

        if (spawnedCombined != null)
        {
            Debug.Log($"[CardManager] ✓ Combinación {partA.cardName}+{partB.cardName} (valor:{operationResult}, op:{opSymbol}) instanciada en {spawnPosition}");
            return true;
        }
        else
        {
            Debug.LogError($"[CardManager] Error al instanciar combinación");

            // Devolver el intelecto si la instanciación falla
            if (intelectManager != null)
            {
                intelectManager.AddIntelect(totalCost);
            }
            return false;
        }
    }


}


