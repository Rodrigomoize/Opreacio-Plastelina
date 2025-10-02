using UnityEngine.AI;
using UnityEngine;

public class CombinedCharacter : MonoBehaviour
{
    public Transform frontSlot;
    public Transform backSlot;
    public GameObject unitPrefab; // fallback
    private CharacterUnit frontUnit;
    private CharacterUnit backUnit;
    private NavMeshAgent agent;
    public Character.Team team;



    // Guardamos el resultado de la operación
    public int operationResult;
    public char opSymbol;

    public void SetupFromCards(CardManager.Card partA, CardManager.Card partB, int operationResult, char opSymbol, Character.Team spawnTeam, CharacterManager manager)
    {
        team = spawnTeam;
        this.operationResult = operationResult;
        this.opSymbol = opSymbol;

        // ordenar front/back según tu regla (valor más pequeño delante)
        CardManager.Card frontCard = partA, backCard = partB;
        if (partA.cardValue > partB.cardValue) { frontCard = partB; backCard = partA; }

        GameObject fGO = Instantiate(frontCard.fbxCharacter != null ? frontCard.fbxCharacter : unitPrefab, frontSlot.position, frontSlot.rotation, frontSlot);
        GameObject bGO = Instantiate(backCard.fbxCharacter != null ? backCard.fbxCharacter : unitPrefab, backSlot.position, backSlot.rotation, backSlot);

        frontUnit = fGO.GetComponent<CharacterUnit>() ?? fGO.AddComponent<CharacterUnit>();
        backUnit = bGO.GetComponent<CharacterUnit>() ?? bGO.AddComponent<CharacterUnit>();

        frontUnit.SetupFromCard(frontCard, team, manager.intelectManager, this.operationResult, opSymbol);
        backUnit.SetupFromCard(backCard, team, manager.intelectManager, this.operationResult, opSymbol);

        // agent speed etc...
    }
}
