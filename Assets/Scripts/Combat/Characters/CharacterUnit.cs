using UnityEngine;

public class CharacterUnit : MonoBehaviour
{
    public string unitName;
    public int maxLife;
    public int currentLife;
    public int value;
    public float moveSpeed = 3.5f;
    public Character.Team team;

    private int operationResult = int.MinValue;
    private char opSymbol;
    private CombinedCharacter parentCombined;
    private IntelectManager intelectManager;

    public int moveSpeedInt => Mathf.RoundToInt(moveSpeed);

    public void SetupFromCard(CardManager.Card card, Character.Team t, IntelectManager intelect = null, int operationResult = int.MinValue, char opSymbol = '\0')
    {
        unitName = card.cardName;
        maxLife = card.cardLife;
        currentLife = maxLife;
        value = card.cardValue;
        team = t;
        this.operationResult = operationResult;
        this.opSymbol = opSymbol;
        intelectManager = intelect;
        parentCombined = GetComponentInParent<CombinedCharacter>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // ejemplo simple: si colisiono con Character (single defender)
        Character defender = other.GetComponent<Character>();
        if (defender != null && defender.team != this.team)
        {
            if (operationResult != int.MinValue)
            {
                if (defender.value == operationResult)
                {
                    Die(); // defensor gana
                }
                else
                {
                    defender.Die();
                    if (intelectManager != null) intelectManager.AddIntelect(1);
                }
            }
            else
            {
                // lógica alternativa para units sin operationResult
            }
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
