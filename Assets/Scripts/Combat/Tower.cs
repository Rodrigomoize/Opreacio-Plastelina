using UnityEngine;
using static CharacterManager;

public class Tower : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 10;
    private int currentHealth;
    
    [Header("UI")]
    public GameObject healthBarPrefab; // Prefab de la barra de vida
    private TowerHealthBar healthBarInstance;
    
    [Header("Tower Type")]
    public bool isEnemyTower = true; // Marcar como true para la torre enemiga
    
    private GameFlow gameFlow;
    
    void Start()
    {
        // Buscar el GameFlow
        gameFlow = FindFirstObjectByType<GameFlow>();
        
        // Inicializar salud
        currentHealth = maxHealth;
        
        // Asegurar que la torre tenga un collider con trigger para detectar tropas
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // Añadir un BoxCollider por defecto
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(2f, 3f, 2f); // Tamaño razonable para una torre
            box.isTrigger = true;
            Debug.Log($"[Tower] Añadido BoxCollider trigger a {gameObject.name}");
        }
        else
        {
            col.isTrigger = true;
            Debug.Log($"[Tower] Collider existente configurado como trigger en {gameObject.name}");
        }
        
        // Crear la barra de vida si hay prefab asignado
        if (healthBarPrefab != null)
        {
            GameObject uiObj = Instantiate(healthBarPrefab);
            healthBarInstance = uiObj.GetComponent<TowerHealthBar>();
            if (healthBarInstance != null)
            {
                // Pasar el tag del equipo para usar los colores correctos
                healthBarInstance.Initialize(transform, maxHealth, gameObject.tag);
            }
        }
    }
    
    /// <summary>
    /// La torre recibe daño
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // Actualizar la barra de vida
        if (healthBarInstance != null)
        {
            healthBarInstance.SetHealth(currentHealth);
        }
        
        Debug.Log($"[Tower] {gameObject.name} recibió {damage} de daño. Vida: {currentHealth}/{maxHealth}");
        
        // Verificar si la torre fue destruida
        if (currentHealth <= 0)
        {
            OnTowerDestroyed();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        // Actualizar la barra de vida
        if (healthBarInstance != null)
        {
            healthBarInstance.SetHealth(currentHealth);
        }
        
        Debug.Log($"[Tower] {gameObject.name} curada por {amount}. Vida: {currentHealth}/{maxHealth}");
    }
    
    /// <summary>
    /// Obtiene la salud actual
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    /// <summary>
    /// Verifica si la torre está destruida
    /// </summary>
    public bool IsDead()
    {
        return currentHealth <= 0;
    }
    
    /// <summary>
    /// Se llama cuando la torre es destruida
    /// </summary>
    private void OnTowerDestroyed()
    {
        Debug.Log($"[Tower] {gameObject.name} ha sido destruida!");

        if (isEnemyTower && gameFlow != null)
        {
            Debug.Log("[Tower] Torre enemiga destruida - Cargando LoseScene");
            gameFlow.LoadWinScene();
        }
        
        else if (!isEnemyTower && gameFlow != null)
        {
            Debug.Log("[Tower] Torre del jugador destruida - Jugador pierde");
            gameFlow.LoadLoseScene();
        }
        
        // Opcionalmente destruir el objeto de la torre
        // Destroy(gameObject);
    }
}
