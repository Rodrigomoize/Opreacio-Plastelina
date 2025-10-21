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
    public GameObject troopUIPrefab;
    private TroopUI troopUIInstance;

    void Start()
    {
        manager = FindObjectOfType<CharacterManager>();
        EnsureColliderSetup();
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
            agent.speed = speed;
            Debug.Log($"[Character] {gameObject.name} - NavMeshAgent.speed configurado a {agent.speed}");

            if (targetBridge != null)
            {
                agent.SetDestination(targetBridge.position);
                Debug.Log($"[Character] {gameObject.name} navegando hacia puente {targetBridge.name}");
            }
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

        if (other.CompareTag("AITeam") || other.CompareTag("PlayerTeam"))
        {
            if (other.tag == gameObject.tag)
            {
                Debug.Log($"[Character] Mismo equipo, ignorando");
                return;
            }

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

            if (value == otherValue)
            {
                Debug.Log($"[Character] ⚔️ COMBATE: {value} == {otherValue} - Ambos se destruyen!");
                if (manager != null)
                {
                    manager.ResolveOperation();
                }

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