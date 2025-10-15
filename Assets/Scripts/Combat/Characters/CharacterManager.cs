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

    public float combinedModelTargetSize = 1.0f;
    public float modelFitPadding = 0.9f;


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

        // --- Validaciones de reglas (rango 0..5)
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
                // intentamos swap automático
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

        // --- Prefab del "camión"
        GameObject truckPrefab = (opSymbol == '+') ? combinedPrefabSum : combinedPrefabSub;
        if (truckPrefab == null)
        {
            Debug.LogError("[CharacterManager] combined prefab missing for op " + opSymbol);
            return null;
        }

        
        GameObject instance = Instantiate(truckPrefab, spawnPosition, Quaternion.identity);
        AssignTag(instance, teamTag);

       
        Transform frontSlot = instance.transform.Find("FrontSlot");
        Transform backSlot = instance.transform.Find("BackSlot");

        
        if (frontSlot == null || backSlot == null)
        {
            foreach (Transform child in instance.transform)
            {
                string n = child.name.ToLower();
                if (frontSlot == null && n.Contains("Delante")) frontSlot = child;
                if (backSlot == null && n.Contains("Atras")) backSlot = child;
            }
        }

        
        if (frontSlot == null || backSlot == null)
        {
            foreach (var col in instance.GetComponentsInChildren<BoxCollider>(true))
            {
                string n = col.gameObject.name.ToLower();
                if (frontSlot == null && n.Contains("Delante")) frontSlot = col.transform;
                if (backSlot == null && n.Contains("Atras")) backSlot = col.transform;
            }
        }

        // fallback: crear transforms vacíos si no hay nada
        if (frontSlot == null)
        {
            GameObject f = new GameObject("FrontSlot");
            f.transform.SetParent(instance.transform);
            f.transform.localPosition = new Vector3(0, 0.3f, 0.3f);
            frontSlot = f.transform;
        }
        if (backSlot == null)
        {
            GameObject b = new GameObject("BackSlot");
            b.transform.SetParent(instance.transform);
            b.transform.localPosition = new Vector3(0, 0.3f, -0.3f);
            backSlot = b.transform;
        }

        // Si el slot detectado es en realidad un objeto con BoxCollider, creamos/obtenemos un "anchor"
        // hijo posicionado en el center del BoxCollider (local).
        Transform GetSlotAnchor(Transform slotTransform, string anchorName)
        {
            if (slotTransform == null) return null;

            BoxCollider box = slotTransform.GetComponent<BoxCollider>();
            if (box == null)
            {
                // si no hay BoxCollider, pero el slotTransform tiene hijos con centro definido,
                // buscamos un child llamado anchorName y lo usamos; si no existe, creamos uno en local (0,0,0).
                Transform existing = slotTransform.Find(anchorName);
                if (existing != null) return existing;
                GameObject a = new GameObject(anchorName);
                a.transform.SetParent(slotTransform, false);
                a.transform.localPosition = Vector3.zero;
                a.transform.localRotation = Quaternion.identity;
                return a.transform;
            }
            else
            {
                // Si hay BoxCollider, intentamos reutilizar un child "Anchor" para ese collider (para no generar muchos objetos)
                Transform existing = slotTransform.Find(anchorName);
                if (existing != null) return existing;

                GameObject anchorGO = new GameObject(anchorName);
                anchorGO.transform.SetParent(slotTransform, false);
                // BoxCollider.center está en espacio local del collider. Lo usamos directamente como localPosition.
                anchorGO.transform.localPosition = box.center;
                anchorGO.transform.localRotation = Quaternion.identity;
                return anchorGO.transform;
            }
        }

        // obtenemos anchors concretos
        Transform frontAnchor = GetSlotAnchor(frontSlot, "FrontAnchor");
        Transform backAnchor = GetSlotAnchor(backSlot, "BackAnchor");

        // Aseguramos que frontAnchor/backAnchor no sean null (fallback)
        if (frontAnchor == null)
        {
            GameObject f = new GameObject("FrontAnchor");
            f.transform.SetParent(instance.transform, false);
            frontAnchor = f.transform;
        }
        if (backAnchor == null)
        {
            GameObject b = new GameObject("BackAnchor");
            b.transform.SetParent(instance.transform, false);
            backAnchor = b.transform;
        }

        // Determinar orden visual: partA (primera elegida) = front, partB = back
        CardManager.Card frontCard = partA;
        CardManager.Card backCard = partB;

        // --- Instanciar los modelos (sin parent todavía)
        GameObject frontModel;
        if (frontCard.fbxCharacter != null)
        {
            frontModel = Instantiate(frontCard.fbxCharacter, frontAnchor.position, frontAnchor.rotation);
            frontModel.name = $"FrontModel_{frontCard.cardName}";
        }
        else
        {
            frontModel = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            frontModel.name = $"PH_Front_{frontCard.cardName}";
            frontModel.transform.position = frontAnchor.position;
        }

        GameObject backModel;
        if (backCard.fbxCharacter != null)
        {
            backModel = Instantiate(backCard.fbxCharacter, backAnchor.position, backAnchor.rotation);
            backModel.name = $"BackModel_{backCard.cardName}";
        }
        else
        {
            backModel = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            backModel.name = $"PH_Back_{backCard.cardName}";
            backModel.transform.position = backAnchor.position;
        }

        
        FitModelToSlot(frontModel, frontAnchor, modelFitPadding);
        FitModelToSlot(backModel, backAnchor, modelFitPadding);

       
        if (frontModel != null)
        {
            foreach (var col in frontModel.GetComponentsInChildren<Collider>(true)) col.enabled = false;
            foreach (var rb in frontModel.GetComponentsInChildren<Rigidbody>(true)) rb.isKinematic = true;
            foreach (var mb in frontModel.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb is Character || mb is CharacterCombined) mb.enabled = false;
            }
        }
        if (backModel != null)
        {
            foreach (var col in backModel.GetComponentsInChildren<Collider>(true)) col.enabled = false;
            foreach (var rb in backModel.GetComponentsInChildren<Rigidbody>(true)) rb.isKinematic = true;
            foreach (var mb in backModel.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb is Character || mb is CharacterCombined) mb.enabled = false;
            }
        }

        // --- Asegurar componente CharacterCombined en el camión (contenedor)
        CharacterCombined cc = instance.GetComponent<CharacterCombined>();
        if (cc == null) cc = instance.AddComponent<CharacterCombined>();

        // Pasamos las referencias de anchor/visual slots al CharacterCombined si lo usa
        cc.frontPosition = frontAnchor;
        cc.backPosition = backAnchor;
        cc.InitializeCombined(result, (frontCard.cardVelocity + backCard.cardVelocity) / 2f);

        // NavMeshAgent en el camión
        NavMeshAgent agent = instance.GetComponent<NavMeshAgent>();
        if (agent == null) agent = instance.AddComponent<NavMeshAgent>();
        agent.speed = (frontCard.cardVelocity + backCard.cardVelocity) / 2f;

        // Ruteo
        Transform targetBridge = (spawnPosition.x < 0) ? leftBridge : rightBridge;
        if (targetBridge != null && enemyTower != null)
            cc.SetupMovement(agent, targetBridge, enemyTower);
        else
            Debug.LogWarning("[CharacterManager] leftBridge/rightBridge/enemyTower no asignados, el camión no tiene ruta.");

        Debug.Log($"[CharacterManager] Combined created: {frontCard.cardName}+{backCard.cardName} -> anchors used (front:{frontAnchor.name}, back:{backAnchor.name}).");
        return instance;
    }


    private void FitModelToSlot(GameObject model, Transform slot, float padding = -1f)
    {
        if (model == null || slot == null) return;
        if (padding <= 0f) padding = modelFitPadding;

        model.transform.SetParent(slot, false);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        // Recolectar renderers para calcular bounds en mundo
        Renderer[] rends = model.GetComponentsInChildren<Renderer>(true);
        if (rends == null || rends.Length == 0)
        {
            // nada que ajustar (placeholder u objeto vacío)
            return;
        }

        Bounds bounds = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
            bounds.Encapsulate(rends[i].bounds);

        float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (maxDim <= 1e-5f) return;

        
        float target = combinedModelTargetSize;

        float scaleFactor = (target * padding) / maxDim;

        // Aplica escala manteniendo uniformidad
        model.transform.localScale = Vector3.one * scaleFactor;

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