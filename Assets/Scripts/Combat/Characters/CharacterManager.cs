using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    
    [System.Serializable]
    public class Characters
    {
        public float velocity;
        public int life;
        public int value;
        NavMeshAgent agent;
        public GameObject characterPrefab;
    }

    public List<Characters> CharacterSetting = new List<Characters>();
    Characters original = new Characters();
    CharacterCombined combined;
    IntelectManager intelectManager;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Characters SetAtributes(Characters character)
    {

        character.velocity = original.velocity;
        character.life = original.life;
        character.value = original.value;

        return character;
    }

    public void OnColliderEnter(Collider playerCollider)
    {
        var deffender = GetComponent<Characters>();
        var b = GetComponent<CharacterCombined>();
        if (playerCollider.CompareTag("AITeam"))
        {
            if (deffender.value == b.attackCharacter.value)
            {
                ResolveOperation();
            }else
            {
                                
            }
        }
        else
        {}
    }

    public void GenerateSingleCharacter()
    {

    }

    public List<Characters> GenerateCombinedCharacter(Characters firstCharacter, Characters secondCharacter)
    {
        Characters characterA = new Characters();
        Characters characterB = new Characters();

        characterA.velocity = firstCharacter.velocity;
        characterB.velocity = secondCharacter.velocity;
        characterA.life = firstCharacter.life;
        characterB.life = secondCharacter.life;
        characterA.value = firstCharacter.value;
        characterB.value = secondCharacter.value;
        characterA.characterPrefab = firstCharacter.characterPrefab;
        characterB.characterPrefab = secondCharacter.characterPrefab;

        return new List<Characters> { characterA, characterB };
    }


    public void AssignTag(GameObject obj, string tagName)
    {
        if (!string.IsNullOrEmpty(tagName) && obj != null)
        {
            obj.tag = tagName;
        }
    }

    public void ResolveOperation()
    {

        intelectManager.AddIntelect(1);
    }

}
