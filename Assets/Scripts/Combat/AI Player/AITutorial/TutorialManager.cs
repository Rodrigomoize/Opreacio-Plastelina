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
    public GameTimer gameTimer;

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

    [Header("Tutorial - Visual Feedback")]
    [Tooltip("Color para elementos bloqueados por el tutorial")]
    public Color tutorialBlockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [Tooltip("Color para elementos permitidos en el tutorial")]
    public Color tutorialAllowedColor = Color.white;

    [Header("Tutorial - Restricción de Despliegue")]
    [Tooltip("Si true, solo permite desplegar en la zona izquierda durante el tutorial")]
    public bool restrictToLeftZone = false;

    [Header("Tutorial - Restricción de Acciones")]
    [Tooltip("Si true, solo permite jugar cartas individuales (bloquea operaciones)")]
    public bool allowOnlySingleCards = false;

    [Tooltip("Si true, solo permite jugar operaciones (bloquea cartas individuales)")]
    public bool allowOnlyOperations = false;

    [Tooltip("Array de nombres de powerups permitidos. Si está vacío, permite todos")]
    public string[] allowedPowerUps = new string[0];

    [Tooltip("Si es mayor que 0, solo permite ese número de acciones antes de bloquear")]
    private int allowedActionsRemaining = -1;

    [Tooltip("Si es >= 1, solo permite jugar esta carta específica (bloquea las demás)")]
    private int allowedSpecificCardValue = -1;

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

    private bool isPopupVisible = false;
    private Vector3 targetPopupScale = Vector3.zero;
    private Vector3 targetCharacterScale = Vector3.zero;
    private RectTransform characterImageRect;
    private RectTransform speechBubbleRect;
    private RectTransform dialogTextRect;
    private RectTransform optionalImageRect;
    private RectTransform optionalImageAttackRect;
    private RectTransform optionalImageDefenseRect;

    private Vector3 initialCharacterLocalScale = Vector3.one;
    private Vector3 initialSpeechBubbleLocalScale = Vector3.one;
    private Vector3 initialDialogLocalScale = Vector3.one;
    private Vector3 initialOptionalImageLocalScale = Vector3.one;
    private Vector3 initialOptionalImageAttackLocalScale = Vector3.one;
    private Vector3 initialOptionalImageDefenseLocalScale = Vector3.one;

    private bool step7_hasDefended = false;
    private bool step7_hasAttacked = false;

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

        if (characterImageRect != null) initialCharacterLocalScale = characterImageRect.localScale;
        if (speechBubbleRect != null) initialSpeechBubbleLocalScale = speechBubbleRect.localScale;
        if (dialogTextRect != null) initialDialogLocalScale = dialogTextRect.localScale;
        if (optionalImageRect != null) initialOptionalImageLocalScale = optionalImageRect.localScale;
        if (optionalImageAttackRect != null) initialOptionalImageAttackLocalScale = optionalImageAttackRect.localScale;
        if (optionalImageDefenseRect != null) initialOptionalImageDefenseLocalScale = optionalImageDefenseRect.localScale;

        if (gameTimer == null)
        {
            gameTimer = FindFirstObjectByType<GameTimer>();
        }
    }

    void Start()
    {
        aiController.enabled = false;

        if (optionalImage != null) optionalImage.gameObject.SetActive(false);
        if (optionalImageAttack != null) optionalImageAttack.gameObject.SetActive(false);
        if (optionalImageDefense != null) optionalImageDefense.gameObject.SetActive(false);

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            continueButton.gameObject.SetActive(false);
        }

        SetPopupScale(popupScaleHidden);
        BlockPlayer();

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.BlockAllPowerUps();
        }

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
                waitingForTowerDamage = false;
            }
        }

        if (waitingForCounterattack && playerIntelect != null)
        {
            if (playerIntelect.currentIntelect > playerIntelectBeforeCounterattack)
            {
                waitingForCounterattack = false;
            }
        }

        if (waitingForEnemyTowerDamage && aiTower != null)
        {
            if (aiTower.currentHealth < aiTowerHealthBeforeAttack)
            {
                waitingForEnemyTowerDamage = false;

                if (currentStep == 7)
                {
                    aiTower.TakeDamage(10);
                }
            }
        }

        AnimatePopupScale();

        if (!isPlayerBlocked)
        {
            UpdateTutorialVisualFeedback();
        }
    }

    void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonClicked);
        }
    }

    // ========================================
    // HELPERS DE BLOQUEO DE CARTAS
    // ========================================

    /// <summary>
    /// Bloquea todas las cartas en modo INTERACTIVO (click para desbloquear)
    /// Útil cuando quieres que el jugador vea grises pero pueda interactuar
    /// </summary>
    private void BlockAllCardsInteractive()
    {
        if (playerCardManager == null) return;

        foreach (Transform slot in playerCardManager.cardSlots)
        {
            if (slot.childCount == 0) continue;

            CardDisplay display = slot.GetComponentInChildren<CardDisplay>();
            if (display == null) continue;

            TutorialHighlight highlight = display.GetComponent<TutorialHighlight>();
            if (highlight == null)
            {
                highlight = display.gameObject.AddComponent<TutorialHighlight>();
            }

            highlight.HideCard_ClickToReveal();
        }

        Debug.Log("[Tutorial] 🔒 Cartas bloqueadas (Click para revelar)");
    }

    /// <summary>
    /// Bloquea todas las cartas en modo BLOQUEADO (solo tutorial puede desbloquear)
    /// Útil para explicaciones donde el jugador NO debe interactuar
    /// </summary>
    private void BlockAllCardsLocked()
    {
        if (playerCardManager == null) return;

        foreach (Transform slot in playerCardManager.cardSlots)
        {
            if (slot.childCount == 0) continue;

            CardDisplay display = slot.GetComponentInChildren<CardDisplay>();
            if (display == null) continue;

            TutorialHighlight highlight = display.GetComponent<TutorialHighlight>();
            if (highlight == null)
            {
                highlight = display.gameObject.AddComponent<TutorialHighlight>();
            }

            // Forzar reset del estado antes de bloquearlo
            highlight.RevealCard(force: true); // Primero reseteamos cualquier estado previo
            highlight.ForceBlockedState(true); // Luego forzamos el estado bloqueado

            // También reseteamos el color de la carta si tiene Image
            Image cardImage = display.GetComponent<Image>();
            if (cardImage != null)
            {
                cardImage.color = tutorialBlockedColor;
            }
        }

        Debug.Log("[Tutorial] 🔒 Cartas bloqueadas (Locked mode)");
    }

    private void UnblockAllCards()
    {
        if (playerCardManager == null) return;

        foreach (Transform slot in playerCardManager.cardSlots)
        {
            if (slot.childCount == 0) continue;

            CardDisplay display = slot.GetComponentInChildren<CardDisplay>();
            if (display == null) continue;

            TutorialHighlight highlight = display.GetComponent<TutorialHighlight>();
            if (highlight != null)
            {
                highlight.RevealCard(force: true);
                highlight.ForceBlockedState(false);
            }

            // Restaurar color original
            Image cardImage = display.GetComponent<Image>();
            if (cardImage != null)
            {
                cardImage.color = Color.white; // O el color original que debería tener
            }
        }

        Debug.Log("[Tutorial] 🔓 Todas las cartas desbloqueadas");
    }


    private void BlockOperatorsInteractive()
    {
        if (playerCardManager == null) return;

        // Bloquear botón Suma
        if (playerCardManager.SumaButton != null)
        {
            TutorialHighlight sumHighlight = playerCardManager.SumaButton.GetComponent<TutorialHighlight>();
            if (sumHighlight == null)
            {
                sumHighlight = playerCardManager.SumaButton.gameObject.AddComponent<TutorialHighlight>();
            }
            sumHighlight.HideCard_ClickToReveal();
        }

        // Bloquear botón Resta
        if (playerCardManager.RestaButton != null)
        {
            TutorialHighlight restHighlight = playerCardManager.RestaButton.GetComponent<TutorialHighlight>();
            if (restHighlight == null)
            {
                restHighlight = playerCardManager.RestaButton.gameObject.AddComponent<TutorialHighlight>();
            }
            restHighlight.HideCard_ClickToReveal();
        }

        Debug.Log("[Tutorial] 🔒 Operadores bloqueados (Click para revelar)");
    }

    /// <summary>
    /// Bloquea los botones de operadores en modo BLOQUEADO (solo tutorial puede desbloquear)
    /// </summary>
    private void BlockOperatorsLocked()
    {
        if (playerCardManager == null) return;

        // Bloquear botón Suma
        if (playerCardManager.SumaButton != null)
        {
            TutorialHighlight sumHighlight = playerCardManager.SumaButton.GetComponent<TutorialHighlight>();
            if (sumHighlight == null)
            {
                sumHighlight = playerCardManager.SumaButton.gameObject.AddComponent<TutorialHighlight>();
            }
            sumHighlight.ForceBlockedState(true);

            // Deshabilitar interacción
            playerCardManager.SumaButton.interactable = false;
        }

        // Bloquear botón Resta
        if (playerCardManager.RestaButton != null)
        {
            TutorialHighlight restHighlight = playerCardManager.RestaButton.GetComponent<TutorialHighlight>();
            if (restHighlight == null)
            {
                restHighlight = playerCardManager.RestaButton.gameObject.AddComponent<TutorialHighlight>();
            }
            restHighlight.ForceBlockedState(true);

            // Deshabilitar interacción
            playerCardManager.RestaButton.interactable = false;
        }

        Debug.Log("[Tutorial] 🔒 Operadores bloqueados (Locked mode)");
    }

    /// <summary>
    /// Desbloquea los botones de operadores (revela y restaura)
    /// </summary>
    private void UnblockOperators()
    {
        if (playerCardManager == null) return;

        // Desbloquear botón Suma
        if (playerCardManager.SumaButton != null)
        {
            TutorialHighlight sumHighlight = playerCardManager.SumaButton.GetComponent<TutorialHighlight>();
            if (sumHighlight != null)
            {
                sumHighlight.RevealCard(force: true);
                sumHighlight.ForceBlockedState(false);
            }

            playerCardManager.SumaButton.interactable = true;

            // Restaurar color
            Image sumImage = playerCardManager.SumaButton.GetComponent<Image>();
            if (sumImage != null)
            {
                sumImage.color = playerCardManager.validOperatorColor;
            }
        }

        // Desbloquear botón Resta
        if (playerCardManager.RestaButton != null)
        {
            TutorialHighlight restHighlight = playerCardManager.RestaButton.GetComponent<TutorialHighlight>();
            if (restHighlight != null)
            {
                restHighlight.RevealCard(force: true);
                restHighlight.ForceBlockedState(false);
            }

            playerCardManager.RestaButton.interactable = true;

            // Restaurar color
            Image restImage = playerCardManager.RestaButton.GetComponent<Image>();
            if (restImage != null)
            {
                restImage.color = playerCardManager.validOperatorColor;
            }
        }
    }

    // ========================================
    // HELPERS COMBINADOS (CARTAS + OPERADORES)
    // ========================================

    /// <summary>
    /// Bloquea TODO (cartas + operadores) en modo INTERACTIVO
    /// </summary>
    private void BlockAllInteractive()
    {
        BlockAllCardsInteractive();
        BlockOperatorsInteractive();
        Debug.Log("[Tutorial] 🔒 Todo bloqueado (modo interactivo)");
    }

    /// <summary>
    /// Bloquea TODO (cartas + operadores) en modo BLOQUEADO
    /// </summary>
    private void BlockAllLocked()
    {
        BlockAllCardsLocked();
        BlockOperatorsLocked();
        Debug.Log("[Tutorial] 🔒 Todo bloqueado (modo locked)");
    }

    /// <summary>
    /// Desbloquea TODO (cartas + operadores)
    /// </summary>
    private void UnblockAll()
    {
        UnblockAllCards();
        UnblockOperators();
        Debug.Log("[Tutorial] 🔓 Todo desbloqueado");
    }

    // ========================================
    // SISTEMA DE CONTINUE BUTTON
    // ========================================

    private IEnumerator ShowContinueButtonAfterDelay(float minDelay = 2f)
    {
        yield return new WaitForSecondsRealtime(minDelay);

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
            Debug.Log("[Tutorial] ✅ Continue button presionado");
        }
    }

    // ========================================
    // FLUJO PRINCIPAL DEL TUTORIAL
    // ========================================

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

    // ========================================
    // PASO 0: BIENVENIDA
    // ========================================

    private IEnumerator Tutorial_Welcome()
    {
        currentStep = 0;

        PauseGame();
        BlockPlayer();
        BlockAllInteractive();

        ShowDialog("BENVINGUT AL TUTORIAL!", showImage: false);
        yield return StartCoroutine(ShowContinueButtonAfterDelay(2f));
        yield return StartCoroutine(HidePopupWithAnimation());

    }

    // ========================================
    // PASO 1: LA IA ATACA
    // ========================================

    private IEnumerator Tutorial_AIAttacks()
    {
        currentStep = 1;

        // Generar enemigo
        var card2 = cardManager.GetCardByIndex(1);
        var card3 = cardManager.GetCardByIndex(2);
        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;
        cardManager.GenerateCombinedCharacter(card2, card3, spawnPos, 5, '+', "AITeam", out result, aiIntelect);

        yield return new WaitForSeconds(4f);

        PauseGame();
        BlockPlayer();
        BlockAllInteractive();

        ShowDialog("VIGILA, T'ATAQUEN!", showImage: false);
        yield return StartCoroutine(ShowContinueButtonAfterDelay(2f));
        yield return StartCoroutine(HidePopupWithAnimation());

        PauseGame();
        // Mostrar explicación de defensa
        yield return StartCoroutine(ShowDefenseExplanation());

        UnblockAll();
    }

    // ========================================
    // PASO 2: EL JUGADOR DEFIENDE
    // ========================================

    private IEnumerator Tutorial_PlayerDefends()
    {
        currentStep = 2;

        allowOnlySingleCards = true;
        allowOnlyOperations = false;
        allowedActionsRemaining = 1;
        allowedSpecificCardValue = 5;
        restrictToLeftZone = true;

        PauseGame();
        BlockPlayer();
        BlockAllCardsLocked();

        ShowDialog("DEFENSA-HO AMB EL RESULTAT!", showImage: true, contextSprite: card5Sprite);

        // Resaltar carta 5
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

        ForceUpdateTutorialVisualFeedback();

        yield return StartCoroutine(ShowContinueButtonAfterDelay(3f));
        yield return StartCoroutine(HidePopupWithAnimation());

        // Desbloquear para jugar
        UnblockAll();
        UnblockPlayer();

        waitingForPlayerAction = true;
        yield return new WaitUntil(() => !waitingForPlayerAction);

        // Limpiar highlights
        if (playerCardManager.cardSlots.Count > 4)
        {
            Transform card5Slot = playerCardManager.cardSlots[4];
            if (card5Slot.childCount > 0)
            {
                GameObject card5 = card5Slot.GetChild(0).gameObject;
                StopHighlightEffect(card5);
            }
        }
        BlockAllLocked();

        yield return new WaitForSeconds(1);
        // Popup de explicación de coste de intelecto
        PauseGame();

        ClearHighlight();
        HideOptionalImage();

        // Limpiar restricciones
        restrictToLeftZone = false;
        allowOnlySingleCards = false;
        allowedActionsRemaining = -1;
        allowedSpecificCardValue = -1;


        BlockPlayer();
        BlockAllLocked();

        ShowDialog("CADA CARTA TÉ UN COST D'ENERGIA", showImage: true, contextSprite: intelectCost);

        if (intelectBarFillImage != null)
        {
            StartHighlightEffect(intelectBarFillImage.gameObject);
        }

        if (playerIntelect != null && playerIntelect.intelectSlider != null)
        {
            HighlightElement(playerIntelect.intelectSlider.GetComponent<RectTransform>());
            StartHighlightEffect(playerIntelect.intelectSlider.gameObject);
        }

        ForceUpdateTutorialVisualFeedback();

        yield return StartCoroutine(ShowContinueButtonAfterDelay(3f));

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

    // ========================================
    // PASO 3: ESPERAR DESTRUCCIÓN Y GANAR INTELECTO
    // ========================================

    private IEnumerator Tutorial_WaitForDestruction()
    {
        currentStep = 3;

        yield return new WaitForSeconds(1f);
        BlockAllLocked();
        if (playerIntelect != null)
        {
            playerIntelectBeforeCounterattack = playerIntelect.currentIntelect;
        }

        aiTroopCount = CountTroopsByTag("AITeam");
        playerTroopCount = CountTroopsByTag("PlayerTeam");

        if (aiTroopCount == 0 && playerTroopCount == 0)
        {
            yield return new WaitForSeconds(2f);
        }
        else
        {
            waitingForTroopsDestroyed = true;

            float timeout = 15f;
            float elapsed = 0f;

            while (waitingForTroopsDestroyed && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            waitingForCounterattack = true;

            elapsed = 0f;
            timeout = 5f;

            while (waitingForCounterattack && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(2f);
        }

        PauseGame();
        BlockPlayer();
        BlockAllLocked();

        ShowDialog("MOLT BÉ, HAS GUANYAT + 1 D'ENERGIA!", showImage: true, contextSprite: intelectBarIcon);

        if (intelectBarFillImage != null)
        {
            StartHighlightEffect(intelectBarFillImage.gameObject);
        }

        if (playerIntelect != null && playerIntelect.intelectSlider != null)
        {
            HighlightElement(playerIntelect.intelectSlider.GetComponent<RectTransform>());
            StartHighlightEffect(playerIntelect.intelectSlider.gameObject);
        }

        ForceUpdateTutorialVisualFeedback();

        yield return StartCoroutine(ShowContinueButtonAfterDelay(3f));

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

        UnblockAll();
        ResumeGame();
    }

    // ========================================
    // PASO 4: ENSEÑAR A ATACAR
    // ========================================

    private IEnumerator Tutorial_TeachAttack()
    {
        currentStep = 4;

        yield return StartCoroutine(ShowAttackExplanation());

        allowOnlySingleCards = false;
        allowOnlyOperations = true;
        allowedActionsRemaining = 1;

        PauseGame();
        BlockPlayer();
        BlockAllLocked();

        ShowDialog("FES UNA OPERACIÓ PER ATACAR", showImage: false);

        UnblockAll();
        UnblockAllCards();

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

        yield return StartCoroutine(ShowContinueButtonAfterDelay(3f));
        yield return StartCoroutine(HidePopupWithAnimation());


        UnblockPlayer();
        ResumeGame();

        waitingForPlayerAction = true;
        yield return new WaitUntil(() => !waitingForPlayerAction);

        allowOnlyOperations = false;
        allowedActionsRemaining = -1;

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

        ClearHighlight();
    }

    // ========================================
    // PASO 5: EL JUGADOR ATACA
    // ========================================

    private IEnumerator Tutorial_PlayerAttacks()
    {
        currentStep = 5;

        BlockAllLocked();

        if (aiTower != null)
        {
            aiTowerHealthBeforeAttack = aiTower.currentHealth;
        }

        waitingForEnemyTowerDamage = true;

        yield return new WaitUntil(() => !waitingForEnemyTowerDamage);

        yield return new WaitForSeconds(1f);
    }

    // ========================================
    // PASO 6: POWERUP DE SALUD
    // ========================================

    private IEnumerator Tutorial_HealthPowerUp()
    {
        currentStep = 6;

        PauseGame();
        BlockPlayer();
        BlockAllLocked();

        if (playerTower != null)
        {
            playerTowerHealthBeforeAttack = playerTower.currentHealth;
        }

        // ✅ FIX: Generar enemigo 2+1 = 3 ANTES de reanudar
        var card2 = cardManager.GetCardByIndex(1);  // Carta 2
        var card1 = cardManager.GetCardByIndex(0);  // Carta 1
        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;

        Debug.Log("[Tutorial] 🔧 Generando enemigo 2+1 para paso 6...");

        // Generar con intelecto de IA suficiente
        if (aiIntelect != null && aiIntelect.currentIntelect < 3)
        {
            aiIntelect.AddIntelect(3 - aiIntelect.currentIntelect);
        }

        bool success = cardManager.GenerateCombinedCharacter(card2, card1, spawnPos, 3, '+', "AITeam", out result, aiIntelect);

        if (success)
        {
            Debug.Log("[Tutorial] ✅ Enemigo 2+1 generado correctamente");
        }
        else
        {
            Debug.LogError("[Tutorial] ❌ ERROR: No se pudo generar enemigo 2+1");
        }

        // Reanudar DESPUÉS de generar
        ResumeGame();

        waitingForTowerDamage = true;

        float timeout = 15f;
        float elapsed = 0f;

        while (waitingForTowerDamage && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        PauseGame();
        BlockPlayer();
        BlockAllLocked();

        ShowDialog("OH NO, CURA'T!", showImage: true, contextSprite: healthPowerUpSprite);

        yield return StartCoroutine(ShowContinueButtonAfterDelay(2f));
        yield return StartCoroutine(HidePopupWithAnimation());


        allowedPowerUps = new string[] { "Health" };

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.SetPowerUpBlocked("Health", false);
        }

        var healPowerUp = powerUpManager.GetPowerUpButton("Health");
        if (healPowerUp != null)
        {
            HighlightElement(healPowerUp.GetComponent<RectTransform>());
            StartHighlightEffect(healPowerUp.gameObject);
        }
        UnblockPlayerForPowerUps();

        waitingForPlayerAction = true;
        yield return new WaitUntil(() => !waitingForPlayerAction);

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.SetPowerUpBlocked("Health", true);
        }

        allowedPowerUps = new string[0];

        if (healPowerUp != null)
        {
            StopHighlightEffect(healPowerUp.gameObject);
        }
        ClearHighlight();
        HideOptionalImage();

        PauseGame();
        BlockPlayer();
        BlockAllLocked();

        ShowDialog("BEN FET!", showImage: false);
        yield return StartCoroutine(ShowContinueButtonAfterDelay(2f));
        yield return StartCoroutine(HidePopupWithAnimation());

        UnblockAll();
        ResumeGame();
    }

    // ========================================
    // PASO 7: POWERUP SLOWTIME + DEFENSA + ATAQUE
    // ========================================

    private IEnumerator Tutorial_SlowTimePowerUp()
    {
        currentStep = 7;

        // Generar enemigo 1+1
        var card1A = cardManager.GetCardByIndex(0);
        var card1B = cardManager.GetCardByIndex(0);
        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;

        cardManager.GenerateCombinedCharacter(card1A, card1B, spawnPos, 2, '+', "AITeam", out result, aiIntelect);

        yield return new WaitForSeconds(2.2f);

        PauseGame();
        BlockPlayer();
        BlockAllLocked();

        ShowDialog("FES QUE VAGIN MÉS LENTS!", showImage: true, contextSprite: slowTimePowerUpSprite);

        yield return StartCoroutine(ShowContinueButtonAfterDelay(2f));
        yield return StartCoroutine(HidePopupWithAnimation());

        allowedPowerUps = new string[] { "SlowTime" };

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.SetPowerUpBlocked("SlowTime", false);
        }

        var slowPowerUp = powerUpManager.GetPowerUpButton("SlowTime");
        if (slowPowerUp != null)
        {
            HighlightElement(slowPowerUp.GetComponent<RectTransform>());
            StartHighlightEffect(slowPowerUp.gameObject);
        }

        UnblockPlayerForPowerUps();
        UnblockAll();
        waitingForPlayerAction = true;

        yield return new WaitUntil(() => !waitingForPlayerAction);

        ResumeGame();

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.SetPowerUpBlocked("SlowTime", true);
        }

        allowedPowerUps = new string[0];

        if (slowPowerUp != null)
        {
            StopHighlightEffect(slowPowerUp.gameObject);
        }
        ClearHighlight();
        HideOptionalImage();

        yield return new WaitForSeconds(1f);

        PauseGame();
        BlockPlayer();

        if (GameSpeedManager.Instance != null)
        {
            GameSpeedManager.Instance.RemoveActiveTagMultiplier("AITeam");
            GameSpeedManager.Instance.ApplyTagSpeedMultiplier("PlayerTeam", 5.0f);
        }

        BlockAllCardsLocked();

        step7_hasDefended = false;
        step7_hasAttacked = false;
        allowedActionsRemaining = 2;
        allowOnlySingleCards = true;
        allowOnlyOperations = false;
        restrictToLeftZone = true;
        allowedSpecificCardValue = 2;

        ShowDialog("PRIMER, DEFENSA'T AMB EL 2!", showImage: true, contextSprite: card2Sprite);

        if (playerCardManager.cardSlots.Count > 1)
        {
            Transform card2Slot = playerCardManager.cardSlots[1];
            HighlightCard(card2Slot);

            if (card2Slot.childCount > 0)
            {
                GameObject card2 = card2Slot.GetChild(0).gameObject;
                StartHighlightEffect(card2);
            }
        }

        ForceUpdateTutorialVisualFeedback();

        yield return StartCoroutine(ShowContinueButtonAfterDelay(3f));
        yield return StartCoroutine(HidePopupWithAnimation());

        UnblockAll();
        UnblockAllCards();
        UnblockPlayer();
        ResumeGameWithoutResetSpeed();

        waitingForPlayerAction = true;
        yield return new WaitUntil(() => step7_hasDefended);

        yield return new WaitForSeconds(1.1f);
        ResumeGameWithoutResetSpeed();
        PauseGame();
        BlockPlayer();

        allowedSpecificCardValue = -1;

        if (playerCardManager.cardSlots.Count > 1)
        {
            Transform card2Slot = playerCardManager.cardSlots[1];
            if (card2Slot.childCount > 0)
            {
                GameObject card2 = card2Slot.GetChild(0).gameObject;
                StopHighlightEffect(card2);
            }
        }

        ClearHighlight();

        allowOnlySingleCards = false;
        allowOnlyOperations = true;
        restrictToLeftZone = false;

        BlockAllCardsLocked();

        ShowDialog("ARA, APROFITA PER ATACAR!", showImage: false);

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

        yield return StartCoroutine(ShowContinueButtonAfterDelay(3f));
        yield return StartCoroutine(HidePopupWithAnimation());

        UnblockAll();
        UnblockPlayer();
        ResumeGameWithoutResetSpeed();

        waitingForPlayerAction = true;
        yield return new WaitUntil(() => step7_hasAttacked);

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

        BlockPlayer();
        allowedActionsRemaining = -1;
        allowOnlySingleCards = false;
        allowOnlyOperations = false;
        restrictToLeftZone = false;
        allowedSpecificCardValue = -1;

        if (aiTower != null)
        {
            aiTowerHealthBeforeAttack = aiTower.currentHealth;
        }

        waitingForEnemyTowerDamage = true;
        yield return new WaitUntil(() => !waitingForEnemyTowerDamage);
        yield return new WaitForSeconds(1f);
    }

    // ========================================
    // PASO 8: TORRE DESTRUIDA - FIN
    // ========================================

    private IEnumerator Tutorial_TowerDestroyed()
    {
        currentStep = 8;

        PauseGame();
        BlockPlayer();
        BlockAllLocked();

        ShowDialog("HAS DESTRUÏT LA TORRE...", showImage: false);
        yield return StartCoroutine(ShowContinueButtonAfterDelay(2f));

        UpdatePopupContent("I COMPLETAT EL TUTORIAL!", showImage: false);
        yield return StartCoroutine(ShowContinueButtonAfterDelay(2f));

        UpdatePopupContent("JA ESTÀS PREPARAT!", showImage: false);
        yield return StartCoroutine(ShowContinueButtonAfterDelay(2f));

        yield return StartCoroutine(HidePopupWithAnimation());

        yield return new WaitForSeconds(1f);

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.UnblockAllPowerUps();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("WinScene");
    }

    // ========================================
    // EXPLICACIONES DE ATAQUE Y DEFENSA
    // ========================================

    private IEnumerator ShowAttackExplanation()
    {
        if (hasShownAttackExplanation) yield break;

        hasShownAttackExplanation = true;

        yield return new WaitForSeconds(1f);

        PauseGame();
        BlockPlayer();
        BlockAllCardsLocked();

        ShowDialog("AIXÍ FUNCIONA L'ATAC!", showImage: false, showSpeechBubble: false);

        HideOptionalImage();

        if (optionalImageAttack != null && attackIcon != null)
        {
            optionalImageAttack.gameObject.SetActive(true);
            optionalImageAttack.sprite = attackIcon;
        }

        ForceUpdateTutorialVisualFeedback();

        yield return StartCoroutine(ShowContinueButtonAfterDelay(2f));

        HideOptionalImageAttack();

        yield return StartCoroutine(HidePopupWithAnimation());

        UnblockAllCards();
    }

    private IEnumerator ShowDefenseExplanation()
    {
        if (hasShownDefenseExplanation) yield break;

        hasShownDefenseExplanation = true;

        yield return new WaitForSeconds(1f);

        PauseGame();
        BlockPlayer();
        BlockAllCardsLocked();

        ShowDialog("AIXÍ ÉS COM ES DEFENSA!", showImage: false, showSpeechBubble: false);

        HideOptionalImage();

        if (optionalImageDefense != null && defenseIcon != null)
        {
            optionalImageDefense.gameObject.SetActive(true);
            optionalImageDefense.sprite = defenseIcon;
        }

        ForceUpdateTutorialVisualFeedback();

        yield return StartCoroutine(ShowContinueButtonAfterDelay(2f));

        HideOptionalImageDefense();

        yield return StartCoroutine(HidePopupWithAnimation());

        UnblockAllCards();
    }

    // ========================================
    // MÉTODOS DE FEEDBACK VISUAL
    // ========================================

    private void UpdateTutorialVisualFeedback()
    {
        if (playerCardManager == null) return;

        foreach (Transform slot in playerCardManager.cardSlots)
        {
            if (slot.childCount == 0) continue;

            CardDisplay display = slot.GetComponentInChildren<CardDisplay>();
            if (display == null || display.cardData == null) continue;

            TutorialHighlight highlight = display.GetComponent<TutorialHighlight>();
            if (highlight == null)
            {
                highlight = display.gameObject.AddComponent<TutorialHighlight>();
            }

            bool hasActiveHighlight = highlight.enabled && highlight != null;
            if (hasActiveHighlight) continue;

            int cardValue = display.cardData.cardValue;
            bool isCardAllowed = IsSpecificCardAllowed(cardValue);

            if (!isCardAllowed || !CanPlaySingleCard() || allowedActionsRemaining == 0)
            {
                highlight.SetBlocked(true);
            }
            else
            {
                highlight.SetBlocked(false);
            }
        }

        UpdateOperatorButtonFeedback();
    }

    private void ForceUpdateTutorialVisualFeedback()
    {
        if (playerCardManager == null) return;

        foreach (Transform slot in playerCardManager.cardSlots)
        {
            if (slot.childCount == 0) continue;

            CardDisplay display = slot.GetComponentInChildren<CardDisplay>();
            if (display == null || display.cardData == null) continue;

            TutorialHighlight highlight = display.GetComponent<TutorialHighlight>();
            if (highlight == null)
            {
                highlight = display.gameObject.AddComponent<TutorialHighlight>();
            }

            int cardValue = display.cardData.cardValue;
            bool isCardAllowed = IsSpecificCardAllowed(cardValue);

            if (!isCardAllowed || !CanPlaySingleCard() || allowedActionsRemaining == 0)
            {
                highlight.SetBlocked(true);
            }
            else
            {
                highlight.SetBlocked(false);
            }
        }

        bool operationsAllowed = CanPlayOperation();

        if (playerCardManager.SumaButton != null)
        {
            Image sumaImage = playerCardManager.SumaButton.GetComponent<Image>();
            if (sumaImage != null)
            {
                if (!operationsAllowed || allowedActionsRemaining == 0)
                {
                    sumaImage.color = tutorialBlockedColor;
                    playerCardManager.SumaButton.interactable = false;
                }
                else
                {
                    sumaImage.color = playerCardManager.validOperatorColor;
                    playerCardManager.SumaButton.interactable = true;
                }
            }
        }

        if (playerCardManager.RestaButton != null)
        {
            Image restaImage = playerCardManager.RestaButton.GetComponent<Image>();
            if (restaImage != null)
            {
                if (!operationsAllowed || allowedActionsRemaining == 0)
                {
                    restaImage.color = tutorialBlockedColor;
                    playerCardManager.RestaButton.interactable = false;
                }
                else
                {
                    restaImage.color = playerCardManager.validOperatorColor;
                    playerCardManager.RestaButton.interactable = true;
                }
            }
        }
    }

    private bool IsSpecificCardAllowed(int cardValue)
    {
        if (allowedSpecificCardValue < 1) return true;

        return cardValue == allowedSpecificCardValue;
    }

    private void UpdateOperatorButtonFeedback()
    {
        if (playerCardManager == null) return;

        bool operationsAllowed = CanPlayOperation();

        if (playerCardManager.SumaButton != null)
        {
            Image sumaImage = playerCardManager.SumaButton.GetComponent<Image>();
            if (sumaImage != null)
            {
                if (!operationsAllowed && allowOnlySingleCards)
                {
                    sumaImage.color = tutorialBlockedColor;
                    playerCardManager.SumaButton.interactable = false;
                }
            }
        }

        if (playerCardManager.RestaButton != null)
        {
            Image restaImage = playerCardManager.RestaButton.GetComponent<Image>();
            if (restaImage != null)
            {
                if (!operationsAllowed && allowOnlySingleCards)
                {
                    restaImage.color = tutorialBlockedColor;
                    playerCardManager.RestaButton.interactable = false;
                }
            }
        }
    }

    private void RestoreTutorialVisualFeedback()
    {
        if (playerCardManager == null) return;

        foreach (Transform slot in playerCardManager.cardSlots)
        {
            if (slot.childCount == 0) continue;

            CardDisplay display = slot.GetComponentInChildren<CardDisplay>();
            if (display == null) continue;

            TutorialHighlight highlight = display.GetComponent<TutorialHighlight>();
            if (highlight != null)
            {
                highlight.SetBlocked(false);
            }
        }

        if (playerCardManager.SumaButton != null)
        {
            Image sumaImage = playerCardManager.SumaButton.GetComponent<Image>();
            if (sumaImage != null)
            {
                sumaImage.color = playerCardManager.validOperatorColor;
            }
            playerCardManager.SumaButton.interactable = true;
        }

        if (playerCardManager.RestaButton != null)
        {
            Image restaImage = playerCardManager.RestaButton.GetComponent<Image>();
            if (restaImage != null)
            {
                restaImage.color = playerCardManager.validOperatorColor;
            }
            playerCardManager.RestaButton.interactable = true;
        }
    }

    // ========================================
    // MÉTODOS DE UI Y ANIMACIÓN
    // ========================================

    private void AnimatePopupScale()
    {
        float speed = Time.unscaledDeltaTime * popupAnimationSpeed;

        if (characterImageRect != null && characterImageRect.localScale != targetCharacterScale)
        {
            characterImageRect.localScale = Vector3.Lerp(characterImageRect.localScale, targetCharacterScale, speed);
        }
        Vector3 speechTarget = Vector3.Scale(initialSpeechBubbleLocalScale, targetPopupScale);
        if (speechBubbleRect != null && speechBubbleRect.localScale != speechTarget)
        {
            speechBubbleRect.localScale = Vector3.Lerp(speechBubbleRect.localScale, speechTarget, speed);
        }

        Vector3 dialogTarget = Vector3.Scale(initialDialogLocalScale, targetPopupScale);
        if (dialogTextRect != null && dialogTextRect.localScale != dialogTarget)
        {
            dialogTextRect.localScale = Vector3.Lerp(dialogTextRect.localScale, dialogTarget, speed);
        }
    }

    private void SetPopupScale(float scale)
    {
        Vector3 relativeScale = Vector3.one * scale;
        Vector3 characterScale = Vector3.Scale(initialCharacterLocalScale, Vector3.one * scale);

        if (characterImageRect != null) characterImageRect.localScale = characterScale;
        if (speechBubbleRect != null) speechBubbleRect.localScale = Vector3.Scale(initialSpeechBubbleLocalScale, relativeScale);
        if (dialogTextRect != null) dialogTextRect.localScale = Vector3.Scale(initialDialogLocalScale, relativeScale);

        targetPopupScale = relativeScale;
        targetCharacterScale = characterScale;
    }

    private void ShowPopupWithAnimation()
    {
        if (!isPopupVisible)
        {
            tutorialPanel.SetActive(true);
            targetPopupScale = Vector3.one * popupScaleTarget;
            targetCharacterScale = Vector3.Scale(initialCharacterLocalScale, Vector3.one * popupScaleTarget);
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
        }
        else if (optionalImage != null)
        {
            optionalImage.gameObject.SetActive(false);
        }
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

    // ========================================
    // MÉTODOS DE CONTROL DEL JUEGO
    // ========================================

    private void PauseGame()
    {
        isTutorialPaused = true;

        if (GameSpeedManager.Instance != null)
        {
            GameSpeedManager.Instance.GameSpeedMultiplier = 0f;
        }

        if (gameTimer != null)
        {
            gameTimer.PauseTimer();
        }

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.PausePowerUps();
        }

        if (playerIntelect != null)
        {
            playerIntelect.PauseRegeneration();
        }
        if (aiIntelect != null)
        {
            aiIntelect.PauseRegeneration();
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

        if (gameTimer != null)
        {
            gameTimer.ResumeTimer();
        }

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.ResumePowerUps();
        }

        if (playerIntelect != null)
        {
            playerIntelect.ResumeRegeneration();
        }
        if (aiIntelect != null)
        {
            aiIntelect.ResumeRegeneration();
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

    private void ResumeGameWithoutResetSpeed()
    {
        isTutorialPaused = false;

        if (gameTimer != null)
        {
            gameTimer.ResumeTimer();
        }

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.ResumePowerUps();
        }

        if (playerIntelect != null)
        {
            playerIntelect.ResumeRegeneration();
        }
        if (aiIntelect != null)
        {
            aiIntelect.ResumeRegeneration();
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

    private void BlockPlayer()
    {
        isPlayerBlocked = true;
        if (playerCardManager != null) playerCardManager.enabled = false;
        if (playableAreaUI != null) playableAreaUI.enabled = false;

        UpdateTutorialVisualFeedback();
    }

    private void UnblockPlayer()
    {
        isPlayerBlocked = false;
        if (playerCardManager != null) playerCardManager.enabled = true;
        if (playableAreaUI != null) playableAreaUI.enabled = true;

        PauseAllHighlightsForPlayerInteraction(true);

        RestoreTutorialVisualFeedback();
    }

    private void UnblockPlayerForPowerUps()
    {
        isPlayerBlocked = false;
        if (playerCardManager != null) playerCardManager.enabled = false;
        if (playableAreaUI != null) playableAreaUI.enabled = false;

        UpdateTutorialVisualFeedback();
    }

    // ========================================
    // MÉTODOS DE HIGHLIGHT
    // ========================================

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

    private void PauseAllHighlightsForPlayerInteraction(bool pause)
    {
        if (playerCardManager == null) return;

        foreach (Transform slot in playerCardManager.cardSlots)
        {
            if (slot.childCount == 0) continue;

            CardDisplay display = slot.GetComponentInChildren<CardDisplay>();
            if (display == null) continue;

            TutorialHighlight highlight = display.GetComponent<TutorialHighlight>();
            if (highlight != null)
            {
                highlight.PauseForPlayerCardManager(pause);
            }
        }
    }

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

    // ========================================
    // MÉTODOS PÚBLICOS PARA VALIDACIÓN
    // ========================================

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

    public bool CanPlaySingleCard()
    {
        if (allowOnlyOperations) return false;

        if (allowedActionsRemaining == 0) return false;

        return true;
    }

    public bool CanPlaySpecificCard(int cardValue)
    {
        if (!CanPlaySingleCard()) return false;

        return IsSpecificCardAllowed(cardValue);
    }

    public bool CanPlayOperation()
    {
        if (allowOnlySingleCards) return false;

        if (allowedActionsRemaining == 0) return false;

        return true;
    }

    public bool IsPowerUpAllowed(string powerUpName)
    {
        if (allowedPowerUps == null || allowedPowerUps.Length == 0) return true;

        foreach (var allowed in allowedPowerUps)
        {
            if (allowed == powerUpName) return true;
        }

        return false;
    }

    public void OnPlayerPlaysCard(int cardValue)
    {
        if (!CanPlaySpecificCard(cardValue))
        {
            if (ScreenFlashEffect.Instance != null)
            {
                ScreenFlashEffect.Instance.Flash();
            }

            return;
        }

        if (currentStep == 2 && cardValue == 5 && waitingForPlayerAction)
        {
            waitingForPlayerAction = false;

            if (allowedActionsRemaining > 0)
            {
                allowedActionsRemaining--;
                if (allowedActionsRemaining == 0)
                {
                    BlockPlayer();
                }
            }
            return;
        }

        if (currentStep == 7 && !step7_hasDefended && cardValue == 2 && waitingForPlayerAction)
        {
            step7_hasDefended = true;
            waitingForPlayerAction = false;

            if (allowedActionsRemaining > 0)
            {
                allowedActionsRemaining--;
                if (allowedActionsRemaining == 0)
                {
                    BlockPlayer();
                }
            }
            return;
        }

        if (allowedActionsRemaining > 0)
        {
            allowedActionsRemaining--;

            if (allowedActionsRemaining == 0)
            {
                BlockPlayer();
            }
        }
    }

    public void OnPlayerPlaysOperation()
    {
        if (!CanPlayOperation())
        {
            if (ScreenFlashEffect.Instance != null)
            {
                ScreenFlashEffect.Instance.Flash();
            }

            return;
        }

        if (currentStep == 7 && step7_hasDefended && !step7_hasAttacked && waitingForPlayerAction)
        {
            step7_hasAttacked = true;
            waitingForPlayerAction = false;

            if (allowedActionsRemaining > 0)
            {
                allowedActionsRemaining--;
                if (allowedActionsRemaining == 0)
                {
                    BlockPlayer();
                }
            }
            return;
        }

        if (currentStep == 4 && waitingForPlayerAction)
        {
            waitingForPlayerAction = false;

            if (allowedActionsRemaining > 0)
            {
                allowedActionsRemaining--;
                if (allowedActionsRemaining == 0)
                {
                    BlockPlayer();
                }
            }
            return;
        }

        if (allowedActionsRemaining > 0)
        {
            allowedActionsRemaining--;

            if (allowedActionsRemaining == 0)
            {
                BlockPlayer();
            }
        }
    }

    public void OnPowerUpActivated(string powerUpName)
    {
        if (currentStep == 6 && powerUpName == "Health" && waitingForPlayerAction)
        {
            waitingForPlayerAction = false;
        }
        else if (currentStep == 7 && powerUpName == "SlowTime" && waitingForPlayerAction)
        {
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

    public bool IsTutorialInProgress()
    {
        return currentStep >= 0 && currentStep < 8;
    }
}