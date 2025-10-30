using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CharacterCombined;

public class PlayerCardManager : MonoBehaviour
{
    public CardManager cardManager;
    public GameObject cardPrefab;
    public List<Transform> cardSlots = new List<Transform>(5);
    public Button SumaButton;
    public Button RestaButton;

    public Transform spawnPoint;

    public float elevationAmount = 500f;

    [Header("Deployment Zone Feedback")]
    [Tooltip("Referencia al PlayableAreaUI para controlar zonas de despliegue")]
    public PlayableAreaUI playableAreaUI;

    [Header("Visual Feedback")]
    [Tooltip("Color para cartas válidas/seleccionables")]
    public Color validCardColor = Color.white;
    [Tooltip("Color para cartas inválidas/no seleccionables")]
    public Color invalidCardColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
    [Tooltip("Color para cartas seleccionadas")]
    public Color selectedCardColor = new Color(1f, 1f, 0.5f, 1f);
    [Tooltip("Color para carta seleccionada 2 veces (auto-combinación)")]
    public Color doubleSelectedCardColor = new Color(1f, 0.5f, 1f, 1f);
    [Tooltip("Color para carta seleccionada pero bloqueada (ej: 2 con operador -, no puede seleccionar 2 otra vez)")]
    public Color selectedButBlockedCardColor = new Color(0.7f, 0.7f, 0.4f, 0.8f);
    [Tooltip("Color para botones de operación válidos")]
    public Color validOperatorColor = Color.white;
    [Tooltip("Color para botones de operación inválidos")]
    public Color invalidOperatorColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
    [Tooltip("Color para el operador actualmente seleccionado")]
    public Color selectedOperatorColor = new Color(1f, 1f, 0.5f, 1f);

    [Header("UI Feedback")]
    public IntelectBar intelectBar;

    private List<CardDisplay> selectedDisplays = new List<CardDisplay>(2);
    private List<CardManager.Card> playerCards = new List<CardManager.Card>();
    private List<GameObject> spawnedCards = new List<GameObject>();
    private char currentOperator = '\0';

    private float lastOperatorToggleTime = -10f;
    private const float operatorToggleCooldown = 0.18f;

    void Awake()
    {
        if (cardPrefab == null) Debug.LogError("Card Prefab is not assigned in the inspector.");
        if (cardSlots == null) Debug.LogError("Card slots not assigned.");
        if (cardManager == null) Debug.LogError("CardManager not assigned.");
    }

    void OnEnable()
    {
        if (SumaButton != null)
        {
            SumaButton.onClick.RemoveListener(OnSumaButtonClicked);
            SumaButton.onClick.AddListener(OnSumaButtonClicked);
        }
        if (RestaButton != null)
        {
            RestaButton.onClick.RemoveListener(OnRestaButtonClicked);
            RestaButton.onClick.AddListener(OnRestaButtonClicked);
        }
    }

    void OnDisable()
    {
        if (SumaButton != null) SumaButton.onClick.RemoveListener(OnSumaButtonClicked);
        if (RestaButton != null) RestaButton.onClick.RemoveListener(OnRestaButtonClicked);
    }

    [Header("Regla especial: Duplicar carta")]
    public bool allowSelfCombination = true;

    void Start()
    {
        for (int i = 0; i < 5 && i < cardSlots.Count; i++)
        {
            CardManager.Card originalCard = cardManager.GetCardByIndex(i);
            if (originalCard != null)
            {
                CardManager.Card clonedCard = cardManager.CloneCard(originalCard);
                CreateCard(clonedCard, cardSlots[i]);
            }
        }

        UpdateVisualFeedback();
    }

    public void CreateCard(CardManager.Card data, Transform forcedSlot = null)
    {
        if (data == null)
        {
            Debug.LogWarning("CreateCard recibió null");
            return;
        }

        playerCards.Add(data);

        Transform slotToUse = forcedSlot != null ? forcedSlot : GetFirstFreeSlot();
        if (slotToUse == null)
        {
            Debug.LogWarning("No hay slots libres");
            playerCards.Remove(data);
            return;
        }

        GameObject newCard = Instantiate(cardPrefab, slotToUse, false);
        newCard.name = data.cardName;

        RectTransform rt = newCard.GetComponent<RectTransform>();
        RectTransform slotRT = slotToUse.GetComponent<RectTransform>();
        if (rt != null && slotRT != null)
        {
            rt.localScale = Vector3.one;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = slotRT.pivot;
        }

        CardDisplay display = newCard.GetComponent<CardDisplay>();
        if (display != null)
        {
            display.ownerManager = this;
            display.SetCardData(data);
        }
        else
        {
            Debug.LogWarning("cardPrefab no contiene CardDisplay.");
        }

        spawnedCards.Add(newCard);

        UpdateVisualFeedback();
    }

    public void OnCardClickedRequest(CardDisplay display)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameplayDisabled)
        {
            return;
        }

        if (display == null)
        {
            Debug.LogWarning("[PlayerCardManager] CardDisplay es null!");
            return;
        }

        // PRIMERA SELECCIÓN
        if (selectedDisplays.Count == 0)
        {
            selectedDisplays.Add(display);
            SetCardElevation(display, true);

            AudioManager.Instance?.PlayCardSelected();

            UpdateVisualFeedback();
            return;
        }

        // YA HAY UNA CARTA SELECCIONADA
        if (selectedDisplays.Count == 1)
        {
            var first = selectedDisplays[0];
            bool isSameCard = ReferenceEquals(display, first);

            if (isSameCard)
            {
                if (currentOperator == '\0')
                {
                    SetCardElevation(first, false);
                    selectedDisplays.Clear();
                    UpdateVisualFeedback();
                    return;
                }
                else if (allowSelfCombination)
                {
                    int cardValue = display.cardData.cardValue;
                    bool isValidResult = false;

                    if (currentOperator == '+')
                    {
                        int result = cardValue + cardValue;
                        isValidResult = (result <= 5);
                    }
                    else if (currentOperator == '-')
                    {
                        isValidResult = false;
                    }

                    if (isValidResult)
                    {
                        selectedDisplays.Add(display);
                        UpdateVisualFeedback();
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    SetCardElevation(first, false);
                    selectedDisplays.Clear();
                    currentOperator = '\0';
                    UpdateVisualFeedback();
                    return;
                }
            }

            if (currentOperator == '\0')
            {
                SetCardElevation(first, false);
                selectedDisplays.Clear();
                selectedDisplays.Add(display);
                SetCardElevation(display, true);
                UpdateVisualFeedback();

                AudioManager.Instance?.PlayCardSelected();

                return;
            }
            else
            {
                int firstValue = first.cardData.cardValue;
                int secondValue = display.cardData.cardValue;
                bool isValidCombination = false;

                if (currentOperator == '+')
                {
                    int result = firstValue + secondValue;
                    isValidCombination = (result <= 5);
                }
                else if (currentOperator == '-')
                {
                    int result = firstValue - secondValue;
                    isValidCombination = (result > 0 && result <= 5);
                }

                if (isValidCombination)
                {
                    selectedDisplays.Add(display);
                    SetCardElevation(display, true);

                    AudioManager.Instance?.PlayCardSelected();

                    UpdateVisualFeedback();
                    return;
                }
                else
                {
                    return;
                }
            }
        }

        // YA HAY DOS CARTAS
        if (selectedDisplays.Count >= 2)
        {
            foreach (var d in selectedDisplays) SetCardElevation(d, false);
            selectedDisplays.Clear();
            currentOperator = '\0';

            selectedDisplays.Add(display);
            SetCardElevation(display, true);

            AudioManager.Instance?.PlayCardSelected();

            UpdateVisualFeedback();
            return;
        }
    }

    private void SetCardElevation(CardDisplay display, bool elevated)
    {
        if (display == null) return;

        RectTransform rt = display.GetComponent<RectTransform>();
        if (rt != null)
        {
            if (elevated)
            {
                rt.offsetMin = new Vector2(rt.offsetMin.x, elevationAmount);
                rt.offsetMax = new Vector2(rt.offsetMax.x, elevationAmount);
            }
            else
            {
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }

        display.SetSelectedVisual(elevated);
    }

    public void DeselectAll()
    {
        foreach (var d in selectedDisplays)
        {
            SetCardElevation(d, false);
        }
        selectedDisplays.Clear();
        currentOperator = '\0';
        UpdateVisualFeedback();
    }

    public bool IsSelected(CardDisplay d) => selectedDisplays.Contains(d);

    private void OnSumaButtonClicked() => TryToggleOperator('+');
    private void OnRestaButtonClicked() => TryToggleOperator('-');

    private void TryToggleOperator(char op)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameplayDisabled)
        {
            return;
        }

        float now = Time.time;
        if (now - lastOperatorToggleTime < operatorToggleCooldown)
        {
            return;
        }
        lastOperatorToggleTime = now;

        if (selectedDisplays.Count == 0)
        {
            return;
        }

        if (selectedDisplays.Count >= 2)
        {
            return;
        }

        // ✅ MODIFICADO: Solo verificar restricción si hay tutorial activo
        if (TutorialManager.Instance != null && !TutorialManager.Instance.CanPlayOperation())
        {
            Debug.LogWarning("[Tutorial] ⛔ No puedes usar operadores en este paso");
            ShowInsufficientIntellectFeedback();
            return;
        }

        if (currentOperator == op)
        {
            currentOperator = '\0';
            if (selectedDisplays.Count == 2)
            {
                var second = selectedDisplays[1];
                SetCardElevation(second, false);
                selectedDisplays.RemoveAt(1);
            }
            UpdateVisualFeedback();
        }
        else
        {
            currentOperator = op;
            AudioManager.Instance?.PlayOperatorSelected();
            UpdateVisualFeedback();
        }
    }

    public void HandlePlayAreaClick(Vector3 spawnPosition)
    {
        if (selectedDisplays.Count == 0)
        {
            return;
        }

        // ✅ MODIFICADO: Solo verificar si hay tutorial activo
        if (selectedDisplays.Count == 1)
        {
            // Carta individual
            if (TutorialManager.Instance != null && !TutorialManager.Instance.CanPlaySingleCard())
            {
                Debug.LogWarning("[Tutorial] ⛔ No puedes jugar cartas individuales en este paso");
                ShowInsufficientIntellectFeedback();
                DeselectAll();
                return;
            }

            CardManager.Card c = selectedDisplays[0].GetCardData();
            CardManager.GenerateResult result;
            GameObject character = cardManager.GenerateCharacter(c, spawnPosition, "PlayerTeam", out result, null);

            if (result == CardManager.GenerateResult.InsufficientIntellect)
            {
                ShowInsufficientIntellectFeedback();
                DeselectAll();
                currentOperator = '\0';
                return;
            }

            if (character != null)
            {
                int cardValuePlayed = c.cardValue;
                Transform slotOfCard = selectedDisplays[0].transform.parent;

                RemoveCardUI(selectedDisplays[0]);
                selectedDisplays.Clear();

                if (slotOfCard != null)
                {
                    int indexToGet = cardValuePlayed - 1;
                    CardManager.Card newCard = cardManager.GetCardByIndex(indexToGet);
                    if (newCard != null)
                    {
                        CreateCard(cardManager.CloneCard(newCard), slotOfCard);
                    }
                }

                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.OnPlayerPlaysCard(cardValuePlayed);
                }
            }

            DeselectAll();
            currentOperator = '\0';
            return;
        }

        if (selectedDisplays.Count == 2)
        {
            if (currentOperator == '\0')
            {
                DeselectAll();
                return;
            }

            // ✅ MODIFICADO: Solo verificar si hay tutorial activo
            if (TutorialManager.Instance != null && !TutorialManager.Instance.CanPlayOperation())
            {
                Debug.LogWarning("[Tutorial] ⛔ No puedes jugar operaciones en este paso");
                ShowInsufficientIntellectFeedback();
                DeselectAll();
                return;
            }

            var firstDisplay = selectedDisplays[0];
            var secondDisplay = selectedDisplays[1];

            bool isAutoCombo = ReferenceEquals(firstDisplay, secondDisplay);

            var a = firstDisplay.GetCardData();
            var b = secondDisplay.GetCardData();
            if (a == null || b == null)
            {
                Debug.LogWarning("Carta null en combinación");
                DeselectAll();
                return;
            }

            int operationResult = 0;

            if (currentOperator == '+')
            {
                operationResult = a.cardValue + b.cardValue;
                if (operationResult > 5)
                {
                    DeselectAll();
                    return;
                }
            }
            else if (currentOperator == '-')
            {
                if (a.cardValue < b.cardValue)
                {
                    selectedDisplays[0] = secondDisplay;
                    selectedDisplays[1] = firstDisplay;
                    var tmp = a; a = b; b = tmp;

                    SetCardElevation(firstDisplay, false);
                    SetCardElevation(secondDisplay, false);
                    SetCardElevation(selectedDisplays[0], true);
                    SetCardElevation(selectedDisplays[1], true);
                }

                operationResult = a.cardValue - b.cardValue;
                if (operationResult <= 0 || operationResult > 5)
                {
                    DeselectAll();
                    return;
                }
            }
            else
            {
                Debug.LogWarning("[PlayerCardManager] Operador desconocido: " + currentOperator);
                DeselectAll();
                return;
            }

            CardManager.GenerateResult result;
            bool success = cardManager.GenerateCombinedCharacter(a, b, spawnPosition, operationResult, currentOperator, "PlayerTeam", out result, null);

            if (result == CardManager.GenerateResult.InsufficientIntellect)
            {
                ShowInsufficientIntellectFeedback();
                DeselectAll();
                currentOperator = '\0';
                return;
            }

            if (success)
            {
                if (isAutoCombo)
                {
                    int valueUsed = a.cardValue;
                    Transform slotToRefill = firstDisplay.transform.parent;

                    RemoveCardUI(firstDisplay);
                    selectedDisplays.Clear();

                    if (slotToRefill != null)
                    {
                        int indexToGet = valueUsed - 1;
                        CardManager.Card newCard = cardManager.GetCardByIndex(indexToGet);
                        if (newCard != null)
                        {
                            CreateCard(cardManager.CloneCard(newCard), slotToRefill);
                        }
                    }
                }
                else
                {
                    int valueA = a.cardValue;
                    int valueB = b.cardValue;

                    List<Transform> slotsToRefill = new List<Transform>();
                    foreach (var d in selectedDisplays)
                    {
                        if (d != null && d.transform != null && d.transform.parent != null)
                            slotsToRefill.Add(d.transform.parent);
                    }

                    foreach (var d in new List<CardDisplay>(selectedDisplays))
                    {
                        RemoveCardUI(d);
                    }
                    selectedDisplays.Clear();

                    for (int i = 0; i < slotsToRefill.Count; i++)
                    {
                        Transform slot = slotsToRefill[i];
                        if (slot == null) continue;

                        int indexToGet = i == 0 ? (valueA - 1) : (valueB - 1);
                        CardManager.Card newCard = cardManager.GetCardByIndex(indexToGet);

                        if (newCard != null)
                        {
                            CreateCard(cardManager.CloneCard(newCard), slot);
                        }
                    }
                }

                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.OnPlayerPlaysOperation();
                    Debug.Log($"[PlayerCardManager] 🔔 Notificando tutorial: operación {currentOperator}");
                }
            }

            currentOperator = '\0';
            UpdateVisualFeedback();
            return;
        }

        DeselectAll();
        currentOperator = '\0';
    }

    public bool RequestGenerateCharacter(CardManager.Card cardData, Vector3 spawnPosition, GameObject cardUI)
    {
        bool ok = cardManager.GenerateCharacter(cardData, spawnPosition, "PlayerTeam");

        if (ok)
        {
            int cardValuePlayed = cardData.cardValue;

            Transform slotOfCard = null;
            if (cardUI != null)
            {
                slotOfCard = cardUI.transform.parent;
            }

            if (cardUI != null && spawnedCards.Contains(cardUI))
            {
                spawnedCards.Remove(cardUI);
                Destroy(cardUI);
            }

            if (playerCards.Contains(cardData)) playerCards.Remove(cardData);

            if (slotOfCard != null)
            {
                int indexToGet = cardValuePlayed - 1;
                CardManager.Card newCard = cardManager.GetCardByIndex(indexToGet);

                if (newCard != null)
                {
                    CreateCard(cardManager.CloneCard(newCard), slotOfCard);
                }
            }

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnPlayerPlaysCard(cardData.cardValue);
                Debug.Log($"[PlayerCardManager] 🔔 Notificando tutorial: carta {cardData.cardValue}");
            }

            return true;
        }
        return false;
    }

    private void RemoveCardUI(CardDisplay display)
    {
        if (display == null) return;
        GameObject go = display.gameObject;
        if (spawnedCards.Contains(go)) spawnedCards.Remove(go);
        Destroy(go);

        var cd = display.GetCardData();
        if (playerCards.Contains(cd)) playerCards.Remove(cd);
    }

    private Transform GetFirstFreeSlot()
    {
        foreach (Transform slot in cardSlots)
        {
            if (slot.childCount == 0) return slot;
        }
        return null;
    }

    public void DiscardCard(GameObject card)
    {
        if (spawnedCards.Contains(card))
        {
            spawnedCards.Remove(card);
            Destroy(card);
        }
    }

    private bool IsCardValid(CardDisplay display)
    {
        if (display == null || display.cardData == null) return false;

        if (selectedDisplays.Count == 0) return true;

        if (selectedDisplays.Count == 1 && currentOperator == '\0') return true;

        if (selectedDisplays.Count >= 2) return false;

        if (selectedDisplays.Count == 1 && currentOperator != '\0')
        {
            var firstCard = selectedDisplays[0];
            int firstValue = firstCard.cardData.cardValue;
            int currentValue = display.cardData.cardValue;

            if (ReferenceEquals(display, firstCard))
            {
                if (!allowSelfCombination) return false;

                if (currentOperator == '+')
                {
                    int result = firstValue + firstValue;
                    return result <= 5;
                }
                else if (currentOperator == '-')
                {
                    return false;
                }
                return false;
            }

            if (currentOperator == '+')
            {
                int result = firstValue + currentValue;
                return result <= 5;
            }
            else if (currentOperator == '-')
            {
                int result = firstValue - currentValue;
                return result > 0 && result <= 5;
            }
        }

        if (selectedDisplays.Count >= 2) return false;

        return true;
    }

    private bool IsOperatorValid(char op)
    {
        // ✅ MODIFICADO: Solo verificar si hay tutorial activo
        if (TutorialManager.Instance != null && !TutorialManager.Instance.CanPlayOperation())
        {
            return false;
        }

        if (selectedDisplays.Count == 0) return false;

        if (selectedDisplays.Count == 1)
        {
            int firstValue = selectedDisplays[0].cardData.cardValue;
            var firstDisplay = selectedDisplays[0];

            if (op == '+')
            {
                foreach (var card in spawnedCards)
                {
                    var display = card.GetComponent<CardDisplay>();
                    if (display != null && display.cardData != null)
                    {
                        if (ReferenceEquals(display, firstDisplay))
                        {
                            if (allowSelfCombination)
                            {
                                int selfResult = firstValue + firstValue;
                                if (selfResult <= 5) return true;
                            }
                            continue;
                        }

                        int result = firstValue + display.cardData.cardValue;
                        if (result <= 5) return true;
                    }
                }
                return false;
            }
            else if (op == '-')
            {
                foreach (var card in spawnedCards)
                {
                    var display = card.GetComponent<CardDisplay>();
                    if (display != null && display.cardData != null)
                    {
                        if (ReferenceEquals(display, firstDisplay))
                        {
                            continue;
                        }

                        int result = firstValue - display.cardData.cardValue;
                        if (result > 0 && result <= 5) return true;
                    }
                }
                return false;
            }
        }

        return false;
    }

    private void UpdateDeploymentZoneFeedback()
    {
        if (playableAreaUI == null) return;

        bool canDeploy = false;

        if (selectedDisplays.Count == 1)
        {
            canDeploy = true;
        }
        else if (selectedDisplays.Count == 2 && currentOperator != '\0')
        {
            canDeploy = true;
        }

        if (canDeploy)
        {
            playableAreaUI.ShowDeploymentZones();
        }
        else
        {
            playableAreaUI.HideDeploymentZones();
        }
    }

    private void UpdateVisualFeedback()
    {
        UpdateDeploymentZoneFeedback();

        UpdateIntelectPreview();

        bool isAutoCombination = false;
        if (selectedDisplays.Count == 2 && ReferenceEquals(selectedDisplays[0], selectedDisplays[1]))
        {
            isAutoCombination = true;
        }

        foreach (var cardObj in spawnedCards)
        {
            if (cardObj == null) continue;

            var display = cardObj.GetComponent<CardDisplay>();
            if (display == null) continue;

            bool isValid = IsCardValid(display);
            bool isSelected = selectedDisplays.Contains(display);

            Image cardImage = display.GetComponent<Image>();
            if (cardImage != null)
            {
                if (isAutoCombination && ReferenceEquals(display, selectedDisplays[0]))
                {
                    cardImage.color = doubleSelectedCardColor;
                }
                else if (isSelected && !isValid && selectedDisplays.Count == 1 && currentOperator != '\0')
                {
                    cardImage.color = selectedButBlockedCardColor;
                }
                else if (isSelected)
                {
                    cardImage.color = selectedCardColor;
                }
                else if (isValid)
                {
                    cardImage.color = validCardColor;
                }
                else
                {
                    cardImage.color = invalidCardColor;
                }
            }
        }

        if (SumaButton != null)
        {
            bool sumaSelected = (currentOperator == '+');
            bool operationComplete = (selectedDisplays.Count >= 2);

            Image btnImage = SumaButton.GetComponent<Image>();
            if (btnImage != null)
            {
                if (sumaSelected)
                {
                    btnImage.color = selectedOperatorColor;
                }
                else
                {
                    bool sumaValid = IsOperatorValid('+');
                    btnImage.color = sumaValid ? validOperatorColor : invalidOperatorColor;
                }
            }

            if (operationComplete)
            {
                SumaButton.interactable = true;
            }
            else
            {
                bool sumaValid = IsOperatorValid('+');
                SumaButton.interactable = sumaValid || sumaSelected;
            }
        }

        if (RestaButton != null)
        {
            bool restaSelected = (currentOperator == '-');
            bool operationComplete = (selectedDisplays.Count >= 2);

            Image btnImage = RestaButton.GetComponent<Image>();
            if (btnImage != null)
            {
                if (restaSelected)
                {
                    btnImage.color = selectedOperatorColor;
                }
                else
                {
                    bool restaValid = IsOperatorValid('-');
                    btnImage.color = restaValid ? validOperatorColor : invalidOperatorColor;
                }
            }

            if (operationComplete)
            {
                RestaButton.interactable = true;
            }
            else
            {
                bool restaValid = IsOperatorValid('-');
                RestaButton.interactable = restaValid || restaSelected;
            }
        }
    }

    private void ShowInsufficientIntellectFeedback()
    {
        if (intelectBar != null)
        {
            intelectBar.ShakeBar();
        }

        if (ScreenFlashEffect.Instance != null)
        {
            ScreenFlashEffect.Instance.Flash();
        }
        else
        {
            Debug.LogWarning("[PlayerCardManager] ScreenFlashEffect.Instance no encontrado");
        }
    }

    private void UpdateIntelectPreview()
    {
        if (intelectBar == null) return;

        int previewCost = 0;

        if (selectedDisplays.Count == 1)
        {
            var card = selectedDisplays[0].GetCardData();
            if (card != null)
            {
                previewCost = (card.intelectCost > 0) ? card.intelectCost : card.cardValue;
            }
        }
        else if (selectedDisplays.Count == 2 && currentOperator != '\0')
        {
            var cardA = selectedDisplays[0].GetCardData();
            var cardB = selectedDisplays[1].GetCardData();

            if (cardA != null && cardB != null)
            {
                int result = 0;

                if (currentOperator == '+')
                {
                    result = cardA.cardValue + cardB.cardValue;
                }
                else if (currentOperator == '-')
                {
                    result = Mathf.Abs(cardA.cardValue - cardB.cardValue);
                }

                previewCost = Mathf.Max(0, result);
            }
        }

        if (previewCost > 0)
        {
            intelectBar.ShowPreview(previewCost);
        }
        else
        {
            intelectBar.HidePreview();
        }
    }
}