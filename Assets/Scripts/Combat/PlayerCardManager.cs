using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CharacterCombined;

public class PlayerCardManager : MonoBehaviour
{
    public CardManager cardManager;
    public GameObject cardPrefab;            
    public List<Transform> cardSlots = new List<Transform>(4);
    public Button SumaButton;
    public Button RestaButton;

    [Header("Spawn figurativo en mundo")]
    public Transform spawnPoint;

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


    void Start()
    {
        // Inicializa mano
        List<CardManager.Card> randomCards = GetRandomCards(4);
        foreach (var card in randomCards) CreateCard(card);
    }

    private List<CardManager.Card> GetRandomCards(int n)
    {
        List<CardManager.Card> tempList = new List<CardManager.Card>(cardManager.availableCards);
        List<CardManager.Card> result = new List<CardManager.Card>();

        for (int i = 0; i < n && tempList.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, tempList.Count);
            CardManager.Card clone = cardManager.CloneCard(tempList[randomIndex]);
            result.Add(clone);
            tempList.RemoveAt(randomIndex);
        }
        return result;
    }


    public void CreateCard(CardManager.Card data, Transform forcedSlot = null)
    {
        if (data == null)
        {
            Debug.LogWarning("CreateCard recibió null");
            return;
        }

        // Añadir al listado lógico de cartas del jugador
        playerCards.Add(data);

        Transform slotToUse = forcedSlot != null ? forcedSlot : GetFirstFreeSlot();
        if (slotToUse == null)
        {
            Debug.LogWarning("No hay slots libres");
            // si no hay slot, revertir la adición lógica
            playerCards.Remove(data);
            return;
        }

        GameObject newCard = Instantiate(cardPrefab, slotToUse, false);
        newCard.name = data.cardName;

        // Ajustar rectTransform
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
    }

    public void OnCardClickedRequest(CardDisplay display)
    {
        if (display == null) return;

        Debug.Log($"[PlayerCardManager] Click en carta {display.GetCardData()?.cardName} (operator='{currentOperator}')");

        // 0) Si no hay ninguna seleccionada -> esta carta = card1
        if (selectedDisplays.Count == 0)
        {
            selectedDisplays.Add(display);
            display.SetSelectedVisual(true);
            Debug.Log($"[PlayerCardManager] Carta1 seleccionada: {display.GetCardData()?.cardName}");
            return;
        }

        // 1) Si hay exactamente 1 seleccionada:
        if (selectedDisplays.Count == 1)
        {
            var first = selectedDisplays[0];

            // Si hacen click en la misma carta -> deseleccionar
            if (display == first)
            {
                first.SetSelectedVisual(false);
                selectedDisplays.Clear();
                Debug.Log("[PlayerCardManager] Carta1 deseleccionada (click en la misma carta).");
                return;
            }

            // Si NO hay operador seleccionado -> reemplazamos la carta1 por la nueva
            if (currentOperator == '\0')
            {
                first.SetSelectedVisual(false);
                selectedDisplays.Clear();
                selectedDisplays.Add(display);
                display.SetSelectedVisual(true);
                Debug.Log($"[PlayerCardManager] Reemplazamos Carta1 por: {display.GetCardData()?.cardName} (aún sin operador).");
                return;
            }
            else
            {
                // Si HAY operador -> esta carta es la carta2 (válida)
                selectedDisplays.Add(display);
                display.SetSelectedVisual(true);
                Debug.Log($"[PlayerCardManager] Carta2 seleccionada: {display.GetCardData()?.cardName}. Lista lista para combinar.");
                return;
            }
        }

        // 2) Si ya había 2 seleccionadas (caso improbable) -> reseteamos y tomamos esta como carta1
        if (selectedDisplays.Count >= 2)
        {
            foreach (var d in selectedDisplays) d.SetSelectedVisual(false);
            selectedDisplays.Clear();

            selectedDisplays.Add(display);
            display.SetSelectedVisual(true);
            Debug.Log("[PlayerCardManager] Había 2 seleccionadas, reseteando. Nueva Carta1 = " + display.GetCardData()?.cardName);
            return;
        }
    }

    public void DeselectAll()
    {
        selectedDisplays.Clear();
        currentOperator = '\0';
    }


    public bool IsSelected(CardDisplay d) => selectedDisplays.Contains(d);

    private void OnSumaButtonClicked() => TryToggleOperator('+');
    private void OnRestaButtonClicked() => TryToggleOperator('-');

    private void TryToggleOperator(char op)
    {
        float now = Time.time;
        if (now - lastOperatorToggleTime < operatorToggleCooldown)
        {
            Debug.Log("[PlayerCardManager] Ignorando toggle rápido (debounce).");
            return;
        }
        lastOperatorToggleTime = now;

        // Forzar regla: primero debe haber carta1 seleccionada
        if (selectedDisplays.Count == 0)
        {
            Debug.Log("[PlayerCardManager] Selecciona primero una carta (card1) antes de elegir operador.");
            return;
        }

        // Comportamiento toggle pero con protecciones
        if (currentOperator == op)
        {
            // deselecciona operador
            currentOperator = '\0';
            Debug.Log("[PlayerCardManager] Operador desactivado");
            // Si había carta2, la quitamos al desactivar operador
            if (selectedDisplays.Count == 2)
            {
                var second = selectedDisplays[1];
                second.SetSelectedVisual(false);
                selectedDisplays.RemoveAt(1);
                Debug.Log("[PlayerCardManager] Carta2 deseleccionada al quitar operador.");
            }
        }
        else
        {
            // seleccionar operador
            currentOperator = op;
            Debug.Log($"[PlayerCardManager] Operador seleccionado: {op}");
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
            // spawn single
            CardManager.Card c = selectedDisplays[0].GetCardData();

            bool ok = RequestGenerateCharacter(c, spawnPosition, selectedDisplays[0].gameObject);

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
                // Queremos que la PRIMERA carta elegida sea la mayor.
                if (a.cardValue < b.cardValue)
                {
                    // Intercambia visual y lógicamente
                    Debug.Log($"[PlayerCardManager] Intercambiando cartas para que la primera sea mayor: {a.cardValue} < {b.cardValue}");
                    // swap in list
                    selectedDisplays[0] = secondDisplay;
                    selectedDisplays[1] = firstDisplay;
                    // actualizar referencias locales
                    var tmp = a; a = b; b = tmp;

                    // actualizar highlight visual si tienes uno
                    firstDisplay.SetSelectedVisual(false);
                    secondDisplay.SetSelectedVisual(false);
                    selectedDisplays[0].SetSelectedVisual(true);
                    selectedDisplays[1].SetSelectedVisual(true);
                }

                operationResult = a.cardValue - b.cardValue; // ahora debería ser >= 0
                if (operationResult < 0 || operationResult > 5)
                {
                    Debug.Log($"[PlayerCardManager] Resta inválida: {a.cardValue} - {b.cardValue} = {operationResult}. Debe estar en [0..5]. Cancela selección.");
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

            // Llamada al CardManager con parámetros correctos (incluyendo opSymbol)
            bool played = cardManager.GenerateCombinedCharacter(a, b, spawnPosition, operationResult, currentOperator, "PlayerTeam");

            if (played)
            {
                List<Transform> slotsToRefill = new List<Transform>();
                foreach (var d in selectedDisplays)
                {
                    if (d != null && d.transform != null && d.transform.parent != null)
                        slotsToRefill.Add(d.transform.parent);
                }

                // 2) Eliminamos las UIs de las cartas (Destroy será procesado al final del frame)
                foreach (var d in new List<CardDisplay>(selectedDisplays))
                {
                    RemoveCardUI(d); // ya no hace AddNextCard()
                }
                selectedDisplays.Clear();

                // 3) Reponemos explícitamente en los mismos slots
                foreach (var slot in slotsToRefill)
                {
                    if (slot == null) continue;
                    // Pedimos una nueva carta y la forzamos en ese mismo slot
                    List<CardManager.Card> newCards = GetRandomCards(1);
                    if (newCards != null && newCards.Count > 0)
                    {
                        CreateCard(newCards[0], slot);
                    }
                    else
                    {
                        // fallback: intenta AddNextCard (igual que antes)
                        AddNextCard();
                    }
                }
            }
            else
            {
                Debug.Log("[PlayerCardManager] No se pudo jugar la combinación.");
                // Nota: CardManager/CharacterManager deberían devolver el intelecto si fallan al instanciar
            }

            currentOperator = '\0';
            return;
        }
    }

    public bool RequestGenerateCharacter(CardManager.Card cardData, Vector3 spawnPosition, GameObject cardUI)
    {
        Debug.Log($"[PlayerCardManager] RequestGenerateCharacter: intentando jugar {cardData.cardName} coste {cardData.intelectCost}");

        // CORREGIDO: Ahora GenerateCharacter espera 3 parámetros
        bool ok = cardManager.GenerateCharacter(cardData, spawnPosition, "PlayerTeam");

        if (ok)
        {
            // Guardar el slot antes de destruir
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
                List<CardManager.Card> newCards = GetRandomCards(1);
                if (newCards != null && newCards.Count > 0)
                {
                    CreateCard(newCards[0], slotOfCard);
                }
                else
                {
                    AddNextCard();
                }
            }
            else
            {
                AddNextCard();
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

    private void AddNextCard()
    {
        List<CardManager.Card> randomCards = GetRandomCards(1);
        foreach (var card in randomCards) CreateCard(card);
    }

    public void DiscardCard(GameObject card)
    {
        if (spawnedCards.Contains(card))
        {
            spawnedCards.Remove(card);
            Destroy(card);
        }
    }
}
