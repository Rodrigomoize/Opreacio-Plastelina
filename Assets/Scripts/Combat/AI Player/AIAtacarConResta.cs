using UnityEngine;

public class AccionAtacarConResta : AIAction
{
    private IntelectManager intelectManager;
    private CardManager cardManager;
    private AICardHand aiHand;
    private AIThreatDetector threatDetector;
    private Transform spawnPoint;
    private float agresividad;
    private AISpawnPositionCalculator spawnCalculator;
    
    // 🔧 Campo para almacenar el mejor combo encontrado durante evaluación
    private AICardHand.ComboAtaque mejorComboEvaluado;

    public AccionAtacarConResta(
        IntelectManager intelecto,
        CardManager cards,
        AICardHand hand,
        AIThreatDetector detector,
        Transform spawn,
        float aggressiveness = 0.5f,
        AISpawnPositionCalculator calculator = null
    ) : base("Atacar con Resta", TipoAccion.Ataque)
    {
        intelectManager = intelecto;
        cardManager = cards;
        aiHand = hand;
        threatDetector = detector;
        spawnPoint = spawn;
        agresividad = Mathf.Clamp01(aggressiveness);
        spawnCalculator = calculator;
    }

    public override float CalcularScore()
    {
        // 🔧 NUEVO: Obtener TODOS los combos posibles
        var todosCombos = aiHand.EncontrarTodosCombosResta();

        if (todosCombos == null || todosCombos.Count == 0)
        {
            scoreFinal = 0f;
            mejorComboEvaluado = null;
            return 0f;
        }

        // Evaluar cada combo y encontrar el mejor según scoring
        float mejorScore = 0f;
        AICardHand.ComboAtaque mejorCombo = null;


        foreach (var combo in todosCombos)
        {
            int resultado = combo.resultado;
            int costeIntelecto = resultado;

            // Validar resultado
            if (resultado < 1 || resultado > 5)
            {
                continue;
            }

            // Verificar si tenemos intelecto suficiente
            if (intelectManager.currentIntelect < costeIntelecto)
            {
                continue;
            }

            // === CALCULAR SCORE PARA ESTE COMBO ===
            
            // 🔧 SCORE DE POTENCIA: Curva con VARIEDAD + randomización
            float scorePotencia = 0f;
            
            if (resultado == 1)
                scorePotencia = 0.40f; // Económico pero débil
            else if (resultado == 2)
                scorePotencia = 0.70f; // Decente
            else if (resultado == 3)
                scorePotencia = 0.85f; // Equilibrado
            else if (resultado == 4)
                scorePotencia = 0.90f; // Potente
            else if (resultado == 5)
                scorePotencia = 0.95f; // Máximo poder
            
            // 🎲 AÑADIR FACTOR ALEATORIO para variedad (±15%)
            float randomFactor = Random.Range(0.85f, 1.15f);
            scorePotencia *= randomFactor;

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

            int valorPerdido = combo.cartaB.cardValue;
            float scoreEficiencia = 1f - Normalizar(valorPerdido, 0, 5);

            int amenazasActivas = threatDetector.ContarAmenazas();
            float scorePresionDefensiva = 1f - Normalizar(amenazasActivas, 0, 3);

            // Combinar scores con PESOS REBALANCEADOS
            float scoreCombo =
                (scorePotencia * 0.40f) +      // ↑ Aumentado: priorizar poder
                (scoreIntelecto * 0.15f) +     // ↓ Reducido
                (scoreCaminoLibre * 0.20f) +   // Mantener
                (scoreEconomia * 0.10f) +      // ↓ Reducido
                (scoreEficiencia * 0.10f) +    // ↓ Reducido
                (scorePresionDefensiva * 0.05f);

            // Penalizaciones POR AMENAZAS (más severas)
            if (amenazasActivas > 0)
            {
                // Penalización base: 10% por amenaza (mucho más severo)
                float penalizacionPorAmenazas = Mathf.Pow(0.1f, amenazasActivas);
                scoreCombo *= penalizacionPorAmenazas;
                
                // 1 amenaza: ×0.1 (reducción del 90%)
                // 2 amenazas: ×0.01 (reducción del 99%)
                // 3+ amenazas: prácticamente 0
            }

            if (threatDetector.HayAmenazaCritica())
            {
                scoreCombo *= 0.05f; // Aumentado de 0.5 a 0.05 (reducción del 95%)
            }


            // ¿Es el mejor hasta ahora?
            if (scoreCombo > mejorScore)
            {
                mejorScore = scoreCombo;
                mejorCombo = combo;
            }
        }

        // Si no encontramos ningún combo viable
        if (mejorCombo == null)
        {
            scoreFinal = 0f;
            mejorComboEvaluado = null;
            return 0f;
        }

        // Guardar el mejor combo encontrado
        mejorComboEvaluado = mejorCombo;
        scoreFinal = mejorScore;

        return mejorScore;
    }

    public override void Ejecutar()
    {
        // Usar el combo ya evaluado en CalcularScore()
        AICardHand.ComboAtaque combo = mejorComboEvaluado;

        if (combo == null)
        {
            Debug.LogWarning("[AccionAtacarResta] Ejecutar llamado pero no hay combo evaluado");
            return;
        }

        // === CALCULAR POSICIÓN DE SPAWN INTELIGENTE ===
        Vector3 posicionSpawn = (spawnCalculator != null)
            ? spawnCalculator.CalcularMejorPosicionAtaque()
            : spawnPoint.position;

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
            // 🔧 FIX: NO remover cartas - la IA siempre tiene 1-5 disponibles
            // La única limitación es el intelecto
            // aiHand.RemoverCarta(combo.cartaA);  // REMOVIDO
            // aiHand.RemoverCarta(combo.cartaB);  // REMOVIDO
            // aiHand.RobarCarta();                // REMOVIDO
            // aiHand.RobarCarta();                // REMOVIDO
        }
        else
        {
            Debug.LogError($"[AccionAtacarResta] ❌ Falló al generar ataque {combo}");
        }
    }
}