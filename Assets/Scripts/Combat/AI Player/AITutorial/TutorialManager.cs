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
    public Sprite intelectBarSprite;

    [Header("UI de Highlight")]
    public Image highlightOverlay;
    public RectTransform highlightRect;

    private int currentStep = 0;
    private bool waitingForPlayerAction = false;
    private bool waitingForContinue = false;
    private bool isTutorialPaused = false;
    private bool isPlayerBlocked = false;
    private bool waitingForTroopsDestroyed = false;
    private int aiTroopCount = 0;
    private int playerTroopCount = 0;

    private bool waitingForTowerDamage = false;
    private int playerTowerHealthBeforeAttack = 0;
    
    // ✅ NUEVO: Control de colisión y contraataque
    private bool waitingForCounterattack = false;
    private int playerIntelectBeforeCounterattack = 0;

    // Control de animación popup
    private bool isPopupVisible = false;
    private Vector3 targetPopupScale = Vector3.zero;
    private Vector3 targetCharacterScale = Vector3.zero;
    private RectTransform characterImageRect;
    private RectTransform speechBubbleRect;
    private RectTransform dialogTextRect;
    private RectTransform optionalImageRect;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Obtener RectTransforms para animación
        if (characterImage != null) characterImageRect = characterImage.GetComponent<RectTransform>();
        if (speechBubble != null) speechBubbleRect = speechBubble.GetComponent<RectTransform>();
        if (dialogText != null) dialogTextRect = dialogText.GetComponent<RectTransform>();
        if (optionalImage != null) optionalImageRect = optionalImage.GetComponent<RectTransform>();
    }

    void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            continueButton.gameObject.SetActive(false);
        }

        if (aiController != null)
        {
            aiController.enabled = false;
        }

        if (optionalImage != null)
        {
            optionalImage.gameObject.SetActive(false);
        }

        // Inicializar popup oculto
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

        // Detectar cuando la torre recibe daño
        if (waitingForTowerDamage && playerTower != null)
        {
            if (playerTower.currentHealth < playerTowerHealthBeforeAttack)
            {
                Debug.Log($"[Tutorial] 💥 Torre dañada! Salud: {playerTowerHealthBeforeAttack} → {playerTower.currentHealth}");
                waitingForTowerDamage = false;
            }
        }
        
        // ✅ NUEVO: Detectar cuando el jugador gana intelecto (contraataque exitoso)
        if (waitingForCounterattack && playerIntelect != null)
        {
            if (playerIntelect.currentIntelect > playerIntelectBeforeCounterattack)
            {
                Debug.Log($"[Tutorial] ⚔️ ¡Contraataque exitoso! Intelecto: {playerIntelectBeforeCounterattack} → {playerIntelect.currentIntelect}");
                waitingForCounterattack = false;
            }
        }

        // Animar escala del popup suavemente
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

    /// <summary>
    /// Anima la escala del popup suavemente usando Lerp (estilo CardDisplay)
    /// </summary>
    private void AnimatePopupScale()
    {
        float speed = Time.unscaledDeltaTime * popupAnimationSpeed;
        
        // ✅ Animar characterImage con escala independiente
        if (characterImageRect != null && characterImageRect.localScale != targetCharacterScale)
        {
            characterImageRect.localScale = Vector3.Lerp(characterImageRect.localScale, targetCharacterScale, speed);
        }

        // Animar resto de elementos con escala normal
        if (speechBubbleRect != null && speechBubbleRect.localScale != targetPopupScale)
        {
            speechBubbleRect.localScale = Vector3.Lerp(speechBubbleRect.localScale, targetPopupScale, speed);
        }

        if (dialogTextRect != null && dialogTextRect.localScale != targetPopupScale)
        {
            dialogTextRect.localScale = Vector3.Lerp(dialogTextRect.localScale, targetPopupScale, speed);
        }

        // Solo animar optionalImage si está activo
        if (optionalImageRect != null && optionalImage.gameObject.activeSelf && optionalImageRect.localScale != targetPopupScale)
        {
            optionalImageRect.localScale = Vector3.Lerp(optionalImageRect.localScale, targetPopupScale, speed);
        }
    }

    /// <summary>
    /// Establece la escala del popup inmediatamente sin animación
    /// </summary>
    private void SetPopupScale(float scale)
    {
        Vector3 scaleVector = Vector3.one * scale;
        Vector3 characterScale = Vector3.one * scale * characterImageScale; // ✅ Escala mayor para personaje

        if (characterImageRect != null) characterImageRect.localScale = characterScale;
        if (speechBubbleRect != null) speechBubbleRect.localScale = scaleVector;
        if (dialogTextRect != null) dialogTextRect.localScale = scaleVector;
        if (optionalImageRect != null) optionalImageRect.localScale = scaleVector;

        targetPopupScale = scaleVector;
        targetCharacterScale = characterScale;
    }

    /// <summary>
    /// Muestra el popup con animación de aparecer (scale up)
    /// </summary>
    private void ShowPopupWithAnimation()
    {
        if (!isPopupVisible)
        {
            tutorialPanel.SetActive(true);
            targetPopupScale = Vector3.one * popupScaleTarget;
            targetCharacterScale = Vector3.one * popupScaleTarget * characterImageScale; // ✅ Escala mayor para personaje
            isPopupVisible = true;
        }
    }

    /// <summary>
    /// Oculta el popup con animación de desaparecer (scale down)
    /// </summary>
    private IEnumerator HidePopupWithAnimation()
    {
        if (isPopupVisible)
        {
            targetPopupScale = Vector3.one * popupScaleHidden;
            targetCharacterScale = Vector3.one * popupScaleHidden; // También ocultar personaje

            // Esperar a que termine la animación
            yield return new WaitForSecondsRealtime(popupDuration);

            tutorialPanel.SetActive(false);
            isPopupVisible = false;
        }
    }

    /// <summary>
    /// Actualiza el contenido del popup SIN animar (para cambios de texto manteniendo el popup visible)
    /// </summary>
    private void UpdatePopupContent(string message, bool showImage, Sprite contextSprite = null)
    {
        dialogText.text = message;

        if (showImage && contextSprite != null && optionalImage != null)
        {
            optionalImage.gameObject.SetActive(true);
            optionalImage.sprite = contextSprite;

            // Si la optional image se acaba de activar, ajustar su escala al popup actual
            if (optionalImageRect != null)
                optionalImageRect.localScale = targetPopupScale;
        }
        else if (optionalImage != null)
        {
            optionalImage.gameObject.SetActive(false);
        }
    }

    // ========== UTILIDADES MODIFICADAS ==========

    private int CountTroopsByTag(string tag)
    {
        Character[] troops = FindObjectsOfType<Character>();
        int count = 0;
        foreach (var troop in troops)
        {
            if (troop.CompareTag(tag)) count++;
        }
        return count;
    }

    private void OnContinueButtonClicked()
    {
        if (waitingForContinue)
        {
            waitingForContinue = false;
        }
    }

    private IEnumerator WaitForSecondsOrContinue(float seconds, bool showButton = true)
    {
        waitingForContinue = true;

        if (showButton && continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }

        float elapsed = 0f;

        while (elapsed < seconds && waitingForContinue)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        waitingForContinue = false;
    }

    private IEnumerator RunTutorial()
    {
        yield return StartCoroutine(Tutorial_AIAttacks());
        yield return StartCoroutine(Tutorial_PlayerDefends());
        yield return StartCoroutine(Tutorial_WaitForDestruction());
        yield return StartCoroutine(Tutorial_TeachAttack());
        yield return StartCoroutine(Tutorial_PlayerAttacks());
        yield return StartCoroutine(Tutorial_HealthPowerUp());
        yield return StartCoroutine(Tutorial_SlowTimePowerUp());
        yield return StartCoroutine(Tutorial_FinalAttack());
        yield return StartCoroutine(Tutorial_Completion());
    }

    // ========== PASO 1: IA ATACA ==========
    private IEnumerator Tutorial_AIAttacks()
    {
        currentStep = 1;

        ShowDialog("¡Te atacan con 2+3!", showImage: false); // Primera aparición → anima
        yield return StartCoroutine(WaitForSecondsOrContinue(1.5f));

        var card2 = cardManager.GetCardByIndex(1);
        var card3 = cardManager.GetCardByIndex(2);
        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;
        cardManager.GenerateCombinedCharacter(card2, card3, spawnPos, 5, '+', "AITeam", out result, aiIntelect);

        yield return StartCoroutine(WaitForSecondsOrContinue(2f));

        PauseGame();
    }

    // ========== PASO 2: JUGADOR DEFIENDE CON 5 ==========
    private IEnumerator Tutorial_PlayerDefends()
    {
        currentStep = 2;

        // ✅ Actualizar contenido SIN animar (popup ya visible del paso 1)
        UpdatePopupContent("¡Defiende con la carta 5!", showImage: true, contextSprite: card5Sprite);

        HighlightCard(playerCardManager.cardSlots[4]);
        StartHighlightEffect(playerCardManager.cardSlots[4].gameObject);

        UnblockPlayer();

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        // ✅ CAMBIO: Esperar 2 segundos y OCULTAR el popup
        yield return new WaitForSeconds(2f);
        
        yield return StartCoroutine(HidePopupWithAnimation());

        waitingForPlayerAction = true;

        Debug.Log("[Tutorial] ⏳ Esperando carta 5...");
        yield return new WaitUntil(() => !waitingForPlayerAction);
        Debug.Log("[Tutorial] ✅ Carta 5 jugada!");

        StopHighlightEffect(playerCardManager.cardSlots[4].gameObject);
        ClearHighlight();
        HideOptionalImage();

        PauseGame();

        // ✅ Nueva aparición → animar
        ShowDialog("¡Muy bien! La carta 5 cuesta 5 de energía.", showImage: true, contextSprite: intelectBarSprite);

        if (playerIntelect != null && playerIntelect.intelectSlider != null)
        {
            HighlightElement(playerIntelect.intelectSlider.GetComponent<RectTransform>());
            StartHighlightEffect(playerIntelect.intelectSlider.gameObject);
        }

        yield return StartCoroutine(WaitForSecondsOrContinue(4f));

        if (playerIntelect != null && playerIntelect.intelectSlider != null)
        {
            StopHighlightEffect(playerIntelect.intelectSlider.gameObject);
        }
        ClearHighlight();
        HideOptionalImage();

        // ✅ Actualizar contenido SIN animar
        UpdatePopupContent("¡Mira cómo chocan!", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(2.5f));

        // ✅ OCULTAR con animación
        yield return StartCoroutine(HidePopupWithAnimation());
        ResumeGame();
    }

    // ========== PASO 3: ESPERAR DESTRUCCIÓN Y CONTRAATAQUE ==========
    private IEnumerator Tutorial_WaitForDestruction()
    {
        currentStep = 3;

        yield return new WaitForSeconds(0.5f); // ✅ Reducido para detectar antes

        // ✅ NUEVO: Guardar intelecto ANTES de la colisión
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

        // ✅ NUEVO: Esperar a que colisionen (tropas se destruyan)
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

        // ✅ NUEVO: Esperar a que se otorgue el intelecto de contraataque
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

        yield return new WaitForSeconds(1.5f); // ✅ Pausa adicional después del contraataque

        PauseGame();

        // ✅ Nueva aparición → animar
        ShowDialog("¡Ganaste +1 de energía por contraatacar!", showImage: true, contextSprite: intelectBarSprite);

        if (playerIntelect != null && playerIntelect.intelectSlider != null)
        {
            HighlightElement(playerIntelect.intelectSlider.GetComponent<RectTransform>());
            StartHighlightEffect(playerIntelect.intelectSlider.gameObject);
        }

        yield return StartCoroutine(WaitForSecondsOrContinue(4f));

        if (playerIntelect != null && playerIntelect.intelectSlider != null)
        {
            StopHighlightEffect(playerIntelect.intelectSlider.gameObject);
        }
        ClearHighlight();
        HideOptionalImage();

        // ✅ OCULTAR con animación
        yield return StartCoroutine(HidePopupWithAnimation());
    }

    // ========== PASO 4: ENSEÑAR A ATACAR ==========
    private IEnumerator Tutorial_TeachAttack()
    {
        currentStep = 4;

        // ✅ Nueva aparición → animar
        ShowDialog("Ahora ataca:\n1. Elige carta\n2. Presiona +/-\n3. Elige otra\n4. ¡Tira!", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(5f));

        // ✅ Actualizar contenido SIN animar
        UpdatePopupContent("¡Hazlo ahora!", showImage: false);
        UnblockPlayer();

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        waitingForPlayerAction = true;

        yield return new WaitUntil(() => !waitingForPlayerAction);

        ClearHighlight();

        // ✅ Actualizar contenido SIN animar
        UpdatePopupContent("¡Bien! Tu ataque va a la torre.", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(2.5f));

        // ✅ OCULTAR con animación
        yield return StartCoroutine(HidePopupWithAnimation());
        ResumeGame();
    }

    // ========== PASO 5: JUGADOR ATACA (ESPERAR QUE LLEGUE) ==========
    private IEnumerator Tutorial_PlayerAttacks()
    {
        currentStep = 5;

        yield return new WaitForSeconds(10f);
    }

    // ========== PASO 6: POWERUP HEALTH (ESPERAR DAÑO A LA TORRE) ==========
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

        // ✅ Nueva aparición → animar
        ShowDialog("¡Te dañaron! Usa el corazón ❤️", showImage: true, contextSprite: healthPowerUpSprite);

        var healPowerUp = powerUpManager.GetPowerUpButton("Health");
        if (healPowerUp != null)
        {
            HighlightElement(healPowerUp.GetComponent<RectTransform>());
            StartHighlightEffect(healPowerUp.gameObject);
        }

        UnblockPlayerForPowerUps();

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        waitingForPlayerAction = true;

        yield return new WaitUntil(() => !waitingForPlayerAction);

        if (healPowerUp != null)
        {
            StopHighlightEffect(healPowerUp.gameObject);
        }
        ClearHighlight();
        HideOptionalImage();

        // ✅ Actualizar contenido SIN animar
        UpdatePopupContent("¡Curado!", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(2.5f));

        // ✅ OCULTAR con animación
        yield return StartCoroutine(HidePopupWithAnimation());
        ResumeGame();
    }

    // ========== PASO 7: POWERUP SLOWTIME ==========
    private IEnumerator Tutorial_SlowTimePowerUp()
    {
        currentStep = 7;

        // ✅ Nueva aparición → animar
        ShowDialog("¡Usa el reloj 🕐 para hacer lentos a los enemigos!", showImage: true, contextSprite: slowTimePowerUpSprite);

        var card1A = cardManager.GetCardByIndex(0);
        var card1B = cardManager.GetCardByIndex(0);
        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;
        cardManager.GenerateCombinedCharacter(card1A, card1B, spawnPos, 2, '+', "AITeam", out result, aiIntelect);

        yield return StartCoroutine(WaitForSecondsOrContinue(3f));

        PauseGame();

        var slowPowerUp = powerUpManager.GetPowerUpButton("SlowTime");
        if (slowPowerUp != null)
        {
            HighlightElement(slowPowerUp.GetComponent<RectTransform>());
            StartHighlightEffect(slowPowerUp.gameObject);
        }

        UnblockPlayerForPowerUps();

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

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

        // ✅ Actualizar contenido SIN animar
        UpdatePopupContent("¡Perfecto! Los enemigos van lentos.", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(3f));

        // ✅ OCULTAR con animación
        yield return StartCoroutine(HidePopupWithAnimation());
        ResumeGame();
    }

    // ========== PASO 8: ATAQUE FINAL ==========
    private IEnumerator Tutorial_FinalAttack()
    {
        currentStep = 8;

        // ✅ Nueva aparición → animar
        ShowDialog("¡ATAQUE FINAL! Lanza tu combinación.", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(3f));

        UnblockPlayer();

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        waitingForPlayerAction = true;

        yield return new WaitUntil(() => !waitingForPlayerAction);

        // ✅ Actualizar contenido SIN animar
        UpdatePopupContent("¡EXCELENTE!", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(2.5f));

        // ✅ OCULTAR con animación
        yield return StartCoroutine(HidePopupWithAnimation());
        ResumeGame();

        yield return new WaitForSeconds(4f);

        if (aiTower != null)
        {
            aiTower.TakeDamage(9999);
        }

        yield return new WaitForSeconds(3f);

        UnityEngine.SceneManagement.SceneManager.LoadScene("WinScene");
    }

    // ========== PASO 9: COMPLETAR ==========
    private IEnumerator Tutorial_Completion()
    {
        currentStep = 9;

        // ✅ Nueva aparición → animar
        ShowDialog("¡TUTORIAL COMPLETADO!\n\n¡A jugar!", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(4f));

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
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

    private void ShowDialog(string message, bool showImage, Sprite contextSprite = null)
    {
        // Si el popup ya está visible, solo actualizar contenido
        if (isPopupVisible)
        {
            UpdatePopupContent(message, showImage, contextSprite);
        }
        else
        {
            // Primera aparición → animar
            ShowPopupWithAnimation();
            UpdatePopupContent(message, showImage, contextSprite);
        }
    }

    private void HideOptionalImage()
    {
        if (optionalImage != null)
        {
            optionalImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ✅ DEPRECADO: Usar HidePopupWithAnimation() en su lugar
    /// </summary>
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

        Character[] troops = FindObjectsOfType<Character>();
        foreach (var troop in troops)
        {
            var agent = troop.GetComponent<UnityEngine.AI.NavMeshAgent>();
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

        Character[] troops = FindObjectsOfType<Character>();
        foreach (var troop in troops)
        {
            var agent = troop.GetComponent<UnityEngine.AI.NavMeshAgent>();
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

        if ((currentStep == 4 || currentStep == 8) && waitingForPlayerAction)
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
}