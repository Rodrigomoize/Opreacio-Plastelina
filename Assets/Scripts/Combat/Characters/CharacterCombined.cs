using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CharacterCombined : MonoBehaviour
{
    private int combinedValue;
    private float velocity;

    private NavMeshAgent agent;
    private Transform targetBridge;    private Transform enemyTower;
    private bool reachedBridge = false;
    private CharacterManager manager;
    public bool isInCombat = false; // Character lo controla durante combate
    private TroopSpawnController spawnController;

    [Header("Posiciones para personajes combinados")]
    public Transform frontPosition;
    public Transform backPosition;

    public Transform AITower;
    public Transform PlayerTower;

    private GameObject frontCharacterInstance;
    private GameObject backCharacterInstance;

    [Header("UI")]
    public GameObject operationUIPrefab;
    public OperationUI operationUIInstance; // Público para acceder desde Character al destruir
    private int valueA;
    private int valueB;
    private char operatorSymbol;
    
    [Header("VFX Torre")]
    [Tooltip("Prefab del VFX cuando el Combined impacta en la torre")]
    public GameObject towerImpactVFXPrefab;
    [Tooltip("Velocidad de encogimiento antes de la explosión")]
    public float shrinkSpeed = 5f;
    [Tooltip("Offset vertical para el VFX en la torre")]
    public float towerVFXOffset = 1f;    void Start()
    {
        manager = FindFirstObjectByType<CharacterManager>();
        spawnController = GetComponent<TroopSpawnController>();
        
        if (enemyTower == null)
        {
            GameObject torreAI = GameObject.Find("TorreEnemiga");
            GameObject torrePlayer = GameObject.Find("TorrePlayer");

            if (torreAI != null) AITower = torreAI.transform;
            if (torrePlayer != null) PlayerTower = torrePlayer.transform;

            Debug.Log("[CharacterCombined] Start(): fallback towers loaded.");
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
    /// ACTUALIZACIÓN CLAVE: calcula speed final como baseSpeed * GameSpeedMultiplier * perObjectMultiplier.
    /// </summary>
    public void UpdateSpeed()
    {
        if (agent == null) return;

        float baseSpeed = velocity;
        float globalMultiplier = 1f;
        float agentMultiplier = 1f;

        if (GameSpeedManager.Instance != null)
        {
            globalMultiplier = GameSpeedManager.Instance.GameSpeedMultiplier;
            agentMultiplier = GameSpeedManager.Instance.GetAgentMultiplier(gameObject);
        }

        agent.speed = baseSpeed * globalMultiplier * agentMultiplier;
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
        if (agent == null || isInCombat) return;

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
                    Debug.LogWarning($"[CharacterCombined] {gameObject.name} cruzó puente pero enemyTower == null");
                }
            }
        }

        if (reachedBridge && enemyTower != null)
        {
            float distanciaTorre = Vector3.Distance(transform.position, enemyTower.position);
            if (distanciaTorre < 2f)
            {
                // Iniciar secuencia de impacto en torre
                StartCoroutine(TowerImpactSequence(enemyTower));
            }
        }
    }    void OnTriggerEnter(Collider other)
    {
        // Verificar si está en spawn - no puede interactuar
        if (spawnController != null && !spawnController.CanAttack()) return;
        
        if (isInCombat) return; // Ya está en combate

        // Verificar si llegó a una torre enemiga
        Tower tower = other.GetComponent<Tower>();
        if (tower != null)
        {
            bool isTowerEnemy = (gameObject.CompareTag("PlayerTeam") && other.CompareTag("AITeam")) ||
                                (gameObject.CompareTag("AITeam") && other.CompareTag("PlayerTeam"));

            if (isTowerEnemy)
            {
                Debug.Log($"[CharacterCombined] {gameObject.name} llegó a torre enemiga {other.name}, causando {combinedValue} de daño!");

                // Iniciar secuencia de impacto en torre
                StartCoroutine(TowerImpactSequence(other.transform));
                return;
            }
        }

    }

    // Métodos de acceso para Character
    public int GetValue() => combinedValue;
    public NavMeshAgent GetAgent() => agent;

    /// <summary>
    /// Secuencia de impacto: Combined se encoge y explota contra la torre
    /// </summary>
    private IEnumerator TowerImpactSequence(Transform tower)
    {
        // Detener movimiento
        if (agent != null)
        {
            agent.isStopped = true;
        }

        // Destruir UI antes de la animación
        if (operationUIInstance != null)
        {
            Destroy(operationUIInstance.gameObject);
        }

        Vector3 originalScale = transform.localScale;
        Vector3 impactPosition = tower.position + Vector3.up * towerVFXOffset;

        // Fase 1: Estiramiento inicial (anticipación)
        float stretchTime = 0.1f;
        float t = 0;
        while (t < stretchTime)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 1.2f, t / stretchTime);
            transform.localScale = originalScale * scale;
            yield return null;
        }

        // Fase 2: Encogimiento rápido
        t = 0;
        float shrinkDuration = 1f / shrinkSpeed;
        while (t < 1f)
        {
            t += Time.deltaTime * shrinkSpeed;
            
            // Encogerse
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            
            // Moverse ligeramente hacia la torre
            transform.position = Vector3.Lerp(transform.position, impactPosition, t * 0.5f);
            
            yield return null;
        }

        // Aplicar daño a la torre
        Tower towerComp = tower.GetComponent<Tower>();
        if (towerComp != null)
        {
            towerComp.TakeDamage(combinedValue);
            Debug.Log($"[CharacterCombined] Aplicado {combinedValue} de daño a {tower.name}");
        }
        else if (manager != null)
        {
            manager.DamageTower(tower, combinedValue);
            Debug.Log($"[CharacterCombined] Fallback - Aplicado {combinedValue} de daño a {tower.name}");
        }

        // Instanciar VFX de impacto en la torre
        if (towerImpactVFXPrefab != null)
        {
            Instantiate(towerImpactVFXPrefab, impactPosition, Quaternion.identity);
            Debug.Log($"[CharacterCombined] 💥 VFX de impacto en torre instanciado en {impactPosition}");
        }
        else
        {
            Debug.LogWarning("[CharacterCombined] towerImpactVFXPrefab no asignado!");
        }

        // Destruir el Combined
        Destroy(gameObject);
    }
}
