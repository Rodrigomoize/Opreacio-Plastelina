using UnityEngine;
using UnityEngine.AI;

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
    
    [Header("UI")]
    public GameObject troopUIPrefab; // Prefab del UI de la tropa
    private TroopUI troopUIInstance;

    void Start()
    {

        manager = FindObjectOfType<CharacterManager>();

        // CRÍTICO: Asegurar que tiene collider trigger
        EnsureColliderSetup();
    }

    private void EnsureColliderSetup()
    {
        // Buscar si ya tiene un SphereCollider
        SphereCollider sphereCol = GetComponent<SphereCollider>();

        if (sphereCol == null)
        {
            // Si no existe, crearlo
            sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.radius = 0.5f; // Ajusta según el tamaño de tus personajes
            Debug.Log($"[Character] Añadido SphereCollider a {gameObject.name}");
        }

        // ASEGURAR que está en modo trigger
        sphereCol.isTrigger = true;

        // CRÍTICO: Asegurar que NO tiene Rigidbody o está en modo kinematic
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.Log($"[Character] Añadido Rigidbody a {gameObject.name}");
        }

        // Configurar para que no interfiera con NavMeshAgent
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

        Debug.Log($"[Character] Inicializado: {gameObject.name}, valor={value}, tag={gameObject.tag}");
    }

    public void SetupMovement(NavMeshAgent navAgent, Transform bridge, Transform tower, bool isDefenderUnit)
    {
        agent = navAgent;
        targetBridge = bridge;
        enemyTower = tower;
        isDefender = isDefenderUnit;

        if (agent != null && targetBridge != null)
        {
            agent.SetDestination(targetBridge.position);
            Debug.Log($"[Character] {gameObject.name} navegando hacia puente {targetBridge.name}");
        }
    }

    void Update()
    {
        if (agent == null || enemyTower == null) return;

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
        Debug.Log($"[Character] {gameObject.name} (tag:{gameObject.tag}, valor:{value}) TRIGGER con {other.gameObject.name} (tag:{other.tag})");

        // Solo comparar con el equipo contrario
        if (other.CompareTag("AITeam") || other.CompareTag("PlayerTeam"))
        {
            // Evitar compararse con su propio equipo
            if (other.tag == gameObject.tag)
            {
                Debug.Log($"[Character] Mismo equipo, ignorando");
                return;
            }

            // Verificar si es Character o CharacterCombined
            Character otherChar = other.GetComponent<Character>();
            CharacterCombined otherCombined = other.GetComponent<CharacterCombined>();

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

            // Comparar valores
            if (value == otherValue)
            {
                Debug.Log($"[Character] ⚔️ COMBATE: {value} == {otherValue} - Ambos se destruyen!");
                if (manager != null)
                {
                    manager.ResolveOperation();
                }
                
                // Destruir el UI junto con el personaje
                if (troopUIInstance != null)
                {
                    Destroy(troopUIInstance.gameObject);
                }
                
                Destroy(other.gameObject);
                Destroy(gameObject);
            }
            else
            {
                Debug.Log($"[Character] Valores diferentes ({value} vs {otherValue}), continúan su camino");
            }
        }
    }

    public int GetValue() => value;
}