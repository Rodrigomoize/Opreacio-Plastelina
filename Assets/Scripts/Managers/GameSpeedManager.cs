using System.Collections.Generic;
using UnityEngine;

public class GameSpeedManager : MonoBehaviour
{
    public static GameSpeedManager Instance { get; private set; }

    [Header("Configuración de Velocidad")]
    [SerializeField, Range(0.1f, 3f)]
    private float gameSpeedMultiplier = 1.0f;
    public float GameSpeedMultiplier
    {
        get => gameSpeedMultiplier;
        set { gameSpeedMultiplier = Mathf.Clamp(value, 0.1f, 3f); OnGameSpeedChanged(); }
    }

    // Multiplicadores temporales por objeto (p.ej. slow powerup)
    private readonly Dictionary<GameObject, float> perObjectMultipliers = new();
    private readonly Dictionary<string, List<GameObject>> tagToObjects = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnGameSpeedChanged()
    {
        UpdateAllCharacterSpeeds();
        Debug.Log($"[GameSpeedManager] Velocidad global actual: {gameSpeedMultiplier}");
    }

    private void UpdateAllCharacterSpeeds()
    {
        Character[] characters = FindObjectsOfType<Character>();
        foreach (var c in characters) c.UpdateSpeed();

        CharacterCombined[] combined = FindObjectsOfType<CharacterCombined>();
        foreach (var cc in combined) cc.UpdateSpeed();

        Debug.Log($"[GameSpeedManager] Recalculadas velocidades. chars:{characters.Length} combined:{combined.Length}");
    }

    public float GetAgentMultiplier(GameObject obj)
    {
        if (obj == null) return 1f;
        return perObjectMultipliers.TryGetValue(obj, out float m) ? m : 1f;
    }

    public int ApplyTagSpeedMultiplier(string tag, float multiplier)
    {
        if (string.IsNullOrEmpty(tag)) return 0;
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        if (objs == null || objs.Length == 0) { Debug.Log($"[GameSpeedManager] No objects with tag {tag}"); return 0; }

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

    private void OnDestroy()
    {
        perObjectMultipliers.Clear();
        tagToObjects.Clear();
    }
}
