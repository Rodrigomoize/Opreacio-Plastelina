using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IA SIMPLIFICADA
/// - Tiene 5 cartas (1-5) como el jugador
/// - Cada X segundos decide: ¿Defender ataque enemigo o lanzar mi propio ataque?
/// - Decisión basada en: distancia del ataque enemigo + agresividad de la IA
/// - Si defiende: espera un tiempo de reacción antes de ejecutar
/// - Marca ataques defendidos para no malgastar intelecto
/// </summary>
public class IAController : MonoBehaviour
{
    [Header("Referencias Obligatorias")]
    public CardManager cardManager;
    public IntelectManager intelectManagerIA;
    public IntelectManager intelectManagerPlayer;
    public Transform spawnPointIA;
    public Transform torreIA;

    [Header("Dificultad")]
    [Tooltip("Selecciona la dificultad de la IA")]
    public AIDificultad dificultadInicial = AIDificultad.Media;

    [Header("Debug")]
    public bool mostrarLogs = true;

    public enum AIDificultad { Facil, Media, Dificil }

    // ========== AJUSTES POR DIFICULTAD (EDITABLES EN INSPECTOR) ==========
    [System.Serializable]
    public class ConfiguracionDificultad
    {
        [Header("Velocidad de Acción")]
        [Tooltip("Tiempo MÍNIMO entre acciones (segundos)")]
        [Range(0.5f, 10f)]
        public float intervaloMin = 1.5f;

        [Tooltip("Tiempo MÁXIMO entre acciones (segundos)")]
        [Range(0.5f, 10f)]
        public float intervaloMax = 3f;

        [Header("Tiempo de Reacción")]
        [Tooltip("Tiempo MÍNIMO para ejecutar defensa")]
        [Range(0.1f, 5f)]
        public float reaccionMin = 0.8f;

        [Tooltip("Tiempo MÁXIMO para ejecutar defensa")]
        [Range(0.1f, 5f)]
        public float reaccionMax = 2f;

        [Header("Comportamiento Táctico")]
        [Tooltip("Distancia para defender siempre (metros)")]
        [Range(5f, 20f)]
        public float umbralDefensa = 12f;

        [Tooltip("Probabilidad de atacar en vez de defender (0=defiende, 1=ataca)")]
        [Range(0f, 1f)]
        public float chanceAtaque = 0.5f;

        [Header("Recursos")]
        [Tooltip("Velocidad de regeneración de intelecto (segundos por punto)")]
        [Range(1f, 5f)]
        public float velocidadRegenIntelecto = 2.8f;
    }

    [Header("=== CONFIGURACIÓN FÁCIL ===")]
    public ConfiguracionDificultad configFacil = new ConfiguracionDificultad
    {
        intervaloMin = 3f,
        intervaloMax = 5f,
        reaccionMin = 1.5f,
        reaccionMax = 3f,
        umbralDefensa = 8f,
        chanceAtaque = 0.4f,
        velocidadRegenIntelecto = 3.5f
    };

    [Header("=== CONFIGURACIÓN MEDIA ===")]
    public ConfiguracionDificultad configMedia = new ConfiguracionDificultad
    {
        intervaloMin = 1.5f,
        intervaloMax = 3f,
        reaccionMin = 0.8f,
        reaccionMax = 2f,
        umbralDefensa = 12f,
        chanceAtaque = 0.5f,
        velocidadRegenIntelecto = 2.8f
    };

    [Header("=== CONFIGURACIÓN DIFÍCIL ===")]
    public ConfiguracionDificultad configDificil = new ConfiguracionDificultad
    {
        intervaloMin = 0.8f,
        intervaloMax = 1.5f,
        reaccionMin = 0.3f,
        reaccionMax = 1f,
        umbralDefensa = 15f,
        chanceAtaque = 0.6f,
        velocidadRegenIntelecto = 2.2f
    };

    // ========== COMPONENTES INTERNOS ==========
    private AICardHand mano;
    private AIThreatDetector detector;
    private AISpawnPositionCalculator spawnCalc;

    // ========== CONFIGURACIÓN ACTIVA ==========
    private AIDificultad dificultadActual;
    private float intervaloMin, intervaloMax;          // Tiempo entre acciones
    private float reaccionMin, reaccionMax;            // Tiempo pensando la defensa
    private float umbralDefensa;                        // Distancia para priorizar defensa
    private float chanceAtaque;                         // Probabilidad de atacar en vez de defender

    // ========== ESTADO ==========
    private float tiempoHastaAccion;                    // Tiempo restante hasta poder actuar
    private bool esperandoDefender;                     // ¿Está esperando para ejecutar defensa?
    private float tiempoEsperaDefensa;                  // Tiempo restante para ejecutar defensa
    private CardManager.Card cartaPendiente;            // Carta que va a usar para defender
    private int valorDefensaPendiente;                  // Valor del ataque que va a defender
    private GameObject amenazaPendiente;                // Ataque que va a defender



    void Start()
    {
        if (!ValidarReferencias()) return;

        // Inicializar componentes
        mano = new AICardHand(cardManager);
        detector = new AIThreatDetector(torreIA);
        spawnCalc = new AISpawnPositionCalculator(spawnPointIA, torreIA, detector);

        // Aplicar dificultad inicial (puede ser sobreescrita por DifficultyManager)
        dificultadActual = dificultadInicial;
        ConfigurarDificultad();

        // Primer intervalo aleatorio
        tiempoHastaAccion = Random.Range(intervaloMin, intervaloMax);

        Log($"IA inicializada - Dificultad: {dificultadActual} - Primera acción en {tiempoHastaAccion:F1}s");
    }

    void Update()
    {
        // ===== 1. ESPERA PARA EJECUTAR DEFENSA =====
        if (esperandoDefender)
        {
            tiempoEsperaDefensa -= Time.deltaTime;

            if (tiempoEsperaDefensa <= 0f)
            {
                // Ejecutar defensa ahora
                EjecutarDefensa();
                esperandoDefender = false;
                
                // Resetear cooldown para próxima acción
                tiempoHastaAccion = Random.Range(intervaloMin, intervaloMax);
            }
            return; // No hacer nada más mientras espera
        }

        // ===== 2. COOLDOWN ENTRE ACCIONES =====
        tiempoHastaAccion -= Time.deltaTime;

        if (tiempoHastaAccion <= 0f)
        {
            TomarDecision();
        }
    }

    /// <summary>
    /// LÓGICA PRINCIPAL: ¿Defender o Atacar?
    /// </summary>
    private void TomarDecision()
    {
        Log("\n========== NUEVA DECISIÓN ==========");
        Log($"Intelecto: {intelectManagerIA.currentIntelect} | Cartas: {mano.CantidadCartas()}");

        // ===== 1. DETECTAR AMENAZAS =====
        var amenazas = detector.DetectarAmenazas();

        if (amenazas.Count > 0)
        {
            var amenazaMasPeligrosa = amenazas[0]; // Ya están ordenadas por peligrosidad
            Log($"Amenaza detectada: Valor={amenazaMasPeligrosa.valor} Dist={amenazaMasPeligrosa.distancia:F1}m");

            // ===== 2. DECIDIR: ¿DEFENDER O ATACAR? =====
            bool debeDefender = DecidirSiDefender(amenazaMasPeligrosa);

            if (debeDefender)
            {
                IniciarDefensa(amenazaMasPeligrosa);
                return;
            }
            else
            {
                Log("Amenaza lejos o baja prioridad - Voy a atacar");
            }
        }
        else
        {
            Log("Sin amenazas - Voy a atacar");
        }

        // ===== 3. INTENTAR ATACAR =====
        bool ataqueExitoso = IntentarAtacar();

        if (!ataqueExitoso)
        {
            // No pudo atacar ni defender - esperar menos tiempo
            tiempoHastaAccion = intervaloMin * 0.5f;
            Log($"No puedo actuar - reintento en {tiempoHastaAccion:F1}s");
        }
    }

    /// <summary>
    /// Decide si debe defender un ataque o ignorarlo
    /// Basado en: Distancia del ataque + Agresividad/Dificultad
    /// </summary>
    private bool DecidirSiDefender(AIThreatDetector.Amenaza amenaza)
    {
        // ¿Tengo intelecto para defender?
        if (intelectManagerIA.currentIntelect < amenaza.valor)
        {
            Log($"No tengo intelecto para defender (necesito {amenaza.valor}, tengo {intelectManagerIA.currentIntelect})");
            return false;
        }

        // ¿Tengo la carta necesaria? (Siempre debería tenerla si tengo intelecto)
        var carta = mano.ObtenerCartaPorValor(amenaza.valor);
        if (carta == null)
        {
            Log($"No tengo carta de valor {amenaza.valor}");
            return false;
        }

        // PRIORIDAD POR DISTANCIA
        // Si el ataque está muy cerca → DEFENDER SIEMPRE
        if (amenaza.distancia < umbralDefensa)
        {
            Log($"Ataque CRÍTICO (dist={amenaza.distancia:F1}m < {umbralDefensa}m) - DEFENDER");
            return true;
        }

        // Si está lejos → Depende de la agresividad/dificultad
        // Más agresiva = más probable que ignore defensa y ataque
        float random = Random.value;
        bool decidoDefender = random > chanceAtaque;

        Log($"Ataque lejano - Random={random:F2} vs ChanceAtaque={chanceAtaque:F2} → {(decidoDefender ? "DEFENDER" : "ATACAR")}");
        return decidoDefender;
    }

    /// <summary>
    /// Inicia el proceso de defensa (con tiempo de reacción)
    /// </summary>
    private void IniciarDefensa(AIThreatDetector.Amenaza amenaza)
    {
        var carta = mano.ObtenerCartaPorValor(amenaza.valor);

        // Marcar amenaza como defendida AHORA (para que no la procese dos veces)
        detector.MarcarAmenazaComoDefendida(amenaza.objeto);

        // Guardar datos para ejecutar después
        cartaPendiente = carta;
        valorDefensaPendiente = amenaza.valor;
        amenazaPendiente = amenaza.objeto;

        // Generar tiempo de reacción aleatorio
        tiempoEsperaDefensa = Random.Range(reaccionMin, reaccionMax);
        esperandoDefender = true;

        Log($"🛡️ Voy a defender con carta {carta.cardName} en {tiempoEsperaDefensa:F1}s");
    }

    /// <summary>
    /// Ejecuta la defensa después del tiempo de reacción
    /// </summary>
    private void EjecutarDefensa()
    {
        if (cartaPendiente == null || amenazaPendiente == null)
        {
            LogError("Error: Datos de defensa pendiente perdidos");
            return;
        }

        // Calcular mejor posición defensiva
        Vector3 posicion = spawnCalc.CalcularMejorPosicionDefensa(amenazaPendiente.transform.position);

        // Generar defensor
        bool exito = cardManager.GenerateCharacter(cartaPendiente, posicion, "AITeam", intelectManagerIA);

        if (exito)
        {
            Log($"✅ Defendido ataque valor {valorDefensaPendiente} con {cartaPendiente.cardName}");
        }
        else
        {
            LogError($"❌ Fallo al generar defensor {cartaPendiente.cardName}");
        }

        // Limpiar datos
        cartaPendiente = null;
        amenazaPendiente = null;
    }

    /// <summary>
    /// Intenta lanzar un ataque (operación matemática)
    /// Alterna aleatoriamente entre suma y resta para variedad
    /// </summary>
    private bool IntentarAtacar()
    {
        // Obtener TODOS los combos posibles
        var combosSuma = mano.EncontrarTodosCombosSuma();
        var combosResta = mano.EncontrarTodosCombosResta();

        // Combinar ambos tipos en una lista
        List<(AICardHand.ComboAtaque combo, bool esResta)> todosLosCombos = new List<(AICardHand.ComboAtaque, bool)>();

        foreach (var combo in combosSuma)
        {
            if (intelectManagerIA.currentIntelect >= combo.resultado)
            {
                todosLosCombos.Add((combo, false)); // false = suma
            }
        }

        foreach (var combo in combosResta)
        {
            if (intelectManagerIA.currentIntelect >= combo.resultado)
            {
                todosLosCombos.Add((combo, true)); // true = resta
            }
        }

        // ¿Hay algún combo viable?
        if (todosLosCombos.Count == 0)
        {
            Log("No tengo intelecto para ningún ataque");
            return false;
        }

        // ELEGIR UNO ALEATORIO para máxima variedad
        int indiceAleatorio = Random.Range(0, todosLosCombos.Count);
        var comboElegido = todosLosCombos[indiceAleatorio];

        return EjecutarAtaque(comboElegido.combo, comboElegido.esResta);
    }

    /// <summary>
    /// Ejecuta un ataque con un combo (suma o resta)
    /// </summary>
    private bool EjecutarAtaque(AICardHand.ComboAtaque combo, bool esResta)
    {
        char operador = esResta ? '-' : '+';
        Vector3 posicion = spawnCalc.CalcularMejorPosicionAtaque();

        bool exito = cardManager.GenerateCombinedCharacter(
            combo.cartaA,
            combo.cartaB,
            posicion,
            combo.resultado,
            operador,
            "AITeam",
            intelectManagerIA
        );

        if (exito)
        {
            // Resetear cooldown para próxima acción
            tiempoHastaAccion = Random.Range(intervaloMin, intervaloMax);

            Log($"⚔️ Ataque lanzado: {combo.cartaA.cardValue}{operador}{combo.cartaB.cardValue}={combo.resultado} - Próxima acción en {tiempoHastaAccion:F1}s");
            return true;
        }
        else
        {
            LogError("❌ Fallo al generar ataque combinado");
            return false;
        }
    }

    /// <summary>
    /// Configura intervalos y comportamiento según dificultad
    /// </summary>
    private void ConfigurarDificultad()
    {
        // Obtener configuración según dificultad
        ConfiguracionDificultad config = dificultadActual switch
        {
            AIDificultad.Facil => configFacil,
            AIDificultad.Media => configMedia,
            AIDificultad.Dificil => configDificil,
            _ => configMedia
        };

        // Aplicar configuración a las variables activas
        intervaloMin = config.intervaloMin;
        intervaloMax = config.intervaloMax;
        reaccionMin = config.reaccionMin;
        reaccionMax = config.reaccionMax;
        umbralDefensa = config.umbralDefensa;
        chanceAtaque = config.chanceAtaque;

        // Aplicar velocidad de regeneración de intelecto
        if (intelectManagerIA != null)
        {
            intelectManagerIA.regenInterval = config.velocidadRegenIntelecto;
        }

        if (intelectManagerPlayer != null)
        {
            intelectManagerPlayer.regenInterval = config.velocidadRegenIntelecto;
        }

        Log($"Dificultad {dificultadActual} configurada:\n" +
            $"  - Intervalo: {intervaloMin}-{intervaloMax}s\n" +
            $"  - Reacción: {reaccionMin}-{reaccionMax}s\n" +
            $"  - Umbral Defensa: {umbralDefensa}m\n" +
            $"  - Chance Ataque: {chanceAtaque:F2}\n" +
            $"  - Regen Intelecto: {config.velocidadRegenIntelecto}s/punto");
    }

    private bool ValidarReferencias()
    {
        if (cardManager == null) { LogError("CardManager no asignado"); return false; }
        if (intelectManagerIA == null) { LogError("IntelectManager no asignado"); return false; }
        if (spawnPointIA == null) { LogError("SpawnPoint no asignado"); return false; }
        if (torreIA == null) { LogError("Torre no asignada"); return false; }
        return true;
    }

    private void Log(string msg)
    {
        if (mostrarLogs) Debug.Log($"[IA] {msg}");
    }

    private void LogError(string msg)
    {
        Debug.LogError($"[IA] {msg}");
    }

    // ========== MÉTODOS PÚBLICOS ==========

    public void SetDificultad(AIDificultad nuevaDificultad)
    {
        dificultadActual = nuevaDificultad;
        ConfigurarDificultad();
    }

    public void ForzarDecision()
    {
        tiempoHastaAccion = 0f;
    }

    /// <summary>
    /// Obtiene información de configuración actual (para DifficultyManager)
    /// </summary>
    public string GetConfigInfo()
    {
        ConfiguracionDificultad config = dificultadActual switch
        {
            AIDificultad.Facil => configFacil,
            AIDificultad.Media => configMedia,
            AIDificultad.Dificil => configDificil,
            _ => configMedia
        };

        string info = "";
        info += $"IA Dificultad: {dificultadActual}\n";
        info += $"IA Intervalo Acción: {intervaloMin:F2}s - {intervaloMax:F2}s\n";
        info += $"IA Tiempo Reacción: {reaccionMin:F2}s - {reaccionMax:F2}s\n";
        info += $"IA Umbral Defensa: {umbralDefensa:F1}m\n";
        info += $"IA Chance Ataque: {chanceAtaque:F2} ({(1-chanceAtaque):F2} defensa)\n";
        info += $"Velocidad Regen Intelecto: {config.velocidadRegenIntelecto:F2}s/punto\n";
        return info;
    }
}
