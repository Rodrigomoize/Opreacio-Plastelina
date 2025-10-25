using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // Para NavMesh projection
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
        clone.intelectCost = (original.intelectCost > 0) ? original.intelectCost : original.cardValue;
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

    /// Resultado de intentar generar un personaje
    public enum GenerateResult
    {
        Success,                // Se generó correctamente
        InsufficientIntellect,  // No hay suficiente intelecto
        OtherError              // Otro tipo de error
    }

    public GameObject GenerateCharacter(Card cardData, Vector3 spawnPosition, string teamTag, IntelectManager customIntelectManager = null)
    {
        GenerateResult result;
        return GenerateCharacter(cardData, spawnPosition, teamTag, out result, customIntelectManager);
    }

    public GameObject GenerateCharacter(Card cardData, Vector3 spawnPosition, string teamTag, out GenerateResult result, IntelectManager customIntelectManager = null)
    {
        result = GenerateResult.OtherError;
        
        if (cardData == null)
        {
            Debug.LogWarning("[CardManager] GenerateCharacter recibió cardData null.");
            return null;
        }

        if (characterManager == null)
        {
            Debug.LogError("[CardManager] CharacterManager no está asignado!");
            return null;
        }

        // Usar fallback: si intelectCost no está definido (>0) usar cardValue
        int costToUse = (cardData.intelectCost > 0) ? cardData.intelectCost : cardData.cardValue;

        IntelectManager intelectToUse = customIntelectManager ?? intelectManagerPlayer;

        if (intelectToUse != null)
        {
            if (!intelectToUse.CanConsume(costToUse))
            {
                Debug.Log($"[CardManager] No hay intelecto suficiente para {cardData.cardName} (coste {costToUse})");
                result = GenerateResult.InsufficientIntellect;
                return null;
            }
            bool consumed = intelectToUse.Consume(costToUse);
            if (!consumed)
            {
                Debug.LogWarning("[CardManager] Consume falló aunque CanConsume devolvió true.");
                result = GenerateResult.InsufficientIntellect;
                return null;
            }
        }

        // La velocidad cardVelocity se pasa automáticamente al CharacterManager
        GameObject spawnedCharacter = characterManager.InstantiateSingleCharacter(cardData, spawnPosition, teamTag);

        if (spawnedCharacter != null)
        {
            Debug.Log($"[CardManager] ✓ Personaje {cardData.cardName} instanciado en {spawnPosition} con tag {teamTag} (coste={costToUse})");
            result = GenerateResult.Success;
            return spawnedCharacter;
        }
        else
        {
            Debug.LogError($"[CardManager] Error al instanciar {cardData.cardName}");
            // Devolver el intelecto si falló
            if (intelectToUse != null)
            {
                intelectToUse.AddIntelect(costToUse);
            }
            result = GenerateResult.OtherError;
            return null;
        }
    }

    public bool GenerateCombinedCharacter(Card partA, Card partB, Vector3 spawnPosition, int operationResult, char opSymbol, string teamTag, IntelectManager customIntelectManager = null)
    {
        GenerateResult result;
        return GenerateCombinedCharacter(partA, partB, spawnPosition, operationResult, opSymbol, teamTag, out result, customIntelectManager);
    }

    public bool GenerateCombinedCharacter(Card partA, Card partB, Vector3 spawnPosition, int operationResult, char opSymbol, string teamTag, out GenerateResult result, IntelectManager customIntelectManager = null)
    {
        result = GenerateResult.OtherError;
        
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
                Debug.Log($"[CardManager] No hay intelecto suficiente para operación (coste {totalCost})");
                result = GenerateResult.InsufficientIntellect;
                return false;
            }
            bool consumed = intelectToUse.Consume(totalCost);
            if (!consumed)
            {
                result = GenerateResult.InsufficientIntellect;
                return false;
            }
        }

        // ⚡ PROYECTAR POSICIÓN AL NAVMESH (CRÍTICO para evitar errores de spawn)
        Vector3 spawnPositionOnNavMesh = ProjectToNavMesh(spawnPosition);

        GameObject spawnedCombined = characterManager.InstantiateCombinedCharacter(
            partA, partB, spawnPositionOnNavMesh, operationResult, opSymbol, teamTag
        );

        if (spawnedCombined != null)
        {
            result = GenerateResult.Success;
            return true;
        }
        else
        {
            if (intelectToUse != null)
            {
                intelectToUse.AddIntelect(totalCost);
            }
            result = GenerateResult.OtherError;
            return false;
        }
    }

    /// <summary>
    /// Proyecta una posición al NavMesh más cercano
    /// Esto evita errores de "not close enough to NavMesh"
    /// </summary>
    private Vector3 ProjectToNavMesh(Vector3 position)
    {
        NavMeshHit hit;
        
        // Buscar el punto más cercano en el NavMesh (radio: 50 unidades)
        if (NavMesh.SamplePosition(position, out hit, 50f, NavMesh.AllAreas))
        {
            Debug.Log($"[CardManager] ✅ Posición proyectada al NavMesh: {position} → {hit.position} (dist: {hit.distance:F2}m)");
            return hit.position;
        }
        else
        {
            Debug.LogError($"[CardManager] ⚠️ No se encontró NavMesh cerca de {position}. Usando posición original (puede fallar).");
            return position;
        }
    }
}