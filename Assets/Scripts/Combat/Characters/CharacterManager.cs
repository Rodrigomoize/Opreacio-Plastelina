using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    [System.Serializable]
    public class Characters
    {
        public float velocity;
        public int life;
        public int value;
        public NavMeshAgent agent;
        public GameObject characterPrefab;
        public GameObject instancedObject; // referencia al objeto instanciado
        public bool isDefender; // true = defender, false = attacker
    }

    public List<Characters> CharacterSetting = new List<Characters>();
    public Transform playerTower; // Torre del jugador
    public Transform enemyTower; // Torre enemiga
    public Transform leftBridge; // Puente izquierdo
    public Transform rightBridge; // Puente derecho

    private IntelectManager intelectManager;

    void Start()
    {
        intelectManager = FindObjectOfType<IntelectManager>();
    }

    // Instanciar personaje simple (defender)
    public GameObject InstantiateSingleCharacter(CardManager.Card cardData, Vector3 spawnPosition, string teamTag)
    {
        if (cardData == null)
        {
            Debug.LogError("[CharacterManager] cardData es null");
            return null;
        }

        if (cardData.fbxCharacter == null)
        {
            Debug.LogError($"[CharacterManager] La carta {cardData.cardName} no tiene fbxCharacter asignado!");
            return null;
        }

        // Instanciar directamente el prefab de la carta (ya tiene el script Character)
        GameObject instance = Instantiate(cardData.fbxCharacter, spawnPosition, Quaternion.identity);

        // Asignar tag
        AssignTag(instance, teamTag);

        // Obtener o añadir componente Character
        Character charScript = instance.GetComponent<Character>();
        if (charScript == null)
        {
            Debug.LogWarning($"[CharacterManager] El prefab {cardData.fbxCharacter.name} no tiene Character script, añadiendo...");
            charScript = instance.AddComponent<Character>();
        }

        // Inicializar con los datos de la carta
        charScript.InitializeCharacter(cardData.cardValue, cardData.cardLife, cardData.cardVelocity, true);

        // Configurar NavMeshAgent
        NavMeshAgent agent = instance.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = instance.AddComponent<NavMeshAgent>();
        }
        agent.speed = cardData.cardVelocity;

        // Decidir camino según posición (izquierda o derecha)
        Transform targetBridge = (spawnPosition.x < 0) ? leftBridge : rightBridge;

        if (targetBridge == null || enemyTower == null)
        {
            Debug.LogError("[CharacterManager] leftBridge, rightBridge o enemyTower no están asignados en el inspector!");
            return instance; // Devuelve el personaje pero sin movimiento configurado
        }

        charScript.SetupMovement(agent, targetBridge, enemyTower, true);

        Debug.Log($"[CharacterManager] ✓ {cardData.cardName} (valor:{cardData.cardValue}) creado como DEFENDER con tag {teamTag}");

        return instance;
    }

    public GameObject InstantiateCombinedCharacter(CardManager.Card cardA, CardManager.Card cardB,
                                                Vector3 spawnPosition, int combinedValue, string teamTag)
    {
        if (cardA == null || cardB == null)
        {
            Debug.LogError("[CharacterManager] Alguna de las cartas es null");
            return null;
        }

        if (cardA.fbxCharacter == null || cardB.fbxCharacter == null)
        {
            Debug.LogError("[CharacterManager] Alguna carta no tiene fbxCharacter asignado");
            return null;
        }

        // Determinar quién va delante según el valor (el mayor va adelante)
        CardManager.Card frontCard = cardA.cardValue >= cardB.cardValue ? cardA : cardB;
        CardManager.Card backCard = cardA.cardValue >= cardB.cardValue ? cardB : cardA;

        // Crear un GameObject vacío como contenedor
        GameObject instance = new GameObject($"Combined_{frontCard.cardName}+{backCard.cardName}");
        instance.transform.position = spawnPosition;
        instance.transform.rotation = Quaternion.identity;

        // Asignar tag
        AssignTag(instance, teamTag);

        // Añadir CharacterCombined
        CharacterCombined combinedScript = instance.AddComponent<CharacterCombined>();

        // Añadir collider para las colisiones (ajusta el tamaño según necesites)
        CapsuleCollider col = instance.AddComponent<CapsuleCollider>();
        col.isTrigger = true;
        col.radius = 0.5f;
        col.height = 2f;

        // Crear posiciones para los personajes
        GameObject frontPosObj = new GameObject("FrontPosition");
        frontPosObj.transform.SetParent(instance.transform);
        frontPosObj.transform.localPosition = new Vector3(0, 0, 0.5f); // Adelante

        GameObject backPosObj = new GameObject("BackPosition");
        backPosObj.transform.SetParent(instance.transform);
        backPosObj.transform.localPosition = new Vector3(0, 0, -0.5f); // Atrás

        // Asignar las referencias en el script
        combinedScript.frontPosition = frontPosObj.transform;
        combinedScript.backPosition = backPosObj.transform;

        // Inicializar valores
        float avgVelocity = (cardA.cardVelocity + cardB.cardVelocity) / 2f;
        combinedScript.InitializeCombined(combinedValue, avgVelocity);

        // Colocar los modelos FBX en sus posiciones
        combinedScript.SetupCharacterModels(frontCard.fbxCharacter, backCard.fbxCharacter);

        // Configurar NavMeshAgent
        NavMeshAgent agent = instance.AddComponent<NavMeshAgent>();
        agent.speed = avgVelocity;

        // Decidir camino
        Transform targetBridge = (spawnPosition.x < 0) ? leftBridge : rightBridge;

        if (targetBridge == null || enemyTower == null)
        {
            Debug.LogError("[CharacterManager] leftBridge, rightBridge o enemyTower no están asignados!");
            return instance;
        }

        combinedScript.SetupMovement(agent, targetBridge, enemyTower);

        Debug.Log($"[CharacterManager] ✓ Combinado {frontCard.cardName}+{backCard.cardName} (valor:{combinedValue}) creado como ATTACKER con tag {teamTag}");

        return instance;
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
        if (intelectManager != null)
        {
            intelectManager.AddIntelect(1);
        }
    }

    // Daño a la torre (por ahora solo logs)
    public void DamageEnemyTower(int damage)
    {
        Debug.Log($"[TOWER DAMAGE] Torre enemiga recibe {damage} de daño");
        // Aquí después añadirás la lógica real de la torre
    }
}