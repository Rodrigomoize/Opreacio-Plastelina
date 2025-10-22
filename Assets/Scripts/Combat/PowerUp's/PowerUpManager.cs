using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Gestor de powerups. Soporta configuración desde inspector para cada powerup.
/// - SlowTime: usa GameSpeedManager.ApplyTagSpeedMultiplier / RemoveTagSpeedMultiplier
/// - Health: busca la torre objetivo vía TowerManager y llama Heal(amount)
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [System.Serializable]
    public class PowerUpData
    {
        public string powerUpName;      // "SlowTime", "Health", etc.
        public float duration = 0f;     // 0 = instantáneo
        public float cooldownTime = 10f;
        public Button powerUpButton;
        public Image cooldownFillImage;

        // Opciones específicas por powerup:
        [Header("SlowTime settings")]
        [Tooltip("Tag objetivo que será afectado por SlowTime (ej: 'AITeam')")]
        public string targetTeam = "AITeam";
        [Tooltip("Multiplicador aplicado durante el SlowTime. 0.3 = 30% de velocidad")]
        public float slowMultiplier = 0.3f;

        [Header("Health settings")]
        [Tooltip("Cantidad de vida a restaurar para powerup Health")]
        public int healAmount = 3;
        [Tooltip("Equipo cuyo TowerHealthBar se curará (ej: 'PlayerTeam' o 'AITeam')")]
        public string healTargetTeam = "PlayerTeam";

        [HideInInspector] public bool isOnCooldown;
        [HideInInspector] public float cooldownTimer;
        [HideInInspector] public bool isActive;
        [HideInInspector] public float durationTimer;
    }

    [Header("Power-Up Configuration")]
    [SerializeField] private List<PowerUpData> powerUps = new List<PowerUpData>();

    [Header("Visual Feedback")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color cooldownColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitializePowerUps();
    }

    private void Update()
    {
        UpdateCooldowns();
    }

    private void InitializePowerUps()
    {
        foreach (var p in powerUps)
        {
            if (p.powerUpButton != null)
            {
                string nameCopy = p.powerUpName;
                p.powerUpButton.onClick.AddListener(() => ActivatePowerUp(nameCopy));
            }
            UpdatePowerUpUI(p);
        }
    }

    private void UpdateCooldowns()
    {
        foreach (var p in powerUps)
        {
            if (p.isOnCooldown)
            {
                p.cooldownTimer -= Time.deltaTime;
                if (p.cooldownTimer <= 0f)
                {
                    p.isOnCooldown = false;
                    p.cooldownTimer = 0f;
                }
                UpdatePowerUpUI(p);
            }

            if (p.isActive && p.duration > 0f)
            {
                p.durationTimer -= Time.deltaTime;
                if (p.durationTimer <= 0f)
                {
                    p.isActive = false;
                    // Desactivar efectos específicos
                    switch (p.powerUpName)
                    {
                        case "SlowTime":
                            DeactivateSlowTimePowerUp(p);
                            break;
                            // otros con duración aquí...
                    }
                    StartCooldown(p);
                    UpdatePowerUpUI(p);
                }
                else
                {
                    UpdatePowerUpUI(p);
                }
            }
        }
    }

    public void ActivatePowerUp(string powerUpName)
    {
        PowerUpData p = powerUps.Find(x => x.powerUpName == powerUpName);
        if (p == null)
        {
            Debug.LogWarning($"PowerUp '{powerUpName}' no encontrado!");
            return;
        }

        if (p.isOnCooldown || p.isActive)
        {
            Debug.Log($"PowerUp '{powerUpName}' no está disponible!");
            return;
        }

        // Ejecutar efecto
        switch (powerUpName)
        {
            case "SlowTime":
                ActivateSlowTimePowerUp(p);
                break;
            case "Health":
                ActivateHealthPowerUp(p);
                break;
            default:
                Debug.LogWarning($"PowerUp '{powerUpName}' no implementado.");
                return;
        }

        // Marcar activo y temporizador de duración
        p.isActive = true;
        p.durationTimer = p.duration;

        // Si es instantáneo, iniciar cooldown ahora
        if (p.duration <= 0f)
        {
            StartCooldown(p);
            // si no necesita duración, también marcamos isActive = false (pero puede usarse para VFX temporales)
            // Mantendremos isActive=true hasta que UpdateCooldowns lo desactive si necesitas ver color activo breve.
        }

        UpdatePowerUpUI(p);
    }

    private void StartCooldown(PowerUpData p)
    {
        p.isOnCooldown = true;
        p.cooldownTimer = p.cooldownTime;
    }

    private void UpdatePowerUpUI(PowerUpData p)
    {
        if (p.powerUpButton == null) return;

        p.powerUpButton.interactable = !p.isOnCooldown && !p.isActive;

        ColorBlock colors = p.powerUpButton.colors;
        if (p.isActive) colors.normalColor = activeColor;
        else if (p.isOnCooldown) colors.normalColor = cooldownColor;
        else colors.normalColor = availableColor;
        p.powerUpButton.colors = colors;

        if (p.cooldownFillImage != null)
        {
            if (p.isOnCooldown && p.cooldownTime > 0f)
                p.cooldownFillImage.fillAmount = 1f - (p.cooldownTimer / p.cooldownTime);
            else p.cooldownFillImage.fillAmount = 0f;
        }
    }

    #region PowerUp Implementations

    private void ActivateSlowTimePowerUp(PowerUpData p)
    {
        if (GameSpeedManager.Instance == null)
        {
            Debug.LogWarning("ActivateSlowTime: GameSpeedManager no encontrado.");
            return;
        }

        int affected = GameSpeedManager.Instance.ApplyTagSpeedMultiplier(p.targetTeam, p.slowMultiplier);
        Debug.Log($"[PowerUpManager] SlowTime activado para tag '{p.targetTeam}' (objetos afectados: {affected})");

        // Si la duración es 0 (instantáneo), programamos la restauración inmediata
        if (p.duration <= 0f)
        {
            // Desactivar inmediatamente
            DeactivateSlowTimePowerUp(p);
            StartCooldown(p);
            p.isActive = false;
            UpdatePowerUpUI(p);
        }
    }

    private void DeactivateSlowTimePowerUp(PowerUpData p)
    {
        if (GameSpeedManager.Instance == null)
        {
            Debug.LogWarning("DeactivateSlowTime: GameSpeedManager no encontrado.");
            return;
        }

        int restored = GameSpeedManager.Instance.RemoveTagSpeedMultiplier(p.targetTeam);
        Debug.Log($"[PowerUpManager] SlowTime desactivado para tag '{p.targetTeam}' (restauradas: {restored})");
    }

    private void ActivateHealthPowerUp(PowerUpData p)
    {
        // Preferimos TowerManager
        TowerHealthBar target = null;
        if (TowerManager.Instance != null)
        {
            target = TowerManager.Instance.GetTowerByTeam(p.healTargetTeam);
        }

        // Fallback: buscar la primera TowerHealthBar con ese teamTag en escena
        if (target == null)
        {
            TowerHealthBar[] bars = FindObjectsOfType<TowerHealthBar>();
            if (bars != null && bars.Length > 0)
            {
                foreach (var b in bars)
                {
                    if (b != null && b.teamTag == p.healTargetTeam) { target = b; break; }
                }
                if (target == null) target = bars[0];
            }
        }

        if (target == null)
        {
            Debug.LogWarning($"ActivateHealthPowerUp: No se encontró TowerHealthBar para team '{p.healTargetTeam}'");
            return;
        }

        target.Heal(p.healAmount);
        Debug.Log($"[PowerUpManager] HealthPowerUp: curados {p.healAmount} en {target.gameObject.name}");

        // si duration == 0, lo consideramos instantáneo y lanzamos cooldown
        if (p.duration <= 0f)
        {
            StartCooldown(p);
            p.isActive = false;
            UpdatePowerUpUI(p);
        }
    }

    #endregion

    // Métodos públicos/útiles
    public bool IsPowerUpAvailable(string powerUpName)
    {
        PowerUpData p = powerUps.Find(x => x.powerUpName == powerUpName);
        return p != null && !p.isOnCooldown && !p.isActive;
    }

    public float GetCooldownProgress(string powerUpName)
    {
        PowerUpData p = powerUps.Find(x => x.powerUpName == powerUpName);
        if (p != null && p.isOnCooldown && p.cooldownTime > 0f)
            return 1f - (p.cooldownTimer / p.cooldownTime);
        return 1f;
    }
}
