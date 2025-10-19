using UnityEngine;

/// <summary>
/// Acción: Combinar 2 cartas con SUMA para ATACAR
/// Solo las CombinedCards hacen daño a torres
/// Regla: resultado debe ser ≤ 5
/// </summary>
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
        // ===== PASO 1: ENCONTRAR MEJOR COMBO DE SUMA =====
        AICardHand.ComboAtaque mejorCombo = aiHand.EncontrarMejorComboSuma();

        // Si no hay combo válido, esta acción no tiene sentido
        if (mejorCombo == null)
        {
            scoreFinal = 0f;
            return 0f;
        }

        int resultado = mejorCombo.resultado;

        // ===== PASO 2: VALIDACIONES =====

        // Validar regla: suma debe ser ≤ 5
        if (resultado > 5)
        {
            Debug.LogWarning($"[AccionAtacarSuma] Combo inválido: {mejorCombo} (>5)");
            scoreFinal = 0f;
            return 0f;
        }

        // Validar intelecto: el coste es el RESULTADO de la operación
        // Ejemplo: 3+2=5 cuesta 5 de intelecto
        int costeIntelecto = resultado;

        if (intelectManager.currentIntelect < costeIntelecto)
        {
            Debug.Log($"[AccionAtacarSuma] No hay intelecto ({intelectManager.currentIntelect}/{costeIntelecto})");
            scoreFinal = 0f;
            return 0f;
        }


        // ===== PASO 3: CALCULAR CONSIDERACIONES =====

        // CONSIDERACIÓN 1: Potencia del ataque (0-1)
        // Un ataque de valor 5 es mejor que uno de valor 1
        float scorePotencia = Normalizar(resultado, 0, 5);
        // Aplicar curva cuadrática para favorecer ataques fuertes
        scorePotencia = CurvaCuadratica(scorePotencia);

        // CONSIDERACIÓN 2: Disponibilidad de intelecto (0-1)
        // ¿Tengo suficiente intelecto cómodamente?
        float ratioIntelecto = (float)intelectManager.currentIntelect / intelectManager.maxIntelect;
        float scoreIntelecto = Normalizar(ratioIntelecto, 0.3f, 1f); // Mínimo cómodo: 30%

        // CONSIDERACIÓN 3: Camino libre (0-1)
        // ¿Cuántos defenders enemigos hay que puedan bloquear mi ataque?
        GameObject[] defendersEnemigos = GameObject.FindGameObjectsWithTag("PlayerTeam");
        int cantidadDefenders = 0;

        foreach (GameObject obj in defendersEnemigos)
        {
            // Solo contar Character (defenders), no CombinedCards (otros ataques)
            Character defender = obj.GetComponent<Character>();
            if (defender != null)
            {
                cantidadDefenders++;
            }
        }

        // Normalizar: 0 defenders = score 1.0, 5+ defenders = score 0
        float scoreCaminoLibre = 1f - Normalizar(cantidadDefenders, 0, 5);

        // CONSIDERACIÓN 4: Economía post-ataque (0-1)
        // ¿Me quedará intelecto después del ataque?
        int intelectoDespues = intelectManager.currentIntelect - costeIntelecto;
        float scoreEconomia = Normalizar(intelectoDespues, 0, intelectManager.maxIntelect);

        // CONSIDERACIÓN 5: Eficiencia (0-1)
        // ¿Estoy usando bien mis cartas?
        // Ejemplo: 4+1=5 es más eficiente que 2+2=4 (mismo coste, más daño)
        float scoreEficiencia = (float)resultado / (mejorCombo.cartaA.cardValue + mejorCombo.cartaB.cardValue);
        scoreEficiencia = Mathf.Clamp01(scoreEficiencia);

        // CONSIDERACIÓN 6: Presión defensiva (0-1)
        // Si hay muchas amenazas del jugador, tal vez no es momento de atacar
        int amenazasActivas = threatDetector.ContarAmenazas();
        float scorePresionDefensiva = 1f - Normalizar(amenazasActivas, 0, 3);


        // ===== PASO 4: COMBINAR CON PESOS =====
        // Atacar es PROACTIVO, requiere buen momento
        scoreFinal =
            (scorePotencia * 0.30f) +           // 30% - Daño del ataque
            (scoreIntelecto * 0.15f) +          // 15% - Tener intelecto
            (scoreCaminoLibre * 0.25f) +        // 25% - Camino libre (MUY importante)
            (scoreEconomia * 0.10f) +           // 10% - Economía
            (scoreEficiencia * 0.10f) +         // 10% - Eficiencia
            (scorePresionDefensiva * 0.10f);    // 10% - No estar bajo presión

        // MODIFICADOR: Agresividad de la IA
        // Una IA agresiva favorece atacar más
        scoreFinal *= (0.7f + (agresividad * 0.6f)); // Rango: 0.7x - 1.3x

        // PENALIZACIÓN: Si hay amenaza crítica cerca, priorizar defender
        if (threatDetector.HayAmenazaCritica())
        {
            scoreFinal *= 0.5f; // Reducir a la mitad
            Debug.Log($"[AccionAtacarSuma] Amenaza crítica detectada, penalizando ataque");
        }

        Debug.Log($"[AccionAtacarSuma] Score: {scoreFinal:F2} para combo {mejorCombo}");

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

        Vector3 posicionSpawn = spawnPoint.position;

        // ⚡ CAMBIO: Pasar intelectManager como IntelectManager base
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
            Debug.Log($"[AccionAtacarSuma] ✅ Ataqué con {combo}");
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
}