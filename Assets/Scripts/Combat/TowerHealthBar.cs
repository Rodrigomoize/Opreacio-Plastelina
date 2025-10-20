using UnityEngine;
using UnityEngine.UI;

public class TowerHealthBar : MonoBehaviour
{
    [Header("UI Elements")]
    public Canvas worldCanvas;
    public Slider healthSlider; // El slider de la barra de vida
    public Image sliderFillImage; // La imagen de relleno del slider (opcional, para cambiar color)
    
    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 1f, 0); // Offset sobre la torre
    
    [Header("Team Colors")]
    public bool useTeamColors = true; // Si está activado, usa colores según el equipo
    public Color blueTeamColor = new Color(0.2f, 0.5f, 1f); // Color para equipo azul
    public Color redTeamColor = new Color(1f, 0.2f, 0.2f); // Color para equipo rojo
    
    [Header("Health Colors")]
    public Color fullHealthColor = Color.green;
    public Color halfHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    
    [Header("Health")]
    private int maxHealth = 10;
    private int currentHealth;
    
    private Transform targetTransform;
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Configurar el canvas para que sea World Space
        if (worldCanvas != null)
        {
            worldCanvas.renderMode = RenderMode.WorldSpace;
        }
        
        // Configurar el slider
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
            healthSlider.interactable = false; // No queremos que sea interactivo
            
            // Obtener la imagen de relleno si no está asignada
            if (sliderFillImage == null)
            {
                Transform fillArea = healthSlider.transform.Find("Fill Area");
                if (fillArea != null)
                {
                    Transform fill = fillArea.Find("Fill");
                    if (fill != null)
                    {
                        sliderFillImage = fill.GetComponent<Image>();
                    }
                }
            }
        }
        
        // Inicializar la salud al máximo
        currentHealth = maxHealth;
        UpdateHealthBar();
    }
    
    void LateUpdate()
    {
        // Seguir la posición de la torre
        if (targetTransform != null)
        {
            transform.position = targetTransform.position + offset;
            
            // Hacer que el canvas mire a la cámara
            if (mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                                 mainCamera.transform.rotation * Vector3.up);
            }
        }
    }
    
    /// <summary>
    /// Inicializa la barra de vida de la torre
    /// </summary>
    /// <param name="target">Transform de la torre a seguir</param>
    /// <param name="health">Vida máxima de la torre</param>
    /// <param name="teamTag">Tag del equipo ("PlayerTeam" o "AITeam")</param>
    public void Initialize(Transform target, int health, string teamTag = "")
    {
        targetTransform = target;
        maxHealth = health;
        currentHealth = health;
        
        // Configurar el slider con los valores máximos
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        
        // Si useTeamColors está activado, usar colores del equipo
        if (useTeamColors && !string.IsNullOrEmpty(teamTag))
        {
            if (teamTag == "PlayerTeam")
            {
                fullHealthColor = blueTeamColor;
                halfHealthColor = blueTeamColor;
                lowHealthColor = blueTeamColor;
            }
            else if (teamTag == "AITeam")
            {
                fullHealthColor = redTeamColor;
                halfHealthColor = redTeamColor;
                lowHealthColor = redTeamColor;
            }
        }
        
        UpdateHealthBar();
    }
    
    /// <summary>
    /// Establece la salud actual
    /// </summary>
    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();
    }
    
    /// <summary>
    /// Reduce la salud
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthBar();
    }
    
    /// <summary>
    /// Cura la torre
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthBar();
    }
    
    /// <summary>
    /// Actualiza la visualización de la barra de vida
    /// </summary>
    private void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            // Actualizar el valor del slider
            healthSlider.value = currentHealth;
            
            // Cambiar color del fill según el porcentaje de vida
            if (sliderFillImage != null)
            {
                float healthPercentage = (float)currentHealth / maxHealth;
                
                if (healthPercentage > 0.6f)
                {
                    sliderFillImage.color = fullHealthColor;
                }
                else if (healthPercentage > 0.3f)
                {
                    sliderFillImage.color = halfHealthColor;
                }
                else
                {
                    sliderFillImage.color = lowHealthColor;
                }
            }
        }
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
}
