using System.Collections.Generic;
using UnityEngine;

public class GameSpeedManager : MonoBehaviour
{
    public enum GameDifficulty
    {
        Facil,
        Media,
        Dificil
    }

    public static GameSpeedManager Instance { get; private set; }

    [Header("Configuración de Velocidad")]
    [SerializeField, Range(0.1f, 3f)]
    private float gameSpeedMultiplier = 1.0f;
    public float GameSpeedMultiplier
    {
        get => gameSpeedMultiplier;
        set { gameSpeedMultiplier = Mathf.Clamp(value, 0.1f, 3f); OnGameSpeedChanged(); }
    }

    [Header("Velocidades por Dificultad")]
    [SerializeField, Range(0.1f, 1.5f)]
    private float easySpeed = 0.4f; // Fácil: 40% velocidad
    [SerializeField, Range(0.1f, 1.5f)]
    private float mediumSpeed = 0.7f; // Media: 70% velocidad
    [SerializeField, Range(0.1f, 1.5f)]
    private float hardSpeed = 1.0f; // Difícil: 100% velocidad

    // Multiplicadores temporales por objeto (p.ej. slow powerup)
    private readonly Dictionary<GameObject, float> perObjectMultipliers = new();
    private readonly Dictionary<string, List<GameObject>> tagToObjects = new();
    
    // Estado activo de slow por tag (para afectar a nuevas tropas)
    private readonly Dictionary<string, float> activeTagMultipliers = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnGameSpeedChanged()
    {
        UpdateAllCharacterSpeeds();
    }

    private void UpdateAllCharacterSpeeds()
    {
        Character[] characters = FindObjectsByType<Character>(FindObjectsSortMode.None);
        foreach (var c in characters) c.UpdateSpeed();

        CharacterCombined[] combined = FindObjectsByType<CharacterCombined>(FindObjectsSortMode.None);
        foreach (var cc in combined) cc.UpdateSpeed();

    }

    public float GetAgentMultiplier(GameObject obj)
    {
        if (obj == null) return 1f;
        
        // Primero verificar si hay un multiplicador específico del objeto
        if (perObjectMultipliers.TryGetValue(obj, out float m)) return m;
        
        // Si no, verificar si su tag tiene un multiplicador activo
        if (!string.IsNullOrEmpty(obj.tag) && activeTagMultipliers.TryGetValue(obj.tag, out float tagM))
        {
            // Aplicar el multiplicador a este objeto también (para futuras consultas)
            perObjectMultipliers[obj] = tagM;
            
            // Añadir a la lista de objetos del tag si existe
            if (tagToObjects.TryGetValue(obj.tag, out var list) && !list.Contains(obj))
            {
                list.Add(obj);
            }
            
            return tagM;
        }
        
        return 1f;
    }

    public int ApplyTagSpeedMultiplier(string tag, float multiplier)
    {
        if (string.IsNullOrEmpty(tag)) return 0;
        
        // Guardar el multiplicador activo para este tag (afectará a nuevas tropas)
        activeTagMultipliers[tag] = multiplier;
        
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        if (objs == null || objs.Length == 0) 
        { 
            return 0; 
        }

        if (!tagToObjects.ContainsKey(tag)) tagToObjects[tag] = new List<GameObject>();
        int applied = 0;
        foreach (var o in objs)
        {
            if (o == null) continue;
            perObjectMultipliers[o] = multiplier;
            if (!tagToObjects[tag].Contains(o)) tagToObjects[tag].Add(o);
            applied++;
        }
        if (applied > 0) OnGameSpeedChanged();
        return applied;
    }

    public int RemoveTagSpeedMultiplier(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return 0;
        
        // Remover el multiplicador activo del tag
        activeTagMultipliers.Remove(tag);
        
        if (!tagToObjects.TryGetValue(tag, out var list)) return 0;
        int removed = 0;
        foreach (var o in list)
        {
            if (o == null) continue;
            if (perObjectMultipliers.Remove(o)) removed++;
        }
        list.Clear();
        tagToObjects.Remove(tag);
        if (removed > 0) OnGameSpeedChanged();
        return removed;
    }

    /// <summary>
    /// Establece la velocidad del juego según la dificultad
    /// </summary>
    public void SetSpeedByDifficulty(GameDifficulty dificultad)
    {
        switch (dificultad)
        {
            case GameDifficulty.Facil:
                GameSpeedMultiplier = easySpeed;
                break;
            case GameDifficulty.Media:
                GameSpeedMultiplier = mediumSpeed;
                break;
            case GameDifficulty.Dificil:
                GameSpeedMultiplier = hardSpeed;
                break;
        }
    }

    /// <summary>
    /// Obtiene la velocidad correspondiente a una dificultad sin aplicarla
    /// </summary>
    public float GetSpeedForDifficulty(GameDifficulty dificultad)
    {
        return dificultad switch
        {
            GameDifficulty.Facil => easySpeed,
            GameDifficulty.Media => mediumSpeed,
            GameDifficulty.Dificil => hardSpeed,
            _ => mediumSpeed
        };
    }

    /// <summary>
    /// Remueve el multiplicador activo de un tag para que nuevas tropas no se vean afectadas,
    /// pero mantiene el slowdown en las tropas existentes con ese tag
    /// </summary>
    public void RemoveActiveTagMultiplier(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return;
        
        // Solo remover de activeTagMultipliers
        // Las tropas existentes en perObjectMultipliers seguirán lentas
        if (activeTagMultipliers.Remove(tag))
        {
            Debug.Log($"[GameSpeedManager] Multiplicador activo removido para tag '{tag}'. Tropas existentes siguen lentas, nuevas tropas irán a velocidad normal.");
        }
    }

    private void OnDestroy()
    {
        perObjectMultipliers.Clear();
        tagToObjects.Clear();
        activeTagMultipliers.Clear();
    }
}
