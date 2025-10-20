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
    private float groundSnapPadding = 0.01f;


    void Start()
    {
        intelectManager = FindFirstObjectByType<IntelectManager>();
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

        GameObject instance = Instantiate(cardData.fbxCharacter, spawnPosition, Quaternion.identity);

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


        AlignModelBottomToGround(instance, spawnPosition.y);
        // Configurar NavMeshAgent
        NavMeshAgent agent = instance.GetComponent<NavMeshAgent>();
        if (agent == null) agent = instance.AddComponent<NavMeshAgent>();

        // intenta colocar sobre NavMesh cercano (rango pequeño)
        NavMeshHit navHit;
        float sampleRadius = 2.0f; // ajusta si necesitas buscar más lejos
        if (NavMesh.SamplePosition(instance.transform.position, out navHit, sampleRadius, NavMesh.AllAreas))
        {
            instance.transform.position = navHit.position;
            agent.Warp(navHit.position);
        }
        else
        {
            // fallback: warp a spawn original (mejor que quedarse fuera)
            agent.Warp(instance.transform.position);
        }

        // Decidir camino según posición (izquierda o derecha)
        Transform targetBridge = (spawnPosition.x < 0) ? leftBridge : rightBridge;

        if (targetBridge == null || enemyTower == null)
        {
            Debug.LogError("[CharacterManager] leftBridge, rightBridge o enemyTower no están asignados en el inspector!");
            return instance; // Devuelve el personaje pero sin movimiento configurado
        }

        Transform enemyForThisTeam = (teamTag == "AITeam") ? playerTower : enemyTower;

        charScript.SetupMovement(agent, targetBridge, enemyForThisTeam, true);

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

        Transform GetSlotAnchor(Transform slotTransform, string anchorName)
        {
            if (slotTransform == null) return null;

            BoxCollider box = slotTransform.GetComponent<BoxCollider>();
            if (box == null)
            {
               
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
                
                Transform existing = slotTransform.Find(anchorName);
                if (existing != null) return existing;

                GameObject anchorGO = new GameObject(anchorName);
                anchorGO.transform.SetParent(slotTransform, false);
                anchorGO.transform.localPosition = box.center;
                anchorGO.transform.localRotation = Quaternion.identity;
                return anchorGO.transform;
            }
        }

        Transform frontAnchor = GetSlotAnchor(frontSlot, "FrontAnchor");
        Transform backAnchor = GetSlotAnchor(backSlot, "BackAnchor");

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

        
        CardManager.Card frontCard = partA;
        CardManager.Card backCard = partB;

        
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

        // --- Asegurar componente CharacterCombined en el camión
        CharacterCombined cc = instance.GetComponent<CharacterCombined>();
        if (cc == null) cc = instance.AddComponent<CharacterCombined>();

        
        cc.frontPosition = frontAnchor;
        cc.backPosition = backAnchor;
        cc.InitializeCombined(result, (frontCard.cardVelocity + backCard.cardVelocity) / 2f);
        
        // Configurar los valores de la operación para el UI
        cc.SetOperationValues(partA.cardValue, partB.cardValue, opSymbol);


        // With this corrected line:
        AlignModelBottomToGround(instance, spawnPosition.y);
        // Configurar NavMeshAgent
        NavMeshAgent agent = instance.GetComponent<NavMeshAgent>();
        if (agent == null) agent = instance.AddComponent<NavMeshAgent>();

        // intenta colocar sobre NavMesh cercano (rango pequeño)
        NavMeshHit navHit;
        float sampleRadius = 2.0f; // ajusta si necesitas buscar más lejos
        if (NavMesh.SamplePosition(instance.transform.position, out navHit, sampleRadius, NavMesh.AllAreas))
        {
            instance.transform.position = navHit.position;
            agent.Warp(navHit.position);
        }
        else
        {
            // fallback: warp a spawn original (mejor que quedarse fuera)
            agent.Warp(instance.transform.position);
        }

        // Ruteo
        Transform targetBridge = (spawnPosition.x < 0) ? leftBridge : rightBridge;

        Transform enemyForThisTeam = (teamTag == "AITeam") ? playerTower : enemyTower;

        if (targetBridge != null && enemyForThisTeam != null)
        {
            cc.SetupMovement(agent, targetBridge, enemyForThisTeam);
        }
        else
        {
            Debug.LogWarning("[CharacterManager] leftBridge/rightBridge/playerTower/enemyTower no asignados correctamente, el camión puede no tener ruta.");
        }



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
        
        // Buscar la torre enemiga y aplicar el daño
        Tower tower = enemyTower?.GetComponent<Tower>();
        if (tower != null)
        {
            tower.TakeDamage(damage);
        }
        else
        {
            Debug.LogWarning("[CharacterManager] No se encontró el componente Tower en la torre enemiga");
        }
    }

    private void AlignModelBottomToGround(GameObject model, float targetY)
    {
        if (model == null) return;

        Renderer[] rends = model.GetComponentsInChildren<Renderer>(true);
        if (rends == null || rends.Length == 0)
        {
            // fallback sencillo: colocar root en targetY
            Vector3 p = model.transform.position; p.y = targetY; model.transform.position = p;
            return;
        }

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);

        float bottomY = b.min.y;
        float delta = (targetY + groundSnapPadding) - bottomY;
        if (Mathf.Abs(delta) > 0.0001f)
        {
            model.transform.position += new Vector3(0f, delta, 0f);
        }
    }
}