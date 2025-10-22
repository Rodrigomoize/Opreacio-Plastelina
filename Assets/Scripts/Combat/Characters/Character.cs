using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Character : MonoBehaviour
{
    private int value;
    private int life;
    private float speed;
    private bool isDefender;
    private NavMeshAgent agent;
    private Transform targetBridge;
    private Transform enemyTower;
    private bool reachedBridge = false;
    private CharacterManager manager;
    public bool isInCombat = false;
    private Animator animator;

    [Header("UI")]
    public GameObject troopUIPrefab;
    public TroopUI troopUIInstance; // Público para acceder desde otros scripts al destruir

    [Header("Combate")]
    [Tooltip("Duración de la animación de combate en segundos")]
    public float combatDuration = 1.1f; // Duración del ataque
    [Tooltip("Velocidad reducida durante el combate (multiplicador, 0.1 = 10% velocidad original)")]
    public float combatSpeedMultiplier = 0.1f;

    [Header("VFX de Impacto")]
    [Tooltip("Prefab del VFX de impacto/splash que aparece al destruirse")]
    public GameObject vfxImpactPrefab;
    [Tooltip("Velocidad de la implosión (tropas acercándose)")]
    public float implosionSpeed = 3f;
    [Tooltip("Escala inicial antes de la implosión")]
    public float preImplosionScale = 1.2f;
    
    [Header("Feedback de Intelecto")]
    [Tooltip("Prefab del feedback +1 Intelecto que aparece al resolver una operación correctamente")]
    public GameObject intellectFeedbackPrefab;
    [Tooltip("Offset vertical donde aparece el feedback")]
    public float feedbackYOffset = 2f;

    void Start()
    {
        manager = FindFirstObjectByType<CharacterManager>();
        EnsureColliderSetup();
        animator = GetComponent<Animator>();
    }

    private void EnsureColliderSetup()
    {
        SphereCollider sphereCol = GetComponent<SphereCollider>();

        if (sphereCol == null)
        {
            sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.radius = 0.5f;
            Debug.Log($"[Character] Añadido SphereCollider a {gameObject.name}");
        }

        sphereCol.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.Log($"[Character] Añadido Rigidbody a {gameObject.name}");
        }

        rb.isKinematic = true;
        rb.useGravity = false;

        Debug.Log($"[Character] {gameObject.name} - Collider configurado: trigger={sphereCol.isTrigger}, radius={sphereCol.radius}");
    }

    public void InitializeCharacter(int val, int hp, float spd, bool defender)
    {
        value = val;
        life = hp;
        speed = spd;
        isDefender = defender;

        Debug.Log($"[Character] Inicializado: {gameObject.name}, valor={value}, velocidad={speed}, tag={gameObject.tag}");
    }

    public void SetupMovement(NavMeshAgent navAgent, Transform bridge, Transform tower, bool isDefenderUnit)
    {
        agent = navAgent;
        targetBridge = bridge;
        enemyTower = tower;
        isDefender = isDefenderUnit;

        if (agent != null)
        {
            UpdateSpeed();
            Debug.Log($"[Character] {gameObject.name} - NavMeshAgent.speed configurado a {agent.speed}");

            if (targetBridge != null)
            {
                agent.SetDestination(targetBridge.position);
                Debug.Log($"[Character] {gameObject.name} navegando hacia puente {targetBridge.name}");
            }
        }

        // Crear UI de la tropa
        CreateTroopUI();
    }

    /// <summary>
    /// ACTUALIZACIÓN CLAVE: calcula speed final como baseSpeed * GameSpeedMultiplier * perObjectMultiplier.
    /// </summary>
    public void UpdateSpeed()
    {
        if (agent == null) return;

        float baseSpeed = speed;
        float globalMultiplier = 1f;
        float agentMultiplier = 1f;

        if (GameSpeedManager.Instance != null)
        {
            globalMultiplier = GameSpeedManager.Instance.GameSpeedMultiplier;
            agentMultiplier = GameSpeedManager.Instance.GetAgentMultiplier(gameObject);
        }

        agent.speed = baseSpeed * globalMultiplier * agentMultiplier;
    }

    /// <summary>
    /// Crea la UI flotante de la tropa mostrando su valor
    /// </summary>
    private void CreateTroopUI()
    {
        if (troopUIPrefab != null)
        {
            GameObject uiObj = Instantiate(troopUIPrefab);
            troopUIInstance = uiObj.GetComponent<TroopUI>();
            if (troopUIInstance != null)
            {
                // Pasar el tag del equipo para que use el sprite correcto
                troopUIInstance.Initialize(transform, value, gameObject.tag);
                Debug.Log($"[Character] UI creada para {gameObject.name} con valor {value}, equipo: {gameObject.tag}");
            }
        }
        else
        {
            Debug.LogWarning($"[Character] troopUIPrefab no asignado para {gameObject.name}");
        }
    }

    void Update()
    {
        if (agent == null || enemyTower == null) return;

        // Actualizar animación de caminar basándose en la velocidad del agente
        // SOLO cuando NO está en combate (durante combate, CombatSequence controla las animaciones)
        if (animator != null && !isInCombat)
        {
            bool isMoving = agent.velocity.magnitude > 0.05f;
            animator.SetBool("isWalking", isMoving);
        }

        if (isInCombat) return;

        if (!reachedBridge && targetBridge != null)
        {
            if (Vector3.Distance(transform.position, targetBridge.position) < 1f)
            {
                reachedBridge = true;
                agent.SetDestination(enemyTower.position);
                Debug.Log($"[Character] {gameObject.name} cruzó el puente, yendo a torre");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isInCombat) return; // Ya está en combate

        Debug.Log($"[Character] {gameObject.name} (tag:{gameObject.tag}, valor:{value}) TRIGGER con {other.gameObject.name} (tag:{other.tag})");

        // Verificar si llegó a una torre enemiga
        Tower tower = other.GetComponent<Tower>();
        if (tower != null)
        {
            bool isTowerEnemy = (gameObject.CompareTag("PlayerTeam") && other.CompareTag("AITeam")) ||
                                (gameObject.CompareTag("AITeam") && other.CompareTag("PlayerTeam"));
            if (isTowerEnemy)
            {
                // Destruir UI antes de destruir la tropa
                if (troopUIInstance != null)
                {
                    Destroy(troopUIInstance.gameObject);
                }
                Destroy(gameObject);
                return;
            }
        }

        if (other.CompareTag("AITeam") || other.CompareTag("PlayerTeam"))
        {
            if (other.tag == gameObject.tag)
            {
                Debug.Log($"[Character] Mismo equipo, ignorando");
                return;
            }

            Character otherChar = other.GetComponent<Character>();
            CharacterCombined otherCombined = other.GetComponent<CharacterCombined>();

            // Evitar combate si el otro ya está en combate
            if (otherChar != null && otherChar.isInCombat) return;
            if (otherCombined != null && otherCombined.isInCombat) return;

            int otherValue = 0;

            if (otherChar != null)
            {
                otherValue = otherChar.value;
                Debug.Log($"[Character] Detectado otro Character con valor {otherValue}");
            }
            else if (otherCombined != null)
            {
                otherValue = otherCombined.GetValue();
                Debug.Log($"[Character] Detectado CharacterCombined con valor {otherValue}");
            }
            else
            {
                Debug.Log($"[Character] El otro objeto no tiene Character ni CharacterCombined");
                return;
            }

            if (value == otherValue)
            {
                Debug.Log($"[Character] ⚔️ COMBATE: {value} == {otherValue} - Iniciando animación!");

                // Marcar ambos como en combate
                isInCombat = true;
                if (otherChar != null) otherChar.isInCombat = true;
                if (otherCombined != null) otherCombined.isInCombat = true;

                // Iniciar corrutina de combate
                StartCoroutine(CombatSequence(other.gameObject));
            }
            else
            {
                Debug.Log($"[Character] Valores diferentes ({value} vs {otherValue}), continúan su camino");
            }
        }
    }

    /// <summary>
    /// Secuencia de combate: Character controla TODO (velocidades, animación, destrucción)
    /// </summary>
    private IEnumerator CombatSequence(GameObject enemy)
    {
        // Identificar el tipo de enemigo
        Character enemyChar = enemy.GetComponent<Character>();
        CharacterCombined enemyCombined = enemy.GetComponent<CharacterCombined>();

        // Reducir velocidad propia
        float originalSpeed = 0f;
        if (agent != null)
        {
            originalSpeed = agent.speed;
            agent.speed = originalSpeed * combatSpeedMultiplier;
            Debug.Log($"[Character] {gameObject.name} velocidad reducida: {originalSpeed} → {agent.speed}");
        }

        // Reducir velocidad del enemigo (Character o CharacterCombined)
        float enemyOriginalSpeed = 0f;
        NavMeshAgent enemyAgent = null;

        if (enemyChar != null)
        {
            enemyAgent = enemyChar.GetComponent<NavMeshAgent>();
        }
        else if (enemyCombined != null)
        {
            enemyAgent = enemyCombined.GetAgent();
        }

        if (enemyAgent != null)
        {
            enemyOriginalSpeed = enemyAgent.speed;
            enemyAgent.speed = enemyOriginalSpeed * combatSpeedMultiplier;
            Debug.Log($"[Character] {enemy.name} velocidad reducida: {enemyOriginalSpeed} → {enemyAgent.speed}");
        }

        // Orientar el Character hacia el enemigo
        Vector3 directionToEnemy = enemy.transform.position - transform.position;
        directionToEnemy.y = 0; // Mantener horizontal

        if (directionToEnemy != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToEnemy);

            // Solo rotar al enemigo si es otro Character (no Combined)
            // Combined mantiene su orientación hacia la torre
            if (enemyChar != null)
            {
                enemy.transform.rotation = Quaternion.LookRotation(-directionToEnemy);
            }
        }

        Debug.Log($"[Character] ⚔️ Combate iniciado entre {gameObject.name} y {enemy.name} - {combatDuration} segundo(s) de animación con velocidad reducida");

        // Reproducir animación de ataque (SOLO LA TROPA ataca, no las operaciones)
        if (animator != null)
        {
            animator.SetTrigger("Attack");
            animator.SetBool("isWalking", false); // Asegurar que no esté en estado de caminar
            Debug.Log($"[Character] Trigger 'Attack' activado en {gameObject.name}");
        }

        // Esperar el tiempo de combate (1.1 segundos para completar la animación de ataque)
        yield return new WaitForSeconds(combatDuration);

        Debug.Log($"[Character] ⚔️ Combate finalizado - Iniciando implosión");

        // Detectar si se resolvió correctamente una operación
        // Si un Character derrota a un Combined enemigo = operación resuelta correctamente
        bool isPlayerResolving = gameObject.CompareTag("PlayerTeam") && enemyCombined != null && enemyCombined.CompareTag("AITeam");
        bool isAIResolving = gameObject.CompareTag("AITeam") && enemyCombined != null && enemyCombined.CompareTag("PlayerTeam");
        bool operationResolved = isPlayerResolving || isAIResolving;

        // Resolver operación: dar intelecto al que ATACÓ (este Character que resolvió la operación)
        if (manager != null)
        {
            string attackerTag = gameObject.tag; // El que atacó es quien gana el intelecto
            manager.ResolveOperation(attackerTag);
            Debug.Log($"[Character] Intelecto otorgado al atacante (quien resolvió): {attackerTag}");
        }

        // Destruir UIs antes de la implosión
        if (troopUIInstance != null)
        {
            Destroy(troopUIInstance.gameObject);
        }

        if (enemyChar != null && enemyChar.troopUIInstance != null)
        {
            Destroy(enemyChar.troopUIInstance.gameObject);
        }

        if (enemyCombined != null && enemyCombined.operationUIInstance != null)
        {
            Destroy(enemyCombined.operationUIInstance.gameObject);
        }

        // ===== IMPLOSIÓN Y VFX =====
        yield return StartCoroutine(ImplodeAndExplode(enemy, enemyChar, enemyCombined, operationResolved));
    }

    /// <summary>
    /// Efecto de implosión: ambas unidades se atraen, encogen y explotan con VFX
    /// </summary>
    private IEnumerator ImplodeAndExplode(GameObject enemy, Character enemyChar, CharacterCombined enemyCombined, bool operationResolved)
    {
        Vector3 startPos = transform.position;
        Vector3 enemyStartPos = enemy.transform.position;
        Vector3 midPoint = (startPos + enemyStartPos) / 2f;

        Vector3 originalScale = transform.localScale;
        Vector3 enemyOriginalScale = enemy.transform.localScale;

        // Efecto de estiramiento inicial (anticipación)
        transform.localScale = originalScale * preImplosionScale;
        enemy.transform.localScale = enemyOriginalScale * preImplosionScale;
        yield return new WaitForSeconds(0.05f);

        // Implosión: ambas unidades se acercan al punto medio y se encogen
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * implosionSpeed;
            float easeT = Mathf.SmoothStep(0, 1, t); // Suavizado

            // Ambas se mueven al punto medio
            transform.position = Vector3.Lerp(startPos, midPoint, easeT);
            enemy.transform.position = Vector3.Lerp(enemyStartPos, midPoint, easeT);

            // Ambas se encogen
            float scaleT = t * 1.2f; // Encogimiento ligeramente más rápido
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, scaleT);
            enemy.transform.localScale = Vector3.Lerp(enemyOriginalScale, Vector3.zero, scaleT);

            yield return null;
        }

        // Instanciar VFX de impacto en el punto medio
        if (vfxImpactPrefab != null)
        {
            Instantiate(vfxImpactPrefab, midPoint, Quaternion.identity);
            Debug.Log($"[Character] 💥 VFX de impacto instanciado en {midPoint}");
        }
        else
        {
            Debug.LogWarning("[Character] vfxImpactPrefab no asignado!");
        }

        // Mostrar feedback visual DESPUÉS del VFX si se resolvió una operación correctamente
        if (operationResolved && intellectFeedbackPrefab != null)
        {
            Vector3 feedbackPosition = midPoint + Vector3.up * feedbackYOffset;
            IntellectFeedback.Create(intellectFeedbackPrefab, feedbackPosition);
            Debug.Log($"[Character] ✅ ¡Operación resuelta correctamente! Mostrando feedback +1 Intelecto");
        }

        // Destruir ambas unidades
        Destroy(enemy);
        Destroy(gameObject);
    }

    public int GetValue() => value;
}
