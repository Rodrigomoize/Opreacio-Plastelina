using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona todo el flujo del tutorial paso a paso de forma TOTALMENTE INTERACTIVA
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Referencias")]
    public PlayerCardManager playerCardManager;
    public CardManager cardManager;
    public IntelectManager playerIntelect;
    public IntelectManager aiIntelect;
    public PowerUpManager powerUpManager;
    public Transform aiSpawnPoint;
    public Transform playerSpawnPoint;
    public Tower playerTower;
    public Tower aiTower;

    [Header("Referencias de IA (para desactivar durante tutorial)")]
    public MonoBehaviour aiController;

    [Header("Referencias de PlayableArea (para bloquear clics)")]
    public PlayableAreaUI playableAreaUI;

    [Header("UI del Tutorial")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public Image highlightOverlay;
    public RectTransform highlightRect;
    public Button continueButton;

    [Header("Configuración")]
    public float pauseDelay = 0.5f;

    private int currentStep = 0;
    private bool waitingForPlayerAction = false;
    private bool waitingForContinue = false;
    private GameObject currentAIAttack;
    private bool isTutorialPaused = false;
    private bool isPlayerBlocked = false; // NUEVO: bloqueo del jugador

    // Detectar cuando las tropas se destruyen
    private bool waitingForTroopsDestroyed = false;
    private int aiTroopCount = 0;
    private int playerTroopCount = 0;

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

        // Desactivar IA durante el tutorial
        if (aiController != null)
        {
            aiController.enabled = false;
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
        // LOG CONTINUO para debugging
        if (waitingForTroopsDestroyed && Time.frameCount % 30 == 0) // Cada 0.5s
        {
            int currentAITroops = CountTroopsByTag("AITeam");
            int currentPlayerTroops = CountTroopsByTag("PlayerTeam");
            
        }

        // Detectar cuando las tropas se destruyen automáticamente
        if (waitingForTroopsDestroyed)
        {
            int currentAITroops = CountTroopsByTag("AITeam");
            int currentPlayerTroops = CountTroopsByTag("PlayerTeam");

            // SOLO comparar con los valores INICIALES guardados
            if (currentAITroops < aiTroopCount || currentPlayerTroops < playerTroopCount)
            {
                waitingForTroopsDestroyed = false;
            }
            
            // ❌ NO actualizar los conteos aquí - deben mantenerse como referencia inicial
            // aiTroopCount = currentAITroops; // <-- ESTO ESTABA MAL
            // playerTroopCount = currentPlayerTroops; // <-- ESTO ESTABA MAL
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
        // FLUJO COMPLETO E INTERACTIVO
        yield return StartCoroutine(Tutorial_AIAttacks());
        yield return StartCoroutine(Tutorial_PlayerDefends());
        yield return StartCoroutine(Tutorial_WaitForDestruction());
        yield return StartCoroutine(Tutorial_TeachAttack());
        yield return StartCoroutine(Tutorial_PlayerAttacks());

        // ========== NUEVA SECCIÓN: POWERUPS ==========
        yield return StartCoroutine(Tutorial_HealthPowerUp());
        yield return StartCoroutine(Tutorial_SlowTimePowerUp());
        yield return StartCoroutine(Tutorial_FinalAttack());

        yield return StartCoroutine(Tutorial_Completion());
    }

    // ========== PASO 1: LA IA ATACA CON 2+3 ==========
    private IEnumerator Tutorial_AIAttacks()
    {
        currentStep = 1;

        ShowTutorial("¡Cuidado! La IA te está atacando con 2+3=5...");
        yield return StartCoroutine(WaitForSecondsOrContinue(2f));


        var card2 = cardManager.GetCardByIndex(1);
        var card3 = cardManager.GetCardByIndex(2);

        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;
        bool success = cardManager.GenerateCombinedCharacter(card2, card3, spawnPos, 5, '+', "AITeam", out result, aiIntelect);

        if (success)
        {
        }

        yield return StartCoroutine(WaitForSecondsOrContinue(1.5f));

        PauseGame(); // USAR PAUSADO COMPLETO
    }

    // ========== PASO 2: JUGADOR DEFIENDE CON 5 ==========
    private IEnumerator Tutorial_PlayerDefends()
    {
        currentStep = 2;

        ShowTutorial("¡Usa tu carta 5 para defenderte!");

        HighlightCard(playerCardManager.cardSlots[4]);
        UnblockPlayer(); // DESBLOQUEAR para que pueda jugar

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        waitingForPlayerAction = true;

        yield return new WaitUntil(() => !waitingForPlayerAction);


        ClearHighlight();
        ShowTutorial("¡Bien hecho! Ahora observa cómo las tropas chocan...");
        yield return StartCoroutine(WaitForSecondsOrContinue(2f));

        ResumeGame(); // REANUDAR JUEGO AQUÍ (ANTES de esperar destrucción)
    }

    // ========== PASO 3: ESPERAR DESTRUCCIÓN DE TROPAS ==========
    private IEnumerator Tutorial_WaitForDestruction()
    {
        currentStep = 3;


        // Esperar 1s para que las tropas se muevan y colisionen
        yield return new WaitForSeconds(1f);

        // GUARDAR conteo INICIAL (solo una vez)
        aiTroopCount = CountTroopsByTag("AITeam");
        playerTroopCount = CountTroopsByTag("PlayerTeam");
        
        
        // Si no hay tropas, saltar espera
        if (aiTroopCount == 0 && playerTroopCount == 0)
        {
            yield return StartCoroutine(WaitForSecondsOrContinue(1.5f));
            PauseGame();
            yield break;
        }

        waitingForTroopsDestroyed = true;

        // Timeout de seguridad: máximo 15 segundos esperando
        float timeout = 15f;
        float elapsed = 0f;

        while (waitingForTroopsDestroyed && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (elapsed >= timeout)
        {
            Debug.LogWarning("[TutorialManager] ⚠️ Timeout esperando destrucción de tropas (15s)");
            waitingForTroopsDestroyed = false;
        }
        else
        {
        }

        yield return StartCoroutine(WaitForSecondsOrContinue(1.5f));

        PauseGame(); // PAUSAR OTRA VEZ
    }

    // ========== PASO 4: ENSEÑAR A ATACAR ==========
    private IEnumerator Tutorial_TeachAttack()
    {
        currentStep = 4;

        ShowTutorial("¡Perfecto! Ahora aprende a ATACAR:\n\n1️⃣ Selecciona 2 cartas\n2️⃣ Elige un operador (+ o -)\n3️⃣ Colócalas en el campo");

        HighlightElement(playerCardManager.SumaButton.GetComponent<RectTransform>());

        yield return StartCoroutine(WaitForSecondsOrContinue(4f));

        ShowTutorial("¡Hazlo ahora! Crea una combinación y ataca.");
        UnblockPlayer(); // DESBLOQUEAR

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        waitingForPlayerAction = true;

        yield return new WaitUntil(() => !waitingForPlayerAction);


        ClearHighlight();
        ShowTutorial("¡Excelente! Observa cómo tu ataque avanza hacia la torre enemiga.");
        yield return StartCoroutine(WaitForSecondsOrContinue(2f));

        ResumeGame(); // REANUDAR
    }

    // ========== PASO 5: JUGADOR ATACA ==========
    private IEnumerator Tutorial_PlayerAttacks()
    {
        currentStep = 5;

        yield return StartCoroutine(WaitForSecondsOrContinue(6f));

        ShowTutorial("¡Bien hecho! Ya sabes cómo atacar y defender.");
        yield return StartCoroutine(WaitForSecondsOrContinue(3f));
    }

    // ========== PASO 6: POWERUP DE CURACIÓN ==========
    private IEnumerator Tutorial_HealthPowerUp()
    {
        currentStep = 6;

        ShowTutorial("Ahora aprenderás sobre los PowerUps. ¡Presta atención!");
        yield return StartCoroutine(WaitForSecondsOrContinue(3f));

        ShowTutorial("¡La IA te ataca con 2+1=3! NO PUEDES DEFENDER esta vez.");
        
        BlockPlayer(); // 🔒 BLOQUEAR JUGADOR COMPLETAMENTE
        
        yield return StartCoroutine(WaitForSecondsOrContinue(2f));

        // Forzar ataque de IA con 2+1=3
        var card2 = cardManager.GetCardByIndex(1);
        var card1 = cardManager.GetCardByIndex(0);

        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;
        cardManager.GenerateCombinedCharacter(card2, card1, spawnPos, 3, '+', "AITeam", out result, aiIntelect);

        yield return StartCoroutine(WaitForSecondsOrContinue(2f));

        // Esperar a que el ataque llegue a la torre (sin pausa)
        yield return new WaitForSeconds(4f);

        PauseGame(); // PAUSAR DESPUÉS DEL DAÑO

        ShowTutorial("¡El ataque llegó a tu torre! Has perdido 3 puntos de vida.");
        yield return StartCoroutine(WaitForSecondsOrContinue(3f));

        // EXPLICAR POWERUP DE CURACIÓN
        ShowTutorial("💚 POWERUP DE CURACIÓN 💚\n\nEl icono del CORAZÓN ❤️ restaura 3 puntos de vida a tu torre.\n\n¡Úsalo ahora!");

        // Resaltar botón de curación
        var healPowerUp = powerUpManager.GetPowerUpButton("Health");
        if (healPowerUp != null)
        {
            HighlightElement(healPowerUp.GetComponent<RectTransform>());
        }

        UnblockPlayerForPowerUps(); // 🔓 DESBLOQUEAR SOLO POWERUPS

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        waitingForPlayerAction = true;

        yield return new WaitUntil(() => !waitingForPlayerAction);


        ClearHighlight();
        ShowTutorial("¡Perfecto! Tu torre se ha curado. ¡Sigamos!");
        yield return StartCoroutine(WaitForSecondsOrContinue(2f));
        
        ResumeGame(); // REANUDAR JUEGO
    }

    // ========== PASO 7: POWERUP DE RALENTIZACIÓN ==========
    private IEnumerator Tutorial_SlowTimePowerUp()
    {
        currentStep = 7;

        ShowTutorial("¡La IA lanza otro ataque! Esta vez es 1+1=2.");
        yield return StartCoroutine(WaitForSecondsOrContinue(2f));

        // Forzar ataque de IA con 1+1=2
        var card1A = cardManager.GetCardByIndex(0);
        var card1B = cardManager.GetCardByIndex(0);

        Vector3 spawnPos = aiSpawnPoint.position;
        CardManager.GenerateResult result;
        currentAIAttack = cardManager.GenerateCombinedCharacter(card1A, card1B, spawnPos, 2, '+', "AITeam", out result, aiIntelect)
            ? FindObjectOfType<Character>()?.gameObject
            : null;

        yield return StartCoroutine(WaitForSecondsOrContinue(1f));

        PauseGame(); // PAUSAR JUEGO

        // EXPLICAR POWERUP DE RALENTIZACIÓN
        ShowTutorial("⏰ POWERUP DE RALENTIZACIÓN ⏰\n\nEl icono del RELOJ 🕐 ralentiza a todos los enemigos durante 10 segundos.\n\n¡Úsalo ahora!");

        // Resaltar botón de SlowTime
        var slowPowerUp = powerUpManager.GetPowerUpButton("SlowTime");
        if (slowPowerUp != null)
        {
            HighlightElement(slowPowerUp.GetComponent<RectTransform>());
        }

        UnblockPlayerForPowerUps(); // DESBLOQUEAR SOLO POWERUPS

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        waitingForPlayerAction = true;

        yield return new WaitUntil(() => !waitingForPlayerAction);


        ClearHighlight();
        ShowTutorial("¡Genial! Los enemigos ahora se mueven más lento.");
        yield return StartCoroutine(WaitForSecondsOrContinue(2f));

        ResumeGame();

        // ESPERAR 4 SEGUNDOS Y ELIMINAR ATAQUE ENEMIGO
        yield return StartCoroutine(WaitForSecondsOrContinue(4f));

        // Destruir ataque enemigo
        Character[] aiTroops = FindObjectsOfType<Character>();
        foreach (var troop in aiTroops)
        {
            if (troop.CompareTag("AITeam"))
            {
                Destroy(troop.gameObject);
            }
        }

        ShowTutorial("¡El ataque enemigo ha sido eliminado! Ahora es tu turno de atacar.");
        yield return StartCoroutine(WaitForSecondsOrContinue(2f));
    }

    // ========== PASO 8: ATAQUE FINAL ==========
    private IEnumerator Tutorial_FinalAttack()
    {
        currentStep = 8;

        ShowTutorial("¡Es tu momento! Lanza un último ataque para terminar el tutorial.");
        UnblockPlayer(); // DESBLOQUEAR

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        waitingForPlayerAction = true;

        yield return new WaitUntil(() => !waitingForPlayerAction);


        ShowTutorial("¡EXCELENTE! Has completado el ataque final.");
        yield return StartCoroutine(WaitForSecondsOrContinue(3f));
    }

    // ========== PASO 9: COMPLETAR TUTORIAL ==========
    private IEnumerator Tutorial_Completion()
    {
        currentStep = 9;

        ShowTutorial("🎉 ¡TUTORIAL COMPLETADO! 🎉\n\nYa sabes:\n✅ Defender con cartas\n✅ Atacar con operaciones\n✅ Usar PowerUps estratégicamente\n\n¡Buena suerte en batalla!");
        yield return StartCoroutine(WaitForSecondsOrContinue(5f));

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // ========== UTILIDADES - PAUSAR/REANUDAR CON GAMESPEEDMANAGER ==========

    /// <summary>
    /// PAUSA COMPLETA: GameSpeedManager + NavMesh
    /// </summary>
    private void PauseGame()
    {

        isTutorialPaused = true;

        // PAUSAR con GameSpeedManager (afecta velocidad global)
        if (GameSpeedManager.Instance != null)
        {
            GameSpeedManager.Instance.GameSpeedMultiplier = 0f; // PAUSA TOTAL
        }
        else
        {
            Debug.LogWarning("[TutorialManager] GameSpeedManager.Instance no encontrado");
        }

        // PAUSAR NavMeshAgents por si acaso (redundancia)
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

    /// <summary>
    /// REANUDAR JUEGO: GameSpeedManager + NavMesh
    /// </summary>
    private void ResumeGame()
    {

        isTutorialPaused = false;

        // REANUDAR con GameSpeedManager
        if (GameSpeedManager.Instance != null)
        {
            GameSpeedManager.Instance.GameSpeedMultiplier = 1f; // VELOCIDAD NORMAL
        }

        // REANUDAR NavMeshAgents
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

    /// <summary>
    /// BLOQUEO TOTAL: PlayerCardManager + PlayableArea + Panel bloqueante
    /// </summary>
    private void BlockPlayer()
    {
        isPlayerBlocked = true;

        // 1. DESACTIVAR PlayerCardManager
        if (playerCardManager != null)
        {
            playerCardManager.enabled = false;
        }

        // 2. DESACTIVAR PlayableAreaUI
        if (playableAreaUI != null)
        {
            playableAreaUI.enabled = false;
        }

        // 4. DESHABILITAR BOTONES DE OPERADORES
        if (playerCardManager != null)
        {
            if (playerCardManager.SumaButton != null)
            {
                playerCardManager.SumaButton.interactable = false;
            }
            if (playerCardManager.RestaButton != null)
            {
                playerCardManager.RestaButton.interactable = false;
            }
        }

    }

    /// <summary>
    /// DESBLOQUEO COMPLETO: Todo vuelve a la normalidad
    /// </summary>
    private void UnblockPlayer()
    {
        isPlayerBlocked = false;

        // 1. REACTIVAR PlayerCardManager
        if (playerCardManager != null)
        {
            playerCardManager.enabled = true;
        }

        // 2. REACTIVAR PlayableAreaUI
        if (playableAreaUI != null)
        {
            playableAreaUI.enabled = true;
        }

        // 4. REHABILITAR BOTONES DE OPERADORES
        if (playerCardManager != null)
        {
            if (playerCardManager.SumaButton != null)
            {
                playerCardManager.SumaButton.interactable = true;
            }
            if (playerCardManager.RestaButton != null)
            {
                playerCardManager.RestaButton.interactable = true;
            }
        }

    }

    /// <summary>
    /// DESBLOQUEO PARCIAL: Solo permite PowerUps (NO cartas ni área de juego)
    /// </summary>
    private void UnblockPlayerForPowerUps()
    {
        isPlayerBlocked = false; // Técnicamente desbloqueado pero limitado

        // MANTENER PlayerCardManager DESACTIVADO
        if (playerCardManager != null)
        {
            playerCardManager.enabled = false;
        }

        // MANTENER PlayableAreaUI DESACTIVADO
        if (playableAreaUI != null)
        {
            playableAreaUI.enabled = false;
        }


    }

    // ========== UTILIDADES DE UI ==========

    private void ShowTutorial(string message)
    {
        tutorialPanel.SetActive(true);
        tutorialText.text = message;
    }

    private void HideTutorial()
    {
        tutorialPanel.SetActive(false);
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

        if (currentStep == 2 && cardValue == 5 && waitingForPlayerAction)
        {
            waitingForPlayerAction = false;
        }
    }

    public void OnPlayerPlaysOperation()
    {

        if ((currentStep == 4 || currentStep == 8) && waitingForPlayerAction)
        {
            waitingForPlayerAction = false;
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
}