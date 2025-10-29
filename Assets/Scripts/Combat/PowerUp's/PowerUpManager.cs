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

        // Actualizar fill del cooldown
        if (p.cooldownFillImage != null)
        {
            if (p.isOnCooldown && p.cooldownTime > 0f)
            {
                // Durante cooldown: el fill va de 1 (recién usado) a 0 (listo)
                float fillProgress = p.cooldownTimer / p.cooldownTime;
                p.cooldownFillImage.fillAmount = fillProgress;
            }
            else
            {
                // Cuando está disponible (sin usar), el fill está vacío
                p.cooldownFillImage.fillAmount = 0f;
            }
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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySlowTimePowerUpSFX();
        }

        int affected = GameSpeedManager.Instance.ApplyTagSpeedMultiplier(p.targetTeam, p.slowMultiplier);

        // Filtro azul constante durante toda la duración del powerup
        if (ScreenFlashEffect.Instance != null && p.duration > 0f)
        {
            ScreenFlashEffect.Instance.SetPersistentFilter(slowTimeFlashColor, p.duration);
        }
        else if (ScreenFlashEffect.Instance != null)
        {
            // Si es instantáneo, un solo flash breve
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
    /// Corrutina para mantener un filtro de color constante durante toda la duración del powerup
    /// </summary>
    private System.Collections.IEnumerator ConstantFilterCoroutine(Color filterColor, float duration)
    {
        if (ScreenFlashEffect.Instance == null || ScreenFlashEffect.Instance.flashImage == null) 
            yield break;
        
        Image flashImage = ScreenFlashEffect.Instance.flashImage;
        
        // Fade in rápido al color del filtro
        float fadeInTime = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0, filterColor.a, elapsed / fadeInTime);
            Color c = filterColor;
            c.a = alpha;
            flashImage.color = c;
            yield return null;
        }
        
        // Mantener el filtro constante
        flashImage.color = filterColor;
        yield return new WaitForSeconds(duration - fadeInTime - 0.3f);
        
        // Fade out al final
        float fadeOutTime = 0.3f;
        elapsed = 0f;
        
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(filterColor.a, 0, elapsed / fadeOutTime);
            Color c = filterColor;
            c.a = alpha;
            flashImage.color = c;
            yield return null;
        }
        
        // Asegurarse de que quede invisible
        Color finalColor = filterColor;
        finalColor.a = 0;
        flashImage.color = finalColor;
    }

    private void DeactivateSlowTimePowerUp(PowerUpData p)
    {
        if (GameSpeedManager.Instance == null)
        {
            Debug.LogWarning("DeactivateSlowTime: GameSpeedManager no encontrado.");
            return;
        }

        int restored = GameSpeedManager.Instance.RemoveTagSpeedMultiplier(p.targetTeam);
    }

    private void ActivateHealthPowerUp(PowerUpData p)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHealPowerUpSFX();
        }
        
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

    public void StopAllPowerUps()
    {
        foreach (var p in powerUps)
        {
            if (p.isActive)
            {
                // Detener efectos específicos según el tipo de powerup
                switch (p.powerUpName)
                {
                    case "SlowTime":
                        DeactivateSlowTimePowerUp(p);
                        break;
                        // Agregar casos para otros powerups con efectos persistentes
                }

                // Resetear estado del powerup
                p.isActive = false;
                p.durationTimer = 0f;

                // Ocultar el temporizador de duración en UI
                if (p.powerUpButton != null)
                {
                    PowerUpDurationUI durationUI = p.powerUpButton.GetComponent<PowerUpDurationUI>();
                    if (durationUI != null)
                    {
                        durationUI.HideTimer();
                    }
                }

                // Actualizar UI
                UpdatePowerUpUI(p);
            }
        }

        // Detener todos los filtros visuales persistentes
        if (ScreenFlashEffect.Instance != null)
        {
            ScreenFlashEffect.Instance.ClearPersistentFilter();
        }
    }
}
