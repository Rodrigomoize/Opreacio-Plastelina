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
        manager = FindFirstObjectByType<CharacterManager>();
        
        // Crear el UI de la tropa si hay prefab asignado
        if (troopUIPrefab != null)
        {
            GameObject uiObj = Instantiate(troopUIPrefab);
            troopUIInstance = uiObj.GetComponent<TroopUI>();
            if (troopUIInstance != null)
            {
                // Pasar el tag del equipo para usar el sprite correcto
                troopUIInstance.Initialize(transform, value, gameObject.tag);
            }
        }
    }

    public void InitializeCharacter(int val, int hp, float spd, bool defender)
    {
        value = val;
        life = hp;
        speed = spd;
        isDefender = defender;
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
        }
    }

    void Update()
    {
        if (agent == null || enemyTower == null) return;

        // Si a�n no lleg� al puente
        if (!reachedBridge && targetBridge != null)
        {
            if (Vector3.Distance(transform.position, targetBridge.position) < 1f)
            {
                reachedBridge = true;
                agent.SetDestination(enemyTower.position);
            }
        }

        // Si lleg� a la torre enemiga (solo defenders se destruyen)
        if (reachedBridge && isDefender)
        {
            if (Vector3.Distance(transform.position, enemyTower.position) < 1.5f)
            {
                Debug.Log($"[Character] Defender {value} lleg� a torre y se destruye");
                
                // Destruir el UI junto con el personaje
                if (troopUIInstance != null)
                {
                    Destroy(troopUIInstance.gameObject);
                }
                
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Solo comparar con el equipo contrario
        if (other.CompareTag("AITeam") || other.CompareTag("PlayerTeam"))
        {
            // Evitar compararse con su propio equipo
            if (other.tag == gameObject.tag) return;

            // Verificar si es Character o CharacterCombined
            Character otherChar = other.GetComponent<Character>();
            CharacterCombined otherCombined = other.GetComponent<CharacterCombined>();

            int otherValue = 0;

            if (otherChar != null)
            {
                otherValue = otherChar.value;
            }
            else if (otherCombined != null)
            {
                otherValue = otherCombined.GetValue();
            }
            else
            {
                return; // No es un personaje v�lido
            }

            // Comparar valores
            if (value == otherValue)
            {
                Debug.Log($"[Character] {value} == {otherValue} - Ambos se destruyen");
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
            // Si no son iguales, no pasa nada (siguen su camino)
        }
    }

    public int GetValue() => value;
}