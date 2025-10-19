using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Acción: NO HACER NADA (esperar y regenerar intelecto)
/// Es buena cuando: poco intelecto, sin amenazas, malas cartas
/// </summary>
public class AccionEsperar : AIAction
{
    private IntelectManager intelectManager;
    private AICardHand aiHand;
    private AIThreatDetector threatDetector;

    public AccionEsperar(
        IntelectManager intelecto,
        AICardHand hand,
        AIThreatDetector detector
    ) : base("Esperar")
    {
        intelectManager = intelecto;
        aiHand = hand;
        threatDetector = detector;
    }

    public override float CalcularScore()
    {
        // ===== CONSIDERACIONES PARA ESPERAR =====

        // CONSIDERACIÓN 1: Bajo intelecto (0-1)
        // Si tengo poco intelecto, mejor esperar a regenerar
        float ratioIntelecto = (float)intelectManager.currentIntelect / intelectManager.maxIntelect;
        float scoreBajoIntelecto = 1f - ratioIntelecto; // Invertido: menos intelecto = más score
        scoreBajoIntelecto = CurvaCuadratica(scoreBajoIntelecto); // Hacer más dramático

        // CONSIDERACIÓN 2: Sin amenazas (0-1)
        // Si no hay amenazas, es seguro esperar
        int amenazasActivas = threatDetector.ContarAmenazas();
        float scoreSinAmenazas = 1f - Normalizar(amenazasActivas, 0, 3);

        // CONSIDERACIÓN 3: Amenazas lejanas (0-1)
        // Si las amenazas están lejos, puedo esperar
        AIThreatDetector.Amenaza amenazaMasPeligrosa = threatDetector.ObtenerAmenazaMasPeligrosa();
        float scoreAmenazasLejanas = 0.5f; // Neutral por defecto

        if (amenazaMasPeligrosa != null)
        {
            // Normalizar distancia: cerca = 0, lejos = 1
            scoreAmenazasLejanas = Normalizar(amenazaMasPeligrosa.distancia, 0, 20);
        }
        else
        {
            // No hay amenazas = score máximo
            scoreAmenazasLejanas = 1f;
        }

        // CONSIDERACIÓN 4: Mano débil (0-1)
        // Si mis cartas actuales son malas, mejor esperar a robar mejores
        // (Esto es una simplificación; podrías analizar combos posibles)
        List<CardManager.Card> cartas = aiHand.ObtenerTodasLasCartas();
        float promedioValor = 0f;

        if (cartas.Count > 0)
        {
            foreach (CardManager.Card c in cartas)
            {
                promedioValor += c.cardValue;
            }
            promedioValor /= cartas.Count;
        }

        // Mano débil = promedio bajo
        float scoreManoDebil = 1f - Normalizar(promedioValor, 1, 5);

        // CONSIDERACIÓN 5: No puedo hacer combo válido
        // Si no tengo combos válidos, forzar esperar
        bool tiengoComboSuma = aiHand.EncontrarMejorComboSuma() != null;
        bool tiengoComboResta = aiHand.EncontrarMejorComboResta() != null;
        float scoreSinCombos = (tiengoComboSuma || tiengoComboResta) ? 0f : 1f;


        // ===== COMBINAR CON PESOS =====
        scoreFinal =
            (scoreBajoIntelecto * 0.35f) +       // 35% - Muy importante
            (scoreSinAmenazas * 0.25f) +         // 25% - Importante
            (scoreAmenazasLejanas * 0.15f) +     // 15% - Moderado
            (scoreManoDebil * 0.10f) +           // 10% - Poco importante
            (scoreSinCombos * 0.15f);            // 15% - Importante

        // PENALIZACIÓN BASE: Esperar es generalmente peor que actuar
        // Solo queremos esperar en situaciones específicas
        scoreFinal *= 0.6f; // Reducir a 60%

        // BOOST: Si realmente no puedo hacer nada (sin intelecto para nada)
        if (intelectManager.currentIntelect < 1)
        {
            scoreFinal = 0.8f; // Forzar esperar
            Debug.Log($"[AccionEsperar] Sin intelecto, forzando espera");
        }

        // PENALIZACIÓN CRÍTICA: Si hay amenaza MUY cerca, nunca esperar
        if (threatDetector.HayAmenazaCritica())
        {
            scoreFinal = 0f;
            Debug.Log($"[AccionEsperar] Amenaza crítica, cancelando espera");
        }

        Debug.Log($"[AccionEsperar] Score: {scoreFinal:F2}");

        return scoreFinal;
    }

    public override void Ejecutar()
    {
        // No hacer nada, solo esperar
        Debug.Log("[AccionEsperar] ⏸️ Esperando... regenerando intelecto");

        // Opcional: Podrías añadir lógica adicional aquí
        // Por ejemplo: descartar una carta mala y robar otra
    }
}