using UnityEngine;

public class AccionAtacarConResta : AIAction
{
    private IntelectManager intelectManager;
    private CardManager cardManager;
    private AICardHand aiHand;
    private AIThreatDetector threatDetector;
    private Transform spawnPoint;
    private float agresividad;

    public AccionAtacarConResta(
        IntelectManager intelecto,
        CardManager cards,
        AICardHand hand,
        AIThreatDetector detector,
        Transform spawn,
        float aggressiveness = 0.5f
    ) : base("Atacar con Resta")
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
        AICardHand.ComboAtaque mejorCombo = aiHand.EncontrarMejorComboResta();

        if (mejorCombo == null)
        {
            scoreFinal = 0f;
            return 0f;
        }

        int resultado = mejorCombo.resultado;

        if (resultado < 0 || resultado > 5)
        {
            Debug.LogWarning($"[AccionAtacarResta] Combo inválido: {mejorCombo}");
            scoreFinal = 0f;
            return 0f;
        }

        int costeIntelecto = resultado;

        if (intelectManager.currentIntelect < costeIntelecto)
        {
            Debug.Log($"[AccionAtacarResta] No hay intelecto ({intelectManager.currentIntelect}/{costeIntelecto})");
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

        int valorPerdido = mejorCombo.cartaB.cardValue;
        float scoreEficiencia = 1f - Normalizar(valorPerdido, 0, 5);

        int amenazasActivas = threatDetector.ContarAmenazas();
        float scorePresionDefensiva = 1f - Normalizar(amenazasActivas, 0, 3);

        scoreFinal =
            (scorePotencia * 0.30f) +
            (scoreIntelecto * 0.15f) +
            (scoreCaminoLibre * 0.25f) +
            (scoreEconomia * 0.10f) +
            (scoreEficiencia * 0.15f) +
            (scorePresionDefensiva * 0.05f);

        // === MODIFICADOR POR AGRESIVIDAD (DIFICULTAD) ===
        if (agresividad <= 0.4f) // FÁCIL
        {
            scoreFinal *= Mathf.Lerp(0.3f, 0.5f, agresividad / 0.4f);
        }
        else if (agresividad <= 0.7f) // MEDIA
        {
            scoreFinal *= Mathf.Lerp(0.7f, 0.9f, (agresividad - 0.4f) / 0.3f);
        }
        else // DIFÍCIL
        {
            scoreFinal *= Mathf.Lerp(1.0f, 1.5f, (agresividad - 0.7f) / 0.3f);

            if (resultado >= 4)
            {
                scoreFinal *= 1.2f;
            }
        }

        // Penalización por amenaza crítica
        if (threatDetector.HayAmenazaCritica())
        {
            scoreFinal *= 0.5f;
        }

        // Penalización fuerte si resultado es 0
        if (resultado == 0)
        {
            scoreFinal *= 0.3f;
            Debug.Log($"[AccionAtacarResta] Resultado es 0, penalizando fuertemente");
        }

        Debug.Log($"[AccionAtacarResta] Score: {scoreFinal:F2} para combo {mejorCombo}");

        return scoreFinal;
    }

    public override void Ejecutar()
    {
        AICardHand.ComboAtaque combo = aiHand.EncontrarMejorComboResta();

        if (combo == null)
        {
            Debug.LogWarning("[AccionAtacarResta] Ejecutar llamado pero no hay combo válido");
            return;
        }

        // === SPAWN ALEATORIO EN EL ÁREA ===
        Vector3 posicionSpawn = CalcularPosicionAtaqueAleatoria();

        bool exito = cardManager.GenerateCombinedCharacter(
            combo.cartaA,
            combo.cartaB,
            posicionSpawn,
            combo.resultado,
            '-',
            "AITeam",
            intelectManager
        );

        if (exito)
        {
            Debug.Log($"[AccionAtacarResta] ✅ Ataqué con {combo} en posición {posicionSpawn}");
            aiHand.RemoverCarta(combo.cartaA);
            aiHand.RemoverCarta(combo.cartaB);
            aiHand.RobarCarta();
            aiHand.RobarCarta();
        }
        else
        {
            Debug.LogError($"[AccionAtacarResta] ❌ Falló al generar ataque {combo}");
        }
    }

    private Vector3 CalcularPosicionAtaqueAleatoria()
    {
        Vector3 posicionBase = spawnPoint.position;

        BoxCollider spawnArea = spawnPoint.GetComponent<BoxCollider>();

        if (spawnArea != null)
        {
            Vector3 halfSize = spawnArea.size * 0.5f;

            float randomX = Random.Range(-halfSize.x, halfSize.x);
            float randomZ = Random.Range(-halfSize.z, halfSize.z);

            posicionBase = spawnPoint.position + spawnPoint.TransformDirection(new Vector3(randomX, 0, randomZ));
        }
        else
        {
            float randomX = Random.Range(-2.5f, 2.5f);
            float randomZ = Random.Range(-2.5f, 2.5f);

            posicionBase += new Vector3(randomX, 0, randomZ);
        }

        posicionBase.y = spawnPoint.position.y;

        return posicionBase;
    }
}