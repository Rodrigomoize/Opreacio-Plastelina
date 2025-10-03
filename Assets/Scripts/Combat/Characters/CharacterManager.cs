using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    public IntelectManager intelectManager;


    public Transform playerCharactersParent;
    public Transform enemyCharactersParent;

    [Header("Prefabs")]
    public GameObject combinedPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Crea el character a partir de la Card (usa card.fbxCharacter como prefab)
    public Character CreateCharacterFromCard(CardManager.Card card, Vector3 spawnPosition, Character.Team team = Character.Team.Player)
    {
        if (card == null)
        {
            Debug.LogError("CreateCharacterFromCard: card null");
            return null;
        }
        if (card.fbxCharacter == null)
        {
            Debug.LogError($"CreateCharacterFromCard: card {card.cardName} no tiene fbxCharacter");
            return null;
        }

        Transform parent = (team == Character.Team.Player) ? playerCharactersParent : enemyCharactersParent;
        GameObject go = Instantiate(card.fbxCharacter, spawnPosition, Quaternion.identity, parent);
        Character charScript = go.GetComponent<Character>();
        if (charScript == null)
        {
            Debug.LogError("Prefab fbxCharacter no contiene componente Character");
            return null;
        }

        charScript.SetupFromCard(card, team, intelectManager);
        return charScript;
    }

    public CombinedCharacter CreateCombinedCharacterFromCards(CardManager.Card partA, CardManager.Card partB, Vector3 spawnPos, Character.Team team, int operationResult, char opSymbol)
    {
        if (combinedPrefab == null) { Debug.LogError("combinedPrefab no asignado"); return null; }
        Transform parent = (team == Character.Team.Player) ? playerCharactersParent : enemyCharactersParent;
        GameObject go = Instantiate(combinedPrefab, spawnPos, Quaternion.identity, parent);
        CombinedCharacter combined = go.GetComponent<CombinedCharacter>();
        if (combined == null) { Debug.LogError("combinedPrefab sin script CombinedCharacter"); Destroy(go); return null; }
        combined.SetupFromCards(partA, partB, operationResult, opSymbol, team, this); // pasamos referencia a manager si hace falta
        return combined;
    }

}
