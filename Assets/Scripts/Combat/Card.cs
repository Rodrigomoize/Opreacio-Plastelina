using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    [Header("Datos visibles en Inspector (debug)")]
    public string cardName;
    public int cardValue;
    public int cardLife;
    public int cardVelocity;
    public int intelectCost;

    [Header("Referencia al prefab 3D")]
    public GameObject fbxCharacter;

    private CardManager.Card cardData;

    public void SetCardData(CardManager.Card data)
    {
        if (data == null)
        {
            Debug.LogWarning("SetCardData recibió null.");
            return;
        }

        cardData = data;

        cardName = data.cardName;
        cardValue = data.cardValue;
        cardLife = data.cardLife;
        cardVelocity = data.cardVelocity;
        intelectCost = data.intelectCost;
        fbxCharacter = data.fbxCharacter;
    }

   
    public CardManager.Card GetCardData()
    {
        return cardData;
    }
}
