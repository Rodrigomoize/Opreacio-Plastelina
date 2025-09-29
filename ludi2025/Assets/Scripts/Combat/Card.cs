using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    public string cardName;
    public int cardValue;
    public int cardLife;
    public int cardVelocity;
    public int intelectCost;

    public GameObject fbxCharacter; 

    public void SetCardData(CardManager.Card data)
    {
        cardName = data.cardName;
        cardValue = data.cardValue;
        cardLife = data.cardLife;
        cardVelocity = data.cardVelocity;
        intelectCost = data.intelectCost;
        fbxCharacter = data.fbxCharacter;
    }
}
