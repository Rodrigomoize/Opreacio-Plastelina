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
        public bool isSingle;
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
        Character.Team teamToSpawn = Character.Team.Player; // o decide según contexto/owner
        if (CharacterManager.Instance != null)
        {
            Character created = CharacterManager.Instance.CreateCharacterFromCard(cardData, spawnPosition, teamToSpawn);
            if (created != null) return true;
            else return false;
        }
        else
        {
            Debug.LogWarning("CharacterManager no existe en escena, instanciando directamente.");
            GameObject go = Instantiate(cardData.fbxCharacter, spawnPosition, Quaternion.identity);
            Character cs = go.GetComponent<Character>();
            if (cs != null) cs.SetupFromCard(cardData, teamToSpawn);
            return cs != null;
        }
    }
}
