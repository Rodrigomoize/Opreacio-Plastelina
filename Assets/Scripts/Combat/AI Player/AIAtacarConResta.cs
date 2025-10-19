using UnityEngine;

/// <summary>
/// Acción: Combinar 2 cartas con RESTA para ATACAR
/// Similar a suma pero con ligera penalización (la resta es menos intuitiva para niños)
/// Regla: resultado debe ser ≥ 0 y ≤ 5
/// </summary>
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
        // ===== PASO 1: ENCONTRAR MEJOR COMBO DE RESTA =====
        AICardHand.ComboAtaque mejorCombo = aiHand.EncontrarMejorComboResta();

        if (mejorCombo == null)
        {
            scoreFinal = 0f;
            return 0f;
        }

        int resultado = mejorCombo.resultado;

        // ===== PASO 2: VALIDACIONES =====

        // Validar reglas: resta debe ser ≥ 0 y ≤ 5
        if (resultado < 0 || resultado > 5)
        {
            Debug.LogWarning($"[AccionAtacarResta] Combo inválido: {mejorCombo}");
            scoreFinal = 0f;
            return 0f;
        }

        // Validar intelecto
        int costeIntelecto = resultado;

        if (intelectManager.currentIntelect < costeIntelecto)
        {
            Debug.Log($"[AccionAtacarResta] No hay intelecto ({intelectManager.currentIntelect}/{costeIntelecto})");
            scoreFinal = 0f;
            return 0f;
        }

        float scorePotencia = Normalizar(resultado, 0, 5);
        scorePotencia = CurvaCuadratica(scorePotencia);

        // CONSIDERACIÓN 2: Disponibilidad de intelecto
        float ratioIntelecto = (float)intelectManager.currentIntelect / intelectManager.maxIntelect;
        float scoreIntelecto = Normalizar(ratioIntelecto, 0.3f, 1f);

        // CONSIDERACIÓN 3: Camino libre
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
            (scorePotencia * 0.30f) +           // 30% - Daño del ataque
            (scoreIntelecto * 0.15f) +          // 15% - Tener intelecto
            (scoreCaminoLibre * 0.25f) +        // 25% - Camino libre
            (scoreEconomia * 0.10f) +           // 10% - Economía
            (scoreEficiencia * 0.15f) +         // 15% - Eficiencia (más importante en resta)
            (scorePresionDefensiva * 0.05f);    // 5% - Presión


        scoreFinal *= (0.7f + (agresividad * 0.6f));

        scoreFinal *= 0.9f;

        if (threatDetector.HayAmenazaCritica())
        {
            scoreFinal *= 0.5f;
        }

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

        Vector3 posicionSpawn = spawnPoint.position;

        // ⚡ CAMBIO: Pasar intelectManager como IntelectManager base
        bool exito = cardManager.GenerateCombinedCharacter(
            combo.cartaA,
            combo.cartaB,
            posicionSpawn,
            combo.resultado,
            '-',
            "AITeam",
            intelectManager  // ⚡ AÑADIDO
        );

        if (exito)
        {
            Debug.Log($"[AccionAtacarResta] ✅ Ataqué con {combo}");
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
}