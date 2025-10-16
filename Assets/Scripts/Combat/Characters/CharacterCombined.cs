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

    private GameObject frontCharacterInstance;
    private GameObject backCharacterInstance;

    void Start()
    {
        manager = FindObjectOfType<CharacterManager>();
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

        if (agent != null && targetBridge != null)
        {
            agent.SetDestination(targetBridge.position);
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
        if (agent == null || enemyTower == null) return;

        if (!reachedBridge && targetBridge != null)
        {
            if (Vector3.Distance(transform.position, targetBridge.position) < 1f)
            {
                reachedBridge = true;
                agent.SetDestination(enemyTower.position);
            }
        }
        if (reachedBridge)
        {
            if (Vector3.Distance(transform.position, enemyTower.position) < 1.5f)
            {
                if (manager != null)
                {
                    manager.DamageEnemyTower(combinedValue);
                }
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
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
                    manager.ResolveOperation();
                }
                Destroy(other.gameObject);
                Destroy(gameObject);
            }
        }
    }

    public int GetValue() => combinedValue;
}