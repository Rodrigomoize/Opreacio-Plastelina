using UnityEngine;
using static CharacterManager;

public class Tower : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("UI")]
    public GameObject healthBarPrefab; // prefab debe contener TowerHealthBar
    private TowerHealthBar healthBarInstance;

    [Header("Tower Type")]
    public bool isEnemyTower = true; // marcar en inspector
    [Tooltip("Tag que identifica al equipo (ej: 'AITeam' o 'PlayerTeam')")]
    public string teamTag = "AITeam";
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;

        // Obtener el Animator (puede estar en un hijo)
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"[Tower] {gameObject.name} no tiene Animator component en sus hijos!");
        }

        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(2f, 3f, 2f);
            box.isTrigger = true;
            Debug.Log($"[Tower] Añadido BoxCollider trigger a {gameObject.name}");
        }
        else
        {
            col.isTrigger = true;
        }

        if (healthBarPrefab != null)
        {
            GameObject uiObj = Instantiate(healthBarPrefab);
            healthBarInstance = uiObj.GetComponent<TowerHealthBar>();
            if (healthBarInstance != null)
            {
                // Inicializar con la transformación y teamTag del prefab
                healthBarInstance.Initialize(transform, maxHealth, teamTag);
                healthBarInstance.teamTag = teamTag; // asegurar que el tag esté puesto
            }
            else
            {
                Debug.LogError("[Tower] healthBarPrefab no contiene TowerHealthBar!");
            }
        }
    }

    /// Reinicializa la vida de la torre con un nuevo máximo
    /// Útil para configurar dificultad antes de que empiece el combate
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;

        if (healthBarInstance != null)
        {
            healthBarInstance.Initialize(transform, maxHealth, teamTag);
        }

        Debug.Log($"[Tower] {gameObject.name} vida configurada a {maxHealth}");
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);

        // Sonido de impacto en torre (suena siempre que recibe daño)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTowerHit();
            Debug.Log($"[Tower] 💥 Sonido de impacto en torre reproducido");
        }
        else
        {
            Debug.LogWarning("[Tower] AudioManager.Instance es NULL, no se puede reproducir sonido");
        }

        // Activar animación de hit
        if (animator != null)
        {
            animator.SetTrigger("hit");
            Debug.Log($"[Tower] {gameObject.name} - Trigger 'hit' activado");
        }

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
        if (healthBarInstance != null)
        {
            healthBarInstance.SetHealth(currentHealth);
        }
        Debug.Log($"[Tower] {gameObject.name} curada por {amount}. Vida: {currentHealth}/{maxHealth}");
    }

    public int GetCurrentHealth() => currentHealth;
    public bool IsDead() => currentHealth <= 0;

    private void OnTowerDestroyed()
    {
        Debug.Log($"[Tower] {gameObject.name} destruida!");

        if (!isEnemyTower) // Si NO es torre enemiga = es torre del jugador
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTowerDestroyed();
                Debug.Log($"[Tower] 💀 Sonido de torre destruida reproducido (torre del jugador)");
            }

            SceneBridge.LoadLoseScene();
        }
        else // Si es torre enemiga = el jugador gana
        {
            Debug.Log($"[Tower] 🎉 Torre enemiga destruida (sin sonido, solo victoria)");
            SceneBridge.LoadWinScene();
        }
    }
}