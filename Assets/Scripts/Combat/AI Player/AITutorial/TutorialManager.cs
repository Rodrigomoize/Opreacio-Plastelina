using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tutorial jugable simplificado con ayudas visuales para niños 6-8 años
/// </summary>
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
    
    // Para detectar daño a la torre
    private bool waitingForTowerDamage = false;
    private int playerTowerHealthBeforeAttack = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

        StartCoroutine(RunTutorial());
    }

    void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonClicked);
        }
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
    }

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

        ShowDialog("¡Te atacan con 2+3!", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(2.5f)); // ✅ +0.5s más lento

        var card2 = cardManager.GetCardByIndex(1);
        var card3 = cardManager.GetCardByIndex(2);
        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;
        cardManager.GenerateCombinedCharacter(card2, card3, spawnPos, 5, '+', "AITeam", out result, aiIntelect);

        yield return StartCoroutine(WaitForSecondsOrContinue(2f)); // ✅ +0.5s más lento

        PauseGame();
    }

    // ========== PASO 2: JUGADOR DEFIENDE CON 5 ==========
    private IEnumerator Tutorial_PlayerDefends()
    {
        currentStep = 2;

        ShowDialog("¡Defiende con la carta 5!", showImage: true, contextSprite: card5Sprite);
        
        HighlightCard(playerCardManager.cardSlots[4]);
        StartHighlightEffect(playerCardManager.cardSlots[4].gameObject);
        
        UnblockPlayer();

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        waitingForPlayerAction = true;

        Debug.Log("[Tutorial] ⏳ Esperando carta 5...");
        yield return new WaitUntil(() => !waitingForPlayerAction);
        Debug.Log("[Tutorial] ✅ Carta 5 jugada!");

        StopHighlightEffect(playerCardManager.cardSlots[4].gameObject);
        ClearHighlight();
        HideOptionalImage();
        
        PauseGame();
        
        ShowDialog("¡Muy bien! La carta 5 cuesta 5 de energía.", showImage: true, contextSprite: intelectBarSprite);
        
        if (playerIntelect != null && playerIntelect.intelectSlider != null)
        {
            HighlightElement(playerIntelect.intelectSlider.GetComponent<RectTransform>());
            StartHighlightEffect(playerIntelect.intelectSlider.gameObject);
        }
        
        yield return StartCoroutine(WaitForSecondsOrContinue(4f)); // ✅ +1s más lento
        
        if (playerIntelect != null && playerIntelect.intelectSlider != null)
        {
            StopHighlightEffect(playerIntelect.intelectSlider.gameObject);
        }
        ClearHighlight();
        HideOptionalImage();
        
        ShowDialog("¡Mira cómo chocan!", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(2.5f)); // ✅ +0.5s más lento

        HideTutorialPanel(); // ✅ OCULTAR UI
        ResumeGame();
    }

    // ========== PASO 3: ESPERAR DESTRUCCIÓN ==========
    private IEnumerator Tutorial_WaitForDestruction()
    {
        currentStep = 3;

        yield return new WaitForSeconds(1.5f); // ✅ +0.5s más lento

        aiTroopCount = CountTroopsByTag("AITeam");
        playerTroopCount = CountTroopsByTag("PlayerTeam");
        
        if (aiTroopCount == 0 && playerTroopCount == 0)
        {
            yield return new WaitForSeconds(2.5f); // ✅ +0.5s más lento
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

        yield return new WaitForSeconds(2.5f); // ✅ +0.5s más lento

        PauseGame();
        
        ShowDialog("¡Ganaste +1 de energía!", showImage: true, contextSprite: intelectBarSprite);
        
        if (playerIntelect != null && playerIntelect.intelectSlider != null)
        {
            HighlightElement(playerIntelect.intelectSlider.GetComponent<RectTransform>());
            StartHighlightEffect(playerIntelect.intelectSlider.gameObject);
        }
        
        yield return StartCoroutine(WaitForSecondsOrContinue(4f)); // ✅ +1s más lento
        
        if (playerIntelect != null && playerIntelect.intelectSlider != null)
        {
            StopHighlightEffect(playerIntelect.intelectSlider.gameObject);
        }
        ClearHighlight();
        HideOptionalImage();
        HideTutorialPanel(); // ✅ OCULTAR UI
    }

    // ========== PASO 4: ENSEÑAR A ATACAR ==========
    private IEnumerator Tutorial_TeachAttack()
    {
        currentStep = 4;

        ShowDialog("Ahora ataca:\n1. Elige carta\n2. Presiona +/-\n3. Elige otra\n4. ¡Tira!", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(5f)); // ✅ +2s MÁS LENTO (era 3f)

        ShowDialog("¡Hazlo ahora!", showImage: false);
        UnblockPlayer();

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        waitingForPlayerAction = true;

        yield return new WaitUntil(() => !waitingForPlayerAction);

        ClearHighlight();
        ShowDialog("¡Bien! Tu ataque va a la torre.", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(2.5f)); // ✅ +0.5s más lento

        HideTutorialPanel(); // ✅ OCULTAR UI
        ResumeGame();
    }

    // ========== PASO 5: JUGADOR ATACA (ESPERAR QUE LLEGUE) ==========
    private IEnumerator Tutorial_PlayerAttacks()
    {
        currentStep = 5;

        // ✅ ESPERAR A QUE EL ATAQUE LLEGUE Y DAÑE LA TORRE ENEMIGA
        yield return new WaitForSeconds(10f); // ✅ Tiempo para que llegue el ataque
    }

    // ========== PASO 6: POWERUP HEALTH (ESPERAR DAÑO A LA TORRE) ==========
    private IEnumerator Tutorial_HealthPowerUp()
    {
        currentStep = 6;

        BlockPlayer();
        
        // GUARDAR salud actual de la torre ANTES del ataque
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

        // ✅ Esperar a que la tropa LLEGUE y DAÑE la torre
        waitingForTowerDamage = true;
        Debug.Log("[Tutorial] ⏳ Esperando que la torre reciba daño...");
        
        float timeout = 15f; // ✅ Aumentado timeout
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
        
        ShowDialog("¡Curado!", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(2.5f)); // ✅ +0.5s más lento
        
        HideTutorialPanel(); // ✅ OCULTAR UI
        ResumeGame();
    }

    // ========== PASO 7: POWERUP SLOWTIME (SIMPLIFICADO - SOLO ENSEÑAR POWERUP) ==========
    private IEnumerator Tutorial_SlowTimePowerUp()
    {
        currentStep = 7;

        ShowDialog("¡Usa el reloj 🕐 para hacer lentos a los enemigos!", showImage: true, contextSprite: slowTimePowerUpSprite);
        
        var card1A = cardManager.GetCardByIndex(0);
        var card1B = cardManager.GetCardByIndex(0);
        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;
        cardManager.GenerateCombinedCharacter(card1A, card1B, spawnPos, 2, '+', "AITeam", out result, aiIntelect);

        yield return StartCoroutine(WaitForSecondsOrContinue(3f)); // ✅ +1s más lento

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
        
        ShowDialog("¡Perfecto! Los enemigos van lentos.", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(3f)); // ✅ +1s más lento
        
        HideTutorialPanel(); // ✅ OCULTAR UI
        ResumeGame();
    }

    // ========== PASO 8: ATAQUE FINAL ==========
    private IEnumerator Tutorial_FinalAttack()
    {
        currentStep = 8;

        ShowDialog("¡ATAQUE FINAL! Lanza tu combinación.", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(3f)); // ✅ +1s más lento
        
        UnblockPlayer();

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        waitingForPlayerAction = true;

        yield return new WaitUntil(() => !waitingForPlayerAction);

        ShowDialog("¡EXCELENTE!", showImage: false);
        yield return StartCoroutine(WaitForSecondsOrContinue(2.5f)); // ✅ +0.5s más lento
        
        HideTutorialPanel(); // ✅ OCULTAR UI
        ResumeGame();

        yield return new WaitForSeconds(4f); // ✅ +1s más lento

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
        tutorialPanel.SetActive(true);
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

    private void HideOptionalImage()
    {
        if (optionalImage != null)
        {
            optionalImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ✅ NUEVO: Oculta completamente el panel de tutorial
    /// </summary>
    private void HideTutorialPanel()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
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