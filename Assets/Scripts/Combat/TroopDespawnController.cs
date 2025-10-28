using UnityEngine;
using System.Collections;

/// <summary>
/// Controla el efecto de despawn (muerte) de tropas cuando cruzan el río.
/// Efecto inverso al spawn: la tropa se reduce de tamaño mientras aparece un charco en el suelo.
/// 
/// IMPORTANTE: Este componente debe estar PRE-CONFIGURADO en el prefab de cada tropa.
/// Asignar un VFX de charco específico para cada tipo de tropa.
/// </summary>
public class TroopDespawnController : MonoBehaviour
{
    [Header("Despawn Settings")]
    [Tooltip("Duración del efecto de despawn en segundos")]
    public float despawnDuration = 1f;

    [Header("VFX")]
    [Tooltip("Prefab del VFX de charco/salpicadura específico para esta tropa")]
    public GameObject puddleVFXPrefab;

    [Header("Visual Effects")]
    [Tooltip("Velocidad de reducción durante el despawn")]
    public float shrinkSpeed = 3f;

    private bool isDespawning = false;
    private float despawnTimeRemaining;
    private Vector3 originalScale;
    private GameObject puddleVFXInstance;
    
    // Referencias
    private Character characterScript;
    private Collider characterCollider;
    private UnityEngine.AI.NavMeshAgent navAgent;
    private TroopUI troopUI;

    public bool IsDespawning => isDespawning;

    /// <summary>
    /// Inicia el proceso de despawn
    /// </summary>
    public void StartDespawn()
    {
        if (isDespawning) return; // Ya está en proceso de despawn

        isDespawning = true;
        despawnTimeRemaining = despawnDuration;
        originalScale = transform.localScale;

        // Obtener referencias
        characterScript = GetComponent<Character>();
        characterCollider = GetComponent<Collider>();
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        
        if (characterScript != null)
        {
            troopUI = characterScript.troopUIInstance;
        }

        // Desactivar combate inmediatamente
        if (characterScript != null)
        {
            characterScript.isInCombat = false;
        }

        // Detener movimiento
        if (navAgent != null && navAgent.enabled)
        {
            navAgent.isStopped = true;
            navAgent.velocity = Vector3.zero;
        }

        // Desactivar collider para que no pueda atacar torres
        if (characterCollider != null)
        {
            characterCollider.enabled = false;
        }

        // Crear VFX de charco en el suelo
        if (puddleVFXPrefab != null)
        {
            Vector3 puddlePosition = transform.position;
            puddlePosition.y = 0.05f; // Ligeramente sobre el suelo

            puddleVFXInstance = Instantiate(puddleVFXPrefab, puddlePosition, Quaternion.identity);
            
            // El charco permanece un poco más que la tropa
            Destroy(puddleVFXInstance, despawnDuration + 0.5f);
            
        }
        else
        {
            Debug.LogWarning($"[TroopDespawn] {gameObject.name} no tiene puddleVFXPrefab asignado! No habrá efecto de charco.");
        }

        // Iniciar corrutina de despawn
        StartCoroutine(DespawnSequence());

    }

    private IEnumerator DespawnSequence()
    {
        float elapsedTime = 0f;

        // Reducir tamaño gradualmente
        while (elapsedTime < despawnDuration)
        {
            elapsedTime += Time.deltaTime;
            despawnTimeRemaining = despawnDuration - elapsedTime;

            // Interpolar desde escala original a 0
            float progress = elapsedTime / despawnDuration;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);

            yield return null;
        }

        // Asegurar que llegó a 0
        transform.localScale = Vector3.zero;

        // Destruir la UI si existe
        if (troopUI != null)
        {
            Destroy(troopUI.gameObject);
        }

        // Destruir la tropa
        Destroy(gameObject);
    }

    /// <summary>
    /// Verifica si la tropa puede atacar (no puede si está en despawn)
    /// </summary>
    public bool CanAttack()
    {
        return !isDespawning;
    }
}
