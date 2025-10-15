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
        public GameObject instancedObject; 
        public bool isDefender;
    }

    public List<Characters> CharacterSetting = new List<Characters>();
    public Transform playerTower; 
    public Transform enemyTower; 
    public Transform leftBridge;
    public Transform rightBridge;

    private IntelectManager intelectManager;

    public GameObject combinedPrefabSum;
    public GameObject combinedPrefabSub;

    void Start()
    {
        intelectManager = FindObjectOfType<IntelectManager>();
    }

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

    public GameObject InstantiateCombinedCharacter(CardManager.Card cardA, CardManager.Card cardB, Vector3 spawnPosition, int combinedValue, char opSymbol, string teamTag)
    {
        GameObject truckPrefab = (opSymbol == '+') ? combinedPrefabSum : combinedPrefabSub;
        if (truckPrefab == null)
        {
            Debug.LogError("[CharacterManager] Prefab combinado no asignado para el operador " + opSymbol);
            return null;
        }

        // Instancia el "camión" que ya contiene dos transforms llamados "FrontSlot" y "BackSlot"
        GameObject instance = Instantiate(truckPrefab, spawnPosition, Quaternion.identity);
        AssignTag(instance, teamTag);

        // Busca dentro del prefab las posiciones donde poner los models
        Transform frontSlot = instance.transform.Find("FrontSlot");
        Transform backSlot = instance.transform.Find("BackSlot");

        if (frontSlot == null || backSlot == null)
        {
            Debug.LogWarning("[CharacterManager] FrontSlot/BackSlot no encontrados en el prefab combinado; se crearán posiciones fallback.");
            GameObject f = new GameObject("FrontSlot"); f.transform.SetParent(instance.transform); f.transform.localPosition = new Vector3(0, 0.5f, 0.5f);
            GameObject b = new GameObject("BackSlot"); b.transform.SetParent(instance.transform); b.transform.localPosition = new Vector3(0, 0.5f, -0.5f);
            frontSlot = f.transform; backSlot = b.transform;
        }

        // Decidir orden: el mayor value delante
        CardManager.Card frontCard = cardA.cardValue >= cardB.cardValue ? cardA : cardB;
        CardManager.Card backCard = cardA.cardValue >= cardB.cardValue ? cardB : cardA;

        // Instanciar los modelos (FBX) de las cartas dentro del camión
        if (frontCard.fbxCharacter != null)
        {
            GameObject fModel = Instantiate(frontCard.fbxCharacter, frontSlot.position, frontSlot.rotation, frontSlot);
            fModel.transform.localPosition = Vector3.zero;
            fModel.transform.localRotation = Quaternion.identity;
        }
        if (backCard.fbxCharacter != null)
        {
            GameObject bModel = Instantiate(backCard.fbxCharacter, backSlot.position, backSlot.rotation, backSlot);
            bModel.transform.localPosition = Vector3.zero;
            bModel.transform.localRotation = Quaternion.identity;
        }

        // Añade/ajusta componente CharacterCombined en el truck (si lo necesita)
        CharacterCombined cc = instance.GetComponent<CharacterCombined>();
        if (cc == null) cc = instance.AddComponent<CharacterCombined>();
        cc.InitializeCombined(combinedValue, (frontCard.cardVelocity + backCard.cardVelocity) / 2f);

        // Configura NavMeshAgent si tu camión se mueve por NavMesh
        NavMeshAgent agent = instance.GetComponent<NavMeshAgent>();
        if (agent == null) agent = instance.AddComponent<NavMeshAgent>();
        agent.speed = (frontCard.cardVelocity + backCard.cardVelocity) / 2f;

        // target bridge / tower logic: igual que antes (left/right)
        Transform targetBridge = (spawnPosition.x < 0) ? leftBridge : rightBridge;
        cc.SetupMovement(agent, targetBridge, enemyTower);

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