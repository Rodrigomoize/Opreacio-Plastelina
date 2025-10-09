using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections.Generic;


public class CharacterCombined : MonoBehaviour
{
    [System.Serializable]
    public class CombinedCharacter
    {
        public float velocity;
        public int life;
        public int value;
        NavMeshAgent agent;
        public GameObject characterCombinedPrefab;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public List<CombinedCharacter> attackCharacters = new List<CombinedCharacter>();
    public CombinedCharacter attackCharacter = new CombinedCharacter();

    CharacterManager characterManager;

    void Start()
    {

    }

    public CombinedCharacter SetCombinedCharacterValues(List<CharacterManager.Characters> characters)
    {
        if (characters.Count != 2)
        {
            Debug.LogError("Se requieren exactamente 2 personajes para combinar");
            return null;
        }

        var characterA = characters[0];
        var characterB = characters[1];

        CombinedCharacter attacker = new CombinedCharacter();

        
        attacker.velocity = (characterA.velocity + characterB.velocity) / 2; 
        attacker.value = characterA.value + characterB.value; 

        
        attacker.characterCombinedPrefab = characterA.characterPrefab;

        attackCharacter = attacker; // Actualiza la referencia del personaje actual
        return attacker;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
