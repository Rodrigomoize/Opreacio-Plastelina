using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de vida de torre en world space. Se registra en TowerManager en Awake para que otros sistemas la encuentren.
/// </summary>
public class TowerHealthBar : MonoBehaviour
{
    [Header("UI Elements")]
    public Canvas worldCanvas;
    public Slider healthSlider;
    public Image sliderFillImage;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 1f, 0);

    [Header("Team")]
    [Tooltip("Tag o identificador del equipo: e.g. 'AITeam' o 'PlayerTeam'")]
    public string teamTag = "";

    [Header("Health Colors")]
    public Color fullHealthColor = Color.green;
    public Color halfHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    [Header("Health")]
    public int maxHealth = 10;
    private int currentHealth;

    private Transform targetTransform;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (worldCanvas != null)
            worldCanvas.renderMode = RenderMode.WorldSpace;

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
            healthSlider.interactable = false;

            if (sliderFillImage == null)
            {
                Transform fillArea = healthSlider.transform.Find("Fill Area");
                if (fillArea != null)
                {
                    Transform fill = fillArea.Find("Fill");
                    if (fill != null) sliderFillImage = fill.GetComponent<Image>();
                }
            }
        }

        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    void LateUpdate()
    {
        if (targetTransform != null)
        {
            transform.position = targetTransform.position + offset;

            if (mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                                 mainCamera.transform.rotation * Vector3.up);
            }
        }
    }

    /// <summary>
    /// Inicializa la barra con la torre y valores.
    /// </summary>
    public void Initialize(Transform target, int health, string team = "")
    {
        targetTransform = target;
        maxHealth = Mathf.Max(1, health);
        currentHealth = maxHealth;
        if (!string.IsNullOrEmpty(team)) teamTag = team;

        // Ajustes por equipo (opcional)
        if (team == "PlayerTeam")
            offset = new Vector3(0, 2f, -3f); // ejemplo
        else if (team == "AITeam")
            offset = new Vector3(0, 3f, 0);

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        UpdateHealthBar();
    }

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthBar();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (sliderFillImage != null)
        {
            float pct = (float)currentHealth / Mathf.Max(1, maxHealth);
            if (pct > 0.6f) sliderFillImage.color = fullHealthColor;
            else if (pct > 0.3f) sliderFillImage.color = halfHealthColor;
            else sliderFillImage.color = lowHealthColor;
        }
    }

    public int GetCurrentHealth() => currentHealth;
    public bool IsDead() => currentHealth <= 0;
}
