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

    public void GenerateCharacter()
    {

    }
    
    public void GenerateCard() 
    {   

    }

    public void GenerateNextCards()
    {

    }
}
