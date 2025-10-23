using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    public Transform playerTower;
    public Transform enemyTower;
    public Transform leftBridge;
    public Transform rightBridge;

    [Header("Área de spawn AI (defensas)")]
    [Tooltip("Transform que define la zona donde la IA puede spawnear defensas (puede tener un BoxCollider para bounds exactos)")]
    public Transform playableAreaAI;

    private IntelectManager intelectManager;

    [Header("Referencias de Intelecto")]
    [Tooltip("IntelectManager del jugador")]
    public IntelectManager playerIntelectManager;

    [Tooltip("IntelectManager de la IA")]
    public IntelectManager aiIntelectManager;

    [Header("Prefabs de operaciones combinadas")]
    public GameObject combinedPrefabSum;
    public GameObject combinedPrefabSub;
    
    [Header("Prefabs de UI")]
    [Tooltip("Prefab de UI para tropas individuales")]
    public GameObject troopUIPrefab;
    [Tooltip("Prefab de UI para operaciones combinadas")]
    public GameObject operationUIPrefab;

    private float groundSnapPadding = 0.01f;


    void Start()
    {
        intelectManager = FindFirstObjectByType<IntelectManager>();

        // Si no se asignó manualmente el playerIntelectManager, usar el encontrado
        if (playerIntelectManager == null)
        {
            playerIntelectManager = intelectManager;
        }
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
        
        // Asignar el prefab de UI
        charScript.troopUIPrefab = troopUIPrefab;

        // Inicializar con los datos de la carta (INCLUYE LA VELOCIDAD)
        charScript.InitializeCharacter(cardData.cardValue, cardData.cardLife, cardData.cardVelocity, true);

        AlignModelBottomToGround(instance, spawnPosition.y);

        // Configurar NavMeshAgent
        NavMeshAgent agent = instance.GetComponent<NavMeshAgent>();
        if (agent == null) agent = instance.AddComponent<NavMeshAgent>();

        // Intenta colocar sobre NavMesh cercano
        NavMeshHit navHit;
        float sampleRadius = 2.0f;
        if (NavMesh.SamplePosition(instance.transform.position, out navHit, sampleRadius, NavMesh.AllAreas))
        {
            instance.transform.position = navHit.position;
            agent.Warp(navHit.position);
        }
        else
        {
            agent.Warp(instance.transform.position);
        }

        // Decidir camino según posición
        Transform targetBridge = (spawnPosition.x < 0) ? leftBridge : rightBridge;

        if (targetBridge == null || enemyTower == null)
        {
            Debug.LogError("[CharacterManager] leftBridge, rightBridge o enemyTower no están asignados en el inspector!");
            return instance;
        }

        Transform enemyForThisTeam = (teamTag == "AITeam") ? playerTower : enemyTower;

        // Orientar hacia la torre enemiga (preservando la altura para evitar inclinación)
        if (enemyForThisTeam != null)
        {
            Vector3 lookTarget = new Vector3(enemyForThisTeam.position.x, instance.transform.position.y, enemyForThisTeam.position.z);
            instance.transform.LookAt(lookTarget);
        }

        // SetupMovement ahora aplicará la velocidad al NavMeshAgent
        // Nota: si luego necesitas que esa unidad cambie su destino (por ejemplo para interceptar un ataque),
        // puedes usar agent.SetDestination(...) desde el código que la creó.
        charScript.SetupMovement(agent, targetBridge, enemyForThisTeam, true);

        Debug.Log($"[CharacterManager] ✓ {cardData.cardName} (valor:{cardData.cardValue}, velocidad:{cardData.cardVelocity}) creado como DEFENDER con tag {teamTag}");

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

        // Validaciones de reglas
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

        // Activar Tint si es enemigo (AITeam)
        if (teamTag == "AITeam")
        {
            EnemyRedEmissiveTint tint = instance.GetComponent<EnemyRedEmissiveTint>();
            if (tint != null)
            {
                tint.enabled = true;
                tint.applyEmissive = true;
                // Forzar la aplicación inmediata del tinte
                tint.ApplyEmissiveToAll();
            }
        }
        else // Desactivar si es PlayerTeam
        {
            EnemyRedEmissiveTint tint = instance.GetComponent<EnemyRedEmissiveTint>();
            if (tint != null)
            {
                tint.enabled = false;
            }
        }

        // Buscar slots recursivamente en toda la jerarquía (no solo hijos directos)
        Transform frontSlot = null;
        Transform backSlot = null;
        
        // Buscar por nombre exacto primero
        foreach (Transform child in instance.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "FrontSlot") frontSlot = child;
            if (child.name == "BackSlot") backSlot = child;
            if (frontSlot != null && backSlot != null) break;
        }

        // Si no se encuentran, buscar por nombres alternativos
        if (frontSlot == null || backSlot == null)
        {
            foreach (Transform child in instance.GetComponentsInChildren<Transform>(true))
            {
                string n = child.name.ToLower();
                if (frontSlot == null && n.Contains("delante")) frontSlot = child;
                if (backSlot == null && n.Contains("atras")) backSlot = child;
                if (frontSlot != null && backSlot != null) break;
            }
        }

        // Si aún no se encuentran, buscar por BoxColliders
        if (frontSlot == null || backSlot == null)
        {
            foreach (var col in instance.GetComponentsInChildren<BoxCollider>(true))
            {
                string n = col.gameObject.name.ToLower();
                if (frontSlot == null && n.Contains("delante")) frontSlot = col.transform;
                if (backSlot == null && n.Contains("atras")) backSlot = col.transform;
                if (frontSlot != null && backSlot != null) break;
            }
        }

        // Solo crear slots nuevos si definitivamente no existen
        if (frontSlot == null)
        {
            Debug.LogWarning($"[CharacterManager] No se encontró FrontSlot en {instance.name}, creando uno nuevo");
            GameObject f = new GameObject("FrontSlot");
            f.transform.SetParent(instance.transform);
            f.transform.localPosition = new Vector3(0, 0.3f, 0.3f);
            frontSlot = f.transform;
        }
        if (backSlot == null)
        {
            Debug.LogWarning($"[CharacterManager] No se encontró BackSlot en {instance.name}, creando uno nuevo");
            GameObject b = new GameObject("BackSlot");
            b.transform.SetParent(instance.transform);
            b.transform.localPosition = new Vector3(0, 0.3f, -0.3f);
            backSlot = b.transform;
        }
        
        Debug.Log($"[CharacterManager] Slots encontrados - Front: {frontSlot.name}, Back: {backSlot.name}");


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
        
        // IMPORTANTE: Forzar escala 1 en los anchors para que no hereden la escala 0 del spawn
        frontAnchor.localScale = Vector3.one;
        backAnchor.localScale = Vector3.one;

        // Asegurar que la tropa más pequeña siempre vaya en el front slot
        CardManager.Card frontCard, backCard;
        if (partA.cardValue <= partB.cardValue)
        {
            frontCard = partA;  // La más pequeña al frente
            backCard = partB;
        }
        else
        {
            frontCard = partB;  // La más pequeña al frente
            backCard = partA;
        }

        GameObject frontModel;
        Vector3 frontOriginalScale = Vector3.one; // Guardar escala original
        if (frontCard.fbxCharacter != null)
        {
            // Guardar la escala del prefab ANTES de instanciar
            frontOriginalScale = frontCard.fbxCharacter.transform.localScale;
            
            frontModel = Instantiate(frontCard.fbxCharacter, frontAnchor.position, frontAnchor.rotation);
            frontModel.name = $"FrontModel_{frontCard.cardName}";
            frontModel.transform.SetParent(frontAnchor, false);
            frontModel.transform.localPosition = Vector3.zero;
            frontModel.transform.localRotation = Quaternion.identity;
            frontModel.transform.localScale = frontOriginalScale; // Restaurar escala original del prefab
        }
        else
        {
            frontModel = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            frontModel.name = $"PH_Front_{frontCard.cardName}";
            frontModel.transform.SetParent(frontAnchor, false);
            frontModel.transform.localPosition = Vector3.zero;
            frontModel.transform.localRotation = Quaternion.identity;
            frontModel.transform.localScale = Vector3.one; // Primitivos usan escala 1
        }

        GameObject backModel;
        Vector3 backOriginalScale = Vector3.one; // Guardar escala original
        if (backCard.fbxCharacter != null)
        {
            // Guardar la escala del prefab ANTES de instanciar
            backOriginalScale = backCard.fbxCharacter.transform.localScale;
            
            backModel = Instantiate(backCard.fbxCharacter, backAnchor.position, backAnchor.rotation);
            backModel.name = $"BackModel_{backCard.cardName}";
            backModel.transform.SetParent(backAnchor, false);
            backModel.transform.localPosition = Vector3.zero;
            backModel.transform.localRotation = Quaternion.identity;
            backModel.transform.localScale = backOriginalScale; // Restaurar escala original del prefab
        }
        else
        {
            backModel = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            backModel.name = $"PH_Back_{backCard.cardName}";
            backModel.transform.SetParent(backAnchor, false);
            backModel.transform.localPosition = Vector3.zero;
            backModel.transform.localRotation = Quaternion.identity;
            backModel.transform.localScale = Vector3.one; // Primitivos usan escala 1
        }

        // Modelos posicionados en (0,0,0) local respecto a sus anchors

        // Limpiar componentes de los modelos - mantener solo visual y animación
        if (frontModel != null)
        {
            // Eliminar todos los Colliders
            foreach (var col in frontModel.GetComponentsInChildren<Collider>(true))
            {
                Destroy(col);
            }

            // Eliminar todos los Rigidbodies
            foreach (var rb in frontModel.GetComponentsInChildren<Rigidbody>(true))
            {
                Destroy(rb);
            }

            // Eliminar todos los NavMeshAgents
            foreach (var navAgent in frontModel.GetComponentsInChildren<NavMeshAgent>(true))
            {
                Destroy(navAgent);
            }

            // Eliminar todos los MonoBehaviour scripts (Character, CharacterCombined, etc.)
            // Mantener solo Animator
            MonoBehaviour[] scripts = frontModel.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mb in scripts)
            {
                if (mb != null)
                {
                    Destroy(mb);
                }
            }
        }

        if (backModel != null)
        {
            // Eliminar todos los Colliders
            foreach (var col in backModel.GetComponentsInChildren<Collider>(true))
            {
                Destroy(col);
            }

            // Eliminar todos los Rigidbodies
            foreach (var rb in backModel.GetComponentsInChildren<Rigidbody>(true))
            {
                Destroy(rb);
            }

            // Eliminar todos los NavMeshAgents
            foreach (var navAgent in backModel.GetComponentsInChildren<NavMeshAgent>(true))
            {
                Destroy(navAgent);
            }

            // Eliminar todos los MonoBehaviour scripts
            // Mantener solo Animator
            MonoBehaviour[] scripts = backModel.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mb in scripts)
            {
                if (mb != null)
                {
                    Destroy(mb);
                }
            }
        }

        // Asegurar componente CharacterCombined
        CharacterCombined cc = instance.GetComponent<CharacterCombined>();
        if (cc == null) cc = instance.AddComponent<CharacterCombined>();
        
        // Asignar el prefab de UI para operaciones
        cc.operationUIPrefab = operationUIPrefab;

        cc.frontPosition = frontAnchor;
        cc.backPosition = backAnchor;
        float MapValueToVelocity(int cardValue)
        {
            cardValue = Mathf.Clamp(cardValue, 1, 5);
            float minVel = 2f;
            float maxVel = 4f;
            float steps = 4f;
            return maxVel - (cardValue - 1) * ((maxVel - minVel) / steps);
        }

        float combinedVelocity = MapValueToVelocity(result);
        cc.InitializeCombined(result, combinedVelocity);

        cc.SetOperationValues(partA.cardValue, partB.cardValue, opSymbol);

        // Alinear camión pero preservar posiciones locales de los modelos de tropas Y sus anchors
        Vector3 frontAnchorLocalPos = frontAnchor.localPosition;
        Quaternion frontAnchorLocalRot = frontAnchor.localRotation;
        Vector3 backAnchorLocalPos = backAnchor.localPosition;
        Quaternion backAnchorLocalRot = backAnchor.localRotation;

        Vector3 frontLocalPos = frontModel.transform.localPosition;
        Quaternion frontLocalRot = frontModel.transform.localRotation;
        Vector3 backLocalPos = backModel.transform.localPosition;
        Quaternion backLocalRot = backModel.transform.localRotation;

        AlignModelBottomToGround(instance, spawnPosition.y);

        // Restaurar posiciones locales de anchors
        frontAnchor.localPosition = frontAnchorLocalPos;
        frontAnchor.localRotation = frontAnchorLocalRot;
        backAnchor.localPosition = backAnchorLocalPos;
        backAnchor.localRotation = backAnchorLocalRot;

        // Restaurar posiciones locales de modelos
        frontModel.transform.localPosition = frontLocalPos;
        frontModel.transform.localRotation = frontLocalRot;
        backModel.transform.localPosition = backLocalPos;
        backModel.transform.localRotation = backLocalRot;

        // Configurar NavMeshAgent
        NavMeshAgent agent = instance.GetComponent<NavMeshAgent>();
        if (agent == null) agent = instance.AddComponent<NavMeshAgent>();

        NavMeshHit navHit;
        float sampleRadius = 2.0f;
        if (NavMesh.SamplePosition(instance.transform.position, out navHit, sampleRadius, NavMesh.AllAreas))
        {
            instance.transform.position = navHit.position;
            agent.Warp(navHit.position);
        }
        else
        {
            agent.Warp(instance.transform.position);
        }

        // Ruteo
        Transform targetBridge = (spawnPosition.x < 0) ? leftBridge : rightBridge;
        Transform enemyForThisTeam = (teamTag == "AITeam") ? playerTower : enemyTower;

        // Orientar camión hacia torre enemiga (preservando altura)
        if (enemyForThisTeam != null)
        {
            Vector3 lookTarget = new Vector3(enemyForThisTeam.position.x, instance.transform.position.y, enemyForThisTeam.position.z);
            instance.transform.LookAt(lookTarget);
        }

        if (targetBridge != null && enemyForThisTeam != null)
        {
            // SetupMovement ahora aplicará la velocidad al NavMeshAgent
            cc.SetupMovement(agent, targetBridge, enemyForThisTeam);
        }
        else
        {
            Debug.LogWarning("[CharacterManager] leftBridge/rightBridge/playerTower/enemyTower no asignados correctamente, el camión puede no tener ruta.");
        }

        Debug.Log($"[CharacterManager] ✓ Camión combinado creado: {partA.cardName}({partA.cardValue}) {opSymbol} {partB.cardName}({partB.cardValue}) = {result}, velocidad promedio={combinedVelocity}{combinedValue}");

        return instance;
    }


    public void AssignTag(GameObject obj, string tagName)
    {
        if (!string.IsNullOrEmpty(tagName) && obj != null)
        {
            obj.tag = tagName;
        }
    }

    public void ResolveOperation(string teamTag)
    {
        // Determinar qué IntelectManager usar según el equipo
        IntelectManager targetManager = null;

        if (teamTag == "PlayerTeam")
        {
            targetManager = playerIntelectManager;

            // Registrar operación correcta para el jugador en el ScoreManager
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.RegisterCorrectOperation(1);
                Debug.Log($"[CharacterManager] Operación correcta registrada. Total: {ScoreManager.Instance.CorrectOperations}");
            }
        }
        else if (teamTag == "AITeam")
        {
            targetManager = aiIntelectManager;
        }

        // Fallback al intelectManager genérico si no hay uno específico asignado
        if (targetManager == null)
        {
            targetManager = intelectManager;
        }

        if (targetManager != null)
        {
            Debug.Log($"[CharacterManager] ResolveOperation - Equipo: {teamTag} - Dando +1 intelecto. Intelecto actual: {targetManager.currentIntelect}");
            targetManager.AddIntelect(1);
            Debug.Log($"[CharacterManager] Después de AddIntelect(1) - Equipo: {teamTag} - Intelecto: {targetManager.currentIntelect}");
        }
        else
        {
            Debug.LogError($"[CharacterManager] ResolveOperation - No se encontró IntelectManager para el equipo: {teamTag}");
        }
    }

    public void DamageTower(Transform towerTransform, int damage)
    {
        Tower towerComp = towerTransform.GetComponent<Tower>();
        if (towerComp != null)
        {
            towerComp.TakeDamage(damage);
            Debug.Log($"[CharacterManager] DamageTower: {towerTransform.name} recibió {damage} de daño vía CharacterManager.");
        }
        else
        {
            Debug.LogWarning($"[CharacterManager] DamageTower: {towerTransform.name} no tiene componente Tower.");
        }
    }

    private void AlignModelBottomToGround(GameObject model, float targetY)
    {
        if (model == null) return;

        // Obtener solo los renderers del modelo principal, excluyendo modelos de tropas en slots
        Renderer[] allRends = model.GetComponentsInChildren<Renderer>(true);
        if (allRends == null || allRends.Length == 0)
        {
            Vector3 p = model.transform.position; p.y = targetY; model.transform.position = p;
            return;
        }

        // Filtrar renderers que NO sean parte de los modelos de tropas (FrontModel/BackModel)
        System.Collections.Generic.List<Renderer> validRends = new System.Collections.Generic.List<Renderer>();
        foreach (Renderer rend in allRends)
        {
            // Excluir renderers que estén en objetos que son modelos de tropas
            if (!rend.gameObject.name.Contains("FrontModel") &&
                !rend.gameObject.name.Contains("BackModel") &&
                !rend.gameObject.name.Contains("PH_Front") &&
                !rend.gameObject.name.Contains("PH_Back"))
            {
                validRends.Add(rend);
            }
        }

        if (validRends.Count == 0)
        {
            Vector3 p = model.transform.position; p.y = targetY; model.transform.position = p;
            return;
        }

        Bounds b = validRends[0].bounds;
        for (int i = 1; i < validRends.Count; i++) b.Encapsulate(validRends[i].bounds);

        float bottomY = b.min.y;
        float delta = (targetY + groundSnapPadding) - bottomY;
        if (Mathf.Abs(delta) > 0.0001f)
        {
            model.transform.position += new Vector3(0f, delta, 0f);
        }
    }
}