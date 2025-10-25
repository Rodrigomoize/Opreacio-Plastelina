using System.Collections.Generic;
using UnityEngine;

public class AICardHand
{
    private List<CardManager.Card> mano;
    private CardManager cardManager;
    public int tamaño = 5;

    public AICardHand(CardManager cm)
    {
        cardManager = cm;
        mano = new List<CardManager.Card>();
        InicializarMano();
    }

    private void InicializarMano()
    {
        // Inicializar con cartas ordenadas del 1-5 (CLONES, no las originales)
        for (int i = 0; i < 5; i++)
        {
            CardManager.Card originalCard = cardManager.GetCardByIndex(i);
            if (originalCard != null)
            {
                CardManager.Card clonedCard = cardManager.CloneCard(originalCard);
                mano.Add(clonedCard);
            }
        }

        Debug.Log($"[AICardHand] Mano inicial ordenada (5 cartas): {ObtenerDescripcionMano()}");
    }

    // Robar carta específica por su valor (índice en availableCards)
    public void RobarCartaPorValor(int cardValue)
    {
        int index = cardValue - 1; // Carta 1 está en índice 0, etc.
        CardManager.Card nueva = cardManager.GetCardByIndex(index);

        if (nueva != null)
        {
            mano.Add(cardManager.CloneCard(nueva));
            Debug.Log($"[AICardHand] Robé carta específica: {nueva.cardName} (valor {nueva.cardValue})");
        }
        else
        {
            Debug.LogWarning($"[AICardHand] No se pudo robar carta con valor {cardValue}");
        }
    }

    // Método legacy por si se necesita
    public void RobarCarta()
    {
        CardManager.Card nueva = cardManager.GetRandomCloneFromAvailable();

        if (nueva != null)
        {
            mano.Add(nueva);
            Debug.Log($"[AICardHand] Robé carta aleatoria: {nueva.cardName} (valor {nueva.cardValue})");
        }
        else
        {
            Debug.LogWarning("[AICardHand] No se pudo robar carta (availableCards vacío?)");
        }
    }

    public CardManager.Card ObtenerCartaPorValor(int valor)
    {
        foreach (CardManager.Card carta in mano)
        {
            if (carta.cardValue == valor)
            {
                return carta;
            }
        }

        return null;
    }

    public void RemoverCarta(CardManager.Card carta)
    {
        if (mano.Contains(carta))
        {
            mano.Remove(carta);
            Debug.Log($"[AICardHand] Removí carta: {carta.cardName}");
        }
    }

    // Remover y robar la misma carta
    public void RemoverYRobarMisma(CardManager.Card carta)
    {
        if (mano.Contains(carta))
        {
            int valorJugado = carta.cardValue;
            mano.Remove(carta);
            Debug.Log($"[AICardHand] Removí carta: {carta.cardName}");

            // Robar la misma carta
            RobarCartaPorValor(valorJugado);
        }
    }

    // Remover dos cartas y robar las mismas
    public void RemoverYRobarDos(CardManager.Card cartaA, CardManager.Card cartaB)
    {
        int valorA = cartaA.cardValue;
        int valorB = cartaB.cardValue;

        RemoverCarta(cartaA);
        RemoverCarta(cartaB);

        RobarCartaPorValor(valorA);
        RobarCartaPorValor(valorB);

        Debug.Log($"[AICardHand] Reemplacé dos cartas. Nueva mano: {ObtenerDescripcionMano()}");
    }

    public int CantidadCartas()
    {
        return mano.Count;
    }

    public List<CardManager.Card> ObtenerTodasLasCartas()
    {
        return new List<CardManager.Card>(mano);
    }

    public class ComboAtaque
    {
        public CardManager.Card cartaA;
        public CardManager.Card cartaB;
        public int resultado;
        public char operador; // '+' o '-'

        public override string ToString()
        {
            return $"{cartaA.cardValue} {operador} {cartaB.cardValue} = {resultado}";
        }
    }

    public ComboAtaque EncontrarMejorComboSuma()
    {
        ComboAtaque mejorCombo = null;
        int mejorResultado = -1;

        // Probar todas las combinaciones de 2 cartas diferentes
        for (int i = 0; i < mano.Count; i++)
        {
            for (int j = i + 1; j < mano.Count; j++) // j empieza en i+1 para evitar repeticiones
            {
                int suma = mano[i].cardValue + mano[j].cardValue;

                // Validar regla del juego: suma <= 5
                if (suma <= 5 && suma > mejorResultado)
                {
                    mejorResultado = suma;
                    mejorCombo = new ComboAtaque
                    {
                        cartaA = mano[i],
                        cartaB = mano[j],
                        resultado = suma,
                        operador = '+'
                    };
                }
            }
        }

        if (mejorCombo != null)
        {
            Debug.Log($"[AICardHand] Mejor combo suma: {mejorCombo}");
        }

        return mejorCombo;
    }

    /// <summary>
    /// Devuelve TODOS los combos de suma posibles (para evaluación de scoring)
    /// </summary>
    public List<ComboAtaque> EncontrarTodosCombosSuma()
    {
        List<ComboAtaque> combos = new List<ComboAtaque>();

        for (int i = 0; i < mano.Count; i++)
        {
            for (int j = i + 1; j < mano.Count; j++)
            {
                int suma = mano[i].cardValue + mano[j].cardValue;

                if (suma >= 1 && suma <= 5) // Válido
                {
                    combos.Add(new ComboAtaque
                    {
                        cartaA = mano[i],
                        cartaB = mano[j],
                        resultado = suma,
                        operador = '+'
                    });
                }
            }
        }

        return combos;
    }
    public CardManager.Card ObtenerCartaDiferenteDe(int valorExcluido)
    {
        if (mano.Count == 0) return null;

        // Buscar cartas que NO sean del valor excluido
        List<CardManager.Card> cartasValidas = new List<CardManager.Card>();

        foreach (var carta in mano)
        {
            if (carta.cardValue != valorExcluido)
            {
                cartasValidas.Add(carta);
            }
        }

        if (cartasValidas.Count == 0) return null;

        // Devolver una al azar
        return cartasValidas[Random.Range(0, cartasValidas.Count)];
    }

    public ComboAtaque EncontrarMejorComboResta()
    {
        ComboAtaque mejorCombo = null;
        int mejorResultado = -1;

        // Probar todas las combinaciones (orden importa en resta)
        for (int i = 0; i < mano.Count; i++)
        {
            for (int j = 0; j < mano.Count; j++)
            {
                // No restar carta consigo misma
                if (i == j) continue;

                int resta = mano[i].cardValue - mano[j].cardValue;

                // Validar reglas: resta >= 1 (NO 0) y <= 5
                // IMPORTANTE: Evitar operaciones con resultado 0 (ej: 4-4, 3-3)
                if (resta >= 1 && resta <= 5 && resta > mejorResultado)
                {
                    mejorResultado = resta;
                    mejorCombo = new ComboAtaque
                    {
                        cartaA = mano[i],
                        cartaB = mano[j],
                        resultado = resta,
                        operador = '-'
                    };
                }
            }
        }

        if (mejorCombo != null)
        {
            Debug.Log($"[AICardHand] Mejor combo resta: {mejorCombo}");
        }

        return mejorCombo;
    }

    /// <summary>
    /// Devuelve TODOS los combos de resta posibles (para evaluación de scoring)
    /// </summary>
    public List<ComboAtaque> EncontrarTodosCombosResta()
    {
        List<ComboAtaque> combos = new List<ComboAtaque>();

        for (int i = 0; i < mano.Count; i++)
        {
            for (int j = 0; j < mano.Count; j++)
            {
                if (i == j) continue;

                int resta = mano[i].cardValue - mano[j].cardValue;

                if (resta >= 1 && resta <= 5) // Válido
                {
                    combos.Add(new ComboAtaque
                    {
                        cartaA = mano[i],
                        cartaB = mano[j],
                        resultado = resta,
                        operador = '-'
                    });
                }
            }
        }

        return combos;
    }

    // Método de debug para ver qué cartas tengo
    private string ObtenerDescripcionMano()
    {
        string desc = "[";
        for (int i = 0; i < mano.Count; i++)
        {
            desc += mano[i].cardValue;
            if (i < mano.Count - 1) desc += ", ";
        }
        desc += "]";
        return desc;
    }
}