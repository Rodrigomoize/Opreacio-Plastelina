using UnityEngine;
using System.Collections;

/// <summary>
/// Controla el estado de spawn de una tropa, haciéndola invulnerable y no atacable
/// durante el tiempo de spawn mientras reproduce un VFX de plastelina saliendo del suelo.
/// </summary>
public class TroopSpawnController : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Duración del spawn en segundos")]
    public float spawnDuration = 2f;
    
    [Header("VFX")]
    [Tooltip("Prefab del VFX de spawn (plastelina saliendo del suelo)")]
    public GameObject spawnVFXPrefab;
    
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
    
    // Referencias
    private Character characterScript;
    private Collider characterCollider;
    private TroopUI troopUI;
    
    public bool IsSpawning => isSpawning;
    public float SpawnTimeRemaining => spawnTimeRemaining;
    public float SpawnProgress => 1f - (spawnTimeRemaining / spawnDuration);
    
    void Awake()
    {
        characterScript = GetComponent<Character>();
        characterCollider = GetComponent<Collider>();
        
        // Guardar la escala objetivo
        targetScale = transform.localScale;
        
        // Iniciar con escala pequeña
        transform.localScale = targetScale * initialScale;
        
        // Deshabilitar collider durante spawn
        if (characterCollider != null)
        {
            characterCollider.enabled = false;
        }
        
        spawnTimeRemaining = spawnDuration;
    }
    
    void Start()
    {
        // Obtener referencia al TroopUI (se crea en Character.Start)
        if (characterScript != null && characterScript.troopUIInstance != null)
        {
            troopUI = characterScript.troopUIInstance;
        }
        
        // Instanciar VFX de spawn
        if (spawnVFXPrefab != null)
        {
            spawnVFXInstance = Instantiate(spawnVFXPrefab, transform.position, Quaternion.identity);
            spawnVFXInstance.transform.SetParent(transform); // Hacer hijo para que siga a la tropa
            Debug.Log($"[TroopSpawn] VFX de spawn instanciado para {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[TroopSpawn] spawnVFXPrefab no asignado en {gameObject.name}");
        }
        
        StartCoroutine(SpawnSequence());
    }
    
    void Update()
    {
        if (!isSpawning) return;
        
        // Actualizar tiempo restante
        spawnTimeRemaining -= Time.deltaTime;
        
        // Animar crecimiento de la tropa
        if (transform.localScale.magnitude < targetScale.magnitude)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * growthSpeed);
        }
        
        // Actualizar UI con tiempo restante
        if (troopUI != null)
        {
            troopUI.UpdateSpawnTimer(spawnTimeRemaining);
        }
    }
    
    /// <summary>
    /// Secuencia de spawn: espera el tiempo definido y luego activa la tropa
    /// </summary>
    private IEnumerator SpawnSequence()
    {
        Debug.Log($"[TroopSpawn] {gameObject.name} iniciando spawn por {spawnDuration} segundos");
        
        // Esperar el tiempo de spawn
        yield return new WaitForSeconds(spawnDuration);
        
        // Finalizar spawn
        CompleteSpawn();
    }
    
    /// <summary>
    /// Completa el spawn y activa la tropa
    /// </summary>
    private void CompleteSpawn()
    {
        isSpawning = false;
        spawnTimeRemaining = 0f;
        
        // Asegurar escala completa
        transform.localScale = targetScale;
        
        // Habilitar collider
        if (characterCollider != null)
        {
            characterCollider.enabled = true;
        }
        
        // Destruir VFX de spawn
        if (spawnVFXInstance != null)
        {
            Destroy(spawnVFXInstance);
        }
        
        // Ocultar temporizador en UI
        if (troopUI != null)
        {
            troopUI.HideSpawnTimer();
        }
        
        Debug.Log($"[TroopSpawn] {gameObject.name} spawn completado, ahora está activo");
    }
    
    /// <summary>
    /// Verifica si la tropa puede atacar (no está en spawn)
    /// </summary>
    public bool CanAttack()
    {
        return !isSpawning;
    }
    
    /// <summary>
    /// Verifica si la tropa puede ser atacada (no está en spawn)
    /// </summary>
    public bool CanBeAttacked()
    {
        return !isSpawning;
    }
}
