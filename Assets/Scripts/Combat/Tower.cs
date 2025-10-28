using UnityEngine;
using System.Collections;
using static CharacterManager;

public class Tower : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("UI")]
    public GameObject healthBarPrefab; // prefab debe contener TowerHealthBar
    private TowerHealthBar healthBarInstance;

    [Header("Tower Type")]
    public bool isEnemyTower = true; // marcar en inspector
    public string teamTag = "AITeam";
    
    [Header("Destruction FX")]
    public FractureObject fractureObject;
    public float explosionDelay = 3f;
    
    private Animator animator;
    private bool isDestroyed = false;

    void Start()
    {
        currentHealth = maxHealth;

        // Obtener el Animator (puede estar en un hijo)
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"[Tower] {gameObject.name} no tiene Animator component en sus hijos!");
        }

        // Buscar FractureObject si no está asignado
        if (fractureObject == null)
        {
            fractureObject = GetComponentInChildren<FractureObject>();
            if (fractureObject == null)
            {
                Debug.LogWarning($"[Tower] {gameObject.name} no tiene FractureObject asignado. No habrá efecto de explosión.");
            }
        }

        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(2f, 3f, 2f);
            box.isTrigger = true;
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

    }

    public void TakeDamage(int damage)
    {
        if (isDestroyed) return; // No recibir más daño si ya está destruida
        
        currentHealth = Mathf.Max(0, currentHealth - damage);

        // Sonido de impacto en torre (suena siempre que recibe daño)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTowerHit();
        }
        else
        {
            Debug.LogWarning("[Tower] AudioManager.Instance es NULL, no se puede reproducir sonido");
        }

        // Activar animación de hit
        if (animator != null)
        {
            animator.SetTrigger("hit");
        }

        // Actualizar la barra de vida
        if (healthBarInstance != null)
        {
            healthBarInstance.SetHealth(currentHealth);
        }


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
    }

    public int GetCurrentHealth() => currentHealth;
    public bool IsDead() => currentHealth <= 0;

    private void OnTowerDestroyed()
    {
        if (isDestroyed) return; // Evitar llamadas múltiples
        isDestroyed = true;
        

        if (!isEnemyTower) // Si NO es torre enemiga = es torre del jugador
        {
            StartCoroutine(DefeatSequence());
        }
        else // Si es torre enemiga = el jugador gana
        {
            StartCoroutine(VictorySequence());
        }
    }

    private IEnumerator DefeatSequence()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTowerDestroyed();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.DisableGameplay();
            GameManager.Instance.FreezeAllTroops();
        }

        if (fractureObject != null)
        {
            fractureObject.Explode();
        }

        yield return new WaitForSeconds(explosionDelay);

        SceneBridge.LoadLoseScene();
    }

    private IEnumerator VictorySequence()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyTowerDestroyed();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.DisableGameplay();
            GameManager.Instance.FreezeAllTroops();
        }

        if (fractureObject != null)
        {
            fractureObject.Explode();
        }

        yield return new WaitForSeconds(explosionDelay);

        SceneBridge.LoadWinScene();
    }
}