using UnityEngine;

/// <summary>
/// Manager que controla la velocidad de juego global.
/// Afecta a la velocidad de movimiento de tropas y operaciones en tiempo real.
/// </summary>
public class GameSpeedManager : MonoBehaviour
{
    public static GameSpeedManager Instance { get; private set; }

    [Header("Configuración de Velocidad")]
    [SerializeField]
    [Range(0.1f, 3.0f)]
    [Tooltip("Multiplicador de velocidad de juego. 1.0 = velocidad normal, 2.0 = doble velocidad")]
    private float gameSpeedMultiplier = 1.0f;

    /// <summary>
    /// Multiplicador actual de velocidad de juego
    /// </summary>
    public float GameSpeedMultiplier
    {
        get => gameSpeedMultiplier;
        set
        {
            gameSpeedMultiplier = Mathf.Clamp(value, 0.1f, 3.0f);
            OnGameSpeedChanged();
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnValidate()
    {
        // Cuando se cambia el valor en el inspector en tiempo real
        if (Application.isPlaying)
        {
            OnGameSpeedChanged();
        }
    }

    /// <summary>
    /// Se llama cuando cambia la velocidad de juego
    /// Actualiza todas las tropas y operaciones activas
    /// </summary>
    private void OnGameSpeedChanged()
    {
        UpdateAllCharacterSpeeds();
        Debug.Log($"[GameSpeedManager] Velocidad de juego cambiada a {gameSpeedMultiplier}x");
    }

    /// <summary>
    /// Actualiza la velocidad de todos los personajes activos en la escena
    /// </summary>
    private void UpdateAllCharacterSpeeds()
    {
        // Actualizar Characters simples
        Character[] characters = FindObjectsByType<Character>(FindObjectsSortMode.None);
        foreach (Character character in characters)
        {
            character.UpdateSpeed();
        }

        // Actualizar CharactersCombined
        CharacterCombined[] combinedCharacters = FindObjectsByType<CharacterCombined>(FindObjectsSortMode.None);
        foreach (CharacterCombined combined in combinedCharacters)
        {
            combined.UpdateSpeed();
        }

        Debug.Log($"[GameSpeedManager] Actualizado velocidad de {characters.Length} personajes simples y {combinedCharacters.Length} combinados");
    }

    /// <summary>
    /// Calcula la velocidad final aplicando el multiplicador de velocidad de juego
    /// </summary>
    /// <param name="baseSpeed">Velocidad base de la tropa</param>
    /// <returns>Velocidad final con multiplicador aplicado</returns>
    public float GetAdjustedSpeed(float baseSpeed)
    {
        return baseSpeed * gameSpeedMultiplier;
    }
}
