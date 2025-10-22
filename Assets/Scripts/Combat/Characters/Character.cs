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

    public void UpdateSpeed()
    {
        if (agent != null && GameSpeedManager.Instance != null)
        {
            agent.speed = GameSpeedManager.Instance.GetAdjustedSpeed(speed);
        }
        else if (agent != null)
        {
            agent.speed = speed;
        }
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

        // Orientar uno hacia el otro
        Vector3 directionToEnemy = enemy.transform.position - transform.position;
        directionToEnemy.y = 0; // Mantener horizontal

        if (directionToEnemy != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToEnemy);
            enemy.transform.rotation = Quaternion.LookRotation(-directionToEnemy);
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

        Debug.Log($"[Character] ⚔️ Combate finalizado - Destruyendo ambas unidades");

        // Resolver operación: dar intelecto al DEFENSOR (el otro)
        // El defensor es quien detuvo la operación enemiga
        if (manager != null)
        {
            string defenderTag = enemy.tag; // El enemigo es quien defendió
            manager.ResolveOperation(defenderTag);
            Debug.Log($"[Character] Intelecto otorgado al defensor: {defenderTag}");
        }

        // Destruir UI propia
        if (troopUIInstance != null)
        {
            Destroy(troopUIInstance.gameObject);
        }
        
        // Destruir UI del enemigo (usar variables ya declaradas al inicio)
        if (enemyChar != null && enemyChar.troopUIInstance != null)
        {
            Destroy(enemyChar.troopUIInstance.gameObject);
        }
        
        if (enemyCombined != null && enemyCombined.operationUIInstance != null)
        {
            Destroy(enemyCombined.operationUIInstance.gameObject);
        }

        // Destruir ambos personajes
        Destroy(enemy);
        Destroy(gameObject);
    }

    public int GetValue() => value;
}