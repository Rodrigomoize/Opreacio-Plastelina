using System.Collections.Generic;
using UnityEngine;

public class AICardHand
{
    private List<CardManager.Card> mano;
    private CardManager cardManager;
    public int tamaño = 4;


    public AICardHand(CardManager cm)
    {
        cardManager = cm;
        mano = new List<CardManager.Card>();
        InicializarMano();
    }

    private void InicializarMano()
    {
        for (int i = 0; i < tamaño; i++)
        {
            RobarCarta();
        }

        Debug.Log($"[AICardHand] Mano inicial: {ObtenerDescripcionMano()}");
    }

    public void RobarCarta()
    {
        // Usa el método que ya tienes en CardManager
        CardManager.Card nueva = cardManager.GetRandomCloneFromAvailable();

        if (nueva != null)
        {
            mano.Add(nueva);
            Debug.Log($"[AICardHand] Robé carta: {nueva.cardName} (valor {nueva.cardValue})");
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

                // Validar reglas: resta >= 0 y <= 5
                if (resta >= 0 && resta <= 5 && resta > mejorResultado)
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