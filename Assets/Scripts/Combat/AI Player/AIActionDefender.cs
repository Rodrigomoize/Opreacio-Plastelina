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
        AIThreatDetector.Amenaza amenaza = threatDetector.ObtenerAmenazaMasPeligrosa();

        if (amenaza == null)
        {
            scoreFinal = 0f;
            return 0f;
        }

        int valorNecesario = amenaza.valor;

        // Buscar la carta correcta para defender (SIN errores intencionales)
        CardManager.Card cartaDefensora = aiHand.ObtenerCartaPorValor(valorNecesario);

        if (cartaDefensora == null)
        {
            Debug.Log($"[AccionDefender] No tengo carta de valor {valorNecesario} para defender");
            scoreFinal = 0f;
            return 0f;
        }

        int costeIntelecto = valorNecesario;

        if (intelectManager.currentIntelect < costeIntelecto)
        {
            Debug.Log($"[AccionDefender] No tengo intelecto suficiente ({intelectManager.currentIntelect}/{costeIntelecto})");
            scoreFinal = 0f;
            return 0f;
        }

        // === CÁLCULO DE SCORE ===
        float scorePeligrosidad = amenaza.peligrosidad;
        float scoreUrgencia = Normalizar(amenaza.distancia, 0f, 20f);
        scoreUrgencia = CurvaInversa(scoreUrgencia);
        scoreUrgencia = CurvaCuadratica(scoreUrgencia);

        float scoreValorAmenaza = Normalizar(valorNecesario, 1, 5);

        int intelectoDespues = intelectManager.currentIntelect - costeIntelecto;
        float scoreEconomia = Normalizar(intelectoDespues, 0, intelectManager.maxIntelect);

        float scoreDesesperacion = 0.5f;

        scoreFinal =
            (scorePeligrosidad * 0.35f) +
            (scoreUrgencia * 0.30f) +
            (scoreValorAmenaza * 0.20f) +
            (scoreEconomia * 0.05f) +
            (scoreDesesperacion * 0.10f);

        // BOOST amenaza crítica
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

        // Buscar la carta correcta (SIN errores intencionales)
        CardManager.Card carta = aiHand.ObtenerCartaPorValor(valorNecesario);

        if (carta == null)
        {
            Debug.LogWarning($"[AccionDefender] No tengo carta para defender");
            return;
        }

        // === CALCULAR POSICIÓN DE SPAWN MEJORADA ===
        // Usar el área completa del spawnPoint (puede tener un BoxCollider para definir bounds)
        Vector3 posicionSpawn = CalcularPosicionDefensaAleatoria(amenaza.objeto.transform.position);

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

    /// Calcula una posición aleatoria en el área de spawn, más cerca de la torre
    private Vector3 CalcularPosicionDefensaAleatoria(Vector3 posicionAmenaza)
    {
        Vector3 posicionBase = spawnPoint.position;

        // Intentar obtener bounds del área de spawn
        BoxCollider spawnArea = spawnPoint.GetComponent<BoxCollider>();

        if (spawnArea != null)
        {
            // Usar el área completa del BoxCollider
            Vector3 halfSize = spawnArea.size * 0.5f;

            float randomX = Random.Range(-halfSize.x, halfSize.x);
            float randomZ = Random.Range(-halfSize.z * 0.3f, halfSize.z * 0.7f); // Más cerca de la torre (atrás)

            posicionBase = spawnPoint.position + spawnPoint.TransformDirection(new Vector3(randomX, 0, randomZ));
        }
        else
        {
            // Fallback: usar un área de 5x5 metros, posicionando más atrás
            float randomX = Random.Range(-2.5f, 2.5f);
            float randomZ = Random.Range(-1f, 1.5f); // Más cerca de la torre

            posicionBase += new Vector3(randomX, 0, randomZ);
        }

        // Mantener la altura del spawn original
        posicionBase.y = spawnPoint.position.y;

        Debug.Log($"[AccionDefender] Spawneando defensa en posición aleatoria: {posicionBase}");

        return posicionBase;
    }
}