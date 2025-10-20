using UnityEngine;
using static CharacterManager;

public class Tower : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;
    
    [Header("UI")]
    public GameObject healthBarPrefab; // Prefab de la barra de vida
    private TowerHealthBar healthBarInstance;
    
    void Start()
    {
        // Inicializar salud
        currentHealth = maxHealth;
        
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
    
    /// <summary>
    /// Cura la torre
    /// </summary>
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
        
        // Aquí puedes añadir lógica de fin de juego
        // Por ejemplo: activar pantalla de victoria/derrota
        
        // Opcionalmente destruir el objeto de la torre
        // Destroy(gameObject);
    }
}
