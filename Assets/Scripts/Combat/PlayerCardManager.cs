using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    void Awake()
    {
        if (cardPrefab == null) Debug.LogError("Card Prefab is not assigned in the inspector.");
        if (cardSlots == null) Debug.LogError("Card slots not assigned.");
        if (cardManager == null) Debug.LogError("CardManager not assigned.");
    }

    void OnEnable()
    {
        if (SumaButton != null) SumaButton.onClick.AddListener(OnSumaButtonClicked);
        if (RestaButton != null) RestaButton.onClick.AddListener(OnRestaButtonClicked);
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
        data.ShowHimSelf();
    }


    public void OnCardClicked(CardDisplay display)
    {
        if (display == null) return;

        if (display.isSelected)
        {
            if (!selectedDisplays.Contains(display))
            {
                if (selectedDisplays.Count >= 2)
                {
                    var old = selectedDisplays[0];
                    selectedDisplays.RemoveAt(0);
                    old.isSelected = false;
                    
                }
                selectedDisplays.Add(display);
                Debug.Log($"[PlayerCardManager] Selected: {display.GetCardData()?.cardName}");
            }
        }
        else
        {
            if (selectedDisplays.Contains(display))
            {
                selectedDisplays.Remove(display);
                Debug.Log($"[PlayerCardManager] Deselected by click: {display.GetCardData()?.cardName}");
            }
        }

        string list = selectedDisplays.Count == 0 ? "(ninguna)" : string.Join(", ", selectedDisplays.ConvertAll(d => d.GetCardData()?.cardName ?? "null"));
    }

    public void DeselectAll()
    {
        selectedDisplays.Clear();
        currentOperator = '\0';
    }


    public bool IsSelected(CardDisplay d) => selectedDisplays.Contains(d);

    private void OnSumaButtonClicked() => ToggleOperator('+');
    private void OnRestaButtonClicked() => ToggleOperator('-');

    private void ToggleOperator(char op)
    {
        if (currentOperator == op)
        {
            currentOperator = '\0';
        }
        else
        {
            currentOperator = op;
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
                Debug.Log("[PlayerCardManager] Dos cartas seleccionadas pero sin operación: se deseleccionan.");
                DeselectAll();
                return;
            }

            var a = selectedDisplays[0].GetCardData();
            var b = selectedDisplays[1].GetCardData();
            if (a == null || b == null) { Debug.LogWarning("Carta null en combinación"); DeselectAll(); return; }

            // ordenar valores: el mayor debe ir segundo
            int firstVal = Mathf.Min(a.cardValue, b.cardValue);
            int secondVal = Mathf.Max(a.cardValue, b.cardValue);
            int operationResult = currentOperator == '+' ? firstVal + secondVal : firstVal - secondVal;

            bool played = cardManager.GenerateCombinedCharacter(a, b, spawnPosition, operationResult, currentOperator);
            if (played)
            {
                // eliminar UI y reponer
                foreach (var d in new List<CardDisplay>(selectedDisplays)) RemoveCardUI(d);
                selectedDisplays.Clear();
            }
            else
            {
                Debug.Log("[PlayerCardManager] No se pudo jugar la combinación (coste u otro fallo).");
            }

            currentOperator = '\0';
            return;
        }
    }

    private void RemoveCardUI(CardDisplay display)
    {
        if (display == null) return;
        GameObject go = display.gameObject;
        if (spawnedCards.Contains(go)) spawnedCards.Remove(go);
        Destroy(go);
        var cd = display.GetCardData();
        if (playerCards.Contains(cd)) playerCards.Remove(cd);
        AddNextCard();
    }

    public bool RequestGenerateCharacter(CardManager.Card cardData, Vector3 spawnPosition, GameObject cardUI)
    {
        Debug.Log($"[PlayerCardManager] RequestGenerateCharacter: intentando jugar {cardData.cardName} coste {cardData.intelectCost}");
        bool ok = cardManager.GenerateCharacter(cardData, spawnPosition);
        if (ok)
        {
            Debug.Log($"[PlayerCardManager] Carta spawn creada (figurative): {cardData.cardName} en {spawnPosition}");

            // Guardar el slot (parent) de la carta antes de destruirla
            Transform slotOfCard = null;
            if (cardUI != null)
            {
                slotOfCard = cardUI.transform.parent;
            }

            // quitar la UI de la mano
            if (cardUI != null && spawnedCards.Contains(cardUI))
            {
                spawnedCards.Remove(cardUI);
                Destroy(cardUI);
            }

            // quitar la carta lógica del mazo/hand
            if (playerCards.Contains(cardData)) playerCards.Remove(cardData);

            // Crear inmediatamente la siguiente carta en el mismo slot si sabemos cuál era
            if (slotOfCard != null)
            {
                // pedir 1 carta nueva y crearla en el slotOfCard
                List<CardManager.Card> newCards = GetRandomCards(1);
                if (newCards != null && newCards.Count > 0)
                {
                    CreateCard(newCards[0], slotOfCard);
                }
                else
                {
                    // fallback: usar AddNextCard normal
                    AddNextCard();
                }
            }
            else
            {
                // fallback cuando no teníamos referencia al slot
                AddNextCard();
            }

            return true;
        }
        return false;
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
