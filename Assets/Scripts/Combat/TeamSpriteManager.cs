using UnityEngine;

/// <summary>
/// Helper para gestionar sprites de UI por equipo
/// Útil para cambios dinámicos o configuración centralizada
/// </summary>
public class TeamSpriteManager : MonoBehaviour
{
    [System.Serializable]
    public class TeamSprites
    {
        [Header("Troop Sprites")]
        public Sprite blueTroopIcon;
        public Sprite redTroopIcon;
        
        [Header("Operation Sprites")]
        public Sprite blueOperationIcon;
        public Sprite redOperationIcon;
        
        [Header("Tower Colors")]
        public Color blueTowerColor = new Color(0.2f, 0.5f, 1f);
        public Color redTowerColor = new Color(1f, 0.2f, 0.2f);
    }
    
    [Header("Team Sprite Configuration")]
    public TeamSprites teamSprites;
    
    private static TeamSpriteManager instance;
    
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Obtiene la instancia del manager
    /// </summary>
    public static TeamSpriteManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("TeamSpriteManager");
                instance = go.AddComponent<TeamSpriteManager>();
            }
            return instance;
        }
    }
    
    /// <summary>
    /// Obtiene el sprite de tropa según el equipo
    /// </summary>
    public Sprite GetTroopSprite(string teamTag)
    {
        if (teamSprites == null) return null;
        
        if (teamTag == "PlayerTeam")
            return teamSprites.blueTroopIcon;
        else if (teamTag == "AITeam")
            return teamSprites.redTroopIcon;
        
        return null;
    }
    
    /// <summary>
    /// Obtiene el sprite de operación según el equipo
    /// </summary>
    public Sprite GetOperationSprite(string teamTag)
    {
        if (teamSprites == null) return null;
        
        if (teamTag == "PlayerTeam")
            return teamSprites.blueOperationIcon;
        else if (teamTag == "AITeam")
            return teamSprites.redOperationIcon;
        
        return null;
    }
    
    /// <summary>
    /// Obtiene el color de torre según el equipo
    /// </summary>
    public Color GetTowerColor(string teamTag)
    {
        if (teamSprites == null) return Color.white;
        
        if (teamTag == "PlayerTeam")
            return teamSprites.blueTowerColor;
        else if (teamTag == "AITeam")
            return teamSprites.redTowerColor;
        
        return Color.white;
    }
    
    /// <summary>
    /// Aplica sprites globalmente a un TroopUI
    /// </summary>
    public void ApplyTroopSprites(TroopUI troopUI, string teamTag)
    {
        if (troopUI == null) return;
        
        Sprite sprite = GetTroopSprite(teamTag);
        if (sprite != null && troopUI.iconImage != null)
        {
            troopUI.iconImage.sprite = sprite;
        }
    }
    
    /// <summary>
    /// Aplica sprites globalmente a un OperationUI
    /// </summary>
    public void ApplyOperationSprites(OperationUI operationUI, string teamTag)
    {
        if (operationUI == null) return;
        
        Sprite sprite = GetOperationSprite(teamTag);
        if (sprite != null && operationUI.iconImage != null)
        {
            operationUI.iconImage.sprite = sprite;
        }
    }
    
    /// <summary>
    /// Aplica colores globalmente a un TowerHealthBar
    /// </summary>
    public void ApplyTowerColors(TowerHealthBar healthBar, string teamTag)
    {
        if (healthBar == null) return;
        
        Color color = GetTowerColor(teamTag);
        if (healthBar.sliderFillImage != null)
        {
            healthBar.sliderFillImage.color = color;
        }
    }
}
