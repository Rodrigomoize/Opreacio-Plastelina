using UnityEngine;
using System.Collections.Generic;


public class AccionEsperar : AIAction
{
    private IntelectManager intelectManager;
    private AICardHand aiHand;
    private AIThreatDetector threatDetector;

    public AccionEsperar(
        IntelectManager intelecto,
        AICardHand hand,
        AIThreatDetector detector
    ) : base("Esperar", TipoAccion.Neutral)
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

        // CONSIDERACIÓN 5: No puedo hacer nada por falta de intelecto
        // 🔧 SIMPLIFICADO: La IA SIEMPRE tiene cartas 1-5, solo importa el intelecto
        bool tiengoDefensa = false; // Verificar si tengo intelecto para defender
        
        AIThreatDetector.Amenaza amenazaMasCercana = threatDetector.ObtenerAmenazaMasPeligrosa();
        if (amenazaMasCercana != null)
        {
            // La carta siempre existe (1-5), solo verificar intelecto
            tiengoDefensa = (intelectManager.currentIntelect >= amenazaMasCercana.valor);
        }
        
        // Combo mínimo requiere 2 de intelecto (1+1=2)
        bool tiengoIntelectoParaAtacar = intelectManager.currentIntelect >= 2;
        
        // No puedo hacer nada si no tengo intelecto para defender NI para atacar
        bool noPuedoHacerNada = !tiengoDefensa && !tiengoIntelectoParaAtacar;
        float scoreSinAcciones = noPuedoHacerNada ? 0.5f : 0f;


        // ===== COMBINAR CON PESOS =====
        scoreFinal =
            (scoreBajoIntelecto * 0.35f) +       // 35% - Muy importante
            (scoreSinAmenazas * 0.25f) +         // 25% - Importante
            (scoreAmenazasLejanas * 0.15f) +     // 15% - Moderado
            (scoreManoDebil * 0.10f) +           // 10% - Poco importante
            (scoreSinAcciones * 0.15f);          // 15% - Importante (renombrado de scoreSinCombos)

        // PENALIZACIÓN BASE: Esperar es generalmente peor que actuar
        // Solo queremos esperar en situaciones específicas
        scoreFinal *= 0.4f; // 🔧 Reducido de 0.6 a 0.4 para ser AÚN MÁS CONSERVADOR

        // BOOST: Si realmente no puedo hacer nada (sin intelecto para nada)
        if (intelectManager.currentIntelect < 1)
        {
            scoreFinal = 0.8f; // Forzar esperar
            Debug.Log($"[AccionEsperar] Sin intelecto, forzando espera (score: {scoreFinal:F3})");
        }
        // 🔧 FIX: Si NO puedo hacer nada por falta de intelecto, score moderado
        else if (noPuedoHacerNada && intelectManager.currentIntelect >= 1)
        {
            scoreFinal = 0.25f;
            Debug.Log($"[AccionEsperar] Sin intelecto suficiente para acciones, espera sugerida (score: {scoreFinal:F3})");
        }
        // 🔧 SIMPLIFICADO: Si tengo intelecto para atacar (≥2) pero bajo intelecto, penalizar espera
        else if (tiengoIntelectoParaAtacar && intelectManager.currentIntelect < 4)
        {
            scoreFinal *= 0.7f; // Penalizar esperar si tengo opciones
            Debug.Log($"[AccionEsperar] Tengo intelecto para atacar, penalizando espera (score: {scoreFinal:F3})");
        }

        // PENALIZACIÓN CRÍTICA: Si hay amenaza MUY cerca, normalmente no esperar
        // EXCEPTO si realmente no puedo hacer nada por falta de intelecto
        if (threatDetector.HayAmenazaCritica())
        {
            if (noPuedoHacerNada)
            {
                // 🔧 FIX CRÍTICO: Si hay amenaza crítica PERO no puedo defender (sin intelecto), 
                // DEBO esperar (es mi única opción)
                scoreFinal = 0.6f; // Score alto para forzar espera
                Debug.Log($"[AccionEsperar] Amenaza crítica PERO sin intelecto para defenderla → Forzando espera (score: {scoreFinal:F3})");
            }
            else
            {
                // Puedo defender, así que NO esperar
                scoreFinal = 0f;
                Debug.Log($"[AccionEsperar] Amenaza crítica y SÍ puedo defender → Cancelando espera");
            }
        }
        
        // 🔧 FIX ADICIONAL: Si no hay amenazas pero tengo recursos, no esperar indefinidamente
        if (amenazasActivas == 0 && intelectManager.currentIntelect >= 3)
        {
            scoreFinal *= 0.5f; // Reducir score cuando pueda atacar sin presión
            Debug.Log($"[AccionEsperar] Sin amenazas pero con recursos, penalizando espera (score: {scoreFinal:F3})");
        }

        Debug.Log($"[AccionEsperar] Score FINAL: {scoreFinal:F3}");

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