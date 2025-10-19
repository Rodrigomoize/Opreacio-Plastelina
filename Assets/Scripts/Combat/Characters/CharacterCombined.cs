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

    void Start()
    {
        manager = FindObjectOfType<CharacterManager>();
        if (enemyTower == null)
        {
            GameObject torreAI = GameObject.Find("TorreEnemiga");
            GameObject torrePlayer = GameObject.Find("TorrePlayer");

            if (torreAI != null) AITower = torreAI.transform;
            if (torrePlayer != null) PlayerTower = torrePlayer.transform;

            // NO sobreescribimos enemyTower automáticamente aquí.
            Debug.Log("[CharacterCombined] Start(): fallback towers loaded (no asignadas a enemyTower si ya venían por param).");
        }
    }

    public void InitializeCombined(int value, float speed)
    {
        combinedValue = value;
        velocity = speed;
    }

    public void SetupMovement(NavMeshAgent navAgent, Transform bridge, Transform tower)
    {
        agent = navAgent;
        targetBridge = bridge;

        enemyTower = tower;

        Debug.Log($"[CharacterCombined] SetupMovement() llamado en {gameObject.name} (tag:{gameObject.tag}) -> bridge:{targetBridge?.name} enemyTower:{enemyTower?.name}");

        if (agent != null && targetBridge != null)
        {
            agent.SetDestination(targetBridge.position);
            Debug.Log($"[CharacterCombined] Navegando hacia puente: {targetBridge.name}");
        }
        else
        {
            Debug.LogError($"[CharacterCombined] Agent o targetBridge son null en {gameObject.name}!");
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
        if (agent == null) return; // solo esto

        // FASE 1: Ir al puente
        if (!reachedBridge && targetBridge != null)
        {
            float distanciaPuente = Vector3.Distance(transform.position, targetBridge.position);
            if (distanciaPuente < 2f)
            {
                reachedBridge = true;
                // si enemyTower ya está asignada, cambiar destino; si no, esperar a que se asigne
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
                Debug.Log($"[CharacterCombined] {gameObject.name} llegó a torre, haciendo {combinedValue} de daño");
                if (manager != null) manager.DamageEnemyTower(combinedValue);
                Destroy(gameObject);
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AITeam") || other.CompareTag("PlayerTeam"))
        {
            // No colisionar con mi propio equipo
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

            // Si los valores son iguales, ambos se destruyen
            if (combinedValue == otherValue)
            {
                Debug.Log($"[CharacterCombined] {combinedValue} == {otherValue} - Ambos se destruyen");
                if (manager != null)
                {
                    manager.ResolveOperation();
                }
                Destroy(other.gameObject);
                Destroy(gameObject);
            }
        }
    }

    public int GetValue() => combinedValue;
}