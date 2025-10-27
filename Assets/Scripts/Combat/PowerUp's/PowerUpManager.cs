using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Gestor de powerups. Soporta configuraci�n desde inspector para cada powerup.
/// - SlowTime: usa GameSpeedManager.ApplyTagSpeedMultiplier / RemoveTagSpeedMultiplier
/// - Health: busca la torre objetivo v�a TowerManager y llama Heal(amount)
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [System.Serializable]
    public class PowerUpData
    {
        public string powerUpName;      // "SlowTime", "Health", etc.
        public float duration = 0f;     // 0 = instant�neo
        public float cooldownTime = 10f;
        public Button powerUpButton;
        public Image cooldownFillImage;

        // Opciones espec�ficas por powerup:
        [Header("SlowTime settings")]
        [Tooltip("Tag objetivo que ser� afectado por SlowTime (ej: 'AITeam')")]
        public string targetTeam = "AITeam";
        [Tooltip("Multiplicador aplicado durante el SlowTime. 0.3 = 30% de velocidad")]
        public float slowMultiplier = 0.3f;

        [Header("Health settings")]
        [Tooltip("Cantidad de vida a restaurar para powerup Health")]
        public int healAmount = 3;
        [Tooltip("Equipo cuyo TowerHealthBar se curar� (ej: 'PlayerTeam' o 'AITeam')")]
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
    
    [Header("Screen Flash Effects")]
    [Tooltip("Color del flash para healing (verde)")]
    [SerializeField] private Color healingFlashColor = new Color(0f, 1f, 0f, 0.4f);
    [Tooltip("Color del flash para slow time (azul)")]
    [SerializeField] private Color slowTimeFlashColor = new Color(0f, 0.5f, 1f, 0.3f);
    [Tooltip("Duración del flash en segundos")]
    [SerializeField] private float flashDuration = 0.5f;

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
        UpdateActiveDurationTimers();
    }
    
    /// <summary>
    /// Actualiza los temporizadores de duración de powerups activos y muestra UI
    /// </summary>
    private void UpdateActiveDurationTimers()
    {
        foreach (var p in powerUps)
        {
            if (p.isActive && p.duration > 0f && p.powerUpButton != null)
            {
                // Buscar o crear el componente PowerUpDurationUI
                PowerUpDurationUI durationUI = p.powerUpButton.GetComponent<PowerUpDurationUI>();
                if (durationUI == null)
                {
                    durationUI = p.powerUpButton.gameObject.AddComponent<PowerUpDurationUI>();
                }
                
                // Actualizar el temporizador
                durationUI.UpdateTimer(p.durationTimer);
            }
            else if (p.powerUpButton != null)
            {
                // Ocultar el temporizador cuando no está activo
                PowerUpDurationUI durationUI = p.powerUpButton.GetComponent<PowerUpDurationUI>();
                if (durationUI != null)
                {
                    durationUI.HideTimer();
                }
            }
        }
    }

    private void InitializePowerUps()
    {
        foreach (var p in powerUps)
        {
            if (p.powerUpButton != null)
            {
                string nameCopy = p.powerUpName;
                p.powerUpButton.onClick.AddListener(() => ActivatePowerUp(nameCopy));

                // Asegurar que el botón tenga el componente de hover
                if (p.powerUpButton.GetComponent<PowerUpButtonHover>() == null)
                {
                    p.powerUpButton.gameObject.AddComponent<PowerUpButtonHover>();
                    Debug.Log($"[PowerUpManager] Añadido PowerUpButtonHover a {p.powerUpName}");
                }
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
                    // Desactivar efectos espec�ficos
                    switch (p.powerUpName)
                    {
                        case "SlowTime":
                            DeactivateSlowTimePowerUp(p);
                            break;
                            // otros con duraci�n aqu�...
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
        }

        UpdatePowerUpUI(p);
        
        // === NOTIFICAR AL TUTORIAL ===
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnPowerUpActivated(powerUpName);
            Debug.Log($"[PowerUpManager] ✅ Notificado al Tutorial: PowerUp '{powerUpName}' activado");
        }
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

        // Flash azul continuo durante toda la duración del powerup
        if (ScreenFlashEffect.Instance != null && p.duration > 0f)
        {
            StartCoroutine(ContinuousFlashCoroutine(slowTimeFlashColor, p.duration));
        }
        else if (ScreenFlashEffect.Instance != null)
        {
            // Si es instantáneo, un solo flash
            ScreenFlashEffect.Instance.Flash(slowTimeFlashColor, flashDuration);
        }

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
    
    /// <summary>
    /// Corrutina para mantener un flash visual durante toda la duración del powerup
    /// </summary>
    private System.Collections.IEnumerator ContinuousFlashCoroutine(Color flashColor, float duration)
    {
        if (ScreenFlashEffect.Instance == null) yield break;
        
        float elapsed = 0f;
        
        // Mantener el flash visible durante toda la duración
        while (elapsed < duration)
        {
            // Aplicar el flash con una duración que se solape
            ScreenFlashEffect.Instance.Flash(flashColor, Mathf.Min(0.5f, duration - elapsed));
            
            // Esperar un poco antes del siguiente flash (pero menos que la duración para mantener continuidad)
            yield return new WaitForSeconds(0.3f);
            elapsed += 0.3f;
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
            TowerHealthBar[] bars = FindObjectsByType<TowerHealthBar>(FindObjectsSortMode.None);
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
            Debug.LogWarning($"ActivateHealthPowerUp: No se encontr� TowerHealthBar para team '{p.healTargetTeam}'");
            return;
        }

        target.Heal(p.healAmount);
        Debug.Log($"[PowerUpManager] HealthPowerUp: curados {p.healAmount} en {target.gameObject.name}");

        // Flash verde para healing
        if (ScreenFlashEffect.Instance != null)
        {
            ScreenFlashEffect.Instance.Flash(healingFlashColor, flashDuration);
        }

        // si duration == 0, lo consideramos instantáneo y lanzamos cooldown
        if (p.duration <= 0f)
        {
            StartCooldown(p);
            p.isActive = false;
            UpdatePowerUpUI(p);
        }
    }

    #endregion

    // M�todos p�blicos/�tiles
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

    public Button GetPowerUpButton(string powerUpName)
    {
        PowerUpData p = powerUps.Find(x => x.powerUpName == powerUpName);
        return p?.powerUpButton;
    }
}
