using UnityEngine;

public class AccionDefender : AIAction
{
    private IntelectManager intelectManager;
    private CardManager cardManager;
    private AICardHand aiHand;
    private AIThreatDetector threatDetector;
    private Transform spawnPoint;
    private Transform miTorre;
    private AISpawnPositionCalculator spawnCalculator;

    public AccionDefender(
        IntelectManager intelecto,
        CardManager cards,
        AICardHand hand,
        AIThreatDetector detector,
        Transform spawn,
        Transform torre,
        AISpawnPositionCalculator calculator
    ) : base("Defender Ataque", TipoAccion.Defensa)
    {
        intelectManager = intelecto;
        cardManager = cards;
        aiHand = hand;
        threatDetector = detector;
        spawnPoint = spawn;
        miTorre = torre;
        spawnCalculator = calculator;
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
        

        CardManager.Card cartaDefensora = aiHand.ObtenerCartaPorValor(valorNecesario);

        if (cartaDefensora == null)
        {
            scoreFinal = 0f;
            return 0f;
        }

        int costeIntelecto = valorNecesario;

        if (intelectManager.currentIntelect < costeIntelecto)
        {
            scoreFinal = 0f;
            return 0f;
        }

        // Cálculo de score
        float scorePeligrosidad = amenaza.peligrosidad;
        float scoreUrgencia = Normalizar(amenaza.distancia, 0f, 20f);
        scoreUrgencia = CurvaInversa(scoreUrgencia);
        scoreUrgencia = CurvaCuadratica(scoreUrgencia);

        float scoreValorAmenaza = Normalizar(valorNecesario, 1, 5);

        int intelectoDespues = intelectManager.currentIntelect - costeIntelecto;
        float scoreEconomia = Normalizar(intelectoDespues, 0, intelectManager.maxIntelect);

        float ratioIntelecto = (float)intelectManager.currentIntelect / intelectManager.maxIntelect;
        float scoreDesesperacion = ratioIntelecto < 0.5f ? 0.8f : 0.5f;

        scoreFinal =
            (scorePeligrosidad * 0.35f) +
            (scoreUrgencia * 0.30f) +
            (scoreValorAmenaza * 0.20f) +
            (scoreEconomia * 0.05f) +
            (scoreDesesperacion * 0.10f);

        // Boost amenaza crítica
        if (amenaza.distancia < 5f)
        {
            scoreFinal = Mathf.Min(1f, scoreFinal * 1.5f);
        }
        
        // Boost múltiples amenazas
        int totalAmenazas = threatDetector.ContarAmenazas();
        if (totalAmenazas > 1)
        {
            float boost = 1.0f + (totalAmenazas * 0.1f);
            scoreFinal *= boost;
        }

        return scoreFinal;
    }

    public override void Ejecutar()
    {
        AIThreatDetector.Amenaza amenaza = threatDetector.ObtenerAmenazaMasPeligrosa();

        if (amenaza == null)
        {
            Debug.LogWarning("[AccionDefender] No hay amenazas para defender");
            return;
        }

        int valorNecesario = amenaza.valor;
        CardManager.Card carta = aiHand.ObtenerCartaPorValor(valorNecesario);

        if (carta == null)
        {
            Debug.LogWarning($"[AccionDefender] No tengo carta de valor {valorNecesario}");
            return;
        }

        // Marcar amenaza como defendida
        threatDetector.MarcarAmenazaComoDefendida(amenaza.objeto);

        // Calcular posición de spawn
        Vector3 posicionSpawn = spawnCalculator.CalcularMejorPosicionDefensa(amenaza.objeto.transform.position);

        // Ejecutar defensa
        bool exito = cardManager.GenerateCharacter(carta, posicionSpawn, "AITeam", intelectManager);

        if (exito)
        {
        }
        else
        {
            Debug.LogError($"[AccionDefender] Falló al generar defensor {carta.cardName}");
        }
    }
}
