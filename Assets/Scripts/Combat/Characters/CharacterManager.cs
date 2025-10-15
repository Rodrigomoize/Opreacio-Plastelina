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

    public GameObject InstantiateCombinedCharacter(CardManager.Card partA, CardManager.Card partB,
                                              Vector3 spawnPosition, int combinedValue, char opSymbol, string teamTag)
    {
        if (partA == null || partB == null)
        {
            Debug.LogError("[CharacterManager] InstantiateCombinedCharacter: cartas null");
            return null;
        }

        // --- Validaciones de reglas (rango 0..5) — repetimos por seguridad
        int result = 0;
        if (opSymbol == '+')
        {
            result = partA.cardValue + partB.cardValue;
            if (result > 5)
            {
                Debug.LogWarning($"[CharacterManager] Suma inválida: {partA.cardValue}+{partB.cardValue} = {result} > 5. Jugada cancelada.");
                return null;
            }
        }
        else if (opSymbol == '-')
        {
            result = partA.cardValue - partB.cardValue;
            if (result < 0)
            {
                // intentamos swap automático (si la lógica lo permite)
                Debug.Log($"[CharacterManager] Resta negativa detectada ({partA.cardValue} - {partB.cardValue}). Intercambiando para intentar corregir.");
                var tmp = partA; partA = partB; partB = tmp;
                result = partA.cardValue - partB.cardValue;
            }
            if (result < 0 || result > 5)
            {
                Debug.LogWarning($"[CharacterManager] Resta inválida: resultado {result}. Jugada cancelada.");
                return null;
            }
        }
        else
        {
            Debug.LogWarning("[CharacterManager] Operador desconocido en InstantiateCombinedCharacter: " + opSymbol);
            return null;
        }

        // --- Elegir prefab del 'camión'
        GameObject truckPrefab = (opSymbol == '+') ? combinedPrefabSum : combinedPrefabSub;
        if (truckPrefab == null)
        {
            Debug.LogError("[CharacterManager] Prefab combinado no asignado para operador " + opSymbol);
            return null;
        }

        // Instanciar el camión contenedor
        GameObject instance = Instantiate(truckPrefab, spawnPosition, Quaternion.identity);
        AssignTag(instance, teamTag);

        // Buscar slots (FrontSlot y BackSlot)
        Transform frontSlot = instance.transform.Find("FrontSlot");
        Transform backSlot = instance.transform.Find("BackSlot");

        if (frontSlot == null || backSlot == null)
        {
            Debug.LogWarning("[CharacterManager] FrontSlot/BackSlot no encontrados en prefab combinado. Creando fallback positions.");
            GameObject f = new GameObject("FrontSlot"); f.transform.SetParent(instance.transform); f.transform.localPosition = new Vector3(0, 0.3f, 0.3f);
            GameObject b = new GameObject("BackSlot"); b.transform.SetParent(instance.transform); b.transform.localPosition = new Vector3(0, 0.3f, -0.3f);
            frontSlot = f.transform;
            backSlot = b.transform;
        }

        // Orden visual: partA = front, partB = back (partA es la primera elegida)
        CardManager.Card frontCard = partA;
        CardManager.Card backCard = partB;

        GameObject frontModel = null;
        GameObject backModel = null;

        // Instanciar modelo frontal
        if (frontCard.fbxCharacter != null)
        {
            frontModel = Instantiate(frontCard.fbxCharacter, frontSlot.position, frontSlot.rotation, frontSlot);
            frontModel.transform.localPosition = Vector3.zero;
            frontModel.transform.localRotation = Quaternion.identity;
            frontModel.transform.localScale = Vector3.one;
            Debug.Log($"[CharacterManager] Front model instantiated: {frontCard.cardName} ({frontCard.fbxCharacter.name})");
        }
        else
        {
            // Placeholder para visibilidad
            frontModel = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            frontModel.name = $"PH_Front_{frontCard.cardName}";
            frontModel.transform.SetParent(frontSlot, false);
            frontModel.transform.localPosition = Vector3.zero;
            frontModel.transform.localScale = Vector3.one * 0.6f;
            Debug.LogWarning($"[CharacterManager] frontCard.fbxCharacter null para {frontCard.cardName}. Creado placeholder.");
        }

        // Instanciar modelo trasero
        if (backCard.fbxCharacter != null)
        {
            backModel = Instantiate(backCard.fbxCharacter, backSlot.position, backSlot.rotation, backSlot);
            backModel.transform.localPosition = Vector3.zero;
            backModel.transform.localRotation = Quaternion.identity;
            backModel.transform.localScale = Vector3.one;
            Debug.Log($"[CharacterManager] Back model instantiated: {backCard.cardName} ({backCard.fbxCharacter.name})");
        }
        else
        {
            backModel = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            backModel.name = $"PH_Back_{backCard.cardName}";
            backModel.transform.SetParent(backSlot, false);
            backModel.transform.localPosition = Vector3.zero;
            backModel.transform.localScale = Vector3.one * 0.6f;
            Debug.LogWarning($"[CharacterManager] backCard.fbxCharacter null para {backCard.cardName}. Creado placeholder.");
        }

        // Desactivar colliders/rigidbodies y scripts en los modelos instanciados para que el camión lleve la lógica
        if (frontModel != null)
        {
            foreach (var col in frontModel.GetComponentsInChildren<Collider>(true)) col.enabled = false;
            foreach (var rb in frontModel.GetComponentsInChildren<Rigidbody>(true)) rb.isKinematic = true;
            foreach (var ch in frontModel.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (ch is Character || ch is CharacterCombined) ch.enabled = false;
            }
        }
        if (backModel != null)
        {
            foreach (var col in backModel.GetComponentsInChildren<Collider>(true)) col.enabled = false;
            foreach (var rb in backModel.GetComponentsInChildren<Rigidbody>(true)) rb.isKinematic = true;
            foreach (var ch in backModel.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (ch is Character || ch is CharacterCombined) ch.enabled = false;
            }
        }

        // --- Asegurar componente CharacterCombined en el camión
        CharacterCombined cc = instance.GetComponent<CharacterCombined>();
        if (cc == null) cc = instance.AddComponent<CharacterCombined>();

        // Punteros para visuales (si tu CharacterCombined los tiene públicos)
        cc.frontPosition = frontSlot;
        cc.backPosition = backSlot;
        // (Opcional) guardar referencias a instancias visibles si las quieres en CharacterCombined:
        // cc.frontCharacterInstance = frontModel;
        // cc.backCharacterInstance = backModel;

        // Inicializar valores
        float avgVelocity = (frontCard.cardVelocity + backCard.cardVelocity) / 2f;
        cc.InitializeCombined(result, avgVelocity);

        // NavMeshAgent en el camión
        NavMeshAgent agent = instance.GetComponent<NavMeshAgent>();
        if (agent == null) agent = instance.AddComponent<NavMeshAgent>();
        agent.speed = avgVelocity;

        Transform targetBridge = (spawnPosition.x < 0) ? leftBridge : rightBridge;
        if (targetBridge != null && enemyTower != null)
        {
            cc.SetupMovement(agent, targetBridge, enemyTower);
        }
        else
        {
            Debug.LogWarning("[CharacterManager] leftBridge/rightBridge/enemyTower no asignados, el camión no tiene ruta.");
        }

        Debug.Log($"[CharacterManager] Instanciado Combined Truck: {frontCard.cardName}+{backCard.cardName} value={result} op={opSymbol}");
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