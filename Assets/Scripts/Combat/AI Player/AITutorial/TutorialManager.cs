using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Referencias de Juego")]
    public PlayerCardManager playerCardManager;
    public CardManager cardManager;
    public IntelectManager playerIntelect;
    public IntelectManager aiIntelect;
    public PowerUpManager powerUpManager;
    public Transform aiSpawnPoint;
    public Transform playerSpawnPoint;
    public Tower playerTower;
    public Tower aiTower;
    public MonoBehaviour aiController;
    public PlayableAreaUI playableAreaUI;

    [Header("Nueva UI del Tutorial")]
    public GameObject tutorialPanel;
    public Image characterImage;
    public Image speechBubble;
    public TextMeshProUGUI dialogText;
    public Image optionalImage;
    public Image optionalImageAttack;
    public Image optionalImageDefense;
    public Button continueButton;

    [Header("Animación Popup")]
    [Tooltip("Escala objetivo cuando aparece (1 = tamaño normal)")]
    [Range(0.8f, 1.5f)]
    public float popupScaleTarget = 1f;

    [Tooltip("Escala de la imagen del personaje (CadetNumeric)")]
    [Range(0.8f, 2f)]
    public float characterImageScale = 1.2f;

    [Tooltip("Escala cuando se oculta (0 = invisible)")]
    [Range(0f, 0.5f)]
    public float popupScaleHidden = 0f;

    [Tooltip("Duración de la animación de popup")]
    [Range(0.1f, 1f)]
    public float popupDuration = 0.3f;

    [Tooltip("Velocidad de animación del popup")]
    [Range(5f, 20f)]
    public float popupAnimationSpeed = 12f;

    [Header("Sprites Contextuales")]
    public Sprite card5Sprite;
    public Sprite card2Sprite;
    public Sprite healthPowerUpSprite;
    public Sprite slowTimePowerUpSprite;
    public Sprite intelectBarIcon;
    public Sprite intelectCost;
    public Sprite attackIcon;
    public Sprite defenseIcon;

    [Header("UI de Highlight")]
    public Image highlightOverlay;
    public RectTransform highlightRect;

    [Header("Referencias para Highlight")]
    [Tooltip("Componente Image del fillScaler para highlight de intelecto")]
    public Image intelectBarFillImage;

    [Header("Tutorial - Restricción de Despliegue")]
    [Tooltip("Si true, solo permite desplegar en la zona izquierda durante el tutorial")]
    public bool restrictToLeftZone = false;

    private int currentStep = 0;
    private bool waitingForPlayerAction = false;
    private bool waitingForContinueButton = false;
    private bool isTutorialPaused = false;
    private bool isPlayerBlocked = false;
    private bool waitingForTroopsDestroyed = false;
    private int aiTroopCount = 0;
    private int playerTroopCount = 0;

    private bool waitingForTowerDamage = false;
    private int playerTowerHealthBeforeAttack = 0;

    private bool waitingForCounterattack = false;
    private int playerIntelectBeforeCounterattack = 0;

    private bool waitingForEnemyTowerDamage = false;
    private int aiTowerHealthBeforeAttack = 0;

    private bool hasShownAttackExplanation = false;
    private bool hasShownDefenseExplanation = false;

    // Control de animación popup
    private bool isPopupVisible = false;
    private Vector3 targetPopupScale = Vector3.zero;
    private Vector3 targetCharacterScale = Vector3.zero;
    private RectTransform characterImageRect;
    private RectTransform speechBubbleRect;
    private RectTransform dialogTextRect;
    private RectTransform optionalImageRect;
    private RectTransform optionalImageAttackRect;
    private RectTransform optionalImageDefenseRect;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (characterImage != null) characterImageRect = characterImage.GetComponent<RectTransform>();
        if (speechBubble != null) speechBubbleRect = speechBubble.GetComponent<RectTransform>();
        if (dialogText != null) dialogTextRect = dialogText.GetComponent<RectTransform>();
        if (optionalImage != null) optionalImageRect = optionalImage.GetComponent<RectTransform>();
        if (optionalImageAttack != null) optionalImageAttackRect = optionalImageAttack.GetComponent<RectTransform>();
        if (optionalImageDefense != null) optionalImageDefenseRect = optionalImageDefense.GetComponent<RectTransform>();
    }

    void Start()
    {
        if (aiController != null)
        {
            aiController.enabled = false;
        }

        if (optionalImage != null)
        {
            optionalImage.gameObject.SetActive(false);
        }

        if (optionalImageAttack != null)
        {
            optionalImageAttack.gameObject.SetActive(false);
        }

        if (optionalImageDefense != null)
        {
            optionalImageDefense.gameObject.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            continueButton.gameObject.SetActive(false);
        }

        SetPopupScale(popupScaleHidden);

        StartCoroutine(RunTutorial());
    }

    void Update()
    {
        if (waitingForTroopsDestroyed)
        {
            int currentAITroops = CountTroopsByTag("AITeam");
            int currentPlayerTroops = CountTroopsByTag("PlayerTeam");

            if (currentAITroops < aiTroopCount || currentPlayerTroops < playerTroopCount)
            {
                waitingForTroopsDestroyed = false;
            }
        }

        if (waitingForTowerDamage && playerTower != null)
        {
            if (playerTower.currentHealth < playerTowerHealthBeforeAttack)
            {
                Debug.Log($"[Tutorial] 💥 Torre dañada! Salud: {playerTowerHealthBeforeAttack} → {playerTower.currentHealth}");
                waitingForTowerDamage = false;
            }
        }

        if (waitingForCounterattack && playerIntelect != null)
        {
            if (playerIntelect.currentIntelect > playerIntelectBeforeCounterattack)
            {
                Debug.Log($"[Tutorial] ⚔️ ¡Contraataque exitoso! Intelecto: {playerIntelectBeforeCounterattack} → {playerIntelect.currentIntelect}");
                waitingForCounterattack = false;
            }
        }

        if (waitingForEnemyTowerDamage && aiTower != null)
        {
            if (aiTower.currentHealth < aiTowerHealthBeforeAttack)
            {
                Debug.Log($"[Tutorial] 💥 Torre enemiga dañada! Salud: {aiTowerHealthBeforeAttack} → {aiTower.currentHealth}");
                waitingForEnemyTowerDamage = false;
            }
        }

        AnimatePopupScale();
    }

    void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonClicked);
        }
    }

    // ========== ANIMACIÓN POPUP ==========

    private void AnimatePopupScale()
    {
        float speed = Time.unscaledDeltaTime * popupAnimationSpeed;

        if (characterImageRect != null && characterImageRect.localScale != targetCharacterScale)
        {
            characterImageRect.localScale = Vector3.Lerp(characterImageRect.localScale, targetCharacterScale, speed);
        }

        if (speechBubbleRect != null && speechBubbleRect.localScale != targetPopupScale)
        {
            speechBubbleRect.localScale = Vector3.Lerp(speechBubbleRect.localScale, targetPopupScale, speed);
        }

        if (dialogTextRect != null && dialogTextRect.localScale != targetPopupScale)
        {
            dialogTextRect.localScale = Vector3.Lerp(dialogTextRect.localScale, targetPopupScale, speed);
        }

        if (optionalImageRect != null && optionalImage.gameObject.activeSelf && optionalImageRect.localScale != targetPopupScale)
        {
            optionalImageRect.localScale = Vector3.Lerp(optionalImageRect.localScale, targetPopupScale, speed);
        }

        if (optionalImageAttackRect != null && optionalImageAttack.gameObject.activeSelf && optionalImageAttackRect.localScale != targetPopupScale)
        {
            optionalImageAttackRect.localScale = Vector3.Lerp(optionalImageAttackRect.localScale, targetPopupScale, speed);
        }

        if (optionalImageDefenseRect != null && optionalImageDefense.gameObject.activeSelf && optionalImageDefenseRect.localScale != targetPopupScale)
        {
            optionalImageDefenseRect.localScale = Vector3.Lerp(optionalImageDefenseRect.localScale, targetPopupScale, speed);
        }
    }

    private void SetPopupScale(float scale)
    {
        Vector3 scaleVector = Vector3.one * scale;
        Vector3 characterScale = Vector3.one * scale * characterImageScale;

        if (characterImageRect != null) characterImageRect.localScale = characterScale;
        if (speechBubbleRect != null) speechBubbleRect.localScale = scaleVector;
        if (dialogTextRect != null) dialogTextRect.localScale = scaleVector;
        if (optionalImageRect != null) optionalImageRect.localScale = scaleVector;
        if (optionalImageAttackRect != null) optionalImageAttackRect.localScale = scaleVector;
        if (optionalImageDefenseRect != null) optionalImageDefenseRect.localScale = scaleVector;

        targetPopupScale = scaleVector;
        targetCharacterScale = characterScale;
    }

    private void ShowPopupWithAnimation()
    {
        if (!isPopupVisible)
        {
            tutorialPanel.SetActive(true);
            targetPopupScale = Vector3.one * popupScaleTarget;
            targetCharacterScale = Vector3.one * popupScaleTarget * characterImageScale;
            isPopupVisible = true;
        }
    }

    private IEnumerator HidePopupWithAnimation()
    {
        if (isPopupVisible)
        {
            targetPopupScale = Vector3.one * popupScaleHidden;
            targetCharacterScale = Vector3.one * popupScaleHidden;

            yield return new WaitForSecondsRealtime(popupDuration);

            tutorialPanel.SetActive(false);
            isPopupVisible = false;
        }
    }

    private void UpdatePopupContent(string message, bool showImage, Sprite contextSprite = null)
    {
        dialogText.text = message;

        if (showImage && contextSprite != null && optionalImage != null)
        {
            optionalImage.gameObject.SetActive(true);
            optionalImage.sprite = contextSprite;

            if (optionalImageRect != null)
                optionalImageRect.localScale = targetPopupScale;
        }
        else if (optionalImage != null)
        {
            optionalImage.gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowContinueButtonAfterDelay(float minDelay = 4f)
    {
        yield return new WaitForSeconds(minDelay);

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }

        waitingForContinueButton = true;

        yield return new WaitUntil(() => !waitingForContinueButton);

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }
    }

    private void OnContinueButtonClicked()
    {
        if (waitingForContinueButton)
        {
            waitingForContinueButton = false;
            Debug.Log("[Tutorial] ✅ Botón de continuar presionado");
        }
    }

    private IEnumerator ShowAttackExplanation()
    {
        if (hasShownAttackExplanation) yield break;

        hasShownAttackExplanation = true;

        PauseGame();

        ShowDialog("AIXÍ FUNCIONA L'ATAC!", showImage: false, showSpeechBubble: false);

        HideOptionalImage();

        if (optionalImageAttack != null && attackIcon != null)
        {
            optionalImageAttack.gameObject.SetActive(true);
            optionalImageAttack.sprite = attackIcon;

            if (optionalImageAttackRect != null)
                optionalImageAttackRect.localScale = targetPopupScale;
        }

        yield return StartCoroutine(ShowContinueButtonAfterDelay(4f));

        HideOptionalImageAttack();

        yield return StartCoroutine(HidePopupWithAnimation());

        ResumeGame();
    }

    private IEnumerator ShowDefenseExplanation()
    {
        if (hasShownDefenseExplanation) yield break;

        hasShownDefenseExplanation = true;

        PauseGame();

        ShowDialog("AIXÍ ÉS COM ES DEFENSA!", showImage: false, showSpeechBubble: false);

        HideOptionalImage();

        if (optionalImageDefense != null && defenseIcon != null)
        {
            optionalImageDefense.gameObject.SetActive(true);
            optionalImageDefense.sprite = defenseIcon;

            if (optionalImageDefenseRect != null)
                optionalImageDefenseRect.localScale = targetPopupScale;
        }

        yield return StartCoroutine(ShowContinueButtonAfterDelay(4f));

        HideOptionalImageDefense();

        yield return StartCoroutine(HidePopupWithAnimation());

        ResumeGame();
    }

    // ========== UTILIDADES MODIFICADAS ==========

    private int CountTroopsByTag(string tag)
    {
        Character[] characters = FindObjectsOfType<Character>();
        CharacterCombined[] combined = FindObjectsOfType<CharacterCombined>();

        int count = 0;

        foreach (var character in characters)
        {
            if (character.CompareTag(tag)) count++;
        }

        foreach (var comb in combined)
        {
            if (comb.CompareTag(tag)) count++;
        }

        return count;
    }

    private IEnumerator RunTutorial()
    {
        yield return StartCoroutine(Tutorial_Welcome());
        yield return StartCoroutine(Tutorial_AIAttacks());
        yield return StartCoroutine(Tutorial_PlayerDefends());
        yield return StartCoroutine(Tutorial_WaitForDestruction());
        yield return StartCoroutine(Tutorial_TeachAttack());
        yield return StartCoroutine(Tutorial_PlayerAttacks());
        yield return StartCoroutine(Tutorial_HealthPowerUp());
        yield return StartCoroutine(Tutorial_SlowTimePowerUp());
        yield return StartCoroutine(Tutorial_TowerDestroyed());
    }

    private IEnumerator Tutorial_Welcome()
    {
        currentStep = 0;

        BlockPlayer();
        PauseGame();

        ShowDialog("BENVINGUT AL TUTORIAL!", showImage: false);

        yield return new WaitForSeconds(4f);

        yield return StartCoroutine(HidePopupWithAnimation());

        Debug.Log("[Tutorial] ✅ Bienvenida completada, continuando al paso 1...");
    }

    private IEnumerator Tutorial_AIAttacks()
    {
        currentStep = 1;

        var card2 = cardManager.GetCardByIndex(1);
        var card3 = cardManager.GetCardByIndex(2);
        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;
        cardManager.GenerateCombinedCharacter(card2, card3, spawnPos, 5, '+', "AITeam", out result, aiIntelect);

        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(ShowDefenseExplanation());

        PauseGame();

        ShowDialog("¡EN COMPTE, T'ATACAN!", showImage: false);

        yield return new WaitForSeconds(2f);
    }

    private IEnumerator Tutorial_PlayerDefends()
    {
        currentStep = 2;

        yield return new WaitForSeconds(2.7f);

        UpdatePopupContent("¡DEFENSA-HO AMB EL RESULTAT!", showImage: true, contextSprite: card5Sprite);

        if (playerCardManager.cardSlots.Count > 4)
        {
            Transform card5Slot = playerCardManager.cardSlots[4];

            HighlightCard(card5Slot);

            if (card5Slot.childCount > 0)
            {
                GameObject card5 = card5Slot.GetChild(0).gameObject;
                StartHighlightEffect(card5);
            }
        }

        restrictToLeftZone = true;
        Debug.Log("[Tutorial] 🛡️ Restricción activada: solo se puede desplegar en zona izquierda");

        UnblockPlayer();

        waitingForPlayerAction = true;

        float elapsed = 0f;
        float timeout = 5f;

        while (elapsed < timeout && waitingForPlayerAction)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (waitingForPlayerAction)
        {
            Debug.Log("[Tutorial] ⏰ Timeout del popup, ocultando pero esperando acción del jugador...");
            yield return StartCoroutine(HidePopupWithAnimation());
        }

        yield return new WaitUntil(() => !waitingForPlayerAction);
        Debug.Log("[Tutorial] ✅ Carta 5 jugada!");

        restrictToLeftZone = false;
        Debug.Log("[Tutorial] 🔓 Restricción desactivada: se puede desplegar en cualquier zona");

        if (playerCardManager.cardSlots.Count > 4)
        {
            Transform card5Slot = playerCardManager.cardSlots[4];
            if (card5Slot.childCount > 0)
            {
                GameObject card5 = card5Slot.GetChild(0).gameObject;
                StopHighlightEffect(card5);
            }
        }

        ClearHighlight();
        HideOptionalImage();

        PauseGame();

        BlockPlayer();

        yield return new WaitForSeconds(1f);

        // ✅ MODIFICADO: Ahora con botón de continuar
        ShowDialog("CADA CARTA TE UN COST D'ENERGIA", showImage: true, contextSprite: intelectCost);

        if (intelectBarFillImage != null)
        {
            StartHighlightEffect(intelectBarFillImage.gameObject);
        }

        if (playerIntelect != null && playerIntelect.intelectSlider != null)
        {
            HighlightElement(playerIntelect.intelectSlider.GetComponent<RectTransform>());
            StartHighlightEffect(playerIntelect.intelectSlider.gameObject);
        }

        // ✅ NUEVO: Botón de continuar
        yield return StartCoroutine(ShowContinueButtonAfterDelay(4f));

        if (intelectBarFillImage != null)
        {
            StopHighlightEffect(intelectBarFillImage.gameObject);
        }

        if (playerIntelect != null && playerIntelect.intelectSlider != null)
        {
            StopHighlightEffect(playerIntelect.intelectSlider.gameObject);
        }

        ClearHighlight();
        HideOptionalImage();

        yield return StartCoroutine(HidePopupWithAnimation());
        ResumeGame();
    }

    private IEnumerator Tutorial_WaitForDestruction()
    {
        currentStep = 3;

        yield return new WaitForSeconds(0.5f);

        if (playerIntelect != null)
        {
            playerIntelectBeforeCounterattack = playerIntelect.currentIntelect;
            Debug.Log($"[Tutorial] 💚 Intelecto ANTES del contraataque: {playerIntelectBeforeCounterattack}");
        }

        aiTroopCount = CountTroopsByTag("AITeam");
        playerTroopCount = CountTroopsByTag("PlayerTeam");

        if (aiTroopCount == 0 && playerTroopCount == 0)
        {
            yield return new WaitForSeconds(2.5f);
            PauseGame();
            yield break;
        }

        waitingForTroopsDestroyed = true;

        float timeout = 15f;
        float elapsed = 0f;

        while (waitingForTroopsDestroyed && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (elapsed >= timeout)
        {
            Debug.LogWarning("[Tutorial] ⚠️ TIMEOUT esperando colisión");
            waitingForTroopsDestroyed = false;
        }

        waitingForCounterattack = true;

        elapsed = 0f;
        timeout = 5f;

        while (waitingForCounterattack && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (elapsed >= timeout)
        {
            Debug.LogWarning("[Tutorial] ⚠️ TIMEOUT esperando contraataque");
        }

        yield return new WaitForSeconds(2f);

        PauseGame();

        // ✅ MODIFICADO: Ahora con botón de continuar
        ShowDialog("¡MOLT BÉ, HAS GUANYAT 1 D'ENERGIA!", showImage: true, contextSprite: intelectBarIcon);

        if (intelectBarFillImage != null)
        {
            StartHighlightEffect(intelectBarFillImage.gameObject);
        }

        if (playerIntelect != null && playerIntelect.intelectSlider != null)
        {
            HighlightElement(playerIntelect.intelectSlider.GetComponent<RectTransform>());
            StartHighlightEffect(playerIntelect.intelectSlider.gameObject);
        }

        // ✅ NUEVO: Botón de continuar
        yield return StartCoroutine(ShowContinueButtonAfterDelay(4f));

        if (intelectBarFillImage != null)
        {
            StopHighlightEffect(intelectBarFillImage.gameObject);
        }

        if (playerIntelect != null && playerIntelect.intelectSlider != null)
        {
            StopHighlightEffect(playerIntelect.intelectSlider.gameObject);
        }

        ClearHighlight();
        HideOptionalImage();

        yield return StartCoroutine(HidePopupWithAnimation());
    }

    private IEnumerator Tutorial_TeachAttack()
    {
        currentStep = 4;

        yield return StartCoroutine(ShowAttackExplanation());

        ShowDialog("FES UNA OPERACIO PER ATACAR", showImage: false);
        UnblockPlayer();

        foreach (Transform slot in playerCardManager.cardSlots)
        {
            if (slot.childCount > 0)
            {
                GameObject card = slot.GetChild(0).gameObject;
                StartHighlightEffect(card);
            }
        }

        if (playerCardManager.SumaButton != null)
        {
            StartHighlightEffect(playerCardManager.SumaButton.gameObject);
        }
        if (playerCardManager.RestaButton != null)
        {
            StartHighlightEffect(playerCardManager.RestaButton.gameObject);
        }

        waitingForPlayerAction = true;

        float elapsed = 0f;
        float timeout = 4f;

        while (elapsed < timeout && waitingForPlayerAction)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!waitingForPlayerAction)
        {
            Debug.Log("[Tutorial] ⚡ Jugador actuó rápidamente, saltando timeout del mensaje");
        }
        else
        {
            Debug.Log("[Tutorial] ⏰ Timeout del popup, ocultando pero esperando operación del jugador...");
            yield return StartCoroutine(HidePopupWithAnimation());
        }

        yield return new WaitUntil(() => !waitingForPlayerAction);

        foreach (Transform slot in playerCardManager.cardSlots)
        {
            if (slot.childCount > 0)
            {
                GameObject card = slot.GetChild(0).gameObject;
                StopHighlightEffect(card);
            }
        }

        if (playerCardManager.SumaButton != null)
        {
            StopHighlightEffect(playerCardManager.SumaButton.gameObject);
        }
        if (playerCardManager.RestaButton != null)
        {
            StopHighlightEffect(playerCardManager.RestaButton.gameObject);
        }

        if (isPopupVisible)
        {
            yield return StartCoroutine(HidePopupWithAnimation());
        }

        ClearHighlight();
        ResumeGame();
    }

    private IEnumerator Tutorial_PlayerAttacks()
    {
        currentStep = 5;

        if (aiTower != null)
        {
            aiTowerHealthBeforeAttack = aiTower.currentHealth;
            Debug.Log($"[Tutorial] 🏰 Salud de torre enemiga ANTES del ataque: {aiTowerHealthBeforeAttack}");
        }

        waitingForEnemyTowerDamage = true;
        Debug.Log("[Tutorial] ⏳ Esperando que el ataque llegue a la torre enemiga...");

        float timeout = 15f;
        float elapsed = 0f;

        while (waitingForEnemyTowerDamage && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (elapsed >= timeout)
        {
            Debug.LogWarning("[Tutorial] ⚠️ TIMEOUT esperando daño a torre enemiga");
        }

        Debug.Log("[Tutorial] ✅ Ataque del jugador alcanzó la torre enemiga!");

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator Tutorial_HealthPowerUp()
    {
        currentStep = 6;

        BlockPlayer();

        if (playerTower != null)
        {
            playerTowerHealthBeforeAttack = playerTower.currentHealth;
            Debug.Log($"[Tutorial] 💚 Salud de torre ANTES del ataque: {playerTowerHealthBeforeAttack}");
        }

        var card2 = cardManager.GetCardByIndex(1);
        var card1 = cardManager.GetCardByIndex(0);
        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;
        cardManager.GenerateCombinedCharacter(card2, card1, spawnPos, 3, '+', "AITeam", out result, aiIntelect);

        waitingForTowerDamage = true;
        Debug.Log("[Tutorial] ⏳ Esperando que la torre reciba daño...");

        float timeout = 15f;
        float elapsed = 0f;

        while (waitingForTowerDamage && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (elapsed >= timeout)
        {
            Debug.LogWarning("[Tutorial] ⚠️ TIMEOUT esperando daño a la torre");
        }

        PauseGame();

        ShowDialog("OH NO, CURA'T!!!", showImage: true, contextSprite: healthPowerUpSprite);

        var healPowerUp = powerUpManager.GetPowerUpButton("Health");
        if (healPowerUp != null)
        {
            HighlightElement(healPowerUp.GetComponent<RectTransform>());
            StartHighlightEffect(healPowerUp.gameObject);
        }

        UnblockPlayerForPowerUps();

        waitingForPlayerAction = true;

        yield return new WaitUntil(() => !waitingForPlayerAction);

        if (healPowerUp != null)
        {
            StopHighlightEffect(healPowerUp.gameObject);
        }
        ClearHighlight();
        HideOptionalImage();

        UpdatePopupContent("BEN FET!", showImage: false);

        yield return new WaitForSeconds(2.5f);

        yield return StartCoroutine(HidePopupWithAnimation());
        ResumeGame();
    }

    private IEnumerator Tutorial_SlowTimePowerUp()
    {
        currentStep = 7;

        ShowDialog("¡FES QUE VAGIN MÉS LENTS!", showImage: true, contextSprite: slowTimePowerUpSprite);

        var card1A = cardManager.GetCardByIndex(0);
        var card1B = cardManager.GetCardByIndex(0);
        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;
        cardManager.GenerateCombinedCharacter(card1A, card1B, spawnPos, 2, '+', "AITeam", out result, aiIntelect);

        yield return new WaitForSeconds(3f);

        PauseGame();

        var slowPowerUp = powerUpManager.GetPowerUpButton("SlowTime");
        if (slowPowerUp != null)
        {
            HighlightElement(slowPowerUp.GetComponent<RectTransform>());
            StartHighlightEffect(slowPowerUp.gameObject);
        }

        UnblockPlayerForPowerUps();

        waitingForPlayerAction = true;

        Debug.Log("[Tutorial] ⏳ Esperando activación de SlowTime...");

        yield return new WaitUntil(() => !waitingForPlayerAction);
        Debug.Log("[Tutorial] ✅ SlowTime activado!");

        if (slowPowerUp != null)
        {
            StopHighlightEffect(slowPowerUp.gameObject);
        }
        ClearHighlight();
        HideOptionalImage();

        yield return StartCoroutine(HidePopupWithAnimation());

        UnblockPlayer();

        // ✅ SOLUCIÓN: No resetear la velocidad del juego, solo reanudar agentes
        ResumeGameWithoutResetSpeed();

        waitingForPlayerAction = true;

        float elapsed = 0f;
        float messageTimeout = 1f;

        Debug.Log("[Tutorial] ⏳ Esperando ataque del jugador (1s de gracia)...");

        while (elapsed < messageTimeout && waitingForPlayerAction)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (waitingForPlayerAction)
        {
            Debug.Log("[Tutorial] 💬 Jugador tardó más de 1s, mostrando mensaje de ayuda");

            PauseGame();

            ShowDialog("¡APROFITA PER ATACAR!", showImage: false);

            yield return new WaitForSeconds(3.5f);

            yield return StartCoroutine(HidePopupWithAnimation());

            // ✅ SOLUCIÓN: Reanudar manteniendo SlowTime
            ResumeGameWithoutResetSpeed();
        }
        else
        {
            Debug.Log("[Tutorial] ⚡ Jugador atacó rápidamente, saltando mensaje de ayuda");
        }

        Debug.Log("[Tutorial] ⏳ Esperando ataque definitivo del jugador...");

        yield return new WaitUntil(() => !waitingForPlayerAction);
        Debug.Log("[Tutorial] ✅ Ataque definitivo lanzado!");

        if (aiTower != null)
        {
            aiTowerHealthBeforeAttack = aiTower.currentHealth;
        }

        waitingForEnemyTowerDamage = true;

        float timeout = 15f;
        elapsed = 0f;

        while (waitingForEnemyTowerDamage && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (aiTower != null)
        {
            Debug.Log("[Tutorial] 💥 Aplicando daño adicional de 10 a la torre enemiga para garantizar destrucción");
            aiTower.TakeDamage(10);
        }

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator Tutorial_TowerDestroyed()
    {
        currentStep = 8;

        PauseGame();

        ShowDialog("LA TORRE HA ESTAT DESTRUÏDA!", showImage: false);

        yield return new WaitForSeconds(4f);

        UpdatePopupContent("HAS COMPLETAT EL TUTORIAL!", showImage: false);
        yield return new WaitForSeconds(4f);

        UpdatePopupContent("JA ESTÀS PREPARAT PER\nRECUPERAR LA LLAVOR NUMÈRICA!", showImage: false);
        yield return new WaitForSeconds(4f);

        yield return StartCoroutine(HidePopupWithAnimation());

        Debug.Log("[Tutorial] 🎉 Tutorial completado! Cargando WinScene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("WinScene");
    }

    // ========== UTILIDADES ==========

    private Transform FindCardSlotByValue(int cardValue)
    {
        foreach (Transform slot in playerCardManager.cardSlots)
        {
            if (slot.childCount > 0)
            {
                CardDisplay display = slot.GetComponentInChildren<CardDisplay>();
                if (display != null && display.cardData != null && display.cardData.cardValue == cardValue)
                {
                    return slot;
                }
            }
        }
        return null;
    }

    private void ShowDialog(string message, bool showImage, Sprite contextSprite = null, bool showSpeechBubble = true)
    {
        if (isPopupVisible)
        {
            UpdatePopupContent(message, showImage, contextSprite);
        }
        else
        {
            ShowPopupWithAnimation();
            UpdatePopupContent(message, showImage, contextSprite);
        }

        if (speechBubble != null)
        {
            speechBubble.gameObject.SetActive(showSpeechBubble);
        }
        if (dialogText != null)
        {
            dialogText.gameObject.SetActive(showSpeechBubble);
        }
    }

    private void HideOptionalImage()
    {
        if (optionalImage != null)
        {
            optionalImage.gameObject.SetActive(false);
        }
    }

    private void HideOptionalImageAttack()
    {
        if (optionalImageAttack != null)
        {
            optionalImageAttack.gameObject.SetActive(false);
        }
    }

    private void HideOptionalImageDefense()
    {
        if (optionalImageDefense != null)
        {
            optionalImageDefense.gameObject.SetActive(false);
        }
    }

    private void HideTutorialPanel()
    {
        StartCoroutine(HidePopupWithAnimation());
    }

    private void PauseGame()
    {
        isTutorialPaused = true;

        if (GameSpeedManager.Instance != null)
        {
            GameSpeedManager.Instance.GameSpeedMultiplier = 0f;
        }

        Character[] characters = FindObjectsOfType<Character>();
        foreach (var character in characters)
        {
            var agent = character.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
            }
        }

        CharacterCombined[] combined = FindObjectsOfType<CharacterCombined>();
        foreach (var comb in combined)
        {
            var agent = comb.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
            }
        }
    }

    private void ResumeGame()
    {
        isTutorialPaused = false;

        if (GameSpeedManager.Instance != null)
        {
            GameSpeedManager.Instance.GameSpeedMultiplier = 1f;
        }

        Character[] characters = FindObjectsOfType<Character>();
        foreach (var character in characters)
        {
            var agent = character.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = false;
            }
        }

        CharacterCombined[] combined = FindObjectsOfType<CharacterCombined>();
        foreach (var comb in combined)
        {
            var agent = comb.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = false;
            }
        }

        ClearHighlight();
    }

    // ✅ MODIFICADO: Ya NO resetea GameSpeedMultiplier (mantiene SlowTime activo)
    private void ResumeGameWithoutResetSpeed()
    {
        isTutorialPaused = false;

        // ✅ NO tocar GameSpeedManager.GameSpeedMultiplier
        // El SlowTime sigue activo afectando a los agentes con su multiplicador

        Character[] characters = FindObjectsOfType<Character>();
        foreach (var character in characters)
        {
            var agent = character.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = false;
            }
        }

        CharacterCombined[] combined = FindObjectsOfType<CharacterCombined>();
        foreach (var comb in combined)
        {
            var agent = comb.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = false;
            }
        }

        ClearHighlight();

        Debug.Log("[Tutorial] ✅ Juego reanudado manteniendo SlowTime activo");
    }

    private void BlockPlayer()
    {
        isPlayerBlocked = true;
        if (playerCardManager != null) playerCardManager.enabled = false;
        if (playableAreaUI != null) playableAreaUI.enabled = false;
    }

    private void UnblockPlayer()
    {
        isPlayerBlocked = false;
        if (playerCardManager != null) playerCardManager.enabled = true;
        if (playableAreaUI != null) playableAreaUI.enabled = true;
    }

    private void UnblockPlayerForPowerUps()
    {
        isPlayerBlocked = false;
        if (playerCardManager != null) playerCardManager.enabled = false;
        if (playableAreaUI != null) playableAreaUI.enabled = false;
    }

    private void HighlightCard(Transform cardSlot)
    {
        if (highlightRect != null && cardSlot != null)
        {
            highlightOverlay.gameObject.SetActive(true);
            highlightRect.position = cardSlot.position;
            highlightRect.sizeDelta = cardSlot.GetComponent<RectTransform>().sizeDelta;
        }
    }

    private void HighlightElement(RectTransform element)
    {
        if (highlightRect != null && element != null)
        {
            highlightOverlay.gameObject.SetActive(true);
            highlightRect.position = element.position;
            highlightRect.sizeDelta = element.sizeDelta;
        }
    }

    private void ClearHighlight()
    {
        if (highlightOverlay != null)
        {
            highlightOverlay.gameObject.SetActive(false);
        }
    }

    private void StartHighlightEffect(GameObject target)
    {
        if (target == null) return;
        TutorialHighlight highlight = target.GetComponent<TutorialHighlight>();
        if (highlight != null) highlight.StartHighlight();
    }

    private void StopHighlightEffect(GameObject target)
    {
        if (target == null) return;
        TutorialHighlight highlight = target.GetComponent<TutorialHighlight>();
        if (highlight != null) highlight.StopHighlight();
    }

    public bool IsTutorialPaused()
    {
        return isTutorialPaused;
    }

    public bool IsPlayerBlocked()
    {
        return isPlayerBlocked;
    }

    public bool IsRestrictedToLeftZone()
    {
        return restrictToLeftZone;
    }

    // ========== EVENTOS PÚBLICOS ==========

    public void OnPlayerPlaysCard(int cardValue)
    {
        Debug.Log($"[Tutorial] 📥 OnPlayerPlaysCard - Valor: {cardValue}, Step: {currentStep}, Esperando: {waitingForPlayerAction}");

        if (currentStep == 2 && cardValue == 5 && waitingForPlayerAction)
        {
            Debug.Log("[Tutorial] ✅ Carta 5 aceptada!");
            waitingForPlayerAction = false;
        }
    }

    public void OnPlayerPlaysOperation()
    {
        Debug.Log($"[Tutorial] 📥 OnPlayerPlaysOperation - Step: {currentStep}, Esperando: {waitingForPlayerAction}");

        if ((currentStep == 4 || currentStep == 7) && waitingForPlayerAction)
        {
            Debug.Log("[Tutorial] ✅ Operación aceptada!");
            waitingForPlayerAction = false;
        }
    }

    public void OnPowerUpActivated(string powerUpName)
    {
        Debug.Log($"[Tutorial] 📥 OnPowerUpActivated - PowerUp: {powerUpName}, Step: {currentStep}, Esperando: {waitingForPlayerAction}");

        if (currentStep == 6 && powerUpName == "Health" && waitingForPlayerAction)
        {
            Debug.Log("[Tutorial] ✅ PowerUp Health aceptado!");
            waitingForPlayerAction = false;
        }
        else if (currentStep == 7 && powerUpName == "SlowTime" && waitingForPlayerAction)
        {
            Debug.Log("[Tutorial] ✅ PowerUp SlowTime aceptado!");
            waitingForPlayerAction = false;
        }
    }

    public bool IsValidDeploymentZone(float normalizedX)
    {
        if (IsRestrictedToLeftZone())
        {
            if (normalizedX >= 0.5f) return false;
        }
        return true;
    }
}