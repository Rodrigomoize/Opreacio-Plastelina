using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Singleton que mantiene referencias a TowerHealthBar instanciadas.
/// Otros sistemas (powerups, UI, objetivos) pueden pedir la torre por teamTag.
/// </summary>
public class TowerManager : MonoBehaviour
{
    public static TowerManager Instance { get; private set; }

    private readonly List<TowerHealthBar> towers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterTower(TowerHealthBar bar)
    {
        if (bar == null) return;
        if (!towers.Contains(bar)) towers.Add(bar);
        Debug.Log($"[TowerManager] Registrada torre: {bar.gameObject.name} (team: {bar.teamTag})");
    }

    public void UnregisterTower(TowerHealthBar bar)
    {
        if (bar == null) return;
        towers.Remove(bar);
        Debug.Log($"[TowerManager] Unregistered tower: {bar.gameObject.name}");
    }

    public TowerHealthBar GetTowerByTeam(string teamTag)
    {
        if (string.IsNullOrEmpty(teamTag)) return null;
        return towers.FirstOrDefault(t => t != null && t.teamTag == teamTag);
    }

    public TowerHealthBar[] GetAllTowers() => towers.Where(t => t != null).ToArray();
}
