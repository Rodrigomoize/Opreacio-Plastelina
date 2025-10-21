using UnityEngine;
using UnityEngine.AI;

public class CharacterCombined : MonoBehaviour
{
    private int combinedValue;
    private float velocity;

    private NavMeshAgent agent;
    private Transform targetBridge;
    private Transform enemyTower;
    private bool reachedBridge = false;
    private CharacterManager manager;

    [Header("Posiciones para personajes combinados")]
    public Transform frontPosition;
    public Transform backPosition;

    public Transform AITower;
    public Transform PlayerTower;

    private GameObject frontCharacterInstance;
    private GameObject backCharacterInstance;

    [Header("UI")]
    public GameObject operationUIPrefab;
    private OperationUI operationUIInstance;
    private int valueA;
    private int valueB;
    private char operatorSymbol;

    void Start()
    {
        manager = FindFirstObjectByType<CharacterManager>();
        if (enemyTower == null)
        {
            GameObject torreAI = GameObject.Find("TorreEnemiga");
            GameObject torrePlayer = GameObject.Find("TorrePlayer");

            if (torreAI != null) AITower = torreAI.transform;
            if (torrePlayer != null) PlayerTower = torrePlayer.transform;

            Debug.Log("[CharacterCombined] Start(): fallback towers loaded (no asignadas a enemyTower si ya venían por param).");
        }
    }

    public void InitializeCombined(int value, float speed)
    {
        combinedValue = value;
        velocity = speed;

        Debug.Log($"[CharacterCombined] Inicializado: valor={combinedValue}, velocidad={velocity}");
    }

    public void SetOperationValues(int valA, int valB, char op)
    {
        valueA = valA;
        valueB = valB;
        operatorSymbol = op;

        if (operationUIPrefab != null)
        {
            GameObject uiObj = Instantiate(operationUIPrefab);
            operationUIInstance = uiObj.GetComponent<OperationUI>();
            if (operationUIInstance != null)
            {
                operationUIInstance.Initialize(transform, valueA, valueB, operatorSymbol, gameObject.tag);
            }
        }
    }

    public void SetupMovement(NavMeshAgent navAgent, Transform bridge, Transform tower)
    {
        agent = navAgent;
        targetBridge = bridge;
        enemyTower = tower;

        
        if (agent != null)
        {
            UpdateSpeed();
            Debug.Log($"[CharacterCombined] {gameObject.name} - NavMeshAgent.speed configurado a {agent.speed}");

            if (targetBridge != null)
            {
                agent.SetDestination(targetBridge.position);
                Debug.Log($"[CharacterCombined] Navegando hacia puente: {targetBridge.name}");
            }
            else
            {
                Debug.LogError($"[CharacterCombined] targetBridge es null en {gameObject.name}!");
            }
        }
        else
        {
            Debug.LogError($"[CharacterCombined] Agent es null en {gameObject.name}!");
        }
    }

    /// <summary>
    /// Actualiza la velocidad del agente aplicando el multiplicador de velocidad de juego
    /// </summary>
    public void UpdateSpeed()
    {
        if (agent != null && GameSpeedManager.Instance != null)
        {
            agent.speed = GameSpeedManager.Instance.GetAdjustedSpeed(velocity);
        }
        else if (agent != null)
        {
            // Si no hay GameSpeedManager, usar velocidad base
            agent.speed = velocity;
        }
    }

    public void SetupCharacterModels(GameObject frontModel, GameObject backModel)
    {
        if (frontPosition != null && frontModel != null)
        {
            frontCharacterInstance = Instantiate(frontModel, frontPosition.position, frontPosition.rotation, frontPosition);
            frontCharacterInstance.transform.localPosition = Vector3.zero;
            frontCharacterInstance.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("[CharacterCombined] frontPosition o frontModel es null");
        }

        if (backPosition != null && backModel != null)
        {
            backCharacterInstance = Instantiate(backModel, backPosition.position, backPosition.rotation, backPosition);
            backCharacterInstance.transform.localPosition = Vector3.zero;
            backCharacterInstance.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("[CharacterCombined] backPosition o backModel es null");
        }
    }

    void Update()
    {
        if (agent == null) return;

        if (!reachedBridge && targetBridge != null)
        {
            float distanciaPuente = Vector3.Distance(transform.position, targetBridge.position);
            if (distanciaPuente < 2f)
            {
                reachedBridge = true;

                if (enemyTower != null)
                {
                    agent.SetDestination(enemyTower.position);
                    Debug.Log($"[CharacterCombined] {gameObject.name} cruzó el puente, yendo a {enemyTower.name}");
                }
                else
                {
                    Debug.LogWarning($"[CharacterCombined] {gameObject.name} cruzó puente pero enemyTower == null -> esperará que se asigne.");
                }
            }
        }

        if (reachedBridge && enemyTower != null)
        {
            float distanciaTorre = Vector3.Distance(transform.position, enemyTower.position);
            if (distanciaTorre < 2f)
            {
                Tower towerComp = enemyTower.GetComponent<Tower>();
                if (towerComp != null)
                {
                    towerComp.TakeDamage(combinedValue);
                    Debug.Log($"[CharacterCombined] Aplicado {combinedValue} de daño directamente a {enemyTower.name}");
                }
                else if (manager != null)
                {
                    manager.DamageTower(enemyTower, combinedValue);
                    Debug.Log($"[CharacterCombined] Fallback a CharacterManager para dañar {enemyTower.name} por {combinedValue}");
                }
                else
                {
                    Debug.LogWarning("[CharacterCombined] No se pudo aplicar daño: no hay Tower ni CharacterManager.");
                }

                if (operationUIInstance != null) Destroy(operationUIInstance.gameObject);

                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Verificar si llegó a una torre enemiga
        Tower tower = other.GetComponent<Tower>();
        if (tower != null)
        {
            // Verificar que es torre enemiga (tag diferente al nuestro)
            bool isTowerEnemy = (gameObject.CompareTag("PlayerTeam") && other.CompareTag("AITeam")) ||
                                (gameObject.CompareTag("AITeam") && other.CompareTag("PlayerTeam"));
            
            if (isTowerEnemy)
            {
                Debug.Log($"[CharacterCombined] {gameObject.name} llegó a torre enemiga {other.name}, causando {combinedValue} de daño!");
                
                if (manager != null)
                {
                    manager.DamageTower(other.transform, combinedValue);
                }
                
                // Destruir el camión después de atacar
                if (operationUIInstance != null)
                {
                    Destroy(operationUIInstance.gameObject);
                }
                Destroy(gameObject);
                return;
            }
        }
        
        if (other.CompareTag("AITeam") || other.CompareTag("PlayerTeam"))
        {
            if (other.tag == gameObject.tag) return;

            Character otherChar = other.GetComponent<Character>();
            CharacterCombined otherCombined = other.GetComponent<CharacterCombined>();

            int otherValue = 0;

            if (otherChar != null)
            {
                otherValue = otherChar.GetValue();
            }
            else if (otherCombined != null)
            {
                otherValue = otherCombined.combinedValue;
            }
            else
            {
                return;
            }

            if (combinedValue == otherValue)
            {
                Debug.Log($"[CharacterCombined] {combinedValue} == {otherValue} - Ambos se destruyen");
                if (manager != null)
                {
                    // Pasar el tag del equipo que hizo la operación (este gameObject)
                    manager.ResolveOperation(gameObject.tag);
                }

                if (operationUIInstance != null)
                {
                    Destroy(operationUIInstance.gameObject);
                }

                Destroy(other.gameObject);
                Destroy(gameObject);
            }
        }
    }

    public int GetValue() => combinedValue;
}