using UnityEngine;

public class AccionAtacarConSuma : AIAction
{
    private IntelectManager intelectManager;
    private CardManager cardManager;
    private AICardHand aiHand;
    private AIThreatDetector threatDetector;
    private Transform spawnPoint;
    private float agresividad;

    public AccionAtacarConSuma(
        IntelectManager intelecto,
        CardManager cards,
        AICardHand hand,
        AIThreatDetector detector,
        Transform spawn,
        float aggressiveness = 0.5f
    ) : base("Atacar con Suma")
    {
        intelectManager = intelecto;
        cardManager = cards;
        aiHand = hand;
        threatDetector = detector;
        spawnPoint = spawn;
        agresividad = Mathf.Clamp01(aggressiveness);
    }

    public override float CalcularScore()
    {
        AICardHand.ComboAtaque mejorCombo = aiHand.EncontrarMejorComboSuma();

        if (mejorCombo == null)
        {
            scoreFinal = 0f;
            return 0f;
        }

        int resultado = mejorCombo.resultado;

        if (resultado > 5)
        {
            Debug.LogWarning($"[AccionAtacarSuma] Combo inválido: {mejorCombo} (>5)");
            scoreFinal = 0f;
            return 0f;
        }

        int costeIntelecto = resultado;

        if (intelectManager.currentIntelect < costeIntelecto)
        {
            Debug.Log($"[AccionAtacarSuma] No hay intelecto ({intelectManager.currentIntelect}/{costeIntelecto})");
            scoreFinal = 0f;
            return 0f;
        }

        float scorePotencia = Normalizar(resultado, 0, 5);
        scorePotencia = CurvaCuadratica(scorePotencia);

        float ratioIntelecto = (float)intelectManager.currentIntelect / intelectManager.maxIntelect;
        float scoreIntelecto = Normalizar(ratioIntelecto, 0.3f, 1f);

        GameObject[] defendersEnemigos = GameObject.FindGameObjectsWithTag("PlayerTeam");
        int cantidadDefenders = 0;

        foreach (GameObject obj in defendersEnemigos)
        {
            Character defender = obj.GetComponent<Character>();
            if (defender != null) cantidadDefenders++;
        }

        float scoreCaminoLibre = 1f - Normalizar(cantidadDefenders, 0, 5);

        int intelectoDespues = intelectManager.currentIntelect - costeIntelecto;
        float scoreEconomia = Normalizar(intelectoDespues, 0, intelectManager.maxIntelect);

        float scoreEficiencia = (float)resultado / (mejorCombo.cartaA.cardValue + mejorCombo.cartaB.cardValue);
        scoreEficiencia = Mathf.Clamp01(scoreEficiencia);

        int amenazasActivas = threatDetector.ContarAmenazas();
        float scorePresionDefensiva = 1f - Normalizar(amenazasActivas, 0, 3);

        scoreFinal =
            (scorePotencia * 0.30f) +
            (scoreIntelecto * 0.15f) +
            (scoreCaminoLibre * 0.25f) +
            (scoreEconomia * 0.10f) +
            (scoreEficiencia * 0.10f) +
            (scorePresionDefensiva * 0.10f);

        // === MODIFICADOR POR AGRESIVIDAD (DIFICULTAD) ===
        // Fácil (0.0-0.4): ataca poco (0.3x - 0.5x)
        // Media (0.4-0.7): equilibrado (0.7x - 0.9x)
        // Difícil (0.7-1.0): ataca mucho (1.0x - 1.5x)

        if (agresividad <= 0.4f) // FÁCIL
        {
            scoreFinal *= Mathf.Lerp(0.3f, 0.5f, agresividad / 0.4f);
            Debug.Log($"[AccionAtacarSuma] Dificultad FÁCIL - Score reducido");
        }
        else if (agresividad <= 0.7f) // MEDIA
        {
            scoreFinal *= Mathf.Lerp(0.7f, 0.9f, (agresividad - 0.4f) / 0.3f);
        }
        else // DIFÍCIL
        {
            scoreFinal *= Mathf.Lerp(1.0f, 1.5f, (agresividad - 0.7f) / 0.3f);

            // Bonus adicional para ataques fuertes en difícil
            if (resultado >= 4)
            {
                scoreFinal *= 1.2f;
                Debug.Log($"[AccionAtacarSuma] Dificultad DIFÍCIL + Ataque fuerte - Score boosted");
            }
        }

        // Penalización por amenaza crítica
        if (threatDetector.HayAmenazaCritica())
        {
            scoreFinal *= 0.7f;
            Debug.Log($"[AccionAtacarSuma] Amenaza crítica detectada, penalizando ataque");
        }

        Debug.Log($"[AccionAtacarSuma] Score: {scoreFinal:F2} para combo {mejorCombo} (agresividad: {agresividad:F2})");

        return scoreFinal;
    }

    public override void Ejecutar()
    {
        AICardHand.ComboAtaque combo = aiHand.EncontrarMejorComboSuma();

        if (combo == null)
        {
            Debug.LogWarning("[AccionAtacarSuma] Ejecutar llamado pero no hay combo válido");
            return;
        }

        // === SPAWN ALEATORIO EN EL ÁREA ===
        Vector3 posicionSpawn = CalcularPosicionAtaqueAleatoria();

        bool exito = cardManager.GenerateCombinedCharacter(
            combo.cartaA,
            combo.cartaB,
            posicionSpawn,
            combo.resultado,
            '+',
            "AITeam",
            intelectManager
        );

        if (exito)
        {
            Debug.Log($"[AccionAtacarSuma] ✅ Ataqué con {combo} en posición {posicionSpawn}");
            aiHand.RemoverCarta(combo.cartaA);
            aiHand.RemoverCarta(combo.cartaB);
            aiHand.RobarCarta();
            aiHand.RobarCarta();
        }
        else
        {
            Debug.LogError($"[AccionAtacarSuma] ❌ Falló al generar ataque {combo}");
        }
    }

    /// <summary>
    /// Calcula una posición aleatoria en el área de spawn (como Clash Royale)
    /// </summary>
    private Vector3 CalcularPosicionAtaqueAleatoria()
    {
        Vector3 posicionBase = spawnPoint.position;

        // Intentar obtener bounds del área de spawn
        BoxCollider spawnArea = spawnPoint.GetComponent<BoxCollider>();

        if (spawnArea != null)
        {
            // Usar el área completa del BoxCollider
            Vector3 halfSize = spawnArea.size * 0.5f;

            float randomX = Random.Range(-halfSize.x, halfSize.x);
            float randomZ = Random.Range(-halfSize.z, halfSize.z);

            posicionBase = spawnPoint.position + spawnPoint.TransformDirection(new Vector3(randomX, 0, randomZ));
        }
        else
        {
            // Fallback: usar un área de 5x5 metros
            float randomX = Random.Range(-2.5f, 2.5f);
            float randomZ = Random.Range(-2.5f, 2.5f);

            posicionBase += new Vector3(randomX, 0, randomZ);
        }

        // Mantener la altura del spawn original
        posicionBase.y = spawnPoint.position.y;

        return posicionBase;
    }
}