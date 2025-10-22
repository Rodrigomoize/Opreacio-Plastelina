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
    
    [Header("Visual Feedback")]
    [Tooltip("Color para cartas válidas/seleccionables")]
    public Color validCardColor = Color.white;
    [Tooltip("Color para cartas inválidas/no seleccionables")]
    public Color invalidCardColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
    [Tooltip("Color para cartas seleccionadas")]
    public Color selectedCardColor = new Color(1f, 1f, 0.5f, 1f);
    [Tooltip("Color para carta seleccionada 2 veces (auto-combinación)")]
    public Color doubleSelectedCardColor = new Color(1f, 0.5f, 1f, 1f); // Rosa/magenta para indicar doble selección
    [Tooltip("Color para carta seleccionada pero bloqueada (ej: 2 con operador -, no puede seleccionar 2 otra vez)")]
    public Color selectedButBlockedCardColor = new Color(0.7f, 0.7f, 0.4f, 0.8f); // Amarillo oscuro/grisáceo
    [Tooltip("Color para botones de operación válidos")]
    public Color validOperatorColor = Color.white;
    [Tooltip("Color para botones de operación inválidos")]
    public Color invalidOperatorColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
    [Tooltip("Color para el operador actualmente seleccionado")]
    public Color selectedOperatorColor = new Color(1f, 1f, 0.5f, 1f);

    // Estado de selección / operación
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
    public bool allowSelfCombination = true; // Permitir usar la misma carta dos veces (ej: 1+1 con una sola carta)

    void Start()
    {
        // Inicializa mano con cartas ordenadas del 1-5 (una de cada)
        for (int i = 0; i < 5 && i < cardSlots.Count; i++)
        {
            CardManager.Card originalCard = cardManager.GetCardByIndex(i);
            if (originalCard != null)
            {
                CardManager.Card clonedCard = cardManager.CloneCard(originalCard);
                CreateCard(clonedCard, cardSlots[i]);
            }
        }
        
        // Estado inicial: todas las cartas válidas
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
        
        // Actualizar feedback visual después de agregar carta
        UpdateVisualFeedback();
    }

    public void OnCardClickedRequest(CardDisplay display)
    {
        if (display == null)
        {
            Debug.LogWarning("[PlayerCardManager] CardDisplay es null!");
            return;
        }

        Debug.Log($"[PlayerCardManager] Click en carta. InstanceID: {display.GetInstanceID()}, Valor: {display.cardData?.cardValue}, Cartas seleccionadas: {selectedDisplays.Count}, Operador: '{currentOperator}'");

        // PRIMERA SELECCIÓN
        if (selectedDisplays.Count == 0)
        {
            selectedDisplays.Add(display);
            SetCardElevation(display, true);
            Debug.Log($"[PlayerCardManager] ✓ Primera carta seleccionada: {display.cardData.cardName} (ID: {display.GetInstanceID()})");
            UpdateVisualFeedback(); // Actualizar feedback visual
            return;
        }

        // YA HAY UNA CARTA SELECCIONADA
        if (selectedDisplays.Count == 1)
        {
            var first = selectedDisplays[0];
            bool isSameCard = ReferenceEquals(display, first);

            Debug.Log($"[PlayerCardManager] Comparando: First ID={first.GetInstanceID()}, Current ID={display.GetInstanceID()}, Same={isSameCard}");

            // Si es LA MISMA carta física
            if (isSameCard)
            {
                // Si NO hay operador: deseleccionar
                if (currentOperator == '\0')
                {
                    Debug.Log($"[PlayerCardManager] ✗ Deseleccionando carta (mismo GameObject, sin operador)");
                    SetCardElevation(first, false);
                    selectedDisplays.Clear();
                    UpdateVisualFeedback();
                    return;
                }
                // Si HAY operador Y permitimos auto-combinación: validar si el resultado es válido
                else if (allowSelfCombination)
                {
                    int cardValue = display.cardData.cardValue;
                    bool isValidResult = false;
                    
                    // Validar según el operador
                    if (currentOperator == '+')
                    {
                        int result = cardValue + cardValue;
                        isValidResult = (result <= 5);
                        
                        if (!isValidResult)
                        {
                            Debug.Log($"[PlayerCardManager] ✗ Auto-combinación inválida: {cardValue}+{cardValue}={result} (>5)");
                        }
                    }
                    else if (currentOperator == '-')
                    {
                        // Carta consigo misma siempre da 0, que NO es válido (debe ser >0)
                        isValidResult = false;
                        Debug.Log($"[PlayerCardManager] ✗ Auto-combinación inválida: {cardValue}-{cardValue}=0 (debe ser >0)");
                    }
                    
                    if (isValidResult)
                    {
                        Debug.Log($"[PlayerCardManager] ✓ Auto-combinación válida: usando la misma carta dos veces con operador '{currentOperator}'");
                        selectedDisplays.Add(display); // Añadir la misma carta otra vez
                        // No elevamos más porque ya está elevada
                        UpdateVisualFeedback();
                        return;
                    }
                    else
                    {
                        Debug.Log($"[PlayerCardManager] ✗ Auto-combinación bloqueada: resultado inválido");
                        // No hacer nada, la carta ya está seleccionada y el operador activo
                        return;
                    }
                }
                // Si HAY operador pero NO permitimos auto-combinación: deseleccionar
                else
                {
                    Debug.Log($"[PlayerCardManager] ✗ Auto-combinación no permitida");
                    SetCardElevation(first, false);
                    selectedDisplays.Clear();
                    currentOperator = '\0';
                    UpdateVisualFeedback();
                    return;
                }
            }

            // Si NO es la misma carta (carta diferente)
            // Si NO hay operador, reemplazar selección
            if (currentOperator == '\0')
            {
                Debug.Log($"[PlayerCardManager] ⚠ Cambiando selección (sin operador activo)");
                SetCardElevation(first, false);
                selectedDisplays.Clear();
                selectedDisplays.Add(display);
                SetCardElevation(display, true);
                UpdateVisualFeedback();
                return;
            }
            else
            {
                // SI hay operador, validar si la combinación es válida antes de añadir
                int firstValue = first.cardData.cardValue;
                int secondValue = display.cardData.cardValue;
                bool isValidCombination = false;
                
                if (currentOperator == '+')
                {
                    int result = firstValue + secondValue;
                    isValidCombination = (result <= 5);
                    
                    if (!isValidCombination)
                    {
                        Debug.Log($"[PlayerCardManager] ✗ Combinación inválida: {firstValue}+{secondValue}={result} (>5)");
                    }
                }
                else if (currentOperator == '-')
                {
                    int result = firstValue - secondValue;
                    isValidCombination = (result > 0 && result <= 5); // Cambiado: debe ser >0, no >=0
                    
                    if (!isValidCombination)
                    {
                        Debug.Log($"[PlayerCardManager] ✗ Combinación inválida: {firstValue}-{secondValue}={result} (debe ser >0 y ≤5)");
                    }
                }
                
                if (isValidCombination)
                {
                    // Combinación válida, añadir segunda carta
                    Debug.Log($"[PlayerCardManager] ✓ Segunda carta seleccionada: {display.cardData.cardName} (ID: {display.GetInstanceID()}) con operador '{currentOperator}'");
                    selectedDisplays.Add(display);
                    SetCardElevation(display, true);
                    UpdateVisualFeedback();
                    return;
                }
                else
                {
                    // Combinación inválida, no hacer nada
                    Debug.Log($"[PlayerCardManager] ✗ Combinación bloqueada");
                    return;
                }
            }
        }

        // YA HAY DOS CARTAS - reiniciar selección
        if (selectedDisplays.Count >= 2)
        {
            Debug.Log($"[PlayerCardManager] ⚠ Ya hay 2 cartas seleccionadas, reiniciando");
            foreach (var d in selectedDisplays) SetCardElevation(d, false);
            selectedDisplays.Clear();
            currentOperator = '\0';

            selectedDisplays.Add(display);
            SetCardElevation(display, true);
            UpdateVisualFeedback();
            return;
        }
    }

    // Método para elevar o bajar la carta (efecto UNO)
    private void SetCardElevation(CardDisplay display, bool elevated)
    {
        if (display == null) return;

        RectTransform rt = display.GetComponent<RectTransform>();
        if (rt != null)
        {
            Vector2 offset = rt.anchoredPosition;
            if (elevated)
            {
                offset.y = elevationAmount;
            }
            else
            {
                offset.y = 0f;
            }
            rt.anchoredPosition = offset;
        }

        // Mantener el visual de selección también
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

        // Si la operación está completa (2 cartas), no permitir cambiar operador
        if (selectedDisplays.Count >= 2)
        {
            Debug.Log("[PlayerCardManager] Operación completa: no se puede cambiar operador");
            return;
        }

        if (currentOperator == op)
        {
            currentOperator = '\0';
            Debug.Log("[PlayerCardManager] Operador desactivado");
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
            UpdateVisualFeedback();
        }
    }

    public void HandlePlayAreaClick(Vector3 spawnPosition)
    {
        if (selectedDisplays.Count == 0)
        {
            Debug.Log("[PlayerCardManager] PlayArea clic: nada seleccionado.");
            return;
        }

        if (selectedDisplays.Count == 1)
        {
            CardManager.Card c = selectedDisplays[0].GetCardData();
            CardManager.GenerateResult result;
            GameObject character = cardManager.GenerateCharacter(c, spawnPosition, "PlayerTeam", out result, null);
            
            // Si falló por falta de intelecto, mostrar feedback negativo
            if (result == CardManager.GenerateResult.InsufficientIntellect)
            {
                ShowInsufficientIntellectFeedback();
                DeselectAll();
                currentOperator = '\0';
                return;
            }
            
            // Si tuvo éxito, procesar normalmente
            if (character != null)
            {
                // Resto del código de éxito (remover carta, etc)
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
            }
            
            DeselectAll();
            currentOperator = '\0';
            return;
        }

        if (selectedDisplays.Count == 2)
        {
            if (currentOperator == '\0')
            {
                Debug.Log("[PlayerCardManager] Dos cartas seleccionadas pero sin operador: se deseleccionan.");
                DeselectAll();
                return;
            }

            var firstDisplay = selectedDisplays[0];
            var secondDisplay = selectedDisplays[1];

            // CASO ESPECIAL: Si ambos displays son la misma referencia (auto-combinación)
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
                    Debug.Log($"[PlayerCardManager] Suma inválida: {a.cardValue} + {b.cardValue} = {operationResult} (>5). Cancela selección.");
                    DeselectAll();
                    return;
                }
            }
            else if (currentOperator == '-')
            {
                if (a.cardValue < b.cardValue)
                {
                    Debug.Log($"[PlayerCardManager] Intercambiando cartas para que la primera sea mayor: {a.cardValue} < {b.cardValue}");

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
                    Debug.Log($"[PlayerCardManager] Resta inválida: {a.cardValue} - {b.cardValue} = {operationResult}. Debe estar en (0..5]. Cancela selección.");
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
            bool played = cardManager.GenerateCombinedCharacter(a, b, spawnPosition, operationResult, currentOperator, "PlayerTeam", out result, null);

            // Si falló por falta de intelecto, mostrar feedback negativo
            if (result == CardManager.GenerateResult.InsufficientIntellect)
            {
                ShowInsufficientIntellectFeedback();
                DeselectAll();
                currentOperator = '\0';
                return;
            }

            if (played)
            {
                // Si es auto-combo, solo guardamos UNA carta y UN slot
                if (isAutoCombo)
                {
                    Debug.Log("[PlayerCardManager] Auto-combo jugado: removiendo UNA carta y reponiendo UNA");
                    int valueUsed = a.cardValue;
                    Transform slotToRefill = firstDisplay.transform.parent;

                    RemoveCardUI(firstDisplay);
                    selectedDisplays.Clear();

                    // Reponer la misma carta
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
                    // Combo normal: dos cartas diferentes
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

                    // Rellenar con las cartas correspondientes a los valores jugados
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
            }
            else
            {
                Debug.Log("[PlayerCardManager] No se pudo jugar la combinación.");
            }

            currentOperator = '\0';
            UpdateVisualFeedback();
            return;
        }
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

            // Robar la misma carta que se jugó
            if (slotOfCard != null)
            {
                int indexToGet = cardValuePlayed - 1;
                CardManager.Card newCard = cardManager.GetCardByIndex(indexToGet);

                if (newCard != null)
                {
                    CreateCard(cardManager.CloneCard(newCard), slotOfCard);
                }
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
    
    /// <summary>
    /// Verifica si una carta es válida para seleccionar según el contexto actual
    /// </summary>
    private bool IsCardValid(CardDisplay display)
    {
        if (display == null || display.cardData == null) return false;
        
        // Sin cartas seleccionadas: todas son válidas
        if (selectedDisplays.Count == 0) return true;
        
        // Una carta seleccionada, sin operador: todas son válidas (puedes cambiar selección)
        if (selectedDisplays.Count == 1 && currentOperator == '\0') return true;
        
        // DOS cartas seleccionadas: operación completa, no se pueden seleccionar más
        if (selectedDisplays.Count >= 2) return false;
        
        // Una carta seleccionada CON operador
        if (selectedDisplays.Count == 1 && currentOperator != '\0')
        {
            var firstCard = selectedDisplays[0];
            int firstValue = firstCard.cardData.cardValue;
            int currentValue = display.cardData.cardValue;
            
            // Si es la misma carta, verificar auto-combinación Y que el resultado sea válido
            if (ReferenceEquals(display, firstCard))
            {
                if (!allowSelfCombination) return false;
                
                // Validar que el resultado de usar la misma carta sea válido
                if (currentOperator == '+')
                {
                    int result = firstValue + firstValue;
                    return result <= 5;
                }
                else if (currentOperator == '-')
                {
                    // Carta consigo misma siempre da 0, que NO es válido (debe ser >0)
                    return false;
                }
                return false;
            }
            
            // Validar según operador para cartas diferentes
            if (currentOperator == '+')
            {
                // Suma: resultado no puede superar 5
                int result = firstValue + currentValue;
                return result <= 5;
            }
            else if (currentOperator == '-')
            {
                // Resta: resultado debe ser >0 y ≤5 (no permitir resultado 0)
                int result = firstValue - currentValue;
                return result > 0 && result <= 5; // Cambiado: >0 en vez de >=0
            }
        }
        
        // Dos cartas seleccionadas: no se pueden seleccionar más
        if (selectedDisplays.Count >= 2) return false;
        
        return true;
    }
    
    /// <summary>
    /// Verifica si un operador es válido según las cartas seleccionadas
    /// </summary>
    private bool IsOperatorValid(char op)
    {
        // Sin cartas: operador no válido
        if (selectedDisplays.Count == 0) return false;
        
        // Con una carta: verificar si hay alguna combinación válida posible
        if (selectedDisplays.Count == 1)
        {
            int firstValue = selectedDisplays[0].cardData.cardValue;
            var firstDisplay = selectedDisplays[0];
            
            if (op == '+')
            {
                // Para suma: ver si hay alguna carta que sumada no supere 5
                foreach (var card in spawnedCards)
                {
                    var display = card.GetComponent<CardDisplay>();
                    if (display != null && display.cardData != null)
                    {
                        // Si es la misma carta, verificar auto-combinación
                        if (ReferenceEquals(display, firstDisplay))
                        {
                            if (allowSelfCombination)
                            {
                                int selfResult = firstValue + firstValue;
                                if (selfResult <= 5) return true;
                            }
                            continue; // Si no permite auto-combo o el resultado es >5, continuar con otras cartas
                        }
                        
                        int result = firstValue + display.cardData.cardValue;
                        if (result <= 5) return true; // Hay al menos una opción válida
                    }
                }
                return false; // No hay opciones válidas
            }
            else if (op == '-')
            {
                // Para resta: ver si hay alguna carta que restada dé resultado >0 y ≤5
                foreach (var card in spawnedCards)
                {
                    var display = card.GetComponent<CardDisplay>();
                    if (display != null && display.cardData != null)
                    {
                        // Si es la misma carta, verificar auto-combinación (siempre da 0, NO válido)
                        if (ReferenceEquals(display, firstDisplay))
                        {
                            // X - X = 0, que NO es válido (debe ser >0)
                            continue; // Saltar esta opción
                        }
                        
                        int result = firstValue - display.cardData.cardValue;
                        if (result > 0 && result <= 5) return true; // Hay al menos una opción válida (>0 y ≤5)
                    }
                }
                return false; // No hay opciones válidas
            }
        }
        
        // Con dos cartas: operación completa, no se puede cambiar operador
        return false;
    }
    
    /// <summary>
    /// Actualiza el estado visual de todas las cartas y botones según el contexto
    /// </summary>
    private void UpdateVisualFeedback()
    {
        Debug.Log($"[UpdateVisualFeedback] Cartas seleccionadas: {selectedDisplays.Count}, Operador actual: '{currentOperator}'");
        
        // Detectar si hay auto-combinación (misma carta seleccionada 2 veces)
        bool isAutoCombination = false;
        if (selectedDisplays.Count == 2 && ReferenceEquals(selectedDisplays[0], selectedDisplays[1]))
        {
            isAutoCombination = true;
        }
        
        // Actualizar estado visual de cada carta
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
                // CASO ESPECIAL: Carta seleccionada 2 veces (auto-combinación)
                if (isAutoCombination && ReferenceEquals(display, selectedDisplays[0]))
                {
                    cardImage.color = doubleSelectedCardColor; // Color especial rosa/magenta
                    Debug.Log($"[UpdateVisualFeedback] Carta {display.cardData.cardValue} seleccionada 2 veces - color MAGENTA");
                }
                // CASO ESPECIAL: Carta seleccionada pero bloqueada (ej: 2- y quieres seleccionar 2 otra vez)
                else if (isSelected && !isValid && selectedDisplays.Count == 1 && currentOperator != '\0')
                {
                    // La carta está seleccionada, hay operador activo, pero no es válida para segunda selección
                    cardImage.color = selectedButBlockedCardColor; // Color amarillo oscuro/grisáceo
                    Debug.Log($"[UpdateVisualFeedback] Carta {display.cardData.cardValue} seleccionada pero BLOQUEADA - color GRIS-AMARILLO");
                }
                // Caso normal: carta seleccionada
                else if (isSelected)
                {
                    cardImage.color = selectedCardColor;
                }
                // Carta válida pero no seleccionada
                else if (isValid)
                {
                    cardImage.color = validCardColor;
                }
                // Carta inválida
                else
                {
                    cardImage.color = invalidCardColor;
                }
            }
        }
        
        // Actualizar botones de operación
        if (SumaButton != null)
        {
            bool sumaSelected = (currentOperator == '+');
            bool operationComplete = (selectedDisplays.Count >= 2);
            
            Debug.Log($"[UpdateVisualFeedback] Botón +: Selected={sumaSelected}, Complete={operationComplete}");
            
            Image btnImage = SumaButton.GetComponent<Image>();
            if (btnImage != null)
            {
                // Si está seleccionado, siempre usar color de selección
                if (sumaSelected)
                {
                    btnImage.color = selectedOperatorColor;
                    Debug.Log($"[UpdateVisualFeedback] Botón + usando selectedOperatorColor: {selectedOperatorColor}");
                }
                else
                {
                    bool sumaValid = IsOperatorValid('+');
                    btnImage.color = sumaValid ? validOperatorColor : invalidOperatorColor;
                }
            }
            
            // Cuando la operación está completa, el botón NO debe ser clickeable pero SÍ verse seleccionado
            // Para evitar que Unity lo oscurezca automáticamente, lo dejamos interactable pero bloqueamos clicks
            if (operationComplete)
            {
                // Mantener interactable para que no se oscurezca, pero no responderá a clicks (la lógica lo previene)
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
                // Si está seleccionado, siempre usar color de selección
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
            
            // Cuando la operación está completa, el botón NO debe ser clickeable pero SÍ verse seleccionado
            // Para evitar que Unity lo oscurezca automáticamente, lo dejamos interactable pero bloqueamos clicks
            if (operationComplete)
            {
                // Mantener interactable para que no se oscurezca, pero no responderá a clicks (la lógica lo previene)
                RestaButton.interactable = true;
            }
            else
            {
                bool restaValid = IsOperatorValid('-');
                RestaButton.interactable = restaValid || restaSelected;
            }
        }
    }
    
    /// <summary>
    /// Muestra feedback visual negativo cuando no hay suficiente intelecto
    /// </summary>
    private void ShowInsufficientIntellectFeedback()
    {
        // Activar camera shake si está disponible
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake();
        }
        else
        {
            Debug.LogWarning("[PlayerCardManager] CameraShake.Instance no encontrado");
        }
        
        // Activar flash rojo de pantalla si está disponible
        if (ScreenFlashEffect.Instance != null)
        {
            ScreenFlashEffect.Instance.Flash();
        }
        else
        {
            Debug.LogWarning("[PlayerCardManager] ScreenFlashEffect.Instance no encontrado");
        }
        
        Debug.Log("[PlayerCardManager] ⚠ Intelecto insuficiente - Feedback visual mostrado");
    }
}