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

        if (gameTimer == null)
        {
            gameTimer = FindFirstObjectByType<GameTimer>();
        }
    }

    void Start()
    {
        if (aiController != null)
        {
            aiController.enabled = false;
        }

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

    private void UpdateTutorialVisualFeedback()
    {
        if (playerCardManager == null) return;

        foreach (Transform slot in playerCardManager.cardSlots)
        {
            if (slot.childCount == 0) continue;

            CardDisplay display = slot.GetComponentInChildren<CardDisplay>();
            if (display == null || display.cardData == null) continue;

            TutorialHighlight highlight = display.GetComponent<TutorialHighlight>();
            bool hasActiveHighlight = (highlight != null && highlight.enabled);

            if (hasActiveHighlight) continue;

            Image cardImage = display.GetComponent<Image>();
            if (cardImage == null) continue;

            int cardValue = display.cardData.cardValue;

            bool isCardAllowed = IsSpecificCardAllowed(cardValue);

            if (!isCardAllowed)
            {
                ApplyTutorialBlockedTint(cardImage);
            }
            else if (!CanPlaySingleCard())
            {
                ApplyTutorialBlockedTint(cardImage);
            }
            else if (allowedActionsRemaining == 0)
            {
                ApplyTutorialBlockedTint(cardImage);
            }
        }

        UpdateOperatorButtonFeedback();
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

    private void ApplyTutorialBlockedTint(Image image)
    {
        if (image == null) return;

        Color currentColor = image.color;
        image.color = Color.Lerp(currentColor, tutorialBlockedColor, 0.7f);
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
            bool hasActiveHighlight = (highlight != null && highlight.enabled);

            if (hasActiveHighlight) continue;

            Image cardImage = display.GetComponent<Image>();
            if (cardImage != null)
            {
                cardImage.color = playerCardManager.validCardColor;
            }
        }

        if (playerCardManager.SumaButton != null)
        {
            Image sumaImage = playerCardManager.SumaButton.GetComponent<Image>();
            if (sumaImage != null)
            {
                sumaImage.color = playerCardManager.validOperatorColor;
            }
        }

        if (playerCardManager.RestaButton != null)
        {
            Image restaImage = playerCardManager.RestaButton.GetComponent<Image>();
            if (restaImage != null)
            {
                restaImage.color = playerCardManager.validOperatorColor;
            }
        }
    }

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
        }
    }

    private IEnumerator ShowAttackExplanation()
    {
        if (hasShownAttackExplanation) yield break;

        hasShownAttackExplanation = true;

        yield return new WaitForSeconds(2f);

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

        yield return new WaitForSeconds(2f);

        PauseGame();

        BlockPlayer();
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
    }

    private IEnumerator Tutorial_AIAttacks()
    {
        currentStep = 1;

        var card2 = cardManager.GetCardByIndex(1);
        var card3 = cardManager.GetCardByIndex(2);
        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;
        cardManager.GenerateCombinedCharacter(card2, card3, spawnPos, 5, '+', "AITeam", out result, aiIntelect);

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(ShowDefenseExplanation());

        PauseGame();

        ShowDialog("VIGILA, T'ATAQUEN!", showImage: false);

        yield return new WaitForSeconds(2f);
    }

    private IEnumerator Tutorial_PlayerDefends()
    {
        currentStep = 2;

        // Configurar restricciones ANTES de mostrar UI
        allowOnlySingleCards = true;
        allowOnlyOperations = false;
        allowedActionsRemaining = 1;
        allowedSpecificCardValue = 5;
        restrictToLeftZone = true;

        Debug.Log("[Tutorial] 🛡️ Paso 2: Solo carta 5 permitida, zona izquierda");

        yield return new WaitForSeconds(2.7f);

        BlockPlayer();
        PauseGame();

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

        UpdateTutorialVisualFeedback();

        // Esperar 5 segundos antes de ocultar el popup (pero seguir pausado)
        yield return new WaitForSeconds(5f);

        // Ocultar el popup pero MANTENER el juego pausado
        yield return StartCoroutine(HidePopupWithAnimation());

        // Desbloquear solo el jugador para que pueda interactuar
        UnblockPlayer();
        // IMPORTANTE: NO llamar a ResumeGame() aquí - mantener pausado

        waitingForPlayerAction = true;

        // ESPERAR hasta que el jugador juegue la carta 5
        // Durante este tiempo el juego está pausado pero el jugador puede interactuar
        yield return new WaitUntil(() => !waitingForPlayerAction);

        Debug.Log("[Tutorial] ✅ Paso 2: Carta 5 jugada - Continuando");

        // AHORA SÍ pausar completamente de nuevo
        yield return new WaitForSeconds(0.1f);

        BlockPlayer();
        PauseGame();

        // Limpiar restricciones
        restrictToLeftZone = false;
        allowOnlySingleCards = false;
        allowedActionsRemaining = -1;
        allowedSpecificCardValue = -1;

        Debug.Log("[Tutorial] 🔓 Paso 2 completado - Restricciones desactivadas");

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

        yield return new WaitForSeconds(1f);

        BlockPlayer();
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

        yield return new WaitForSeconds(2f);

        PauseGame();

        ShowDialog("MOLT BÉ, HAS GUANYAT + 1 D'ENERGIA!", showImage: true, contextSprite: intelectBarIcon);
        BlockPlayer();
        if (intelectBarFillImage != null)
        {
            StartHighlightEffect(intelectBarFillImage.gameObject);
        }

        if (playerIntelect != null && playerIntelect.intelectSlider != null)
        {
            HighlightElement(playerIntelect.intelectSlider.GetComponent<RectTransform>());
            StartHighlightEffect(playerIntelect.intelectSlider.gameObject);
        }

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

        allowOnlySingleCards = false;
        allowOnlyOperations = true;
        allowedActionsRemaining = 1;

        ShowDialog("FES UNA OPERACIÓ PER ATACAR", showImage: false);
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

        if (waitingForPlayerAction)
        {
            yield return StartCoroutine(HidePopupWithAnimation());
        }

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
        }

        waitingForEnemyTowerDamage = true;

        yield return new WaitUntil(() => !waitingForEnemyTowerDamage);

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator Tutorial_HealthPowerUp()
    {
        currentStep = 6;

        BlockPlayer();

        if (playerTower != null)
        {
            playerTowerHealthBeforeAttack = playerTower.currentHealth;
        }

        var card2 = cardManager.GetCardByIndex(1);
        var card1 = cardManager.GetCardByIndex(0);
        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;
        cardManager.GenerateCombinedCharacter(card2, card1, spawnPos, 3, '+', "AITeam", out result, aiIntelect);

        waitingForTowerDamage = true;

        float timeout = 15f;
        float elapsed = 0f;

        while (waitingForTowerDamage && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        PauseGame();

        ShowDialog("OH NO, CURA'T!", showImage: true, contextSprite: healthPowerUpSprite);

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

        float elapsedAction = 0f;
        float messageTimeout = 1f;

        while (elapsedAction < messageTimeout && waitingForPlayerAction)
        {
            elapsedAction += Time.deltaTime;
            yield return null;
        }

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

        UpdatePopupContent("BEN FET!", showImage: false);

        yield return new WaitForSeconds(2.5f);

        yield return StartCoroutine(HidePopupWithAnimation());
        ResumeGame();
    }

    private IEnumerator Tutorial_SlowTimePowerUp()
    {
        currentStep = 7;

        // Generar ataque enemigo
        var card1A = cardManager.GetCardByIndex(0);
        var card1B = cardManager.GetCardByIndex(0);
        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;
        cardManager.GenerateCombinedCharacter(card1A, card1B, spawnPos, 2, '+', "AITeam", out result, aiIntelect);

        // ⏸️ PAUSAR INMEDIATAMENTE después de generar el ataque
        yield return new WaitForSeconds(2.5f);

        BlockPlayer();
        PauseGame();

        // Primera pausa: Usar SlowTime
        ShowDialog("FES QUE VAGIN MÉS LENTS!", showImage: true, contextSprite: slowTimePowerUpSprite);

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
        ResumeGame();

        waitingForPlayerAction = true;

        yield return new WaitUntil(() => !waitingForPlayerAction);

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

        yield return StartCoroutine(HidePopupWithAnimation());

        yield return new WaitForSeconds(2f);

        // Aplicar boost al jugador
        if (GameSpeedManager.Instance != null)
        {
            GameSpeedManager.Instance.RemoveActiveTagMultiplier("AITeam");
            GameSpeedManager.Instance.ApplyTagSpeedMultiplier("PlayerTeam", 5.0f);
            Debug.Log("[Tutorial] 🚀 Boost x5 aplicado a tropas del jugador!");
        }

        // Configurar restricciones para defensa
        step7_hasDefended = false;
        step7_hasAttacked = false;
        allowedActionsRemaining = 2;
        allowOnlySingleCards = true;
        allowOnlyOperations = false;
        restrictToLeftZone = true;
        allowedSpecificCardValue = 2;

        // Segunda pausa: Explicar defensa con carta 2
        BlockPlayer();
        PauseGame();

        ShowDialog("PRIMER, DEFENSA'T AMB EL 2!", showImage: true, contextSprite: card2Sprite);
        yield return new WaitForSeconds(3f);
        yield return StartCoroutine(HidePopupWithAnimation());

        Debug.Log("[Tutorial] 🛡️ Paso 7 - Fase Defensa: Solo carta 2 permitida, solo zona izquierda");

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

        UpdateTutorialVisualFeedback();

        UnblockPlayer();
        ResumeGameWithoutResetSpeed();

        waitingForPlayerAction = true;

        // ESPERAR hasta que defienda con carta 2
        yield return new WaitUntil(() => step7_hasDefended);
        Debug.Log("[Tutorial] ✅ Defensa completada en paso 7");

        // PAUSAR inmediatamente después de defender
        BlockPlayer();
        PauseGame();

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

        // Cambiar a modo ataque
        allowOnlySingleCards = false;
        allowOnlyOperations = true;
        restrictToLeftZone = false;

        Debug.Log("[Tutorial] ⚔️ Paso 7 - Fase Ataque: Solo operaciones, cualquier zona");

        ShowDialog("ARA, APROFITA PER ATACAR!", showImage: false);
        yield return new WaitForSeconds(3f);
        yield return StartCoroutine(HidePopupWithAnimation());

        UnblockPlayer();
        ResumeGameWithoutResetSpeed();

        waitingForPlayerAction = true;

        // ESPERAR hasta que ataque con operación
        yield return new WaitUntil(() => step7_hasAttacked);
        Debug.Log("[Tutorial] ✅ Ataque completado en paso 7");

        // Limpiar restricciones
        BlockPlayer();
        allowedActionsRemaining = -1;
        allowOnlySingleCards = false;
        allowOnlyOperations = false;
        restrictToLeftZone = false;
        allowedSpecificCardValue = -1;

        Debug.Log("[Tutorial] 🔓 Paso 7 completado - Todas las restricciones desactivadas");

        if (aiTower != null)
        {
            aiTowerHealthBeforeAttack = aiTower.currentHealth;
        }

        waitingForEnemyTowerDamage = true;

        yield return new WaitUntil(() => !waitingForEnemyTowerDamage);

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator Tutorial_TowerDestroyed()
    {
        currentStep = 8;

        PauseGame();

        ShowDialog("HAS DESTRUÏT LA TORRE...", showImage: false);

        yield return new WaitForSeconds(4f);

        UpdatePopupContent("I COMPLETAT EL TUTORIAL!", showImage: false);
        yield return new WaitForSeconds(4f);

        UpdatePopupContent("JA ESTÀS PREPARAT!", showImage: false);
        yield return new WaitForSeconds(4f);

        yield return StartCoroutine(HidePopupWithAnimation());

        yield return new WaitForSeconds(1f);

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.UnblockAllPowerUps();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("WinScene");
    }

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

        if (gameTimer != null)
        {
            gameTimer.PauseTimer();
        }

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.PausePowerUps();
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
        
        RestoreTutorialVisualFeedback();
    }

    private void UnblockPlayerForPowerUps()
    {
        isPlayerBlocked = false;
        if (playerCardManager != null) playerCardManager.enabled = false;
        if (playableAreaUI != null) playableAreaUI.enabled = false;
        
        UpdateTutorialVisualFeedback();
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
        Debug.Log($"[Tutorial] 🎴 OnPlayerPlaysCard llamado: valor={cardValue}, paso={currentStep}");
        
        // Verificar si puede jugar esta carta específica
        if (!CanPlaySpecificCard(cardValue))
        {
            Debug.LogWarning($"[Tutorial] ⛔ Carta {cardValue} NO permitida en este paso (solo se permite: {allowedSpecificCardValue})");
            
            if (ScreenFlashEffect.Instance != null)
            {
                ScreenFlashEffect.Instance.Flash();
            }
            
            return;
        }

        Debug.Log($"[Tutorial] ✅ Carta {cardValue} permitida - Procesando acción");

        // Paso 2: Solo carta 5 para defender
        if (currentStep == 2 && cardValue == 5 && waitingForPlayerAction)
        {
            Debug.Log("[Tutorial] 🛡️ Paso 2: Carta 5 jugada - Completando defensa");
            waitingForPlayerAction = false;
            
            // Decrementar acciones permitidas
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

        // Paso 7: Solo carta 2 para defender
        if (currentStep == 7 && !step7_hasDefended && cardValue == 2 && waitingForPlayerAction)
        {
            Debug.Log("[Tutorial] 🛡️ Paso 7: Carta 2 jugada - Registrando defensa");
            step7_hasDefended = true;
            waitingForPlayerAction = false;
            
            // Decrementar acciones permitidas
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

        // Cualquier otro caso: decrementar acciones si aplica
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
        Debug.Log($"[Tutorial] ⚔️ OnPlayerPlaysOperation llamado - paso={currentStep}");
        
        // Verificar si puede jugar operaciones
        if (!CanPlayOperation())
        {
            Debug.LogWarning($"[Tutorial] ⛔ Operaciones NO permitidas en este paso");
            
            if (ScreenFlashEffect.Instance != null)
            {
                ScreenFlashEffect.Instance.Flash();
            }
            
            return;
        }

        Debug.Log($"[Tutorial] ✅ Operación permitida - Procesando acción");

        // Paso 7: Ataque después de defender
        if (currentStep == 7 && step7_hasDefended && !step7_hasAttacked && waitingForPlayerAction)
        {
            Debug.Log("[Tutorial] ⚔️ Paso 7: Operación jugada - Registrando ataque");
            step7_hasAttacked = true;
            waitingForPlayerAction = false;
            
            // Decrementar acciones permitidas
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

        // Paso 4: Enseñar ataque
        if (currentStep == 4 && waitingForPlayerAction)
        {
            Debug.Log("[Tutorial] ⚔️ Paso 4: Operación jugada - Completando enseñanza de ataque");
            waitingForPlayerAction = false;
            
            // Decrementar acciones permitidas
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

        // Cualquier otro caso: decrementar acciones si aplica
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