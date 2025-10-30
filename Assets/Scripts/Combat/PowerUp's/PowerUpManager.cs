using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [System.Serializable]
    public class PowerUpData
    {
        public string powerUpName;
        public float duration = 0f;
        public float cooldownTime = 10f;
        public Button powerUpButton;
        public Image cooldownFillImage;

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

        // ✅ NUEVO: Control de bloqueo por tutorial
        [HideInInspector] public bool isBlockedByTutorial = false;
    }

    [Header("Power-Up Configuration")]
    [SerializeField] private List<PowerUpData> powerUps = new List<PowerUpData>();

    [Header("Visual Feedback")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color cooldownColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;

    // ✅ NUEVO: Color para powerups bloqueados por tutorial
    [SerializeField] private Color blockedByTutorialColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

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

    private void UpdateActiveDurationTimers()
    {
        foreach (var p in powerUps)
        {
            if (p.isActive && p.duration > 0f && p.powerUpButton != null)
            {
                PowerUpDurationUI durationUI = p.powerUpButton.GetComponent<PowerUpDurationUI>();
                if (durationUI == null)
                {
                    durationUI = p.powerUpButton.gameObject.AddComponent<PowerUpDurationUI>();
                }

                durationUI.UpdateTimer(p.durationTimer);
            }
            else if (p.powerUpButton != null)
            {
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

                if (p.powerUpButton.GetComponent<PowerUpButtonHover>() == null)
                {
                    p.powerUpButton.gameObject.AddComponent<PowerUpButtonHover>();
                }
            }
            
            // ✅ MODIFICADO: Solo bloquear si hay tutorial activo
            if (TutorialManager.Instance != null)
            {
                p.isBlockedByTutorial = true;
                Debug.Log($"[PowerUpManager] PowerUp '{p.powerUpName}' bloqueado (tutorial detectado)");
            }
            else
            {
                p.isBlockedByTutorial = false;
                Debug.Log($"[PowerUpManager] PowerUp '{p.powerUpName}' disponible (sin tutorial)");
            }
            
            UpdatePowerUpUI(p);
        }
    }

    private void UpdateCooldowns()
    {
        // Si los PowerUps están pausados, no actualizar timers
        if (isPaused) return;

        foreach (var p in powerUps)
        {
            if (p.isOnCooldown)
            {
                p.cooldownTimer -= Time.deltaTime;
                if (p.cooldownTimer <= 0f)
                {
                    p.isOnCooldown = false;
                    p.cooldownTimer = 0f;
                    UpdatePowerUpUI(p);
                    continue;
                }
                UpdatePowerUpUI(p);
            }

            if (p.isActive && p.duration > 0f)
            {
                p.durationTimer -= Time.deltaTime;
                if (p.durationTimer <= 0f)
                {
                    p.isActive = false;
                    switch (p.powerUpName)
                    {
                        case "SlowTime":
                            DeactivateSlowTimePowerUp(p);
                            break;
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

        // ✅ NUEVO: Solo verificar bloqueo si hay un tutorial activo
        if (TutorialManager.Instance != null && p.isBlockedByTutorial)
        {
            Debug.LogWarning($"[Tutorial] ⛔ PowerUp '{powerUpName}' bloqueado por tutorial");
            
            if (ScreenFlashEffect.Instance != null)
            {
                ScreenFlashEffect.Instance.Flash();
            }
            
            if (p.powerUpButton != null)
            {
                StartCoroutine(ShakeButton(p.powerUpButton.GetComponent<RectTransform>()));
            }
            
            return;
        }

        if (p.isOnCooldown || p.isActive)
        {
            return;
        }

        // ✅ MODIFICADO: Segunda verificación solo si hay tutorial
        if (TutorialManager.Instance != null && !TutorialManager.Instance.IsPowerUpAllowed(powerUpName))
        {
            Debug.LogWarning($"[Tutorial] ⛔ PowerUp '{powerUpName}' no permitido en este paso");
            
            if (ScreenFlashEffect.Instance != null)
            {
                ScreenFlashEffect.Instance.Flash();
            }
            return;
        }

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

        p.isActive = true;
        p.durationTimer = p.duration;

        if (p.duration <= 0f)
        {
            StartCooldown(p);
            p.isActive = false;
        }

        UpdatePowerUpUI(p);

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

        // ✅ MODIFICADO: Prioridad al bloqueo por tutorial
        if (p.isBlockedByTutorial)
        {
            p.powerUpButton.interactable = false;

            ColorBlock colors = p.powerUpButton.colors;
            colors.normalColor = blockedByTutorialColor;
            colors.disabledColor = blockedByTutorialColor;
            p.powerUpButton.colors = colors;

            // Ocultar fill durante bloqueo
            if (p.cooldownFillImage != null)
            {
                p.cooldownFillImage.fillAmount = 0f;
            }

            return;
        }

        // Lógica normal si NO está bloqueado
        p.powerUpButton.interactable = !p.isOnCooldown && !p.isActive;

        ColorBlock normalColors = p.powerUpButton.colors;
        if (p.isActive)
            normalColors.normalColor = activeColor;
        else if (p.isOnCooldown)
            normalColors.normalColor = cooldownColor;
        else
            normalColors.normalColor = availableColor;

        p.powerUpButton.colors = normalColors;

        if (p.cooldownFillImage != null)
        {
            if (p.isOnCooldown && p.cooldownTime > 0f)
            {
                float fillProgress = p.cooldownTimer / p.cooldownTime;
                p.cooldownFillImage.fillAmount = fillProgress;
            }
            else
            {
                p.cooldownFillImage.fillAmount = 0f;
            }
        }
    }

    // ✅ NUEVO: Efecto de shake para botones bloqueados
    private System.Collections.IEnumerator ShakeButton(RectTransform buttonRect)
    {
        if (buttonRect == null) yield break;

        Vector3 originalPos = buttonRect.anchoredPosition;
        float duration = 0.3f;
        float magnitude = 10f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            buttonRect.anchoredPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        buttonRect.anchoredPosition = originalPos;
    }

    // ✅ NUEVO: Métodos públicos para controlar el bloqueo desde el tutorial
    public void SetPowerUpBlocked(string powerUpName, bool blocked)
    {
        PowerUpData p = powerUps.Find(x => x.powerUpName == powerUpName);
        if (p != null)
        {
            p.isBlockedByTutorial = blocked;
            UpdatePowerUpUI(p);
            Debug.Log($"[PowerUpManager] PowerUp '{powerUpName}' bloqueado: {blocked}");
        }
    }

    public void BlockAllPowerUps()
    {
        foreach (var p in powerUps)
        {
            p.isBlockedByTutorial = true;
            UpdatePowerUpUI(p);
        }
        Debug.Log("[PowerUpManager] 🔒 Todos los powerups bloqueados");
    }

    public void UnblockAllPowerUps()
    {
        foreach (var p in powerUps)
        {
            p.isBlockedByTutorial = false;
            UpdatePowerUpUI(p);
        }
        Debug.Log("[PowerUpManager] 🔓 Todos los powerups desbloqueados");
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

        if (ScreenFlashEffect.Instance != null && p.duration > 0f)
        {
            ScreenFlashEffect.Instance.SetPersistentFilter(slowTimeFlashColor, p.duration);
        }
        else if (ScreenFlashEffect.Instance != null)
        {
            ScreenFlashEffect.Instance.Flash(slowTimeFlashColor, flashDuration);
        }

        if (p.duration <= 0f)
        {
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
    }

    private void ActivateHealthPowerUp(PowerUpData p)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHealPowerUpSFX();
        }

        TowerHealthBar target = null;
        if (TowerManager.Instance != null)
        {
            target = TowerManager.Instance.GetTowerByTeam(p.healTargetTeam);
        }

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
            Debug.LogWarning($"ActivateHealthPowerUp: No se encontró TowerHealthBar para team '{p.healTargetTeam}'");
            return;
        }

        target.Heal(p.healAmount);

        if (ScreenFlashEffect.Instance != null)
        {
            ScreenFlashEffect.Instance.Flash(healingFlashColor, flashDuration);
        }

        if (p.duration <= 0f)
        {
            StartCooldown(p);
            p.isActive = false;
            UpdatePowerUpUI(p);
        }
    }

    #endregion

    public bool IsPowerUpAvailable(string powerUpName)
    {
        PowerUpData p = powerUps.Find(x => x.powerUpName == powerUpName);
        return p != null && !p.isOnCooldown && !p.isActive && !p.isBlockedByTutorial;
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
                switch (p.powerUpName)
                {
                    case "SlowTime":
                        DeactivateSlowTimePowerUp(p);
                        break;
                }

                p.isActive = false;
                p.durationTimer = 0f;

                if (p.powerUpButton != null)
                {
                    PowerUpDurationUI durationUI = p.powerUpButton.GetComponent<PowerUpDurationUI>();
                    if (durationUI != null)
                    {
                        durationUI.HideTimer();
                    }
                }

                UpdatePowerUpUI(p);
            }
        }

        if (ScreenFlashEffect.Instance != null)
        {
            ScreenFlashEffect.Instance.ClearPersistentFilter();
        }
    }

    // ✅ NUEVO: Variable para trackear si los PowerUps están pausados
    private bool isPaused = false;

    /// <summary>
    /// Pausa todos los timers de PowerUps activos
    /// </summary>
    public void PausePowerUps()
    {
        isPaused = true;
        Debug.Log("[PowerUpManager] ⏸️ PowerUps pausados");
    }

    /// <summary>
    /// Reanuda todos los timers de PowerUps activos
    /// </summary>
    public void ResumePowerUps()
    {
        isPaused = false;
        Debug.Log("[PowerUpManager] ▶️ PowerUps reanudados");
    }
}