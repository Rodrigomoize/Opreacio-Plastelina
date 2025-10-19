using UnityEngine;

public class AccionDefender : AIAction
{
    private IntelectManager intelectManager;
    private CardManager cardManager;
    private AICardHand aiHand;
    private AIThreatDetector threatDetector;
    private Transform spawnPoint;
    private Transform miTorre;

    public AccionDefender(
        IntelectManager intelecto,
        CardManager cards,
        AICardHand hand,
        AIThreatDetector detector,
        Transform spawn,
        Transform torre
    ) : base("Defender Ataque")
    {
        intelectManager = intelecto;
        cardManager = cards;
        aiHand = hand;
        threatDetector = detector;
        spawnPoint = spawn;
        miTorre = torre;
    }

    public override float CalcularScore()
    {
        // ===== PASO 1: DETECTAR AMENAZA MÁS PELIGROSA =====
        AIThreatDetector.Amenaza amenaza = threatDetector.ObtenerAmenazaMasPeligrosa();

        // Si no hay amenazas, esta acción no tiene sentido
        if (amenaza == null)
        {
            scoreFinal = 0f;
            return 0f;
        }

        // ===== PASO 2: ¿TENGO LA CARTA NECESARIA? =====
        // CRÍTICO: Para defender un ataque de valor 3, necesito carta de valor 3 EXACTO
        int valorNecesario = amenaza.valor;
        CardManager.Card cartaDefensora = aiHand.ObtenerCartaPorValor(valorNecesario);

        // Si no tengo la carta exacta, NO PUEDO defender
        if (cartaDefensora == null)
        {
            Debug.Log($"[AccionDefender] No tengo carta de valor {valorNecesario} para defender");
            scoreFinal = 0f;
            return 0f;
        }

        // ===== PASO 3: ¿TENGO INTELECTO SUFICIENTE? =====
        // Coste = valor de la carta (carta 3 = 3 intelecto)
        int costeIntelecto = valorNecesario;

        if (intelectManager.currentIntelect < costeIntelecto)
        {
            Debug.Log($"[AccionDefender] No tengo intelecto suficiente ({intelectManager.currentIntelect}/{costeIntelecto})");
            scoreFinal = 0f;
            return 0f;
        }

        // Si llegamos aquí, SÍ PODEMOS defender


        // ===== PASO 4: CALCULAR CONSIDERACIONES =====

        // CONSIDERACIÓN 1: Peligrosidad de la amenaza (0-1)
        // Viene del detector (ya incluye valor + distancia)
        float scorePeligrosidad = amenaza.peligrosidad;

        // CONSIDERACIÓN 2: Urgencia por distancia (0-1)
        // Más cerca = más urgente
        float scoreUrgencia = Normalizar(amenaza.distancia, 0f, 20f);
        scoreUrgencia = CurvaInversa(scoreUrgencia); // Invertir: cerca=1, lejos=0
        scoreUrgencia = CurvaCuadratica(scoreUrgencia); // Hacer más dramático

        // CONSIDERACIÓN 3: Valor del ataque (0-1)
        // Un ataque de valor 5 es peor que uno de valor 1
        float scoreValorAmenaza = Normalizar(valorNecesario, 1, 5);

        // CONSIDERACIÓN 4: Economía de intelecto (0-1)
        // ¿Me quedará intelecto después de defender?
        int intelectoDespues = intelectManager.currentIntelect - costeIntelecto;
        float scoreEconomia = Normalizar(intelectoDespues, 0, intelectManager.maxIntelect);

        // CONSIDERACIÓN 5: Desesperación (placeholder para vida de torre)
        // TODO: Cuando implementes vida de torre, usa: vidaTorre / vidaMaxTorre
        float scoreDesesperacion = 0.5f; // Neutral por ahora


        // ===== PASO 5: COMBINAR CON PESOS =====
        // Defender es REACTIVO y PRIORITARIO cuando hay amenazas
        scoreFinal =
            (scorePeligrosidad * 0.35f) +    // 35% - Muy importante
            (scoreUrgencia * 0.30f) +        // 30% - Muy importante
            (scoreValorAmenaza * 0.20f) +    // 20% - Importante
            (scoreEconomia * 0.05f) +        // 5% - Poco importante (defender es prioritario)
            (scoreDesesperacion * 0.10f);    // 10% - Moderado

        // BOOST: Si hay amenaza crítica (muy cerca), aumentar score dramáticamente
        if (amenaza.distancia < 5f)
        {
            scoreFinal = Mathf.Min(1f, scoreFinal * 1.3f);
            Debug.Log($"[AccionDefender] ¡AMENAZA CRÍTICA! Boosting score a {scoreFinal:F2}");
        }

        Debug.Log($"[AccionDefender] Score calculado: {scoreFinal:F2} para amenaza valor {valorNecesario}");

        return scoreFinal;
    }

    public override void Ejecutar()
    {
        AIThreatDetector.Amenaza amenaza = threatDetector.ObtenerAmenazaMasPeligrosa();

        if (amenaza == null)
        {
            Debug.LogWarning("[AccionDefender] Ejecutar llamado pero no hay amenaza");
            return;
        }

        int valorNecesario = amenaza.valor;
        CardManager.Card carta = aiHand.ObtenerCartaPorValor(valorNecesario);

        if (carta == null)
        {
            Debug.LogWarning($"[AccionDefender] No tengo carta {valorNecesario} para defender");
            return;
        }

        Vector3 posicionAmenaza = amenaza.objeto.transform.position;
        Vector3 posicionTorre = miTorre.position;
        Vector3 posicionIntermedia = (posicionAmenaza + posicionTorre) / 2f;
        Vector3 posicionSpawn = new Vector3(
            posicionIntermedia.x,
            spawnPoint.position.y,
            posicionIntermedia.z
        );

        bool exito = cardManager.GenerateCharacter(carta, posicionSpawn, "AITeam", intelectManager);

        if (exito)
        {
            Debug.Log($"[AccionDefender] ✅ Defendí ataque de valor {valorNecesario} con carta {carta.cardName}");
            aiHand.RemoverCarta(carta);
            aiHand.RobarCarta();
        }
        else
        {
            Debug.LogError($"[AccionDefender] ❌ Falló al generar defender {carta.cardName}");
        }
    }
}