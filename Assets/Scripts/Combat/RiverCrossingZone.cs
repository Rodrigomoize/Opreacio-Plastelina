using UnityEngine;

/// <summary>
/// Zona de trigger que detecta cuando las tropas individuales (Character) cruzan el río
/// y las destruye con un efecto de despawn
/// </summary>
public class RiverCrossingZone : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("Mostrar logs de debug")]
    public bool showLogs = true;

    private void Awake()
    {
        // Asegurar que tiene un Collider configurado como trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            Log("Añadido BoxCollider como trigger");
        }
        else
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Solo afectar a Characters (tropas individuales), NO a CharacterCombined (camiones)
        Character character = other.GetComponent<Character>();
        
        if (character != null)
        {
            // Verificar que NO es un camión (CharacterCombined también tiene Character)
            CharacterCombined combined = other.GetComponent<CharacterCombined>();
            if (combined != null)
            {
                Log($"Ignorando camión {other.gameObject.name} - los camiones pueden cruzar el río");
                return;
            }

            Log($"Tropa {other.gameObject.name} (valor {character.GetValue()}) cruzó el río - iniciando despawn");
            
            // Buscar el componente TroopDespawnController (debe estar pre-configurado en el prefab)
            TroopDespawnController despawner = other.GetComponent<TroopDespawnController>();
            if (despawner != null)
            {
                // Iniciar el proceso de despawn
                despawner.StartDespawn();
            }
            else
            {
                // Si no tiene el componente, advertir y destruir inmediatamente
                Debug.LogWarning($"[RiverCrossing] {other.gameObject.name} no tiene TroopDespawnController! Destruyendo inmediatamente.");
                Destroy(other.gameObject);
            }
        }
    }

    private void Log(string message)
    {
        if (showLogs)
        {
        }
    }
}
