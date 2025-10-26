using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// Tipos de sonido para spawn de unidades
public enum SpawnSoundType
{
    Operation,      // Camión/CharacterCombined
    TroopValue1,    // Personaje valor 1
    TroopValue2,    // Personaje valor 2
    TroopValue3,    // Personaje valor 3
    TroopValue4,    // Personaje valor 4
    TroopValue5     // Personaje valor 5
}

/// Controla el estado de spawn de una tropa, haciéndola invulnerable y no atacable
/// durante el tiempo de spawn mientras reproduce un VFX de plastelina saliendo del suelo.
public class TroopSpawnController : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Duración del spawn en segundos")]
    public float spawnDuration = 2f;

    [Header("VFX")]
    [Tooltip("Prefab del VFX de spawn (plastelina saliendo del suelo)")]
    public GameObject spawnVFXPrefab;

    [Header("Audio")]
    [Tooltip("Tipo de sonido que reproduce esta unidad al spawnearse")]
    public SpawnSoundType soundType = SpawnSoundType.TroopValue1;

    [Header("Visual Effects")]
    [Tooltip("Escala inicial al empezar el spawn (0 = invisible)")]
    public float initialScale = 0f;
    [Tooltip("Velocidad de crecimiento durante el spawn")]
    public float growthSpeed = 2f;

    // Estado del spawn
    private bool isSpawning = true;
    private float spawnTimeRemaining;
    private Vector3 targetScale;
    private GameObject spawnVFXInstance;
    private bool hasPlayedSound = false;
    private bool hasCompletedSpawn = false;

    // Referencias
    private Character characterScript;
    private CharacterCombined characterCombined;
    private Collider characterCollider;
    private TroopUI troopUI;
    private OperationUI operationUI;
    private UnityEngine.AI.NavMeshAgent navAgent;
    private Coroutine spawnSequenceCoroutine;

    public bool IsSpawning => isSpawning;
    public float SpawnTimeRemaining => spawnTimeRemaining;
    public float SpawnProgress => 1f - (spawnTimeRemaining / spawnDuration);

    void Awake()
    {
        characterScript = GetComponent<Character>();
        characterCombined = GetComponent<CharacterCombined>();
        characterCollider = GetComponent<Collider>();
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        targetScale = transform.localScale;
        transform.localScale = targetScale * initialScale;

        if (characterCollider != null)
        {
            characterCollider.enabled = false;
        }

        spawnTimeRemaining = spawnDuration;

        // Auto-detectar tipo de sonido si no está configurado manualmente
        AutoDetectSoundType();
    }

    /// Detecta automáticamente el tipo de sonido basándose en el componente y valor
    private void AutoDetectSoundType()
    {
        // Si es un camión/operación
        if (characterCombined != null)
        {
            soundType = SpawnSoundType.Operation;
            Debug.Log($"[TroopSpawn] Auto-detectado como CAMIÓN");
            return;
        }

        // Si es una tropa normal, detectar por el valor
        if (characterScript != null)
        {
            int troopValue = characterScript.GetValue();
            string prefabName = gameObject.name.Replace("(Clone)", "").Trim();

            // Detectar por nombre del prefab o valor
            if (prefabName.Contains("1") || troopValue == 1)
            {
                soundType = SpawnSoundType.TroopValue1;
            }
            else if (prefabName.Contains("2") || troopValue == 2)
            {
                soundType = SpawnSoundType.TroopValue2;
            }
            else if (prefabName.Contains("3") || troopValue == 3)
            {
                soundType = SpawnSoundType.TroopValue3;
            }
            else if (prefabName.Contains("4") || troopValue == 4)
            {
                soundType = SpawnSoundType.TroopValue4;
            }
            else if (prefabName.Contains("5") || troopValue == 5)
            {
                soundType = SpawnSoundType.TroopValue5;
            }

            Debug.Log($"[TroopSpawn] Auto-detectado como TROPA valor {soundType} (prefab: {prefabName}, valor: {troopValue})");
        }
    }

    void Start()
    {
        if (characterScript != null && characterScript.troopUIInstance != null)
        {
            troopUI = characterScript.troopUIInstance;
        }

        if (characterCombined != null && characterCombined.operationUIInstance != null)
        {
            operationUI = characterCombined.operationUIInstance;
        }

        if (navAgent != null)
        {
            navAgent.enabled = false;
            Debug.Log($"[TroopSpawn] NavMeshAgent deshabilitado durante spawn para {gameObject.name}");
        }

        if (spawnVFXPrefab != null)
        {
            spawnVFXInstance = Instantiate(spawnVFXPrefab, transform.position, Quaternion.identity);
            spawnVFXInstance.transform.SetParent(transform);
            spawnVFXInstance.transform.localScale = Vector3.one;

            Debug.Log($"[TroopSpawn] VFX de spawn instanciado para {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[TroopSpawn] spawnVFXPrefab no asignado en {gameObject.name}");
        }

        spawnSequenceCoroutine = StartCoroutine(SpawnSequence());
    }

    void Update()
    {
        if (!isSpawning) return;

        spawnTimeRemaining -= Time.deltaTime;

        if (transform.localScale.magnitude < targetScale.magnitude)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * growthSpeed);
        }

        if (troopUI != null)
        {
            troopUI.UpdateSpawnTimer(spawnTimeRemaining);
        }

        if (operationUI != null)
        {
            operationUI.UpdateSpawnTimer(spawnTimeRemaining);
        }
    }

    private void OnDestroy()
    {
        if (spawnSequenceCoroutine != null && !hasCompletedSpawn)
        {
            StopCoroutine(spawnSequenceCoroutine);
            spawnSequenceCoroutine = null;
            Debug.Log($"[TroopSpawn] ❌ {gameObject.name} destruido ANTES de completar spawn - sonido NO reproducido");
        }
    }

    /// Secuencia de spawn: espera el tiempo definido y luego activa la tropa
    private IEnumerator SpawnSequence()
    {
        Debug.Log($"[TroopSpawn] {gameObject.name} iniciando spawn por {spawnDuration} segundos");

        yield return new WaitForSeconds(spawnDuration);

        CompleteSpawn();
    }

    /// Completa el spawn y activa la tropa
    private void CompleteSpawn()
    {
        if (hasCompletedSpawn)
        {
            Debug.LogWarning($"[TroopSpawn] ⚠️ CompleteSpawn ya fue llamado para {gameObject.name}");
            return;
        }

        hasCompletedSpawn = true;
        isSpawning = false;
        spawnTimeRemaining = 0f;
        spawnSequenceCoroutine = null;

        transform.localScale = targetScale;

        if (characterCollider != null)
        {
            characterCollider.enabled = true;
        }

        if (navAgent != null)
        {
            navAgent.enabled = true;
            Debug.Log($"[TroopSpawn] NavMeshAgent habilitado, {gameObject.name} puede moverse ahora");
        }

        if (characterScript != null)
        {
            characterScript.ResumeMovement();
        }
        else if (characterCombined != null)
        {
            characterCombined.ResumeMovement();
        }

        // 🔊 Reproducir sonido según el tipo
        PlaySpawnSound();

        if (spawnVFXInstance != null)
        {
            Destroy(spawnVFXInstance);
        }

        if (troopUI != null)
        {
            troopUI.HideSpawnTimer();
        }

        if (operationUI != null)
        {
            operationUI.HideSpawnTimer();
        }

        Debug.Log($"[TroopSpawn] ✅ {gameObject.name} spawn completado");
    }

    /// Reproduce el sonido correspondiente al tipo de unidad
    private void PlaySpawnSound()
    {
        if (hasPlayedSound)
        {
            Debug.LogWarning($"[TroopSpawn] ⚠️ Sonido ya reproducido para {gameObject.name}");
            return;
        }

        hasPlayedSound = true;

        if (AudioManager.Instance == null)
        {
            Debug.LogError("[TroopSpawn] ❌ AudioManager.Instance es NULL");
            return;
        }

        // Reproducir sonido según el tipo configurado
        switch (soundType)
        {
            case SpawnSoundType.Operation:
                AudioManager.Instance.PlayOperationCreated();
                Debug.Log($"[TroopSpawn] 🚛 Sonido de CAMIÓN reproducido");
                break;

            case SpawnSoundType.TroopValue1:
                AudioManager.Instance.PlayTroopValue1Created();
                Debug.Log($"[TroopSpawn] 1️⃣ Sonido de TROPA valor 1 reproducido");
                break;

            case SpawnSoundType.TroopValue2:
                AudioManager.Instance.PlayTroopValue2Created();
                Debug.Log($"[TroopSpawn] 2️⃣ Sonido de TROPA valor 2 reproducido");
                break;

            case SpawnSoundType.TroopValue3:
                AudioManager.Instance.PlayTroopValue3Created();
                Debug.Log($"[TroopSpawn] 3️⃣ Sonido de TROPA valor 3 reproducido");
                break;

            case SpawnSoundType.TroopValue4:
                AudioManager.Instance.PlayTroopValue4Created();
                Debug.Log($"[TroopSpawn] 4️⃣ Sonido de TROPA valor 4 reproducido");
                break;

            case SpawnSoundType.TroopValue5:
                AudioManager.Instance.PlayTroopValue5Created();
                Debug.Log($"[TroopSpawn] 5️⃣ Sonido de TROPA valor 5 reproducido");
                break;

            default:
                Debug.LogWarning($"[TroopSpawn] ⚠️ Tipo de sonido desconocido: {soundType}");
                break;
        }
    }

    /// Verifica si la tropa puede atacar (no está en spawn)
    public bool CanAttack()
    {
        return !isSpawning;
    }

    /// Verifica si la tropa puede ser atacada (no está en spawn)
    public bool CanBeAttacked()
    {
        return !isSpawning;
    }
}