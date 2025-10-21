using UnityEngine;

public class AccionDefender : AIAction
{
    private IntelectManager intelectManager;
    private CardManager cardManager;
    private AICardHand aiHand;
    private AIThreatDetector threatDetector;
    private Transform spawnPoint;
    private Transform miTorre;
    private float dificultad; // 0=fácil, 0.5=media, 1=difícil

    public AccionDefender(
        IntelectManager intelecto,
        CardManager cards,
        AICardHand hand,
        AIThreatDetector detector,
        Transform spawn,
        Transform torre,
        float difficulty = 0.5f
    ) : base("Defender Ataque")
    {
        intelectManager = intelecto;
        cardManager = cards;
        aiHand = hand;
        threatDetector = detector;
        spawnPoint = spawn;
        miTorre = torre;
        dificultad = Mathf.Clamp01(difficulty);
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

        // === SISTEMA DE ERRORES SEGÚN DIFICULTAD ===
        // Fácil (0-0.3): 40% probabilidad de error en la defensa
        // Media (0.3-0.7): 15% probabilidad de error
        // Difícil (0.7-1.0): 5% probabilidad de error

        float probabilidadError = 0f;
        if (dificultad < 0.3f) // Fácil
        {
            probabilidadError = 0.4f;
        }
        else if (dificultad < 0.7f) // Media
        {
            probabilidadError = 0.15f;
        }
        else // Difícil
        {
            probabilidadError = 0.05f;
        }

        // Simular error: buscar carta incorrecta a propósito
        if (Random.value < probabilidadError)
        {
            Debug.Log($"[AccionDefender] ❌ ERROR DE IA - Intentando defender {valorNecesario} con carta incorrecta");

            // Buscar una carta diferente al valor necesario
            CardManager.Card cartaIncorrecta = aiHand.ObtenerCartaDiferenteDe(valorNecesario);

            if (cartaIncorrecta != null)
            {
                // Penalizar pero no hacer imposible (la IA "cree" que es buena idea)
                scoreFinal = 0.3f;
                return scoreFinal;
            }
        }

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

        // === MODIFICADOR POR DIFICULTAD ===
        // Fácil: defiende menos (0.6x)
        // Media: normal (1.0x)
        // Difícil: defiende más (1.2x)
        float modificadorDificultad = Mathf.Lerp(0.6f, 1.2f, dificultad);
        scoreFinal *= modificadorDificultad;

        Debug.Log($"[AccionDefender] Score calculado: {scoreFinal:F2} para amenaza valor {valorNecesario} (dificultad: {dificultad:F2})");

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

        // Verificar si hay error intencional
        float probabilidadError = dificultad < 0.3f ? 0.4f : (dificultad < 0.7f ? 0.15f : 0.05f);

        CardManager.Card carta = null;

        if (Random.value < probabilidadError)
        {
            // Buscar carta incorrecta
            carta = aiHand.ObtenerCartaDiferenteDe(valorNecesario);
            if (carta != null)
            {
                Debug.Log($"[AccionDefender] ⚠️ DEFENDIENDO MAL A PROPÓSITO: usando {carta.cardValue} contra ataque {valorNecesario}");
            }
        }

        // Si no hay error o no se encontró carta incorrecta, usar la correcta
        if (carta == null)
        {
            carta = aiHand.ObtenerCartaPorValor(valorNecesario);
        }

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

    /// <summary>
    /// Calcula una posición aleatoria en el área de spawn, más cerca de la torre
    /// </summary>
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